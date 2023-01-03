extends Node

class_name AgentWorld

var entities = []
var coordinates = [] # x[y[z[]]]. x, y for 2D plane, top down perspective. z for layer depth

# each entity has a center coordinate + a radius for body size
# center + radius determines which world coordinates are occupied by the entity's body

func add_entity(entity):
	entities.append(entity)

func place_entity(entity, center_point):
	print("place_entity")
	# with 
	
	# if entity cannot be placed at this center_point
	return false

func grab_at_location(location:Vector3, grab_target, grabber):
	var grabbed = null
	# check if there is an entity at location
	# is entity type of grab_target
	# if grab_target can be grabbed by grabber
	return grabbed

# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	
	pass

class Entity:
	
	var center_point: Vector3
	var radius:float
	
	# world layers which this entity has.
	var placement = [
		"seed", "potato",
		"grounded",
	]
	# world layers which entities of type can share a world zone.
	var shareable_placement = [
		"grounded"
	]
	# world layers which entities of type can NOT share a world zone.
	var nonshareable_placement = [
		"seed", "potato"
	]
	
	# return world areas occupied by entity.
	func get_body_boundary(world_centerpoint):
		var body_zones = []
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
