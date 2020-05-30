extends Node

signal action_complete

export(float) var glance_time = 0.5;

onready var player = get_parent()

var done = false

var start_dir 
var delta 
var timer = 0.0


func start( amt ):
	start_dir = player.dir
	delta = amt
	timer = 0.0;
	player.active_action= self 

func process(dt):
	if timer>=glance_time:
		player.set_dir(start_dir+delta)
		return
	timer+= dt
	var ang = (timer / glance_time) * delta
	player.set_dir(start_dir +ang)
	
	

func input(evt):
	if evt is InputEventKey and evt.pressed==false and evt.scancode in [KEY_Q, KEY_E]:
		player.set_dir( start_dir )
		emit_signal("action_complete", self.name)


