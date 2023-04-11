extends Node3D

# Handle player movement, this is called from player.gd

const MoveTime = 0.75
const TurnTime = 0.5
const GlanceTime = 0.25

var glance_amt = 0

# keep a local copy of dir
var dir = 270

# keep for retreat
var prev_coord


@onready var player = get_parent()
@onready var dungeon = player.get_parent()
@onready var hud = player.get_node("Camera3D/HUD")


func _ready():
	pass


func reset():
	self.glance_amt = 0



func move_back():
	var wall = player.wall_behind()
	if wall and wall.is_blocked():
		return
		
	var pos = player.position
	var pvec = player.transform.basis.z * -3
	player.player_state = player.PlayerState.MOVING
	await create_tween().tween_property(player, "position", pos - pvec, MoveTime).finished
	hud.update()
	player.player_state = player.PlayerState.IDLE



func move_forward():
	var wall = player.wall_ahead()
	if wall and wall.is_blocked():
		return
	prev_coord = player.get_coord()
	var pos = player.position
	var pvec = player.transform.basis.z * -3
	player.player_state = player.PlayerState.MOVING
	assert( player.position is Vector3)
	assert( pos is Vector3)
	assert( (pos+pvec) is Vector3)
	await create_tween().tween_property(player, "position", pos + pvec, MoveTime).finished
	hud.update()	
	player.player_state = player.PlayerState.IDLE


func turn_player(amt: int):
	dir += amt
	var crot = hud.compass.rotation_degrees
	var tween = create_tween().set_parallel();
	var rot = player.get_dir() + amt
	tween.tween_property(player, "rotation_degrees:y", rot, TurnTime )
	tween.tween_property(hud.compass, "rotation_degrees", crot - amt, TurnTime )
	player.player_state = player.PlayerState.TURNING
	await tween.finished
	player.rotation_degrees.y = wrapi(rot, 0, 360)
	player.player_state = player.PlayerState.IDLE


func glance(amt: int):
	var rot = player.get_dir() + amt
	self.glance_amt = amt
	player.player_state=player.PlayerState.GLANCING
	await create_tween().tween_property(player, "rotation_degrees:y", rot, GlanceTime ).finished
	player.rotation_degrees.y = wrapi(rot, 0, 360)	
	player.player_state=player.PlayerState.IDLE

func unglance():
	var rot = player.get_dir() - self.glance_amt
	var tween = create_tween()
	await tween.tween_property(player, "rotation_degrees:y", rot, GlanceTime ).finished
	self.glance_amt = 0
	player.rotation_degrees.y = wrapi(rot, 0, 360)	
	player.player_state=player.PlayerState.IDLE


func flee():
	if not prev_coord:
		await self.move_back()
	else:
		player.position = player.coord_to_world(prev_coord)
		prev_coord = null


func _input(_event):
	if glance_amt:
		if Input.is_action_just_released("look_left") or Input.is_action_just_released("look_right"):
			unglance()
		return


