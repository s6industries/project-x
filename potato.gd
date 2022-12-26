#const Metabot = preload("res://metabot.gd")

class_name Potato extends Metabot
	
# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass


func _init():
	super._init()
	
	species = "potato"
	
	# total body mass required to progress
	life_stage_progression = [
		3, 5, 10
	]

# input water, minerals
# convert to body mass
# lifecycle stage advances with body mass at certain level
	
