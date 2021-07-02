extends Spatial

# Handle player movement and input except Combat

# time to move one square forward or back
const MoveTime = 0.75

const GlanceTime = 0.35


onready var player = get_parent()
onready var dungeon = player.get_parent()
onready var hud = player.get_node("Camera/HUD")

var enabled = false;
var tween : Tween

func _ready():
	tween = Tween.new()
	tween.name = "MovementTween"
	self.add_child(tween)
	tween.set_active(true)
	tween.connect("tween_completed", self, "tween_complete")


func enable():
	self.enabled = true

func disable():
	self.enabled = false


func tween_complete(_obj, _path):
	hud.update()
	# if new_cell ? current_cell.enter() 
	# check for monsters


func move_back():
	var wall = player.wall_behind()
	if wall and wall.is_blocked():
		return
		
	var pos = player.translation
	var pvec = player.transform.basis.z * -3 
	tween.interpolate_property(player, "translation", pos, pos - pvec, MoveTime)
	tween.start()


func move_forward():
	var wall = player.wall_ahead()
	if wall and wall.is_blocked():
		return

	var pos = player.translation
	var pvec = player.transform.basis.z * -3
	tween.interpolate_property(player, "translation", pos, pos + pvec, MoveTime)
	tween.start()


func turn_player(amt: float ):
	var rot = player.rotation_degrees
	tween.interpolate_property(player, "rotation_degrees", rot, rot + Vector3(0,amt,0), 1.0 )
	var crot = hud.compass.rotation_degrees
	tween.interpolate_property(hud.compass, "rotation_degrees", crot, crot - amt, 1.0 )
	tween.start()


var glance_amt = 0

func glance(amt: float):
	var rot = player.get_dir()
	glance_amt = amt
	tween.interpolate_property(player, "rotation_degrees:y", rot, rot + amt, GlanceTime )
	tween.start()


func unglance():
	var rot = player.get_dir()
	tween.interpolate_property(player, "rotation_degrees:y", rot, rot - glance_amt, GlanceTime )
	glance_amt = 0
	tween.start()


func _input(_event):
	if tween.is_active():
		return
		
	if glance_amt:
		if Input.is_action_just_released("look_left") or Input.is_action_just_released("look_right"):
			unglance()
		return
		
	if Input.is_action_just_pressed("forward"):
		self.move_forward()
	elif Input.is_action_just_pressed("back"):
		self.move_back()
	elif Input.is_action_just_pressed("left"):
		self.turn_player(90)
	elif Input.is_action_just_pressed("right"):
		self.turn_player(-90)
	elif Input.is_action_just_pressed("look_left"):
		glance(90)
	elif Input.is_action_just_pressed("look_right"):
		glance(-90)
	elif Input.is_action_just_pressed("attack"):
		player.attack_ahead()
	elif Input.is_action_just_pressed("descend"):
		player.use_exit()
	elif Input.is_action_just_pressed("rest"):
		player.rest()	
	elif Input.is_action_just_pressed("open"):
		player.open_door()


