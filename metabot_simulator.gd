extends Node

# https://docs.godotengine.org/en/latest/tutorials/scripting/gdscript/gdscript_documentation_comments.html

class_name MetabotSimulator

#const Metabot = preload("res://metabot.gd")

var timer: Timer = null
var metabots = []
var tick_interval = 0.3

func initiate_timer():
	timer = Timer.new()
	timer.set_one_shot(false)
	timer.connect("timeout", self.tick)
	add_child(timer)
	# autostart
	timer.start(tick_interval)
	
func tick():
	print("TICK.")
	for mbot in metabots:
		mbot.tick()
	
# Called when the node enters the scene tree for the first time.
func _ready():
	print("_ready")
	initiate_timer()


func plant_potato(id: int):
	# Plant a potato
	var pool_water = Metabot.Pool.new("water", 0)
	pool_water.add(100)
	var pool_minerals = Metabot.Pool.new("minerals", 0)
	pool_minerals.add(100)

	var potato = Potato.new(id)

	potato.collector.add_source(pool_water)
	potato.collector.add_source(pool_minerals)

	metabots.append(potato)
	return potato
	
# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass


func _init():
	print("MetabotSimulator")
	
#	var mbot = Metabot.new()



