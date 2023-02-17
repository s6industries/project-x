extends CharacterBody2D


const SPEED = 300.0
const JUMP_VELOCITY = -400.0

## Get the gravity from the project settings to be synced with RigidBody nodes.
#var gravity = ProjectSettings.get_setting("physics/2d/default_gravity")
#
#
#func _physics_process(delta):
#	# Add the gravity.
#	if not is_on_floor():
#		velocity.y += gravity * delta
#
#	# Handle Jump.
#	if Input.is_action_just_pressed("ui_accept") and is_on_floor():
#		velocity.y = JUMP_VELOCITY
#
#	# Get the input direction and handle the movement/deceleration.
#	# As good practice, you should replace UI actions with custom gameplay actions.
#	var direction = Input.get_axis("ui_left", "ui_right")
#	if direction:
#		velocity.x = direction * SPEED
#	else:
#		velocity.x = move_toward(velocity.x, 0, SPEED)
#	move_and_slide()


# Player

const ACCELERATION = 500
const MAX_SPEED = 800
const FRICTION = 200

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
	velocity = input_vector.normalized() * MAX_SPEED
	print("INPUT:", input_vector)

	if input_vector != Vector2.ZERO:
		velocity = velocity.move_toward(input_vector * MAX_SPEED, ACCELERATION * delta)
	else:
		velocity = velocity.move_toward(Vector2.ZERO, FRICTION * delta)
	move_and_slide()
