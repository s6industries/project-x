extends Node

# https://docs.godotengine.org/en/latest/tutorials/scripting/gdscript/gdscript_documentation_comments.html

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
	

	var pool_water = Pool.new("water", 0)
	pool_water.add(100)
	var pool_minerals = Pool.new("minerals", 0)
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
	
#	var mbot = Metabot.new()

# inner classes

class Pool:
	var count = 0
	var name = null
	
	func _init(n, c):
		name = n
		count = c

	func add(amount):
		count += amount
		
	func drain(amount):
		var output = min(amount, count)
		count -= output
		return output

class mComponent:
	var type = null
	var connections = []
	var pool = Pool.new("default",0)
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
		print("tick: component")
		# TODO pull resource from upstream connection
		var available_resource = 1
		var input = pull(available_resource)
#		var output = _process(input)
#		push(output)

#class mSensor extends mComponent:
#	var sensing = true
#	func _init():
#		print("mSensor")
	
	
## Collects resources from attached sources on each tick
class mCollector extends mComponent:
	var objects_collected = []
	var sources = []
	var collections = {}
	var amount_collected = 1
	var collection_efficiency = 1.0
	
	func _init():
		print("mCollector")

	func add_source(source:Pool):
		sources.append(source)
		collections[source.name] = 0
		
	func tick():
		print("tick: mCollector")
		for source in sources:
			var amount = amount_collected * collection_efficiency
			var collected = source.drain(amount)
			collections[source.name] += collected

#class mDecomposer extends mComponent:
#	var objects_input = []
#	var objects_processing = []
#	var objects_output = []
#
#	func _init():
#		print("mDecomposer")
#
#class mStorer extends mComponent:
#	var stores = []
#
#	func _init():
#		print("mStorer")

class Recipe:
	
	var ingredients = {
		"water": 2,
		"minerals": 2
	}
	
	var results = {
		"plantcell": 1
	}
	
	func _init():
		print("Recipe")
		
	func has_ingredients(collector:mCollector):
		for i in ingredients:
			if !collector.collections.has(i):
				return false
			elif collector.collections[i] < ingredients[i]:
				return false
		
		return true
	
	func execute(collector:mCollector):
		for i in ingredients:
			collector.collections[i] -= ingredients[i]
		
		return results

## Converts resources from [mCollector] into energy, body mass, etc.
class mConverter extends  mComponent:
#	var objects_input = []
#	var resources_output = []

	var collector:mCollector = null
	
	var conversion_recipes = [
		Recipe.new()
	]
	
	var converted = {
		"plantcell": 0
	}
	
	func _init():
		print("mConverter")
		
	func attach_collector(c:mCollector):
		collector = c
		
	func tick():
		print("tick: mConverter")
		
		for recipe in conversion_recipes:
			if recipe.has_ingredients(collector):
				var results = recipe.execute(collector)
				for r in results:
					print("converter result: %s x %f" % [r, results[r]])
					
					if converted.has(r):
						converted[r] += results[r]
					else:
						converted[r] = results[r]
		for c in converted:
			print("converted: %s x %f" % [c, converted[c]])

## Blueprints create/augment body parts from raw materials (converted from collected resources)
class Blueprint:
	
	var active = true
	var materials = {
		"plantcell": 3
	}
	
	var results = {
		"root": 1
	}
	
	func _init():
		print("Blueprint")
		
	func has_materials(converter:mConverter):
		for m in materials:
			if !converter.converted.has(m):
				return false
			elif converter.converted[m] < materials[m]:
				return false
		
		return true
	
	func execute(converter:mConverter):
		for m in materials:
			converter.converted[m] -= materials[m]
		
		var bodyparts = []
		
		for r in results:
			var count = results[r]
			while count > 0:
				bodyparts.append(BODYPARTS[r])
				count -= 1
			
		return bodyparts

		
const BODYPARTS = {
	"root": {
		"name": "root",
		"mass": 1.0,
		"layer": "underground"
	}
}

class mComposer extends mComponent:
#	var objects_input = []
#	var objects_processing = []
#	var objects_output = []
	
	var converter:mConverter = null
	var body = null
	
	var blueprints = [
		Blueprint.new()
	]
	
	func _init():
		print("mComposer")
	
	func attach_converter(c:mConverter):
		converter = c
		
	func attach_body(b:Array):
		body = b
		
	func tick():
		print("tick: mComposer")
		
		for blueprint in blueprints:
			if blueprint.active:
				if blueprint.has_materials(converter):
					var bodyparts = blueprint.execute(converter)
					body.append_array(bodyparts)
			

class Metabot extends  Node:
	
	var species = null
	var species_instance_id = 0
	
	## all the body parts
	var body = []

#	var sensor = mSensor.new()
	var collector = mCollector.new()
#	var decomposer = mDecomposer.new()
#	var storer = mStorer.new()
	var converter = mConverter.new()
	var composer = mComposer.new()
	
	## NOTE: tick execution order
	var components = [
#		sensor,
		collector,
#		decomposer,
#		storer, 
		converter, 
		composer
	]
	
	var life_stage = 0
	var life_stage_progression = []

	var body_mass = 0
	
	func _init():
		print("Metabot")
		converter.attach_collector(collector)
		composer.attach_converter(converter)
		composer.attach_body(body)
		
	# 1 tick = 1 hour, game time
	# 1 day, game time = 24 ticks
	func tick():
		for component in components:
			component.tick()
		
		check_life_stage()
		report_status()

	func check_life_stage():
		# recalculate bodymass
		body_mass = 0
		for part in body:
			body_mass += part["mass"]
			print("bodypart: %s" % part["name"])
		
		print("body mass: %f" % body_mass)
		if body_mass >= life_stage_progression[life_stage]:
			life_stage += 1

			print(">>> !!! life_stage progressed to %f !!!" % life_stage)
	
	func report_status():
		var log = "species: %s, instance: %f, life_stage: %f"
		print(log % [species, species_instance_id, life_stage])
		
		log = "collector: pool count: %f"
		print(log % [collector.pool.count])
		
		for c in collector.collections:
			log = "collection %s count: %f"
			print(log % [c, collector.collections[c]])
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
		
