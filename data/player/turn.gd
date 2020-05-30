extends Node

signal action_complete


onready var player = get_parent()

export(float) var turn_time = 0.75;

var alpha
var delta = 0
var timer = 0


func start( amt ):
	alpha = float(player.dir)
	delta = amt
	timer = 0
	player.active_action = self
	set_process(true)
	
	
func input(_evt):
	pass
	
	
func process( dt ):
	timer += dt
	if timer >= turn_time:
		player.set_dir( int(alpha+delta) )
		emit_signal("action_complete", self.name)
	else:
		var ratio = timer / turn_time
		player.set_dir( alpha + (delta * ratio) )

