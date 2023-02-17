extends CharacterBody2D

const ACCELERATION = 2000
const MAX_SPEED = 500
const FRICTION = 2000

enum { MOVE, ROLL, ATTACK }

var state = MOVE


# Called when the node enters the scene tree for the first time.
func _ready():
	velocity = Vector2.ZERO


func _physics_process(delta):
	match state:
		MOVE:
			move_state(delta)
		ROLL:
			pass
		ATTACK:
			pass


func move_state(delta):
	var input_vector = Vector2.ZERO
	input_vector.x = Input.get_action_strength(("ui_right")) - Input.get_action_strength(("ui_left"))
	input_vector.y = Input.get_action_strength(("ui_down")) - Input.get_action_strength(("ui_up"))
	input_vector = input_vector.normalized()
	
	print("INPUT:", input_vector)

	if input_vector != Vector2.ZERO:
		velocity = velocity.move_toward(input_vector * MAX_SPEED, ACCELERATION * delta)
	else:
		velocity = velocity.move_toward(Vector2.ZERO, FRICTION * delta)
	move_and_slide()
