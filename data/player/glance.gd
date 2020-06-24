extends Node

var glance_time = 0.4

var player

func _ready():
	player = self.get_parent()


var target = 0
var time = 0

func start(amt : int):
	target = amt
	time = 0
	player.player_state = "glance"


func input(evt):
	if evt is InputEventKey:
		if (not evt.pressed) and evt.scancode in [KEY_Q, KEY_E]:
			player.set_glance(0)
			player.player_state ="idle"

func process(delta):
	time += delta
	if time < glance_time:
		var ang = target * (time / glance_time)
		player.set_glance(ang)









