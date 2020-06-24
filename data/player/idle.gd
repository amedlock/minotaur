extends Node

const Turn_Time = 0.5

var player

var turning = false


func _ready():
	player = self.get_parent()


func input(evt):
	if turning:
		return
	if not evt is InputEventKey:
		return
	if not evt.pressed:
		return
	match evt.scancode:
		KEY_F:
			player.attack_ahead()		
		KEY_R:
			player.rest()
		KEY_T:
			player.use_item()		
		KEY_W:
			player.move_forward()
		KEY_S:
			player.move_backward()
		KEY_A:
			turn_to(90)
		KEY_D:
			turn_to(-90)
		KEY_X:
			player.use_exit()
		KEY_SPACE:
			player.open_door()
		KEY_Q:
			player.glance.start(90)
		KEY_E:
			player.glance.start(-90)
		KEY_TAB:
			player.show_map()

var dir_start = 0
var turn_amount = 0
var time = 0


func turn_to( d : int ):
	time = 0
	dir_start = player.dir
	turn_amount = d
	turning = true

func process(delta):
	if turning:		
		time += delta
		if time >= Turn_Time:
			player.set_dir( dir_start + turn_amount )
			turning = false			
		else:
			var t = time / Turn_Time
			player.set_dir( dir_start + (turn_amount * t) )
