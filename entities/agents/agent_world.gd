extends Node

class_name AgentWorld

# https://github.com/binogure-studio/godot-uuid
const uuid_util = preload('res://utils/uuid.gd')

var agents = []

var entities = []
var entities_by_id = {}
# x[y[z[]]]. x, y for 2D plane, top down perspective. z for layer depth
# z[y[x[]]]. x, y for 2D plane, top down perspective. z for layer depth
var coordinates = [] 

var timer:Timer
var tick_interval:float
var autostart_timer: bool

var metabot_world:MetabotWorld


static func generate_coordinates(_z, _y, _x):
	var coordinates = []
	var x = 0
	var y = 0
	var z = 0
	# create a 3D array
	while z < _z:
		var c_z = []
		while y < _y:
			var c_y = []
			while x < _x:
				var c_x = []
				c_y.append(c_x)
				x += 1
			c_z.append(c_y)
			x = 0
			y += 1
		coordinates.append(c_z)
		y = 0
		z += 1
		
	return coordinates


func _init(size_3D:Vector3i, _autostart_timer: bool):
	print("AgentWorld %d, %d, %d " % [size_3D.x, size_3D.y, size_3D.z])
#	coordinates = generate_coordinates(size_3D.x, size_3D.y, size_3D.z)
	coordinates = generate_coordinates(size_3D.z, size_3D.y, size_3D.x)
	print(coordinates)
	autostart_timer = _autostart_timer
	
	
# Called when the node enters the scene tree for the first time.
func _ready():
	print("_ready AgentWorld ")
	if (autostart_timer):
		tick_interval = 1.0
		initiate_timer()
	else:
		print("must manually initiate_timer() or tick()")


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
	print("tick AgentWorld")
	var actions_from_agents = queue_actions_from_agents()
	execute_actions_in_queue(actions_from_agents)
	

func queue_actions_from_agents():
	var actions_from_agents = []
	for agent in agents:
		var action_in_environment = agent.tick()
		if action_in_environment:
			var action_for_agent = [
				agent.entity.id,
				action_in_environment,
			]
			actions_from_agents.append(action_for_agent)
	print(actions_from_agents)

	return actions_from_agents

# all actions from agents are queued, now proceed to resolve them and mutate all entities 
# before agents next gather data from their environment 
# ex. ["android_AI", ["translate_body", (0, -1)]]
# ex. ["android_AI", ["attach_body", ["any", "seed", "potato"], "backpack", "grab", (0, 0)]] 
func execute_actions_in_queue(actions_from_agents):
	for action_queued in actions_from_agents:
		var entity_id = action_queued[0]
		var action_info =  action_queued[1]
		var entity:Entity = get_entity_by_id(entity_id)
		if entity:
			if action_info.has("steps"):
				print("multistep action")
				var action_steps = action_info["steps"]
				var input = null
				for step in action_steps:
					input = resolve_action(entity, step, input)
			else:
				resolve_action(entity, action_info)


func resolve_action(entity, action_info, input = null):
	print("resolve_action %s %s" % [entity.id, action_info])
	
	var result = null
	var action_type = action_info[0]
	match action_type:
		
		# ex. [["android_AI", ["translate_body", (0, -1)]]]
		"translate_body":
			var movement_vector = action_info[1]
			# change entity center point
			var new_center_point = Vector3(movement_vector.x, movement_vector.y, 0) + entity.center_point
			
			if check_world_bounds(new_center_point):
				# remove entity from coordinate data
				place_entity(entity, entity.center_point, false)
				# re-add entity coordinate data at new center point
				place_entity(entity, new_center_point, true)
#					model_state["body_position"] += next_action[1]
#					return true
			else:
				print("new_center_point out of world bounds")
				
		# ["attach_body", ["any", "seed", "potato"], "backpack", "grab", (0, 0)] 
		"attach_body":
			var attach_target_location = action_info[4]
			var grab_target_types = action_info[1]
			var entity_attach_node = action_info[2]
			var world_location = Vector3i(entity.center_point) + Vector3i()
			var all_grabbed = grab_at_location(world_location, grab_target_types, entity_attach_node)
			
			if all_grabbed.size() > 0:
				for grabbed in all_grabbed:
					print("grabbed entity %s" % [grabbed])
					grabbed = get_entity_by_id(grabbed)
					print(grabbed.tags)
					place_entity(grabbed, world_location, false)
					entity.agent.attach_entity(grabbed, entity_attach_node)
				
				print(entity.agent.model_state)

			# var grabbed = grab_at_location(world_location, grab_target_types, entity_attach_node)
			# if grabbed:
			# 	print("grabbed entity %s" % [grabbed])
			# 	grabbed = get_entity_by_id(grabbed)
			# 	print(grabbed.tags)
			# 	place_entity(grabbed, world_location, false)
			# 	entity.agent.attach_entity(grabbed, entity_attach_node)
			# 	print(entity.agent.model_state)
		
		# ex. ["detach_body", ["seed"], "backpack", "bury", (0, 0)]
		"detach_body":
			print(input)
			# retrieve item from backpack
			var detach_target = action_info[1][0]
			var detach_node = action_info[2]
#			entity.agent.get_attachments("backpack", "seed")
			var detached = entity.agent.get_attachments(detach_node, detach_target)[0]
			entity.agent.detach_entity(detached, detach_node)
			result = detached
			
		# ex. ["attach_to", "seed", "soil", "bury", (0, 0)]
		"attach_to":
			print(input)
			# get soil entity at agent entity's world location
			var receiver_type = action_info[2]
			var world_location = Vector3i(entity.center_point) + Vector3i()
			print(receiver_type)
			place_entity(input, world_location, true)
			var receiver_ids = get_entities_of_type([ receiver_type ], world_location)
			print(receiver_ids)
			var receiver:Entity = get_entity_by_id(receiver_ids[receiver_type][0])
			print(receiver.tags)
			
			if (receiver.tags.has("soil")):
				print("attach seed to soil")
				var seed:Entity = input
				print(input.tags)
				# TODO attach to soil the seed entity (which is the result of previous detach body action)
				# TODO when the seed is attached to soil, activate the seed's metabolism
				
				var mbot_id = uuid_util.v4()
				var mbot_potato = metabot_world.plant_potato(mbot_id)
				seed.attach_metabot(mbot_potato)

				# TODO attach soil's resource pools to seed
				metabot_world.attach_pools_for_potato(mbot_potato)
			
		
		# ex. ["remember", "plant", "seed"]
		"remember":
			print(input)
			var goal_type = action_info[1]
			var goal_target = action_info[2]
			entity.agent.create_memory(goal_type, goal_target)
			
	return result


func get_entity_by_id(_id:String) -> Entity:
	var entity:Entity = null
	if entities_by_id.has(_id):
		entity = entities_by_id[_id]
	return entity
	
# each entity has a center coordinate + a radius for body size
# center + radius determines which world coordinates are occupied by the entity's body
func add_entity(entity:Entity, center_point:Vector3i, id:String = ''):
	if id == '':
		id = uuid_util.v4()
		
	entity.id = id
	entities_by_id[id] = entity
	entities.append(entity)
	place_entity(entity, center_point, true)


func place_entity(entity:Entity, center_point:Vector3i, is_present:bool):
	print("place_entity %s" % [entity.id])
	
	var body_zones = entity.get_body_boundary(center_point)
	
	if (body_zones.size() > 0):
		# entity is assigned a world coordinate
		entity.center_point = center_point
		for zone in body_zones:
			print(zone)
			set_entity_at_coordinate(entity.id, is_present, zone.x, zone.y, zone.z)
		return true
	else:
	# if entity cannot be placed at this center_point
		return false


func check_world_bounds(point:Vector3i):
	var point_in_bounds = true
	
	if point.x < 0 or point.x >= coordinates[0][0].size():
		return false
	elif  point.y < 0 or point.y >= coordinates[0].size():
		return false
	elif point.z < 0 or point.z >= coordinates.size():
		return false
		
	return point_in_bounds


func set_entity_at_coordinate(id:String, is_present:bool, x:int,  y:int, z:int):
	print("set_entity_at_coordinate %s: %d, %d, %d" % [id, x, y, z])
	var coordinate_data:Array = coordinates[z][y][x]
	if id not in coordinate_data:
		if is_present:
			coordinate_data.append(id)
	else:
		if !is_present:
			coordinate_data.erase(id)


func get_entities_of_type(entity_types, world_point:Vector3i):
	var coordinate_data:Array = coordinates[world_point.z][world_point.y][world_point.x]
	var entities_found = {}
	for id in coordinate_data:
		var entity:Entity = get_entity_by_id(id)
		# check if any of the target types match the entities' types at this world point
		for type in entity_types:
			if entity.placement.has(type):
				if !entities_found.has(type):
					entities_found[type] = []
				entities_found[type].append(id)
	return entities_found


func grab_at_location(location:Vector3, grab_targets, grabber):
	var grabbed = []
	# check if there is an entity at location
	# is entity type of grab_target
	# if grab_target can be grabbed by grabber
	var found_entities = get_entities_of_type(grab_targets, location)
	
	for target in grab_targets:
		if target in found_entities:
			grabbed.append_array(found_entities[target])

	# if found_entities["seed"]:
	# 	grabbed = found_entities["seed"][0]
		
	return grabbed


func get_sensor_data_for_entity(_id:String):
	var sensor_data = {}
	if entities_by_id.has(_id):
		var entity:Entity = entities_by_id[_id]
		
		sensor_data = get_sensor_data(entity.agent.sensors, entity.center_point, [_id])
	else:
		print("no entity registered with id %s" % [_id])
	return sensor_data
# ex. senses of an agent
#var senses = [
#	{
#		"type":"vision",
#		"mods": [
#			"day",
#			"night"
#		]
#	}
#]
# get all world zones within sensor range
# scan each zone for entities detectable by sensor
# return all detected entities
func get_sensor_data(sensors:Array, origin:Vector3, ignored_entities = []):
	var sensor_data = {}
	print("get_sensor_data")
	print(sensors)
	print(origin)
	
	for sensor in sensors:
#		print(sensor.type)
#		print(sensor.mods)
#		print(sensor.range)
		match sensor.type:
			"vision":
				print("vision")
		sensor_data[sensor.type] = get_detectable_entities(sensor, origin, ignored_entities)
				
	return sensor_data
#{
#	"type":"vision",
#	"range": 3,
#	"mods": [
#		"day",
#		"night"
#	],
#}
# for vision sensor, this can be replaced by raycasting from origin (within an angle from eye direction)
# for sound, smell, or temperature sensors, the presence of an entity is detectable within a certain world zone 
# that contains the data signature & intensity from the entity source 
# ex. does the world zone contain: smellable particles, heat radiation, soundwaves
func get_detectable_entities(sensor, origin:Vector3, ignored_entities = []):
	var data_for_sensor = []
	print("get_detectable_entities")
	var delta_x = sensor.range
	var delta_y = sensor.range
	var delta_z = 0 # only scan on the same horizontal plane
	
#	var range_x = [origin.x - delta_x, origin.x + delta_x]
#	var range_y = [origin.y - delta_y, origin.y + delta_y]
#	var range_z = [origin.z - delta_z, origin.z + delta_z]
	var range_x = get_range(origin.x, delta_x, 0, coordinates[0][0].size()-1)
	var range_y = get_range(origin.y, delta_y, 0, coordinates[0].size()-1)
	var range_z = get_range(origin.z, delta_z, 0, coordinates.size()-1)
#	print(range_x)
#	print(range_y)
#	print(range_z)
	# from origin set scan range across all coordinate dimensions
	var d = scan_coordinates_for_entities(range_x, range_y, range_z, sensor.type, sensor.mods, ignored_entities)
	data_for_sensor.append_array(d)
	return data_for_sensor


func get_range(origin, delta, min, max):
	# prevent start range < 0
	# return a coordinate dimension range, ex. [ start, end ]
	return [maxi(min, origin - delta), mini(max, origin + delta) ]


func scan_coordinates_for_entities(range_x, range_y, range_z, sensor_type, sensor_mods, ignored_entities = []):
	print("scan_coordinates_for_entities")
	var all_sensor_data = []
	
	var x = range_x[0]
	var end_x = range_x[1]
	var y = range_y[0]
	var end_y = range_y[1]
	var z = range_z[0]
	var end_z = range_z[1]
	
	while z <= end_z:
#		var c_z = []
		while y <= end_y:
#			var c_y = []
			while x <= end_x:
#				var c_x = []
#				c_y.append(c_x)
				var sensor_data_at_coordinate = check_coordinate_with_sensor(sensor_type, sensor_mods, x, y, z, ignored_entities)
				if sensor_data_at_coordinate.size() > 0:
					all_sensor_data.append({
						"loc": Vector3i(x, y, z),
						"data": sensor_data_at_coordinate
					})
				x += 1
#			c_z.append(c_y)
			x = 0
			y += 1
#		coordinates.append(c_z)
		y = 0
		z += 1
	
	print("all_sensor_data for %s:" % [sensor_type])
	print(all_sensor_data)
	return all_sensor_data


func check_coordinate_with_sensor(sensor_type, sensor_mods, x, y, z, ignored_entities = []):
	var sensor_data = []
	var coordinate_data = coordinates[z][y][x]
	# print("checking coordinate %d, %d, %d" % [x, y, z])
	# print(coordinate_data)
	#	sensor_data.append_array(coordinate_data)
	for entity_id in coordinate_data:
		if entities_by_id.has(entity_id) and entity_id not in ignored_entities:
			var entity:Entity = entities_by_id[entity_id]
			if entity.detectable.has(sensor_type):
				print("entity ID %s sensed by %s" % [entity_id, sensor_type])
				# TODO clarify how sensors detect morphology
				sensor_data.append_array(entity.placement)
				sensor_data.append(entity.morphology)
		
	
	return sensor_data


# a thing that occupies dimensional space. Entities can collide/intersect with, attach/detach to, and be created/destroyed by other entities
# an entity can contain a metabolism (an acive or dormant metabot)
# an entity can have a body that moves in dimensional space
# an entity body that moves in physical space has a metabolism that provides the energy for movement
# an entity body is controlled by an agent which manages goals, uses local environment data gathered via sensors
# to determine actions to achieve the active goal. 
# an agent creates input > command > impulse > action through its body to manipulate entities and complete its goals
# a metabolism is the power supply for an agent and its body entity
class Entity:
	
	var id:String
	var center_point: Vector3
	var radius:float
	var agent:Agent
	var metabot:Metabot = null
	var metabot_species:String
	
	var pools = [] # resource pools that are available to attached metabots
	
	var tags = [
#		"soil",
#		"plant",
	]
	
	# world layers which this entity has.
	var placement = [
#		"seed", 
#		"potato",
#		"grounded",
	]
	# world layers which entities of type can share a world zone.
	var shareable_placement = [
#		"grounded",
	]
	# world layers which entities of type can NOT share a world zone.
	var nonshareable_placement = [
#		"seed", 
#		"potato",
	]
	# which senses can detect this entity
	var detectable = [
#		"vision"
	]
	
	# physical appearance / body shape
	# this is often directly related to the metabot's lifecycle stage / health / metabolic state
	# morphology features should update with lifecycle stage progression event
	# changing morphology is generally how an agent senses that the entity is a valid target
	# ex. potato is ready for harvesting 
	var morphology = {
#		"depth": "underground",
#		"height": "short",
	}
	
	func _init(
				_placement = [], _shareable_placement = [], _nonshareable_placement = [],
				_detectable = [], _tags = []
				):
		print("new Entity")
		placement = _placement
		shareable_placement = _shareable_placement
		nonshareable_placement = _nonshareable_placement
		detectable = _detectable
		tags = _tags
		
		print(placement)
		print(shareable_placement)
		print(nonshareable_placement)
		print(detectable)
	
	
	# return world areas occupied by entity.
	func get_body_boundary(world_centerpoint):
		var body_zones = []
		var zone = world_centerpoint
		body_zones.append(zone)
		return body_zones


	func on_attach(attached_entity:Entity):
		print("on_attach")
		print(attached_entity)
		
		# TODO when seed attaches to soil, add the soil's water and mineral pools 
		# to the seed's metabot (potato) collector


	func attach_metabot(_metabot): 
		print("attach_metabot")
		# watch for morphology changes
		_metabot.connect("body_changed", self.on_change_body)
		metabot = _metabot
		pass

	# signal propagating from Metabot on_bodyparts_added(bodyparts)
	# when metabot body changes, update the morphology presented to the environment/agents by the entity
	func on_change_body(_bodymass, _body):
		print("on_change_body", _bodymass, _body)

		# TODO implement morphology changes in the species' class definition
		# morphology change for potato
		if (_bodymass >= 3):
			morphology["height"] = _bodymass / 3
			morphology["symbol"] = "potato"

			placement.append_array(["plant", "potato"])
		
		print(morphology)
		pass


# https://docs.godotengine.org/en/stable/classes/class_astar.html
# input agent, world, and type of motion (ground, air, liquid)
# mark points not traversable
# generate path map from agent's type of motion (ground, air, liquid)
# get point on path map from target's location in world
# get shortest paths to target point
# did environment change? did goal change? if yes, regenerate path map and shortest paths
class Pathfinder:
	
	var points = []
	
	func _init():
		print("new Pathfinder")
