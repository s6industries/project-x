#const Metabot = preload("res://metabot.gd")

class_name Potato extends Metabot
	
# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass


func _init(id: int):
	super._init(id)
	
	species = "potato"
	
	var recipe = Recipe.new()
	recipe.ingredients = {
		"water": 2,
		"minerals": 2
	}
	recipe.results = {
		"plantcell": 1
	}
	
	converter.conversion_recipes = [
		recipe
	]
	converter.converted = {
		"plantcell": 0
	}
	
	var blueprint = Blueprint.new()
	blueprint.materials = {
		"plantcell": 3
	}
	blueprint.results = {
		"root": 1
	}
	
	composer.blueprints = [
		blueprint
	]
	composer.bodypart_templates = {
		"root": {
			"name": "root",
			"mass": 1.0,
			"layer": "underground"
		}
	}
	
	# total body mass required to progress
	life_stage_progression = [
		3, 5, 10, 20
	]

# input water, minerals
# convert to body mass
# lifecycle stage advances with body mass at certain level
	
