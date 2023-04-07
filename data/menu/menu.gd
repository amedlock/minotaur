extends Node2D

var game 

func _ready():
	game = get_parent()

func enable():
	show()
	set_process_input( true )
	
func disable():
	hide()
	set_process_input( false )


func _input(evt):
	if evt is InputEventKey:
		if evt.keycode==KEY_1:
			game.start_game( 1 )
		if evt.keycode==KEY_2:
			game.start_game( 2 )
		if evt.keycode==KEY_3:
			game.start_game( 3 )
		if evt.keycode==KEY_4:
			game.start_game( 4 )

