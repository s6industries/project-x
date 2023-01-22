# the game world + configuration for a play session
# can be created from saved data (CSV, SQL)
class_name Scenario

var world_renderer # WorldGenerator.WorldRenderer2Di
var world # WorldGenerator.World2Di

var map_file: String

var timekeeper: WorldGenerator.Timekeeper
var auto_tick = false


func _init(_world_renderer):
	print("init Scenaroi")
	world = WorldGenerator.World2Di.new()
	world_renderer = _world_renderer


func run():
	pass


class Tester:
	
	func _init():
		pass

	func load(world_renderer):
		
		var scenario = TestScenarioAndroid.new(world_renderer)

		var world_generator = WorldGenerator.new()
		world_generator.load_scenario_from_txt_map(scenario.map_file, world_renderer, scenario)

		return scenario


# START TESTS


class TestScenarioAndroid extends Scenario:

	func _init(_world_renderer):
		super._init(_world_renderer)
		map_file = "res://scenarios/test_android.txt"

	func run():
		print("TestScenarioAndroid run")
		var tick_limit = 0

		# the android has planted the potato seed in the tilled soil
		# the seed entity has been attached to the soil entity
		# on attach to soil, the resource pools of the soil are connected to the 
		# seed's metabot
		# on connect resource pools to seed's metabot, check if the resource requirement met
		# to acticate the seed's metabolism 
		# on next metabot tick, the seed should pull resources and grow
		tick_limit = 4
		for t in tick_limit:
			print(">>>> test agent_world tick # %f" % t)
			world.agent_world.tick()
		
		# the potato has grown and its morphology has changed
		tick_limit = 6
		for t in tick_limit:
			print(">>>> test metabot_world tick # %f" % t)
			world.metabot_world.tick()
		
		# the android perceives the updated morphology of potato as ready for harvest
		# >>> all_sensor_data for vision:
		# >>> [{ "loc": (36, 13, 0), "data": ["soil", "grounded", {  }, "seed", "grounded", { "height": 1, "symbol": "potato" }] }]
		# the android grabs the potato
		world.agent_world.tick()
		
		# with the potato collected in backpack, the android sets destination for HQ 
		# deposting harvest at HQ is the second part to goal harvesting
		world.agent_world.tick()

		# tick_limit = 1
		# for t in tick_limit:
		# 	print(">>>> test agent_world tick # %f" % t)
		# 	world.agent_world.tick()

		pass


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



# END TESTS


# on soil created, it should have pools of water and minerals that plants buried in it will use to grow
# soil becomes a passthrough entity for plant metabots attached to it
# water and minerals added to soil, its pools increase, the attached seed passes to the plant metabot

# func on_new_soil(_self:AgentWorld.Entity):
# 	pass
# 	print("on_new_soil")
# 	print(_self)
	
# 	var pool_water = Metabot.Pool.new("water", 0)
# 	pool_water.add(100)
# 	var pool_minerals = Metabot.Pool.new("minerals", 0)
# 	pool_minerals.add(100)
	
# 	_self.pools.append_array([ pool_water, pool_minerals ])

