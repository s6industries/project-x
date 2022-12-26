extends Metabot


# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass

func _init():
	super._init()
	
	species = "android"
	
	var recipe = Recipe.new()
	recipe.ingredients = {
		"water": 2,
		"potato": 3
	}
	recipe.results = {
		"droidcell": 1
	}
	
	converter.conversion_recipes = [
		recipe
	]
	converter.converted = {
		"droidcell": 0
	}
	
	var blueprint = Blueprint.new()
	blueprint.materials = {
		"droidcell": 3
	}
	blueprint.results = {
		"arm": 2
	}
	
	composer.blueprints = [
		blueprint
	]
	composer.bodypart_templates = {
		"arm": {
			"name": "arm",
			"mass": 2.0,
			"layer": "ground"
		}
	}
	
	# total body mass required to progress
	life_stage_progression = [
		8, 24, 40
	]
