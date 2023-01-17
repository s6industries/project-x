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
const FONT_OFFSET = Vector2i(3, 4)
const FONT_SIZE = Vector2i(6, 14)

var player_pos: Vector2i
var input_direction: Vector2i
var world_map: Array
var id = 0
var metabots: Dictionary # id : [stage, position]
var state: State = State.IDLE
var timer: Timer = null
# var can_move: bool = true

# var metabot_simulator
var potato_stage: int
var diagonal_moving_toggle: bool = false
var inventory: Dictionary = Dictionary()
var inventory_array: Array = Array()
var equipped: int = -1
var action_button_pressed = false
var inventory_selection_index = 0

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
	## CHECKPOINT
	var num_rows = world_map.size()
	var num_cols = world_map[0].length()
	
	# Instantiate Agent World
	agent_world = AgentWorld.new(Vector3i(num_cols, num_rows, 1))
	# Spawn the entities
	for y2 in range(num_rows):
		for x2 in range(num_cols):
			# print("x2, y2: ", x2, " ", y2)
			var new_location = Vector3i(x2, y2, 0)
#			print( world_map[y2], " ", typeof( world_map[y2]))
#			print(world_map[y2][x2])
			if world_map[y2][x2] == ANDROID:
				spawn_android(new_location)
				world_map[y2][x2] = " "
			elif world_map[y2][x2] == TILLED_SOIL:
				spawn_tilled_soil(new_location)
				world_map[y2][x2] = " "
			elif world_map[y2][x2] == SEED:
				spawn_seed(new_location)
				world_map[y2][x2] = " "
	# func initiate_agents():
	# TODO setup scenarios from data file (SQLite?)
	add_child(agent_world)


func initiate_timer():
	timer = Timer.new()
	timer.set_one_shot(true)
	timer.connect("timeout", self.animation_completed)
	add_child(timer)


func initiate_metabots():
	metabot_simulator = MetabotSimulator.new()
	add_child(metabot_simulator)

# Called when the node enters the scene tree for the first time.
func _ready():
	button.text = "Pick up"
	button.hide()
	button2.text = "Use"
	button2.hide()
	
	load_world()
	initiate_timer()
	inventory_menu_visible(false)
	# TODO setup scenarios from data file (SQLite?)
	# agent_world = AgentWorld.new(Vector3i(3, 4, 1))


func test_metabots():
	# metabots plant potat AT
	metabots[id] = [0, Vector2i(20, 10)]
	var potato = metabot_simulator.plant_potato(id)
#	potato.life_stage_progressed.connect(self.potato_life_stage_progressed.bind(stage))
	potato.connect("life_stage_progressed", self.potato_life_stage_progressed)
	id += 1
	
	# metabots plant potat AT
	metabots[id] = [0, Vector2i(40, 10)]
	var potato2 = metabot_simulator.plant_potato(id)
#	potato.life_stage_progressed.connect(self.potato_life_stage_progressed.bind(stage))
	potato2.connect("life_stage_progressed", self.potato_life_stage_progressed)
	id += 1


func test_entities_with_metabots():
	metabots[id] = [0, Vector2i(20, 10)]
	var potato = metabot_simulator.plant_potato(id)
#	potato.life_stage_progressed.connect(self.potato_life_stage_progressed.bind(stage))
	potato.connect("life_stage_progressed", self.potato_life_stage_progressed)
	id += 1
	
	var placement:Array
	var shareable_placement:Array
	var nonshareable_placement:Array
	var detectable:Array
	var tags:Array
	
	placement = [
		"seed",
		"grounded",
	]
	# world layers which entities of type can share a world zone.
	shareable_placement = [
		"grounded",
	]
	# world layers which entities of type can NOT share a world zone.
	nonshareable_placement = [
		"seed", 
	]
	# which senses can detect this entity
	detectable = [
		"vision"
	]
	tags = [
		"seed"
	]
	
	# TODO implement seed source (as spaceship / headquarters?)
	var e_seed_locations = [
		Vector3i(1, 1, 0),
	]
	for location in e_seed_locations:
		var e_seed = AgentWorld.Entity.new(placement, shareable_placement, nonshareable_placement, detectable, tags)
		agent_world.add_entity(e_seed, location)
		
	
	placement = [
		"soil",
		"grounded",
	]
	# world layers which entities of type can share a world zone.
	shareable_placement = [
		"grounded",
	]
	# world layers which entities of type can NOT share a world zone.
	nonshareable_placement = [
		"soil", 
	]
	# which senses can detect this entity
	detectable = [
		"vision"
	]
	tags = [
		"soil"
	]
	
	# on soil created, it should have pools of water and minerals that plants buried in it will use to grow
	# soil becomes a passthrough entity for plant metabots attached to it
	# water and minerals added to soil, its pools increase, the attached seed passes to the plant metabot
	
	
# func initiate_agents():	
# 	# TODO setup scenarios from data file (SQLite?)
# 	agent_world = AgentWorld.new(Vector3i(3, 4, 1))


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
	print("potato_life_stage_progressed: ", id, stage)
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
	elif position == SHIP:
		button.text = "Get seeds"
		button.show()
	else:
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
	if item == HOE and get_world_item_at(player_pos) == BLANK:
		set_world_item_at(player_pos, TILLED_SOIL)
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
	
	for id in metabots:
		var pos = metabots[id][1]
		var stage = metabots[id][0]
		temp_world[pos[1]][pos[0]] = POTATO_STAGE[stage]
	
#	print(agent_world.entities)
	var x2 = 0

	# TODO: fix this. do not loop twice.
	for entity in agent_world.entities:
#		print("placement", entity.placement)
		x = entity.center_point[0]
		y = entity.center_point[1]
		if entity.placement.has("seed"):
			temp_world[y][x] = SEED
		elif entity.placement.has("soil"):
			temp_world[y][x] = TILLED_SOIL

	for entity in agent_world.entities:
#		print("placement", entity.placement)
		x = entity.center_point[0]
		y = entity.center_point[1]
		if entity.placement.has("android"):
			temp_world[y][x] = ANDROID
	
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
	if inventory.size() == 0:
		inventory_label.text = ""
		return
	inventory_menu_visible(true)
	var text: String = "Inventory: "
	for item in inventory:
		var count = inventory[item]
		text += item + " x" + str(count) + ", "
	inventory_label.text = text
	if equipped < 0 or equipped >= inventory_array.size():
		button2.hide()
		return
	button2.text = "Use " + inventory_array[equipped]
	button2.show()
	inventory_label.text += "\nEquipped: " + inventory_array[equipped]
	


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


func _on_button_pressed():
	print("PICKUP BUTTON PRESSED")
	var item = get_world_item_at(player_pos)
	if item == HOE:
		item = remove_world_item_at(player_pos)
		add_item_to_inventory(item)
	elif item == SHIP:
		item = SEED
		add_item_to_inventory(item)	


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
	if inventory[item] < 1:		
		# unequip item if it is currently equipped
		var item_index = inventory_array.find(item)
		if item_index != -1 and equipped == item_index:
			equipped = -1
		# erase item from inventory
		inventory.erase(item)
		inventory_array.erase(item)
		return


func inventory_menu_visible(is_visible: bool):
	inventory_button_equip.visible = is_visible
	inventory_button_left.visible = is_visible
	inventory_button_right.visible = is_visible


func _on_inventory_left_pressed():
	if inventory_selection_index == 0:
		print("CANNOT GO LEFT ANY FURTHER")
		return
	inventory_selection_index -= 1
	update_inventory_button_equip()


func _on_inventory_right_pressed():
	if inventory_selection_index == inventory_array.size() - 1:
		print("CANNOT GO RIGHT ANY FURTHER")
		return
	inventory_selection_index += 1
	update_inventory_button_equip()


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
	update_inventory_button_equip()
	update_inventory()
