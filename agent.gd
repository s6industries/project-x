extends Node

class_name Agent

#const Metabot = preload("res://metabot.gd")

# Sensory data creates: model of environment
# combined with goals and rules 
# generates Actions
# ex. target = food. move towards food

# Actions translate to Input
# ex. KeyboardInput: arrow keys

# Brain accepts input and creates Commands
# ex. move tow along 2D vector

# Actuator sends signal to motor
# ex. Signal: move legs alond 2D vector

# Motor receives Signal and consumes Resources to execute Action
# ex. Action: move legs along 2D vector, adds force along 2D vector to body

# Action executed in the physics simulation mutates physics state of body
# ex. body changes position from before action executed to after action

var metabot:Metabot = null
var motor = null

var inputs = []
var commands = []
var signals = []
var actuators = {}
var motors = {}
var actions = []

# Called when the node enters the scene tree for the first time.
func _ready():
#	pass # Replace with function body.
	print("Agent ready")
	
# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass
	
func attach_metabot(mbot:Metabot):
	metabot = mbot
	
func receive_input():
	print("input")

func create_command():
	print("command")
	


# inner classes

class PlayerAgent extends Agent:
	func _init():
		print("PlayerAgent")
