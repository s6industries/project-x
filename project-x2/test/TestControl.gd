extends Control

# https://docs.godotengine.org/en/stable/tutorials/scripting/cross_language_scripting.html

var my_csharp_script = load("res://test/TestControl.cs")
var my_csharp_node = my_csharp_script.new()
# print(my_csharp_node.str2) # barbar

# Called when the node enters the scene tree for the first time.
func _ready():
	print("_ready() TestControl.gd")
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass
