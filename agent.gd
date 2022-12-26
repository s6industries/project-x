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
var metabots = []

var motor = null

var inputs = []
var commands = []
var signals = []
var actuators = {}
var motors = {}
var actions = []

var timer: Timer = null
var tick_interval = 0.3

func initiate_timer():
	timer = Timer.new()
	timer.set_one_shot(false)
	timer.connect("timeout", self.tick)
	add_child(timer)
	# autostart
	timer.start(tick_interval)
	
## Time ticks for the Agent when it is conscious.
## When it is unconscious/sleeping, time does not tick.
func tick():
	print("TICK.")
#	for mbot in metabots:
#		mbot.tick()
	process_input()
	create_command()
	

# Called when the node enters the scene tree for the first time.
func _ready():
#	pass # Replace with function body.
	print("Agent ready")
	initiate_timer()
	
# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass
	
func attach_metabot(mbot:Metabot):
	metabot = mbot

# https://docs.godotengine.org/en/stable/tutorials/inputs/inputevent.html
# TODO handle platform specific input before this (ex. gamepad vs touch vs keyboard)
# https://docs.godotengine.org/en/stable/classes/class_%40globalscope.html#enum-globalscope-keylist
func send_input(input):
	print("input: %s" % input)
	inputs.appenmd(input)

func process_input():
	var input = inputs.pop_front()
	
	match input:
		KEY_DOWN:
			input = "down"	
		KEY_UP:
			input = "up"
		KEY_RIGHT: 
			input = "right"
		KEY_LEFT:
			input = "left"
			
	return input
	
func create_command():
	print("command")
	

func send_signal():
	print("signal")
	
func execute_action():
	print("action")
	
func report_status():
	print("agent status")

# inner classes

class PlayerAgent extends Agent:
	func _init():
		print("PlayerAgent")
