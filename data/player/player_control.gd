extends Spatial

# Handle player movement, this is called from player.gd


# time to move one square forward or back
const MoveTime = 0.75

const GlanceTime = 0.35

var glance_amt = 0

# keep a local copy of dir
var dir = 270

# keep for retreat
var prev_coord


onready var player = get_parent()
onready var dungeon = player.get_parent()
onready var hud = player.get_node("Camera/HUD")

var tween : Tween

func _ready():
	tween = Tween.new()
	tween.name = "MovementTween"
	tween.connect("tween_completed", self, "tween_complete")
	self.add_child(tween)
	tween.set_active(true)


func reset():
	self.dir = 270


func tween_complete(_obj, _path):
	player.player_state = player.PlayerState.IDLE
	hud.update()
	dir = fposmod(dir, 360)
	player.rotation_degrees.y = dir
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
	prev_coord = player.get_coord()
	var pos = player.translation
	var pvec = player.transform.basis.z * -3
	tween.interpolate_property(player, "translation", pos, pos + pvec, MoveTime)
	tween.start()
	player.player_state = player.PlayerState.MOVING


func turn_player(amt: int):
	dir += amt
	var rot = player.get_dir()
	tween.interpolate_property(player, "rotation_degrees:y", rot, rot + amt, 1.0 )
	var crot = hud.compass.rotation_degrees
	tween.interpolate_property(hud.compass, "rotation_degrees", crot, crot - amt, 1.0 )
	tween.start()
	player.player_state = player.PlayerState.TURNING


func glance(amt: int):
	var rot = player.get_dir()
	glance_amt = amt
	dir += amt
	tween.interpolate_property(player, "rotation_degrees:y", rot, rot + amt, GlanceTime )
	tween.start()
	player.player_state=player.PlayerState.GLANCING


func unglance():
	var rot = player.get_dir()
	dir -= glance_amt
	tween.interpolate_property(player, "rotation_degrees:y", rot, rot - glance_amt, GlanceTime )
	glance_amt = 0
	tween.start()


func flee():
	if not prev_coord:
		self.move_back()
	else:
		player.translation = player.coord_to_world(prev_coord)
		prev_coord = null


func _input(_event):
	if tween.is_active():
		return
		
	if glance_amt:
		if Input.is_action_just_released("look_left") or Input.is_action_just_released("look_right"):
			unglance()
		return


