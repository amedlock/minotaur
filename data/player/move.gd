extends "action.gd"

onready var player = get_parent()

var timer = null
var start = null
var dir = null

export(float)  var move_time = 0.5 ;
	
func forward( loc, vdir ):
	if player.wall_ahead and player.wall_ahead.is_blocked():
		return
	if player.outer_wall_ahead or player.enemy_near[0]!=null:
		return
	timer = 0
	start = loc
	dir = vdir
	player.mark_last()
	player.active_action = self
	
func backward( loc, vdir ):
	if player.wall_behind and player.wall_behind.is_blocked():
		return
	if player.outer_wall_behind:
		return
	timer = 0
	start = loc
	dir = -vdir
	player.mark_last()
	player.active_action = self	
	
func process(dt):
	timer  += dt
	if timer>= move_time:
		var np = start + dir
		player.set_pos( np.x, np.y )
		complete()
	else:
		var ratio = timer / move_time;
		var loc = start + (ratio * dir)
		player.set_pos( loc.x, loc.y )		

