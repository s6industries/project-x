extends Node2D

@export var world_label: Label

enum State { IDLE, MOVING, ACTION }
const PLAYER = "@"
const POTATO_STAGE = [".", ";", "i", "P"]
const MOVE_DELAY = 0.12

var player_pos: Vector2i
var input_direction: Vector2i
var world_map: Array
var id = 0
var metabots: Dictionary # id : [stage, position]
var state: State = State.IDLE
var timer: Timer = null
var can_move: bool = true
var metabot_simulator
var potato_stage: int

const MetabotSimulator = preload("res://metabot_simulator.gd")

func load_world():
	var file_path = "res://world.txt"
	var file = FileAccess.open(file_path, FileAccess.READ)
	var y = 0
	while not file.eof_reached():
		var line = file.get_line()
		var x = line.find(PLAYER)
		if x >= 0:
			player_pos = Vector2i(x, y)
			print("FOUND PLAYER")
			line[x] = " "
		# Replace player with blank space bc player gets rendered separately
		world_map.append(line)
		y += 1
	print(world_map)


func initiate_timer():
	timer = Timer.new()
	timer.set_one_shot(true)
	timer.connect("timeout", self.animation_completed)
	add_child(timer)


func initiate_simulator():
	metabot_simulator = MetabotSimulator.new()
	add_child(metabot_simulator)
	
func test_agent():
	var agentWorld = AgentWorld.new()
	var player_agent = Agent.PlayerAgent.new(agentWorld)
	add_child(player_agent)

# Called when the node enters the scene tree for the first time.
func _ready():
	
	initiate_simulator()
	
	# metabots plant potat AT
	metabots[id] = [0, Vector2i(20, 10)]
	var potato = metabot_simulator.plant_potato(id)
#	potato.life_stage_progressed.connect(self.potato_life_stage_progressed.bind(stage))
	potato.connect("life_stage_progressed", self.potato_life_stage_progressed)
	id += 1
	
	# metabots plant potat AT
	metabots[id] = [0, Vector2i(40, 10)]
	var potato2 = metabot_simulator.plant_potato(id)
#	potato.life_stage_progressed.connect(self.potato_life_stage_progressed.bind(stage))
	potato2.connect("life_stage_progressed", self.potato_life_stage_progressed)
	id += 1
	
#	test_agent()
	
	load_world()
	initiate_timer()
	
	var test_class = TestClass.new()
	test_class.hello_world()





func potato_life_stage_progressed(id, stage):
	print("potato_life_stage_progressed: ", id, stage)
	potato_stage = stage
	metabots[id][0] = stage



# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(_delta):
	match state:
		State.IDLE:
			idle_state()
		State.MOVING: 
			moving_state()
#		State.ACTION:
#			action_state()
	update_world()


func get_player_input():
	if Input.is_action_pressed("ui_left"):
		return Vector2i.LEFT
	if Input.is_action_pressed("ui_right"):
		return Vector2i.RIGHT
	if Input.is_action_pressed("ui_up"):
		return Vector2i.UP
	if Input.is_action_pressed("ui_down"):
		return Vector2i.DOWN
	return Vector2i.ZERO


func idle_state():
	var direction = get_player_input()
	if direction != Vector2i.ZERO:
		input_direction = direction
		state = State.MOVING


func moving_state():
	if can_move:
		move(input_direction)
		can_move = false
		timer.set_wait_time(MOVE_DELAY)
		timer.start()


func update_world():
	var x = player_pos[0]
	var y = player_pos[1]
	var temp_world = world_map.duplicate()
	if y < len(temp_world) and x < len(temp_world[0]):
		temp_world[y][x] = PLAYER
	
	# Temp potato. TODO
	for id in metabots:
		var pos = metabots[id][1]
		var stage = metabots[id][0]
		temp_world[pos[1]][pos[0]] = POTATO_STAGE[stage]
		
#	for fruit in groceries:
#    var amount = groceries[fruit]

#	var potato_pos = Vector2i(20, 10)
#	temp_world[potato_pos[1]][potato_pos[0]] = POTATO_STAGE[potato_stage]
	
	var world_string = ""
	for row in temp_world:
		world_string += row + "\n"
	world_label.text = world_string


func move(direction: Vector2i):
	if is_position_walkable(player_pos + direction):
		player_pos += direction


func is_position_walkable(pos):
	return true


func animation_completed():
	print("ANIMATION_COMPLETED!")
	can_move = true
	state = State.IDLE
