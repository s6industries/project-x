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

var model_state = {
	"body_position": [0,0]
}
var motor = null
var motors = {}

var actuator = null
var actuators = {}

var inputs = []
var commands = []
var signals = []
var actions = []

var executing_action = false

var timer: Timer = null
var tick_interval = 0.7
#var tick_interval = 0.3

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
	print("TICK. Agent")
#	for mbot in metabots:
#		mbot.tick()
	var input = process_input()
	
	if input == null:
		print("no input")
	else:
		print(input)
		create_command()
		send_signal()
		execute_action()
		report_status()

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
	print("send_input: %s" % input)
	inputs.append(input)

func process_input():
	print("process_input")
	var input = inputs.pop_front()
			
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
	
	var timer_input = null
	var tick_interval_input = 0.3
	
	func _init():
		print("PlayerAgent")
		
	func _ready():
		print("PlaeyrAgent ready")
#		initiate_timer_input()
		super.initiate_timer()
	
	func _input(event):
#		print(event.as_text())
		
		# directly mapping raw input to actions
#		if event is InputEventKey and event.pressed:
#			print(event.as_text())
#			var event_key = event as InputEventKey
#
#			if event_key.keycode == KEY_T:
#				if event_key.shift_pressed:
#					print("Shift+T was pressed")
#				else:
#					print("T was pressed")
#
		# using Input Maps to abstract away the raw input outside of scripts
		# Project -> Project Settings -> Input Map => Show Built-in Action
		if event.is_action_type():
			var input = null
			print(event.as_text())
			if event.is_action_pressed("ui_down"):
				input = "down"
			if event.is_action_pressed("ui_up"):
				input = "up"
			if event.is_action_pressed("ui_left"):
				input = "left"
			if event.is_action_pressed("ui_right"):
				input = "right"
			
			if input != null:
				print(input)
				send_input(input)
			
	func initiate_timer_input():
		timer_input = Timer.new()
		timer_input.set_one_shot(false)
		timer_input.connect("timeout", self.tick_input)
		add_child(timer_input)
		# autostart
		timer_input.start(tick_interval_input)
		
	func tick_input():
		print("tick_input")

#https://docs.godotengine.org/en/stable/classes/class_input.html
