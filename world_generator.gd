# WorldGenerator takes in raw world data 
# and creates a world instance from it
class_name WorldGenerator extends Node

var scenarios = []
# var active_scenario: Scenario

const PLAYER = "@"
const ANDROID = "A"
const WALL = "#"
const HOE = "h"
const BLANK = " "
const TILLED_SOIL = "="
const POTATO_STAGE = [".", ";", "i", "P"]
const SEED = "."

func _init():
	pass


func _ready():
	pass

# TODO create a scenario from a CSV file
# TODO setup scenarios from data file (SQLite?)


func load_scenario_from_txt_map(file_path: String, world_renderer, scenario):

	var player_pos: Vector2i
	var world_map = []

	# var file_path = "res://world.txt"
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
	
	# data loaded from file now
	var num_rows = world_map.size()
	var num_cols = world_map[0].length()
	print(world_map)
	
	# setup scenario instance
	# var scenario = Scenario.new(world_renderer)
	var metabot_world = MetabotWorld.new(false)
	var agent_world = AgentWorld.new(Vector3i(num_cols, num_rows, 1), false)
	agent_world.metabot_world = metabot_world
			
	scenario.world.metabot_world = metabot_world
	scenario.world.agent_world = agent_world

	var world2D = scenario.world
	# Spawn the entities
	for y2 in range(num_rows):
		for x2 in range(num_cols):
			# print("x2, y2: ", x2, " ", y2)
			var new_location = Vector3i(x2, y2, 0)
#			print( world_map[y2], " ", typeof( world_map[y2]))
#			print(world_map[y2][x2])
			if world_map[y2][x2] == ANDROID:
				world2D.spawn_android(new_location)
				world_map[y2][x2] = " "
			elif world_map[y2][x2] == TILLED_SOIL:
				world2D.spawn_tilled_soil(new_location)
				world_map[y2][x2] = " "
			elif world_map[y2][x2] == SEED:
				world2D.spawn_seed(new_location)
				world_map[y2][x2] = " "
	# add_child(agent_world)

	var timekeeper = Timekeeper.new(false, agent_world, metabot_world)
	scenario.timekeeper = timekeeper

	return scenario


class Timekeeper extends Node:

	# var metabots = []
	# var agents = []
	var metabot_world: MetabotWorld
	var agent_world: AgentWorld

	var timer: Timer = null
	var tick_interval = 1.0
	var timer_autostart: bool


	func _init(_timer_autostart: bool, _agent_world: AgentWorld, _metabot_world: MetabotWorld):
		print("init Timekeeper")
		timer_autostart = _timer_autostart
		agent_world = _agent_world
		metabot_world = _metabot_world


	# Called when the node enters the scene tree for the first time.
	func _ready():
		print("_ready")
		if timer_autostart:
			initiate_timer()


	# Called every rendered frame. 'delta' is the elapsed time since the previous frame.
	func _process(delta):
		pass


	func initiate_timer():
		timer = Timer.new()
		timer.set_one_shot(false)
		timer.connect("timeout", self.tick)
		add_child(timer)
		# autostart
		timer.start(tick_interval)
		

	func tick():
		print("tick Timekeeper")
		# for mbot in metabots:
		# 	mbot.tick()
		metabot_world.tick()
		agent_world.tick()

	
# # the game world + configuration for a play session
# # can be created from saved data (CSV, SQL)
# class Scenario:

# 	var world: World2Di
# 	var world_renderer: WorldRenderer2Di

# 	var timekeeper: Timekeeper
# 	var auto_tick = false

# 	func _init(_world_renderer: WorldRenderer2Di):
# 		print("init Scenaroi")
# 		world = World2Di.new()
# 		world_renderer = _world_renderer
# 		pass

# 	func run():
# 		pass


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


# World2Di tracks all entities sharing a physical 2D space & time
class World2Di extends Node:

	#const MetabotWorld = preload("res://metabot_world.gd")
	var agent_world:AgentWorld
	var metabot_world:MetabotWorld
	var metabots: Dictionary # id : [stage, position]
	var entities: Dictionary # id : [position]
	var id = 0

	# variables to spawn entities
	var placement:Array
	var shareable_placement:Array
	var nonshareable_placement:Array
	var detectable:Array
	var tags:Array


	func _init():
		pass


	func spawn_android(location: Vector3i):
		placement = ["android", "grounded",]
		# world layers which entities of type can share a world zone.
		shareable_placement = ["grounded",]
		# world layers which entities of type can NOT share a world zone.
		nonshareable_placement = ["android", ]
		# which senses can detect this entity
		detectable = ["vision"]
		tags = ["android"]
		
		# setup AI android
		var e_android = AgentWorld.Entity.new(placement, shareable_placement, nonshareable_placement, detectable, tags)
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
		tags = ["seed", "potato"]
		
		# TODO implement seed source as spaceship
		var e_seed = AgentWorld.Entity.new(placement, shareable_placement, nonshareable_placement, detectable, tags)
		agent_world.add_entity(e_seed, location)
	
	
	func spawn_tilled_soil(location: Vector3i):    
		placement = ["soil", "grounded"]
		# world layers which entities of type can share a world zone.
		shareable_placement = ["grounded",]
		# world layers which entities of type can NOT share a world zone.
		nonshareable_placement = ["soil",]
		# which senses can detect this entity
		detectable = ["vision"]
		tags = [ "soil", "tilled" ]
		
		# on soil created, it should have pools of water and minerals that plants buried in it will use to grow
		# soil becomes a passthrough entity for plant metabots attached to it
		# water and minerals added to soil, its pools increase, the attached seed passes to the plant metabot		
		var e_soil = AgentWorld.Entity.new(placement, shareable_placement, nonshareable_placement, detectable, tags)
		agent_world.add_entity(e_soil, location)
	
	
	# func spawn_potato(location: Vector3i):
	# 	metabots[id] = [0, Vector2i(20, 10)]
	# 	var potato = metabot_world.plant_potato(id)
	# #	potato.life_stage_progressed.connect(self.potato_life_stage_progressed.bind(stage))
	# 	potato.connect("life_stage_progressed", self.potato_life_stage_progressed)
	# 	id += 1
	# 	pass
		
