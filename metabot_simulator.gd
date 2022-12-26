extends Node

class_name MetabotSimulator

var timer: Timer = null
var metabots = []

func initiate_timer():
	timer = Timer.new()
	timer.set_one_shot(false)
	timer.connect("timeout", self.tick)
	add_child(timer)
	
	timer.start(1)
	
func tick():
	print("TICK.")
	for mbot in metabots:
		mbot.tick()
	
# Called when the node enters the scene tree for the first time.
func _ready():
	print("_ready")
	

	var pool_water = Pool.new()
	pool_water.add(100)
	var pool_minerals = Pool.new()
	pool_minerals.add(100)

	var potato = Potato.new()

	potato.collector.add_source(pool_water)
	potato.collector.add_source(pool_minerals)

	metabots.append(potato)

	initiate_timer()

	
# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass


func _init():
	print("MetabotSimulator")
	
	var mbot = Metabot.new()
	
	

# inner classes

class Pool:
	var count = 0

	func add(amount):
		count += amount

class mComponent:
	var type = null
	var connections = []
	var pool = Pool.new()
	var output = 0
	
	func _init():
		print("mComponent")
	
	func pull(input:int):
		pool.count += input
		return pool.count
		
	func _process(amount:int):
		print("process")
		output = amount
		pool.count -= amount
		return pool.count
		
	func push(amount:int):
		var released = amount
		output -= amount
		return released
		
	func tick():
		# TODO pull resource from upstream connection
		var available_resource = 1
		var input = pull(available_resource)
		var output = _process(input)
		push(output)

class mSensor extends mComponent:
	var sensing = true
	func _init():
		print("mSensor")
	
class mCollector extends mComponent:
	var objects_collected = []
	var sources = []
	
	func _init():
		print("mCollector")

	func add_source(source:Pool):
		sources.append(source)

class mDecomposer extends mComponent:
	var objects_input = []
	var objects_processing = []
	var objects_output = []
	
	func _init():
		print("mDecomposer")

class mStorer extends mComponent:
	var stores = []
	
	func _init():
		print("mStorer")

class mConverter extends  mComponent:
	var objects_input = []
	var resources_output = []
	
	func _init():
		print("mConverter")

class mComposer extends mComponent:
	var objects_input = []
	var objects_processing = []
	var objects_output = []
	
	func _init():
		print("mComposer")

class Metabot extends  Node:
	
	var species = null
	var species_instance_id = 0

	var sensor = mSensor.new()
	var collector = mCollector.new()
	var decomposer = mDecomposer.new()
	var storer = mStorer.new()
	var converter = mConverter.new()
	var composer = mComposer.new()
	
	var components = [
#		sensor,
		collector,
#		decomposer,
#		storer, 
#		converter, 
#		composer
	]
	
	var life_stage = 0
	var life_stage_progression = []

	var body_mass = 0
	
	func _init():
		print("Metabot")
		
		
	# 1 tick = 1 hour, game time
	# 1 day, game time = 24 ticks
	func tick():
		for component in components:
			component.tick()
		
		check_life_stage()
		report_status()

	func check_life_stage():
		if body_mass >= life_stage_progression[life_stage]:
			life_stage += 1

			print("! life_stage progressed to %f" % life_stage)
	
	func report_status():
		var log = "species: %s, instance: %f"
		print(log % [species, species_instance_id])
		
# sensor
#agent 
#actuator
#motor
#collector
#decomposer
#storer
#converter
#composer
#
#body

class Potato extends Metabot:
	
	func _init():
		super._init()
		
		species = "potato"
		
		# total body mass required to progress
		life_stage_progression = [
			3, 5, 10
		]


	# input water, minerals
	# convert to body mass
	# lifecycle stage advances with body mass at certain level
		
