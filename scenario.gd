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



class TestScenarioAndroid extends Scenario:

	func _init(_world_renderer):
		super._init(_world_renderer)
		map_file = "res://scenarios/test_android.txt"

	func run():
		print("TestScenarioAndroid run")
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
			world.agent_world.tick()
			
		tick_limit = 6
		
		for t in tick_limit:
			print(">>>> test metabot_world tick # %f" % t)
			world.metabot_world.tick()


# START TESTS

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
