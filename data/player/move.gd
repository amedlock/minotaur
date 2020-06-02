extends Node

signal action_complete

onready var player = get_parent()

var timer = null
var start = null
var dir = null

export(float)  var move_time = 0.5 ;


func forward():
	var wall = player.wall_ahead()
	if wall and wall.is_blocked():
		return
	var cell = player.cell_ahead()
	if not cell:
		return
	if cell.enemy:
		player.combat.fight(cell.enemy, 0)
		return
	timer = 0
	start = player.loc
	dir = player.face_vector()
	player.mark_last()
	player.active_action = self
	
func backward():
	var wall = player.wall_behind()
	if wall and wall.is_blocked():
		return
	var cell = player.cell_behind()
	if not cell:
		return
	if cell.enemy:
		player.combat.fight(cell.enemy, 180)
		return
	timer = 0
	start = player.loc
	dir = -player.face_vector()
	player.mark_last()
	player.active_action = self	
	


func input(_evt):
	pass	



func process(dt):
	timer += dt
	if timer>= move_time:
		var np = start + dir
		player.set_pos( np.x, np.y )
		emit_signal("action_complete", self.name)
	else:
		var ratio = timer / move_time;
		var loc = start + (ratio * dir)
		player.set_pos( loc.x, loc.y )		

