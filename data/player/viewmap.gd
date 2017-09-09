extends "action.gd"


onready var player = get_parent()
var game

func _ready():
	game = player.get_parent()

func start():
	game.show_map()
	player.active_action = self

	
func input(evt):
	if evt.type==InputEvent.KEY and evt.pressed==false and evt.scancode==KEY_TAB:
		game.show_game()
		complete()
