extends "action.gd"


export(float) var glance_time = 0.5;

onready var player = get_parent()

var done = false

var start 
var delta 
var timer = 0.0

func start( amt ):
	start = player.dir
	delta = amt
	timer = 0.0;
	player.active_action= self 

func process(dt):
	if timer>=glance_time:
		player.set_dir(start+delta)
		return
	timer+= dt
	var ratio = timer / glance_time
	player.set_dir(start + (ratio*delta) )
	

func input(evt):
	if evt is InputEventKey and evt.pressed==false and evt.scancode in [KEY_Q, KEY_E]:
		player.set_dir( start )
		complete()


