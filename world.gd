extends Node2D

@export var world_label: Label

enum State { IDLE, MOVING, ACTION }
const PLAYER = "@"
const POTATO_STAGE = [".", ";", "i", "P"]
const MOVE_DELAY = 0.12

var player_pos: Vector2i
var input_direction: Vector2i
var world_map: Array
var id = 0
var metabots: Dictionary # id : [stage, position]
var state: State = State.IDLE
var timer: Timer = null
var can_move: bool = true

var potato_stage: int

#const MetabotSimulator = preload("res://metabot_simulator.gd")
var agent_world:AgentWorld
var metabot_simulator:MetabotSimulator


func load_world():
	var file_path = "res://world.txt"
	var file = FileAccess.open(file_path, FileAccess.READ)
	var y = 0
	while not file.eof_reached():
		var line = file.get_line()
		var x = line.find(PLAYER)
		if x >= 0:
			player_pos = Vector2i(x, y)
			print("FOUND PLAYER")
			line[x] = " "
		# Replace player with blank space bc player gets rendered separately
		world_map.append(line)
		y += 1
	print(world_map)


func initiate_timer():
	timer = Timer.new()
	timer.set_one_shot(true)
	timer.connect("timeout", self.animation_completed)
	add_child(timer)


func initiate_metabots():
	metabot_simulator = MetabotSimulator.new()
	add_child(metabot_simulator)


func initiate_agents():
	
	# TODO setup scenarios from data file (SQLite?)
	agent_world = AgentWorld.new(Vector3i(3, 4, 1))


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
	
	
	var e_soil_locations = [
		Vector3i(1, 1, 0),
	]
	for location in e_soil_locations:
		var e_soil = AgentWorld.Entity.new(placement, shareable_placement, nonshareable_placement, detectable, tags)
		agent_world.add_entity(e_soil, location)
	
	# bury seeds in soil
	# if seed is buried in soil, attach seed entity to soil entity
	#
	
	# seed metabot should detect conditions to activate its metabolism
	# sufficient pools of water and minerals connected to its collectors
	
	# as the potato grows, its morphology should change
	
	
	# an agent with vision sensor should detect the potato has grown to sufficient size for harvesting
	
	pass


func on_new_soil(_self:AgentWorld.Entity):
	pass
	print("on_new_soil")
	print(_self)
	
	var pool_water = Metabot.Pool.new("water", 0)
	pool_water.add(100)
	var pool_minerals = Metabot.Pool.new("minerals", 0)
	pool_minerals.add(100)
	
	_self.pools.append_array([ pool_water, pool_minerals ])


func test_agents():	
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
		Vector3i(1, 2, 0),
	]
	for location in e_seed_locations:
		var e_seed = AgentWorld.Entity.new(placement, shareable_placement, nonshareable_placement, 
				detectable, tags)
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
	
	
	var e_soil_locations = [
		Vector3i(1, 1, 0),
	]
	for location in e_soil_locations:
		var e_soil = AgentWorld.Entity.new(placement, shareable_placement, nonshareable_placement, 
				detectable, tags)
		agent_world.add_entity(e_soil, location)
	
	placement = [
		"android",
		"grounded",
	]
	# world layers which entities of type can share a world zone.
	shareable_placement = [
		"grounded",
	]
	# world layers which entities of type can NOT share a world zone.
	nonshareable_placement = [
		"android", 
	]
	# which senses can detect this entity
	detectable = [
		"vision"
	]
	
	# setup player android
#	var e_android_player = AgentWorld.Entity.new()
#	var e_android_player_location = Vector3i(1, 3, 0)
#	agent_world.add_entity(e_android_player, e_android_player_location, 'android_player')
#	var player_agent = Agent.PlayerAgent.new(agent_world)
#	player_agent.entity = e_android_player
#	e_android_player.agent = player_agent
#	add_child(player_agent)

	# setup AI android
	var e_android = AgentWorld.Entity.new()
	var e_android_location = Vector3i(2, 3, 0)
	agent_world.add_entity(e_android, e_android_location, 'android_AI')
	var ai_agent = Agent.AIAgent.new(agent_world)
	# agent and entity instances are mutually registered
	ai_agent.entity = e_android
	e_android.agent = ai_agent
	add_child(ai_agent)
	
	print(agent_world.coordinates)

	# simulate agent ticks
#	var t = 4 # TODO integration test: AI android should complete goal "collect seed 1" after this many ticks
	var t = 5
#	var t = 6 # TODO integration test: AI android should complete goal "plant seed 1" after this many ticks
	var tick_count = 0
	while (t > 0):
		tick_count += 1
		print(">>>>>>>>> simulate agent_world.tick(): tick %d >>>>>>>>> " % [tick_count])
		agent_world.tick()
		t -= 1


# Called when the node enters the scene tree for the first time.
func _ready():
	
	initiate_agents()
#	initiate_metabots()
	
#	test_entities_with_metabots()
	test_agents()
#	test_metabots()
	
#	load_world()
#	initiate_timer()
	
#	var test_class = TestClass.new()
#	test_class.hello_world()


func potato_life_stage_progressed(id, stage):
	print("potato_life_stage_progressed: ", id, stage)
	potato_stage = stage
	metabots[id][0] = stage


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(_delta):
	match state:
		State.IDLE:
			idle_state()
		State.MOVING: 
			moving_state()
#		State.ACTION:
#			action_state()
#	update_world()


func get_player_input():
	if Input.is_action_pressed("ui_left"):
		return Vector2i.LEFT
	if Input.is_action_pressed("ui_right"):
		return Vector2i.RIGHT
	if Input.is_action_pressed("ui_up"):
		return Vector2i.UP
	if Input.is_action_pressed("ui_down"):
		return Vector2i.DOWN
	return Vector2i.ZERO


func idle_state():
	var direction = get_player_input()
	if direction != Vector2i.ZERO:
		input_direction = direction
		state = State.MOVING


func moving_state():
	if can_move:
		move(input_direction)
		can_move = false
		timer.set_wait_time(MOVE_DELAY)
		timer.start()


func update_world():
	var x = player_pos[0]
	var y = player_pos[1]
	var temp_world = world_map.duplicate()
	if y < len(temp_world) and x < len(temp_world[0]):
		temp_world[y][x] = PLAYER
	
	# Temp potato. TODO
	for id in metabots:
		var pos = metabots[id][1]
		var stage = metabots[id][0]
		temp_world[pos[1]][pos[0]] = POTATO_STAGE[stage]
		
#	for fruit in groceries:
#    var amount = groceries[fruit]

#	var potato_pos = Vector2i(20, 10)
#	temp_world[potato_pos[1]][potato_pos[0]] = POTATO_STAGE[potato_stage]
	
	var world_string = ""
	for row in temp_world:
		world_string += row + "\n"
	world_label.text = world_string


func move(direction: Vector2i):
	if is_position_walkable(player_pos + direction):
		player_pos += direction


func is_position_walkable(pos):
	return true


func animation_completed():
	print("ANIMATION_COMPLETED!")
	can_move = true
	state = State.IDLE
