extends Node

class_name AgentWorld

var entities = []
var coordinates = [] # x[y[z[]]]

func grabAtLocation(location:Vector3, grab_target, grabber):
	var grabbed = null
	# if grab_target can be grabbed by grabber
	return grabbed

# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass


class Pathfinder:
	
	var points = []
	
	func _init():
		print("new Pathfinder")
