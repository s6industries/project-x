extends Node

class_name Metabot

signal life_stage_progressed
signal body_changed

# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass


var is_active = false
var environment = {}
var func_activate = null # FuncRef
	
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
var id = null

var body_mass = 0

func _init(temp_id: String):
	id = temp_id
	print("Metabot")
	
	converter.attach_collector(collector)
	composer.attach_converter(converter)
	composer.attach_body(body)
	composer.connect("bodyparts_added", self.on_bodyparts_added)
	
	func_activate = Callable(self, "auto_activate")


func on_bodyparts_added(bodyparts):
	print("on_bodyparts_added")
	pass
	# recalculate bodymass
	body_mass = 0
	for part in body:
		body_mass += part["mass"]
		print("bodypart: %s" % part["name"])
	print("body mass: %f" % body_mass)
	emit_signal("body_changed", body_mass, body)


# default to start metabolism on first tick
func auto_activate(_self, environment):
	return true


func check_active(environment):
	if func_activate:
		if func_activate.call(self, environment):
			is_active = true
	return is_active


# 1 tick = 1 hour, game time
# 1 day, game time = 24 ticks
func tick():
	if not is_active:
		check_active(environment)
		if not is_active:
			return 
		
	for component in components:
		component.tick()
	
	check_life_stage()
	report_status()


func check_life_stage():
	# recalculate bodymass
	body_mass = 0
	for part in body:
		body_mass += part["mass"]
		# print("bodypart: %s" % part["name"])
	
	# print("body mass: %f" % body_mass)
	if body_mass >= life_stage_progression[life_stage]:
		life_stage += 1
		print(">>> !!! life_stage progressed to %f !!!" % life_stage)
		# emit_signal("life_stage_progressed", life_stage)
		emit_signal("life_stage_progressed", id, life_stage)


func report_status():
	var log = "species: %s, instance: %f, life_stage: %f, body_mass: %f"
	print(log % [species, species_instance_id, life_stage, body_mass])

	for c in collector.collections:
		print("collection %s count: %f" % [c, collector.collections[c]])
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


	func get_source_amount(source_name):
		var amount = 0
		for source in sources:
			if source.name == source_name:
				amount += source.count 

		return amount


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
#		"water": 2,
#		"minerals": 2
	}
	
	var results = {
#		"plantcell": 1
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
#		"plantcell": 0
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
#		"plantcell": 3
	}
	
	var results = {
#		"root": 1
	}
	
	var bodypart_templates =  {
#		"root": {
#			"name": "root",
#			"mass": 1.0,
#			"layer": "underground"
#		}
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


	func execute(converter:mConverter, templates:Dictionary):
		for m in materials:
			converter.converted[m] -= materials[m]
		
		var bodyparts_created = []
		
		for r in results:
			var count = results[r]
			while count > 0:
				bodyparts_created.append(templates[r])
				count -= 1
			
		return bodyparts_created


class mComposer extends mComponent:
	
	signal bodyparts_added
#	var objects_input = []
#	var objects_processing = []
#	var objects_output = []
	
	var converter:mConverter = null
	var body = null
	
	var blueprints = [
#		Blueprint.new()
	]
	
	var bodypart_templates =  {
#		"root": {
#			"name": "root",
#			"mass": 1.0,
#			"layer": "underground"
#		}
	}


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
					var bodyparts = blueprint.execute(converter, bodypart_templates)
					body.append_array(bodyparts)
					# notify body is growing
					emit_signal("bodyparts_added", bodyparts)
