extends Node

class_name MetabotSimulator

# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass


func _init():
	print("MetabotSimulator")
	
	var mbot = Metabot.new()

# inner classes

class Pool:
	var count = 0

class mComponent:
	var type = null
	var connections = []
	
	func _init():
		print("mComponent")

class mSensor extends mComponent:
	var sensing = true
	func _init():
		print("mSensor")
	
class mCollector extends mComponent:
	var objects_collected = []
	
	func _init():
		print("mCollector")

class mDecomposer extends mComponent:
	var objects_input = []
	var objects_processing = []
	var objects_output = []
	
	func _init():
		print("mDecomposer")

class mStorer extends mComponent:
	var stores = []
	
	func _init():
		print("mStorer")

class mConverter extends  mComponent:
	var objects_input = []
	var resources_output = []
	
	func _init():
		print("mConverter")

class mComposer extends mComponent:
	var objects_input = []
	var objects_processing = []
	var objects_output = []
	
	func _init():
		print("mComposer")

class Metabot extends  Node:
	var components = []
	var sensor = mSensor.new()
	var collector = mCollector.new()
	var decomposer = mDecomposer.new()
	var storer = mStorer.new()
	var converter = mConverter.new()
	var composer = mComposer.new()
	
	func _init():
		print("Metabot")
		
		

# sensor
#agent 
#actuator
#motor
#collector
#decomposer
#storer
#converter
#composer
#
#body
