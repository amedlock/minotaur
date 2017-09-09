extends "action.gd"


onready var player = get_parent()

export(float) var turn_time = 0.75;

var start
var delta
var timer

func start( cur, amt ):
	start = float(cur)
	delta = amt
	timer = 0
	player.active_action = self
	
func input( evt ): pass
	
func process( dt ):
	timer += dt	
	if timer >= turn_time:
		player.set_dir( int(start+delta) )
		complete()
	else:
		var ratio = timer / turn_time
		player.set_dir( start + (delta * ratio) )
