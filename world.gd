class_name WorldRenderer2Di extends Node2D

@export var world_label: Label
@export var inventory_label: Label
@export var player_marker: Marker2D
@export var button: Button
@export var button2: Button

# const WorldGenerator = preload('res://world_generator.gd')

enum Env { DEV, PROD }
# TODO set environment from environment file/build settings
const env: Env = Env.DEV

enum State { IDLE, MOVING, ACTION, DELAY }
const PLAYER = "@"
const ANDROID = "A"
const WALL = "#"
const HOE = "h"
const BLANK = " "
const TILLED_SOIL = "="
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

# var metabot_world
var potato_stage: int
var diagonal_moving_toggle: bool = false
var inventory: Array = Array()
var equipped: int = -1
var action_button_pressed = false

#const MetabotWorld = preload("res://metabot_world.gd")
var agent_world:AgentWorld
var metabot_world:MetabotWorld
# var placement:Array
# var shareable_placement:Array
# var nonshareable_placement:Array
# var detectable:Array
# var tags:Array

# func spawn_android(location: Vector3i):
# 	placement = ["android", "grounded",]
# 	# world layers which entities of type can share a world zone.
# 	shareable_placement = ["grounded",]
# 	# world layers which entities of type can NOT share a world zone.
# 	nonshareable_placement = ["android", ]
# 	# which senses can detect this entity
# 	detectable = ["vision"]
# 	tags = ["android"]
	
# 	# setup AI android
# 	var e_android = AgentWorld.Entity.new(placement, shareable_placement, nonshareable_placement, detectable, tags)
# 	agent_world.add_entity(e_android, location, 'android_AI')
# 	# agent and entity instances are mutually registered
# 	var ai_agent = Agent.AIAgent.new(agent_world)
# 	ai_agent.entity = e_android
# 	e_android.agent = ai_agent
# 	add_child(ai_agent)
# 	print(agent_world.coordinates)


# func spawn_seed(location: Vector3i):    
# 	placement = ["seed", "grounded",]
# 	# world layers which entities of type can share a world zone.
# 	shareable_placement = ["grounded",]
# 	# world layers which entities of type can NOT share a world zone.
# 	nonshareable_placement = ["seed",]
# 	# which senses can detect this entity
# 	detectable = ["vision"]
# 	tags = ["seed", "potato"]
	
# 	# TODO implement seed source as spaceship
# 	var e_seed = AgentWorld.Entity.new(placement, shareable_placement, nonshareable_placement, detectable, tags)
# 	agent_world.add_entity(e_seed, location)


# func spawn_tilled_soil(location: Vector3i):    
# 	placement = ["soil", "grounded"]
# 	# world layers which entities of type can share a world zone.
# 	shareable_placement = ["grounded",]
# 	# world layers which entities of type can NOT share a world zone.
# 	nonshareable_placement = ["soil",]
# 	# which senses can detect this entity
# 	detectable = ["vision"]
# 	tags = [ "soil", "tilled" ]
	
# 	# on soil created, it should have pools of water and minerals that plants buried in it will use to grow
# 	# soil becomes a passthrough entity for plant metabots attached to it
# 	# water and minerals added to soil, its pools increase, the attached seed passes to the plant metabot		
# 	var e_soil = AgentWorld.Entity.new(placement, shareable_placement, nonshareable_placement, detectable, tags)
# 	agent_world.add_entity(e_soil, location)

# 	# var pool_water = 


# func spawn_potato(location: Vector3i):
# 	metabots[id] = [0, Vector2i(20, 10)]
# 	var potato = metabot_world.plant_potato(id)
# #	potato.life_stage_progressed.connect(self.potato_life_stage_progressed.bind(stage))
# 	potato.connect("life_stage_progressed", self.potato_life_stage_progressed)
# 	id += 1
# 	pass


# func load_world():
# 	var file_path = "res://world.txt"
# 	var file = FileAccess.open(file_path, FileAccess.READ)
# 	var y = 0
# 	while not file.eof_reached():
# 		var line = file.get_line()
# 		if line.is_empty():
# 			printerr("EMPTY LINE")
# 			continue
# 		var x = line.find(PLAYER)
# 		if x >= 0:
# 			player_pos = Vector2i(x, y)
# 			print("FOUND PLAYER")
# 			line[x] = " "
# 		# Replace player with blank space bc player gets rendered separately
# 		world_map.append(line)
# 		y += 1
# 	print(world_map)
# 	## CHECKPOINT
# 	var num_rows = world_map.size()
# 	var num_cols = world_map[0].length()
	
# 	# Instantiate Agent World
# 	agent_world = AgentWorld.new(Vector3i(num_cols, num_rows, 1), true)
# 	# Spawn the entities
# 	for y2 in range(num_rows):
# 		for x2 in range(num_cols):
# 			# print("x2, y2: ", x2, " ", y2)
# 			var new_location = Vector3i(x2, y2, 0)
# #			print( world_map[y2], " ", typeof( world_map[y2]))
# #			print(world_map[y2][x2])
# 			if world_map[y2][x2] == ANDROID:
# 				spawn_android(new_location)
# 				world_map[y2][x2] = " "
# 			elif world_map[y2][x2] == TILLED_SOIL:
# 				spawn_tilled_soil(new_location)
# 				world_map[y2][x2] = " "
# 			elif world_map[y2][x2] == SEED:
# 				spawn_seed(new_location)
# 				world_map[y2][x2] = " "
# 	# func initiate_agents():
# 	# TODO setup scenarios from data file (SQLite?)
# 	add_child(agent_world)


func initiate_timer():
	timer = Timer.new()
	timer.set_one_shot(true)
	timer.connect("timeout", self.animation_completed)
	add_child(timer)


# func initiate_metabots_test():
# 	metabot_world = MetabotWorld.new(false)
# 	add_child(metabot_world)


# Called when the node enters the scene tree for the first time.
func _ready():
	button.text = "Pick up"
	button.hide()
	button2.text = "Use"
	button2.hide()
	
	if env == Env.PROD:
		print("Env.PROD")
		# load_world()
		# initiate_timer()
		
	elif env == Env.DEV:
		print("Env.DEV")
		# when designing a scenario, designer should be able to
		# define entities in text map
		# setup world from text map
		# precisely control time: manually, how many ticks to prgress
		# visually confirm the world state as rendered by WorldRenderer

		var scenario_map_file = "res://world.txt"
		var world_generator = WorldGenerator.new()
		var scenario = world_generator.load_scenario_from_txt_map(scenario_map_file, self)
		
		# link metabots and agents for the rendering in update_world()
		metabots = scenario.world.metabot_world.metabots_by_id
		agent_world = scenario.world.agent_world

		# add_child(world_generator)
		add_child(scenario.world.metabot_world)
		add_child(scenario.world.agent_world)

		var tick_limit = 4 
		# at 4 ticks, the android has planted the potato seed in the tilled soil
		# the seed entity has been attached to the soil entity
		# on attach to soil, the resource pools of the soil are connected to the 
		# seed's metabot
		# on connect resource pools to seed's metabot, check if the resource requirement met
		# to acticate the seed's metabolism 
		# on next metabot tick, the seed should pull resources and grow

		for t in tick_limit:
			print(">>>> test agent_world tick # %f" % t)
			agent_world.tick()
	
	# TODO setup scenarios from data file (SQLite?)


# START TESTS


# func test_metabots():

# 	# var agent_world = AgentWorld.new(Vector3i(num_cols, num_rows, 1), true)
# 	agent_world = AgentWorld.new(Vector3i(3, 4, 1), true)

# 	# metabots plant potat AT
# 	metabots[id] = [0, Vector2i(20, 10)]
# 	var potato = metabot_world.plant_potato(id)
# 	attach_pools_for_potato(potato)

# #	potato.life_stage_progressed.connect(self.potato_life_stage_progressed.bind(stage))
# 	potato.connect("life_stage_progressed", self.potato_life_stage_progressed)
# 	id += 1
	
# 	# metabots plant potat AT
# 	metabots[id] = [0, Vector2i(40, 10)]
# 	var potato2 = metabot_world.plant_potato(id)
# 	attach_pools_for_potato(potato2)

# #	potato.life_stage_progressed.connect(self.potato_life_stage_progressed.bind(stage))
# 	potato2.connect("life_stage_progressed", self.potato_life_stage_progressed)
# 	id += 1


# func test_entities_with_metabots():
# 	agent_world = AgentWorld.new(Vector3i(3, 4, 1), true)
# 	agent_world.metabot_world = metabot_world

# 	# TODO implement seed source (as spaceship / headquarters?)
# 	var e_seed_locations = [
# 		Vector3i(1, 1, 0),
# 	]
# 	for location in e_seed_locations:
# 		spawn_seed(location)

# 	var e_soil_locations = [
# 		Vector3i(1, 2, 0),
# 	]
# 	for location in e_soil_locations:
# 		spawn_tilled_soil(location)
	
# 	var e_android_locations = [
# 		Vector3i(1, 3, 0),
# 	]
# 	for location in e_soil_locations:
# 		spawn_android(location)

	
		
	# on soil created, it should have pools of water and minerals that plants buried in it will use to grow
	# soil becomes a passthrough entity for plant metabots attached to it
	# water and minerals added to soil, its pools increase, the attached seed passes to the plant metabot


# END TESTS


# func on_new_soil(_self:AgentWorld.Entity):
# 	pass
# 	print("on_new_soil")
# 	print(_self)
	
# 	var pool_water = Metabot.Pool.new("water", 0)
# 	pool_water.add(100)
# 	var pool_minerals = Metabot.Pool.new("minerals", 0)
# 	pool_minerals.add(100)
	
# 	_self.pools.append_array([ pool_water, pool_minerals ])


func potato_life_stage_progressed(id, stage):
	print("potato_life_stage_progressed: ", id, stage)
	potato_stage = stage
	metabots[id][0] = stage
 

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _physics_process(_delta):
	if env == Env.PROD:
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
	var text: String = "Inventory: "
	for item in inventory:
		text += item + " "
	inventory_label.text = text
	if equipped < 0 or equipped >= inventory.size():
		button2.hide()
		return
	button2.text = "Use " + inventory[equipped]
	button2.show()
	inventory_label.text += "\nEquipped: " + inventory[equipped]
	


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
	var item = remove_world_item_at(player_pos)
	inventory.append(item)
	equipped = inventory.size() - 1


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
	action_button_pressed = true
	
	
func get_equipped_item() -> String:
	if equipped >= 0 and equipped < inventory.size():
		return inventory[equipped]
	return ""
