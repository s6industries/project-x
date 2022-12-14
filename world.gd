extends Node2D

@export var mylabel: Label



# Called when the node enters the scene tree for the first time.
func _ready():
	var test_class = TestClass.new()
	test_class.hello_world()
	print("HELLO WORLD - DEBUG")
	mylabel.text += "HELLO WORLD STRING"

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass
