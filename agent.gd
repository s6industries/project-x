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
	},
	"sensor_data": [],
	"memory":{
		"plant":{
			"seed": 0
		}
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

# goal [ goal_type, [goal_modifiers], [goal_targets] ]
var goals = [
	["survive", [], []],
	["explore", [], []],
	["collect", ["seed", 1], []],
	["plant", ["seed", 1], []],
]
var goals_prioritized = [
	2, 3, 1, 0
]
var active_goal = null

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
	world.agents.append(self)


# Called when the node enters the scene tree for the first time.
func _ready():
#	pass # Replace with function body.
	print("Agent ready")
#	initiate_timer()


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass


func create_input(goal):
	print("create_input")
	var input = null
	
	var goal_type = goal[0]
	var goal_target = goal[2][0]
	
	match goal_type:
		
		# ex. ["collect", ["seed"], [(1, 2, 0)]]
		"collect":
			var delta_vector:Vector3i = get_delta_to_goal(entity.center_point, goal_target)
			
			if delta_vector.length() >= 1:
				input = create_input_move_to_goal(delta_vector)
			else:
				input = "collect_here"
				print("entity is at goal. %s" % [input])
				
		# ex. ["plant", ["seed"], [(1, 2, 0)]]
		"plant":
			var delta_vector:Vector3i = get_delta_to_goal(entity.center_point, goal_target)
			
			if delta_vector.length() >= 1:
				input = create_input_move_to_goal(delta_vector)
			else:
				input = "plant_here"
				print("entity is at goal. %s" % [input])
		
	if input != null:
		send_input(input)


func create_input_move_to_goal(delta_vector:Vector3i):
	var input = null
	
	print("agent move entity towards goal")
	# clamp delta_vector to compare with input for orthogonal movement
#	delta_vector = delta_vector.clamp(Vector3i.ZERO, Vector3i(1, 1, 0))
	delta_vector = delta_vector.clamp( Vector3i(-1, -1, 0), Vector3i(1, 1, 0))
	print(delta_vector)
	print("delta vector clamped:")
	print(delta_vector)
	
	if delta_vector.y == 1:
#	if event.is_action_pressed("ui_down"):
		input = "move_screen_down"
	elif delta_vector.y == -1:
#	if event.is_action_pressed("ui_up"):
		input = "move_screen_up"
	elif delta_vector.x == -1:
#	if event.is_action_pressed("ui_left"):
		input = "move_screen_left"
	elif delta_vector.x == 1:
#	if event.is_action_pressed("ui_right"):
		input = "move_screen_right"
	
	return input


func get_attachments(attach_node, entity_type):
	var attachments = []
	if model_state["attachments"].has(attach_node):
		for entity in model_state["attachments"][attach_node]:
			if entity.placement.has(entity_type):
				attachments.append(entity)
			
	return attachments


func get_memory(goal_type, goal_target):
	var memory = null
	print("get_memory %s %s" % [goal_type, goal_target])
	if model_state["memory"].has(goal_type):
		if model_state["memory"][goal_type].has(goal_target):
			memory = model_state["memory"][goal_type][goal_target]
	
	return memory


func create_memory(goal_type, goal_target):
	print("create_memory %s %s" % [goal_type, goal_target])
	if model_state["memory"].has(goal_type):
		if model_state["memory"][goal_type].has(goal_target):
			model_state["memory"][goal_type][goal_target] += 1


func check_goal_completed(goal):
	print("check_goal_completed")
	print(goal)
	var is_goal_completed = false
	
	var goal_type = goal[0]
	var target_info = goal[1]
	match goal_type:
		
		# ex. ["collect", ["seed", 1], []],
		"collect":
			var target_type = target_info[0]
			var current_amount = get_attachments("backpack", "seed").size()
			var goal_amount = target_info[1]
			print("%s target %s: %d of %d" % [goal_type, target_type, current_amount, goal_amount])
			is_goal_completed = current_amount >= goal_amount
			
		# ex. ["plant", ["seed", 1], []],
		"plant":
			var target_type = target_info[0]
			var memory = get_memory(goal_type, target_type)
			if memory:
				var current_amount = get_memory(goal_type, target_type)
				var goal_amount = target_info[1]
				print("%s target %s: %d of %d" % [goal_type, target_type, current_amount, goal_amount])
				is_goal_completed = current_amount >= goal_amount
			else:
				print("no memory")	
	
	return is_goal_completed


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
	report_status()
	
	active_goal = goals[goals_prioritized[0]]
	if check_goal_completed(active_goal):
		print("!!! goal completed !!!")
		print(active_goal)
		# TODO: how to handle completed goals?
		# continue to next prioritized goal
		goals_prioritized.pop_front()
	
	sense_world()
	print("goals after sense_world")
	print(goals)
	
	# TODO automatically prioritize goals
	
	goals[goals_prioritized[0]] = set_goal(goals[goals_prioritized[0]])
	print("goals after set_goal")
	print(goals)
	
	# an AI behavior is an algorithm that generates inputs based on the agent's active goal and model state
	# AI agent sends input to achieve active goal
	if is_AI:
		# TODO implement AI Behaviors here
		active_goal = goals[goals_prioritized[0]]
		var goal_targets = active_goal[2]
		if goal_targets.size() > 0:
			print("sending AI input for goal %s" % [active_goal[0]])
			print(active_goal)
			create_input(active_goal)
		
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
		
#	var is_environment_affected = affect_environment()
	var action_in_environment = affect_environment()
	
	return action_in_environment


func set_goal(goal):
	print("set_goal")
	print(goal)
	
	var goal_type = goal[0]
	var goal_mods = goal[1]
	var goal_targets = goal[2]
	print(goal_type)
	print(goal_mods)
	print(goal_targets)
	
	match goal_type:
		
		"collect":
			# TODO how to handle multiple target types to collect?
			# taking only the first target type to collect
			var target = goal_mods[0]
			var sensor_type_data = "vision"
			# zones with entities were detected per sensor type in sense_world()
			var zones_with_entities = model_state["sensor_data"][sensor_type_data]
			for zone in zones_with_entities:
				if target in zone["data"]:
					var location = zone["loc"]
					# prevent duplicate locations
					if location not in goal_targets:
						goal_targets.append(location)
		
		"plant":
			var target = "soil"
			var sensor_type_data = "vision"
			# zones with entities were detected per sensor type in sense_world()
			var zones_with_entities = model_state["sensor_data"][sensor_type_data]
			for zone in zones_with_entities:
				if target in zone["data"]:
					var location = zone["loc"]
					# prevent duplicate locations
					if location not in goal_targets:
						goal_targets.append(location)
			
			
	if goal_targets.size() > 0:
		print("goal '%s' has possible targets: %d" % [goal_type, goal_targets.size()])
		print(goal_targets)
		goal[2] = goal_targets
		
		# TODO weight and prioritize targets for goal
		for target in goal_targets:
			var delta_to_goal = get_delta_to_goal(entity.center_point, target)
			print(delta_to_goal)
			var distance_to_goal = delta_to_goal.length()
			print(distance_to_goal)
	else:
		print("no targets found for goal %s" % [goal_type])
	
	return goal


func get_delta_to_goal(origin, target):
	var delta:Vector3i = Vector3i.ZERO
	print("get_delta_to_goal")
	print(origin)
	print(target)
#	delta = Vector3i(origin) - Vector3i(target)
	delta = Vector3i(target) - Vector3i(origin) 
	
	return delta


func attach_metabot(mbot:Metabot):
	metabot = mbot


## primarily for AI agent, as player agent senses environment from their own eyes and ears
func sense_world():
	print("sense_world")
	var sensor_data_frame = world.get_sensor_data_for_entity(entity.id)
	model_state["sensor_data"] = sensor_data_frame
	print("model_state for agent %s" % [entity.id])
	print(model_state)
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
		
		"plant_here":
			command = ["plant","world","local", 1.0]
		# eat held item = attach source to Metabot collector 
		# generally results in stored energy for Metabot
#		"eat_here":
#			command = ["eat","equipment","main"]
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
		
		"plant":
			impulse = ["hands","bury",Vector2.ZERO]
			
	return impulse


# impulse translated to mutation of body in environment
# ex. "legs, normal speed, vector (0, -1)" => add force to body vector (0, -1)
# the action is the agent's last intention to manipulate the world before all actions are resolved
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
			var action_method = impulse[1]
			var relative_location = impulse[2]
			
			match action_method:
				"grab":
		#			var attach_node = bodypart
					var attach_node = "backpack"
					var attach_method = action_method
					var attach_target = ["any", "seed", "potato"]
					action = ["attach_body", attach_target, attach_node, attach_method, relative_location]
				"bury":
					# detach item from inventory
					var detach_node = "backpack"
					var detach_method = action_method
					var detach_target = ["seed"]
#					action = ["detach_body", detach_target, detach_node, detach_method, location]

					var attach_location = "soil"
					var attach_method = action_method
					var attach_object = "seed"
					# burying a seed in the soil is multiple actions simultaneously: detach seed from body entity > attach seed to soil entity
					# TODO cleaner way to define multistep / simultaneous actions?
					action = {
						"steps":[
							["detach_body", detach_target, detach_node, detach_method, relative_location],
							["attach_to", attach_object, attach_location, attach_method, relative_location],
							["remember", "plant", "seed"]
						]
					}
	return action


## Interact with the world, 
## such as simulating physics for moving body, collecting objects/resources
## translate body by vector (0, -1)
## collect tool or potato
func affect_environment():
	print("affect_environment")
	
	if actions.size() == 0:
		return false
	
	# the agent's actions that affect the world / environment
	# are queued for simultaneous processing in the universal physics simulation
	# TODO how to implement a fair tiebreaker when multiple agents share a goal target?
	var next_action = actions.pop_front()
	
	return next_action
	
#	match next_action[0]:
#		"translate_body":
#			model_state["body_position"] += next_action[1]
#			return true
#		"attach_body":
#			var world_location = Vector3(0,0,0)
#			world.grabAtLocation(world_location, next_action[1], next_action[2])


func attach_entity(entity, attach_node):
	print("attach_entity %s to %s" % [entity, attach_node])
	if model_state["attachments"].has(attach_node):
		model_state["attachments"][attach_node].append(entity)
	else:
		print("entity has no attach node '%s'" % [attach_node])


func detach_entity(entity, attach_node):
	if model_state["attachments"].has(attach_node):
		model_state["attachments"][attach_node].erase(entity)


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
			if event.is_action_pressed("agent_plant"):
				input = "plant_here"
#			if event.is_action_pressed("agent_eat"):
#				input = "eat_here"
			
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



