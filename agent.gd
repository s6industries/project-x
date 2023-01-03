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

# Actuator sends impulse to motor
# ex. impulse: move legs alond 2D vector

# Motor receives impulse and consumes Resources to execute Action
# ex. Action: move legs along 2D vector, adds force along 2D vector to body

# Action executed in the physics simulation mutates physics state of body
# ex. body changes position from before action executed to after action

var metabot:Metabot = null
var metabots = []

var entity:AgentWorld.Entity
var entities = []

var world: AgentWorld

var model_state = {
	"body_position": Vector2(0,0),
	"attachments": {
		"hands":[],
		"backpack":[],
		"digestion":[]
	}
}

 
var motor = null
var motors = {}

var actuator = null
var actuators = {}

# Behaviors are finite state machines 
# a behavior state executes a sequence of commands or a nested behavior state machine
var behaviors = []
var active_behavior = null

var goals = []
var goals_prioritized = []

#var sensors = []
var sensors = [
	{
		"type":"vision",
		"range": 3,
		"mods": [
			"day",
			"night"
		],
	}
]

var inputs = []

var active_command = null
var commands = []
var impulses = []
var actions = []

var is_AI = false
var is_executing_action = false

#var timer: Timer = null
#var tick_interval = 0.7
#var tick_interval = 0.3

func _init(_world:AgentWorld):
	print("new agent in world")
	world = _world


# Called when the node enters the scene tree for the first time.
func _ready():
#	pass # Replace with function body.
	print("Agent ready")
#	initiate_timer()
	
	
# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass
	

#func initiate_timer():
#	timer = Timer.new()
#	timer.set_one_shot(false)
#	timer.connect("timeout", self.tick)
#	add_child(timer)
#	# autostart
#	timer.start(tick_interval)
	
## Time ticks for the Agent when it is conscious.
## When it is unconscious/sleeping, time does not tick.
func tick():
	print("TICK. Agent")
	
	sense_world()
	
#	for mbot in metabots:
#		mbot.tick()
	var input = process_input()
	
	if input == null:
		print("no input")
		
	else:
		print(input)
		var command = create_command(input)
		if command == null:
			return
		# move legs in direction
		var impulse = send_impulse(command)
		# move legs along vector
		if impulse == null:
			return
			
		if !is_executing_action:
			var action = execute_action(impulse)
			# add force to body along vector
			if action != null:
				actions.append(action)
		
	var is_environment_affected = affect_environment()
	
	report_status()
	
	
func attach_metabot(mbot:Metabot):
	metabot = mbot


## primarily for AI agent, as player agent senses environment from their own eyes and ears
func sense_world():
	print("sense_world")
	world.get_sensor_data_for_entity(entity.id)


#func sense_environment():
#	if is_AI:
#		print("environment")
#		for sense in sensors:
#			print("sensing")


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
	
## Input translated to body-specific command
## for a bipedal "move down" => "walk down
## for a flier, "move down" => "fly down"
func create_command(input):
	print("command")
	
	# translation the input relative to agent's state model 
	# to an action relative to the agent's associated body in the environment
	var command = null
	
	# command structure:
	# [ action, relativeTo, perspective, modifier1, modifier2 ]
	match input:
		# movement
		"move_screen_right":
			command = ["move","world","topdown","right", 1.0]
		"move_screen_left":
			command = ["move","world","topdown","left", 1.0]
		"move_screen_up":
			command = ["move","world","topdown","up", 1.0]
		"move_screen_down":
			command = ["move","world","topdown","down", 1.0] 
		# object interactions
		# put item in inventory
		"collect_here":
			command = ["collect","world","local", 1.0]
		# eat held item = attach source to Metabot collector 
		# generally results in stored energy for Metabot
		"eat_here":
			command = ["eat","equipment","main"]
		# hold an item from world in body range
#		"pickup_here":
#			command = ["pickup","world","topdown","local", 1.0]
#		# release the currently held item
#		"drop_here":
#			command = ["drop","world","topdown","local", 0.0]
#		# put item from inventory into the main equipment slot
#		"equip_selected":
#			command = ["equip","inventory","selected","main"]
#		# use item in main equipment slot
#		# using item may consume Metabot's stored energy 
#		"activate_equip":
#			command = ["activate","equipment","main"]
#		# move item from main equipment slot to inventory
#		"unequip_selected":
#			command = ["unequip","inventory","selected"]
		
		# object => equipment 
	
	return command

## command translated to body part-specific impulse
## "walk down" => "legs, normal speed, vector (0, -1)"
func send_impulse(command):
	print("impulse")
	var impulse = null
	
	# impulse knows about body parts
	
	var bodypart = null
	
	match command[0]:
#		["move","world","topdown","right", 1.0]
		"move":
			# if bipedal
			bodypart = "legs"
			var vector = null
			var magnitude = command[4]
			if command[1] == "world" && command[2] == "topdown":
				match command[3]:
					"up":
						vector = Vector2.UP
					"down":
						vector = Vector2.DOWN
					"left":
						vector = Vector2.LEFT
					"right":
						vector = Vector2.RIGHT
						
				impulse = [bodypart, vector, magnitude]
				
#		["collect","world","local", 1.0]
		"collect":
			impulse = ["hands","grab",Vector2.ZERO]
			
	return impulse
## impulse translated to mutation of body in environment
## "legs, normal speed, vector (0, -1)" => add force to body vector (0, -1)
func execute_action(impulse):
	print("action")
	var action = null
	var bodypart = impulse[0]
	
	match bodypart:
		"legs":	
			var vector = impulse[1]
			var magnitude = impulse[2]
			action = ["translate_body", vector * magnitude ]
		"hands":
			var location = impulse[2]
#			var attach_node = bodypart
			var attach_node = "backpack"
			var attach_method = impulse[1]
			var attach_target = ["any", "seed", "potato"]
			action = ["attach_body", attach_target, attach_node, attach_method, location]
	return action
## Interact with the world, 
## such as simulating physics for moving body, collecting objects/resources
## translate body by vector (0, -1)
## collect tool or potato
func affect_environment():
	print("environment")
	
	if actions.size() == 0:
		return false

	var next_action = actions.pop_front()
	
	match next_action[0]:
		"translate_body":
			model_state["body_position"] += next_action[1]
			return true
		"attach_body":
			var world_location = Vector3(0,0,0)
			world.grabAtLocation(world_location, next_action[1], next_action[2])

func report_status():
	print("agent status")
	print("body position: [%f. %f]" % [model_state["body_position"].x, model_state["body_position"].y])


# inner classes


class AIAgent extends Agent:

	
	func _init(_world:AgentWorld):
		is_AI = true
		super._init(_world)
		print("_init AIAgent")
		
		
	func _ready():
#		initiate_timer_input()
		super._ready()
		print("_ready AIAgent")


class PlayerAgent extends Agent:
	
#	var timer_input = null
#	var tick_interval_input = 0.3
	
	func _init(_world:AgentWorld):
		super._init(_world)
		print("PlayerAgent")
		
	func _ready():
		print("PlayerAgent ready")
#		initiate_timer_input()
		super._ready()
		
	#https://docs.godotengine.org/en/stable/classes/class_input.html
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
				input = "move_screen_down"
			if event.is_action_pressed("ui_up"):
				input = "move_screen_up"
			if event.is_action_pressed("ui_left"):
				input = "move_screen_left"
			if event.is_action_pressed("ui_right"):
				input = "move_screen_right"
			
			if event.is_action_pressed("agent_collect"):
				input = "collect_here"
			if event.is_action_pressed("agent_eat"):
				input = "eat_here"
			
			if input != null:
#				print(input)
				send_input(input)
			
#	func initiate_timer_input():
#		timer_input = Timer.new()
#		timer_input.set_one_shot(false)
#		timer_input.connect("timeout", self.tick_input)
#		add_child(timer_input)
#		# autostart
#		timer_input.start(tick_interval_input)
#
#	func tick_input():
#		print("tick_input")



