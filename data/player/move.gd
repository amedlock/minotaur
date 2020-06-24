extends Node

var Move_Time = 0.6

var player

func _ready():
	self.player = get_parent()

var cell : Spatial
var v_start : Vector2
var v_end : Vector2
var time

func start(target):
	cell = target
	time = 0
	v_start = player.loc
	v_end = cell.grid_pos()
	player.player_state = "move"

func input(_evt):
	pass


func process(delta):
	time += delta
	if time>=Move_Time:
		player.set_pos( v_end )
		player.player_state ="idle"
		cell.on_enter(self)
		player.hud.update()
		player.check_for_monster()
		player.reduce_potion_turn()
	else:
		var t = (time / Move_Time)
		player.set_pos( v_start.linear_interpolate(v_end, t) )
