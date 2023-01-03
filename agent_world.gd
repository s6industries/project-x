extends Node

class_name AgentWorld

# https://github.com/binogure-studio/godot-uuid
const uuid_util = preload('res://uuid.gd')

var agents = []

var entities = []
var entities_by_id = {}
# x[y[z[]]]. x, y for 2D plane, top down perspective. z for layer depth
# z[y[x[]]]. x, y for 2D plane, top down perspective. z for layer depth
var coordinates = [] 

var timer:Timer
var tick_interval:float

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


func initiate_timer():
	timer = Timer.new()
	timer.set_one_shot(false)
	timer.connect("timeout", self.tick)
	add_child(timer)
	# autostart
	timer.start(tick_interval)

func tick():
	for agent in agents:
		agent.tick()
		
		
func _init(size_3D:Vector3i):
	print("AgentWorld %d, %d, %d " % [size_3D.x, size_3D.y, size_3D.z])
#	coordinates = generate_coordinates(size_3D.x, size_3D.y, size_3D.z)
	coordinates = generate_coordinates(size_3D.z, size_3D.y, size_3D.x)
	print(coordinates)
	
	
# Called when the node enters the scene tree for the first time.
func _ready():
	print("_ready AgentWorld ")
	tick_interval = 1.0
#	initiate_timer()


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	
	pass
	
	
# each entity has a center coordinate + a radius for body size
# center + radius determines which world coordinates are occupied by the entity's body
func add_entity(entity:Entity, center_point:Vector3i, id:String = uuid_util.v4()):	
	entity.id = id
	entities_by_id[id] = entity
	entities.append(entity)
	place_entity(entity, center_point)


func place_entity(entity:Entity, center_point:Vector3i):
	print("place_entity %s" % [entity.id])
	
	var body_zones = entity.get_body_boundary(center_point)
	
	if (body_zones.size() > 0):
		# entity is assigned a world coordinate
		entity.center_point = center_point
		for zone in body_zones:
			print(zone)
			set_entity_at_coordinate(entity.id, true, zone.x, zone.y, zone.z)
		return true
	else:
	# if entity cannot be placed at this center_point
		return false


func set_entity_at_coordinate(id:String, is_present:bool, x:int,  y:int, z:int):
	print("set_entity_at_coordinate %s: %d, %d, %d" % [id, x, y, z])
	var coordinate_data:Array = coordinates[z][y][x]
	if id not in coordinate_data:
		if is_present:
			coordinate_data.append(id)
	else:
		if !is_present:
			coordinate_data.erase(id)


func grab_at_location(location:Vector3, grab_target, grabber):
	var grabbed = null
	# check if there is an entity at location
	# is entity type of grab_target
	# if grab_target can be grabbed by grabber
	return grabbed


func get_sensor_data_for_entity(_id:String):
	if entities_by_id.has(_id):
		var entity:Entity = entities_by_id[_id]
		
		get_sensor_data(entity.agent.sensors, entity.center_point)
	else:
		print("no entity registered with id %s" % [_id])
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
func get_sensor_data(sensors:Array, origin:Vector3):
	print("get_sensor_data")
	print(sensors)
	print(origin)
	
	for sensor in sensors:
		print(sensor.type)
		print(sensor.mods)
		print(sensor.range)
		match sensor.type:
			"vision":
				print("vision")
				get_detectable_entities(sensor, origin)
				

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
func get_detectable_entities(sensor, origin:Vector3):
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
	print(range_x)
	print(range_y)
	print(range_z)
	# from origin set scan range across all coordinate dimensions
	scan_coordinates_for_entities(range_x, range_y, range_z, sensor.type, sensor.mods)


func get_range(origin, delta, min, max):
	# prevent start range < 0
	# return a coordinate dimension range, ex. [ start, end ]
	return [maxi(min, origin - delta), mini(max, origin + delta) ]


func scan_coordinates_for_entities(range_x, range_y, range_z, sensor_type, sensor_mods):
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
				var sensor_data_at_coordinate = check_coordinate_with_sensor(sensor_type, sensor_mods, x, y, z)
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
	
	print("all_sensor_data:")
	print(all_sensor_data)


func check_coordinate_with_sensor(sensor_type, sensor_mods, x, y, z):
	var sensor_data = []
	var coordinate_data = coordinates[z][y][x]
	print("checking coordinate %d, %d, %d" % [x, y, z])
	print(coordinate_data)
	sensor_data.append_array(coordinate_data)
	return sensor_data


class Entity:
	
	var id:String
	var center_point: Vector3
	var radius:float
	var agent:Agent
	
	# world layers which this entity has.
	var placement = [
		"seed", "potato",
		"grounded",
	]
	# world layers which entities of type can share a world zone.
	var shareable_placement = [
		"grounded",
	]
	# world layers which entities of type can NOT share a world zone.
	var nonshareable_placement = [
		"seed", "potato",
	]
	# which senses can detect this entity
	var detectable = [
		"vision"
	]
	
	func _init():
		print("new Entity")
		print(placement)
		print(shareable_placement)
		print(nonshareable_placement)
	
	
	# return world areas occupied by entity.
	func get_body_boundary(world_centerpoint):
		var body_zones = []
		var zone = world_centerpoint
		body_zones.append(zone)
		return body_zones
		
		
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
