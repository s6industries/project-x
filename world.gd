extends Node2D

@export var world_label: Label
@export var inventory_label: Label
@export var player_marker: Marker2D
@export var button: Button
@export var button2: Button
@export var inventory_button_left: Button
@export var inventory_button_right: Button
@export var inventory_button_equip: Button

enum State { IDLE, MOVING, ACTION, DELAY }
const PLAYER = "@"
const ANDROID = "A"
const WALL = "#"
const HOE = "h"
const BLANK = " "
const TILLED_SOIL = "="
const SHIP = "S"
const POTATO_STAGE = [".", ";", "i", "P"]
const SEED = "."
const MOVE_DELAY = 0.12
const WORLD_TICK = 1.0
const FONT_OFFSET = Vector2i(3, 4)
const FONT_SIZE = Vector2i(6, 14)
const MAX_HEALTH = 100
const MAX_SATIATION = 100

var player_pos: Vector2i
var input_direction: Vector2i
var world_map: Array
var id = 0
var metabots: Dictionary # { id : [stage, position], etc }
var state: State = State.IDLE
var timer: Timer = null
var world_clock: Timer = null

# var metabot_simulator
var potato_stage: int
var diagonal_moving_toggle: bool = false
var inventory: Dictionary = Dictionary()
var inventory_array: Array = Array()
var equipped: int = -1
var action_button_pressed = false
var inventory_selection_index = 0
var hunger = MAX_SATIATION
var health = MAX_HEALTH


#const MetabotSimulator = preload("res://metabot_simulator.gd")
var agent_world:AgentWorld
var metabot_simulator:MetabotSimulator
var placement:Array
var shareable_placement:Array
var nonshareable_placement:Array
var detectable:Array

func spawn_android(location: Vector3i):
	placement = ["android", "grounded",]
	# world layers which entities of type can share a world zone.
	shareable_placement = ["grounded",]
	# world layers which entities of type can NOT share a world zone.
	nonshareable_placement = ["android", ]
	# which senses can detect this entity
	detectable = ["vision"]
	
	# setup AI android
	var e_android = AgentWorld.Entity.new(placement, shareable_placement, nonshareable_placement, detectable)
	agent_world.add_entity(e_android, location, 'android_AI')
	# agent and entity instances are mutually registered
	var ai_agent = Agent.AIAgent.new(agent_world)
	ai_agent.entity = e_android
	e_android.agent = ai_agent
	add_child(ai_agent)
	print(agent_world.coordinates)


func spawn_seed(location: Vector3i):    
	placement = ["seed", "grounded",]
	# world layers which entities of type can share a world zone.
	shareable_placement = ["grounded",]
	# world layers which entities of type can NOT share a world zone.
	nonshareable_placement = ["seed",]
	# which senses can detect this entity
	detectable = ["vision"]
	
	# TODO implement seed source as spaceship
	var e_seed = AgentWorld.Entity.new(placement, shareable_placement, nonshareable_placement, detectable)
	agent_world.add_entity(e_seed, location)
	
	# metabots plant potato at location
	metabots[id] = [0, location]
	var potato = metabot_simulator.plant_potato(id)
	potato.connect("life_stage_progressed", self.potato_life_stage_progressed)
	id += 1
	
	# Attach water resource
	print("ATTACHING WATER RESOURCE TO SOIL")
	var pool_water = Metabot.Pool.new("water", 100)
	var pool_minerals = Metabot.Pool.new("minerals", 100)
	potato.collector.add_source(pool_water)
	potato.collector.add_source(pool_minerals)


func spawn_tilled_soil(location: Vector3i):    
	placement = ["soil", "grounded"]
	# world layers which entities of type can share a world zone.
	shareable_placement = ["grounded",]
	# world layers which entities of type can NOT share a world zone.
	nonshareable_placement = ["soil",]
	# which senses can detect this entity
	detectable = ["vision"]
	
	# on soil created, it should have pools of water and minerals that plants buried in it will use to grow
	# soil becomes a passthrough entity for plant metabots attached to it
	# water and minerals added to soil, its pools increase, the attached seed passes to the plant metabot		
	var e_soil = AgentWorld.Entity.new(placement, shareable_placement, nonshareable_placement, detectable)
	agent_world.add_entity(e_soil, location)


func load_world():		
	var file_path = "res://world.txt"
	var file = FileAccess.open(file_path, FileAccess.READ)
	var y = 0
	while not file.eof_reached():
		var line = file.get_line()
		if line.is_empty():
			printerr("EMPTY LINE")
			continue
		var x = line.find(PLAYER)
		if x >= 0:
			player_pos = Vector2i(x, y)
			print("FOUND PLAYER")
			line[x] = " "
		# Replace player with blank space bc player gets rendered separately
		world_map.append(line)
		y += 1
	
	print(world_map)
	var num_rows = world_map.size()
	var num_cols = world_map[0].length()
	
	agent_world = AgentWorld.new(Vector3i(num_cols, num_rows, 1))
	inititate_entities(num_rows, num_cols)
	add_child(agent_world)


func inititate_entities(rows, cols):
	# Required to spawn potato metabot, which are used in spawn_seed()
	metabot_simulator = MetabotSimulator.new()
	add_child(metabot_simulator)

	# Spawn entities from world map
	for y in range(rows):
		for x in range(cols):
			var new_location = Vector3i(x, y, 0)
			if world_map[y][x] == ANDROID:
				spawn_android(new_location)
				world_map[y][x] = " "
			elif world_map[y][x] == TILLED_SOIL:
				spawn_tilled_soil(new_location)
				world_map[y][x] = " "
			elif world_map[y][x] == SEED:
				spawn_seed(new_location)
				world_map[y][x] = " "


func initiate_timers():
	timer = Timer.new()
	timer.set_one_shot(true)
	timer.connect("timeout", self.animation_completed)
	add_child(timer)

	world_clock = Timer.new()
	world_clock.set_one_shot(false)
	world_clock.connect("timeout", self.world_ticked)
	world_clock.set_wait_time(WORLD_TICK)	
	add_child(world_clock)
	world_clock.start()


# Called when the node enters the scene tree for the first time.
func _ready():
	button.text = "Pick up"
	button.hide()
	button2.text = "Use"
	button2.hide()
	

	load_world()
	initiate_timers()
	inventory_menu_visible(false)
	# TODO setup scenarios from data file (SQLite?)
	# agent_world = AgentWorld.new(Vector3i(3, 4, 1))


func on_new_soil(_self:AgentWorld.Entity):
	pass
	print("on_new_soil")
	print(_self)
	
	var pool_water = Metabot.Pool.new("water", 0)
	pool_water.add(100)
	var pool_minerals = Metabot.Pool.new("minerals", 0)
	pool_minerals.add(100)
	
	_self.pools.append_array([ pool_water, pool_minerals ])


func potato_life_stage_progressed(id, stage):
	print("potato_life_stage_progressed id:", id, ", stage:", stage)
	potato_stage = stage
	metabots[id][0] = stage
 

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _physics_process(_delta):
	match state:
		State.IDLE:
			idle_state()
		State.MOVING: 
			moving_state()
		State.ACTION:
			action_state()
		State.DELAY:
			# do nothing
			pass
	update_world()
	update_inventory()


func get_player_input():
	var x = round(Input.get_action_strength("ui_right") - Input.get_action_strength("ui_left"))
	var y = round(Input.get_action_strength("ui_down") - Input.get_action_strength("ui_up"))
	if x == 0 and y == 0:
		return Vector2i.ZERO
	# Cardinal movement (LEFT, RIGHT, UP, DOWN)
	if x == 0 or y == 0:
		return Vector2i(x, y)
	# Diagonal movement
	diagonal_moving_toggle = !diagonal_moving_toggle
	if diagonal_moving_toggle:
		return Vector2i(x, 0)
	return Vector2i(0, y)


func idle_state():
	observe_surroundings()
	
	if action_button_pressed:
		action_button_pressed = false
		state = State.ACTION
		return
	
	var direction = get_player_input()
	if direction != Vector2i.ZERO:
		input_direction = direction
		state = State.MOVING


func observe_surroundings():
	var x = player_pos[0]
	var y = player_pos[1]
	var position = world_map[y][x]
	if position == HOE:
		button.text = "Pick up hoe"
		button.show()
		return
	if position == SHIP:
		button.text = "Get seeds"
		button.show()
		return
	
	var player_pos3i = Vector3i(x, y, 0)
	for id in metabots:
		var pos: Vector3i = metabots[id][1]
		var stage: int = min(metabots[id][0], POTATO_STAGE.size() - 1)
		if player_pos3i == pos and stage == POTATO_STAGE.size() - 1:
			button.text = "Pick up potato"
			button.show()
			return

	button.hide()


func moving_state():
	if state != State.MOVING:
		printerr("ERROR: arrived at moving_state() while state is at: ", state)
		return
	state = State.DELAY
	move(input_direction)
	timer.set_wait_time(MOVE_DELAY)
	timer.start()


func action_state():
	if state != State.ACTION:
		printerr("ERROR: arrived at action_state() while state is at: ", state)
		return
	state = State.DELAY
	var item = get_equipped_item()
	var location = Vector3i(player_pos[0], player_pos[1], 0)
	var tilled_soil: Dictionary = agent_world.get_entities_of_type(["soil", "seed"], location)
	print("TILLED SOIL DICT: ", tilled_soil)
	if item == HOE and "soil" not in tilled_soil:
		print("FOUND BLANK SPOT TO TILL SOIL!")
		spawn_tilled_soil(location)	
	elif item == SEED and "soil" in tilled_soil:
		print("FOUND TILLED SOIL TO PLANT SEED!")
		spawn_seed(location)
		remove_item_from_inventory(SEED)
	elif item == POTATO_STAGE[POTATO_STAGE.size() - 1]:		
		remove_item_from_inventory(item)
		health = min(health + 10, MAX_HEALTH)
		hunger = MAX_SATIATION
		print("EATING POTATO. Health: ", health, " Hunger: ", hunger)
	timer.set_wait_time(MOVE_DELAY)
	timer.start()


func update_world():
	var x = player_pos[0]
	var y = player_pos[1]
	var temp_world = world_map.duplicate()
	if y < len(temp_world) and x < len(temp_world[0]):
		temp_world[y][x] = PLAYER
		player_marker.position.x = x * FONT_SIZE.x + FONT_OFFSET.x
		player_marker.position.y = y * FONT_SIZE.y + FONT_OFFSET.y
	
	var android_location: Vector3i
	for entity in agent_world.entities:
		x = entity.center_point[0]
		y = entity.center_point[1]
		if entity.placement.has("seed"):
			temp_world[y][x] = SEED
		if entity.placement.has("soil"):
			temp_world[y][x] = TILLED_SOIL
		if entity.placement.has("android"):
			# Save android location to render on top of metabots below
			android_location = entity.center_point			
	
	# ORDER MATTERS. The metabot life stages should be rendered on top of the others
	for id in metabots:
		var pos: Vector3i = metabots[id][1]
		var stage: int = min(metabots[id][0], POTATO_STAGE.size() - 1)
		temp_world[pos.y][pos.x] = POTATO_STAGE[stage]
	
	# Render Android
	temp_world[android_location.y][android_location.x] = ANDROID

	# Render player
	x = player_pos[0]
	y = player_pos[1]
	# TODO: do you need this bounds check?
	if y < len(temp_world) and x < len(temp_world[0]):
		temp_world[y][x] = PLAYER

	# create a multiline string of the map
	var world_string = ""
	for row in temp_world:
		world_string += row + "\n"
	world_label.text = world_string


func update_inventory():
	# Hijacking the inventory label to include health and hunger
	inventory_label.text = "Health: " + str(health) + ", Hunger: " + str(hunger)	
	# Inventory empty
	if inventory.size() == 0:
		inventory_label.text = ""
		inventory_menu_visible(false)
		return
	# Non-empty inventory
	inventory_menu_visible(true)
	var text: String = "\nInventory: "
	for item in inventory:
		var count = inventory[item]
		text += item + " x" + str(count) + ", "
	inventory_label.text += text
	# If no item equipped:
	if equipped < 0 or equipped >= inventory_array.size():
		button2.hide()
		return
	# If equipped
	button2.text = "Use " + inventory_array[equipped]
	button2.show()
	inventory_label.text += "\nEquipped: " + inventory_array[equipped]
	update_inventory_button_equip()


func move(direction: Vector2i):
	if is_position_walkable(player_pos + direction):
		player_pos += direction


func is_position_walkable(pos):
	if world_map[pos[1]][pos[0]] == WALL:
		return false
	return true


func animation_completed():
	print("ANIMATION_COMPLETED!")	
	state = State.IDLE


func world_ticked():
	print("WORLD TICKED")
	hunger = max(hunger - 1, 0)
	if hunger == 0:
		health = max(health - 1, 0)


func _on_button_pressed():
	print("PICKUP BUTTON PRESSED")
	var item = get_world_item_at(player_pos)
	if item == HOE:
		item = remove_world_item_at(player_pos)
		add_item_to_inventory(item)
		return
	if item == SHIP:
		item = SEED
		add_item_to_inventory(item)
		return
	# Check if item is a metabot potato
	var player_pos3i = Vector3i(player_pos.x, player_pos.y, 0)
	for id in metabots:
		var pos: Vector3i = metabots[id][1]
		var stage: int = min(metabots[id][0], POTATO_STAGE.size() - 1)
		if player_pos3i == pos and stage == POTATO_STAGE.size() - 1:
			item = POTATO_STAGE[POTATO_STAGE.size() - 1]			
			add_item_to_inventory(item)
			metabot_simulator.harvest_potato(id)
			metabots.erase(id)
			return


func get_world_item_at(position: Vector2i):
	var x = position.x
	var y = position.y
	return world_map[y][x]


func set_world_item_at(position: Vector2i, item: String):
	var x = position.x
	var y = position.y
	world_map[y][x] = item


func remove_world_item_at(position: Vector2i):
	var x = position.x
	var y = position.y
	var item = world_map[y][x]
	world_map[y][x] = BLANK
	return item


func _on_button_2_pressed():
	print("ACTION BUTTON PRESSED")
	action_button_pressed = true
	
	
func get_equipped_item() -> String:
	if equipped >= 0 and equipped < inventory.size():
		return inventory_array[equipped]
	return ""


func add_item_to_inventory(item: String):
	if item not in inventory:
		inventory[item] = 0
		inventory_array.append(item)
	inventory[item] += 1
	


func remove_item_from_inventory(item: String):
	if item not in inventory:
		return
	inventory[item] -= 1
	if inventory[item] > 0:
		return
	# Item quantity is 0. Delete item
	# unequip item if it is currently equipped
	var item_index = inventory_array.find(item)
	if item_index != -1 and equipped == item_index:			
		equipped = -1
	# erase item from inventory
	inventory.erase(item)
	inventory_array.erase(item)
	if inventory_selection_index >= inventory_array.size():
		inventory_selection_index = inventory_array.size() - 1


func inventory_menu_visible(is_visible: bool):
	inventory_button_equip.visible = is_visible
	inventory_button_left.visible = is_visible
	inventory_button_right.visible = is_visible


func _on_inventory_left_pressed():
	if inventory_selection_index == 0:
		print("CANNOT GO LEFT ANY FURTHER")
		return
	inventory_selection_index -= 1
	# update_inventory_button_equip()


func _on_inventory_right_pressed():
	if inventory_selection_index == inventory_array.size() - 1:
		print("CANNOT GO RIGHT ANY FURTHER")
		return
	inventory_selection_index += 1
	# update_inventory_button_equip()


func update_inventory_button_equip():
	if inventory_array.size() < 1:
		return
	if equipped == inventory_selection_index:
		inventory_button_equip.text = "Unequip " + inventory_array[inventory_selection_index]
		return
	inventory_button_equip.text = "Equip " + inventory_array[inventory_selection_index]


func _on_inventory_equip_pressed():
	assert(inventory_selection_index >= 0)
	assert(inventory_selection_index < inventory_array.size())
	if equipped == inventory_selection_index:
		# unequip item
		equipped = -1
	else:
		# equip item
		equipped = inventory_selection_index
	# update_inventory_button_equip()
