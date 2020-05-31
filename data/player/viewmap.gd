extends Node

signal action_complete

var player
var game

func _ready():
	player = get_parent()
	game = player.find_parent("Game")

func start():
	game.show_map()
	player.active_action = self

	
func input(evt):
	if evt is InputEventKey and evt.pressed==false and evt.scancode==KEY_TAB:
		game.show_game()
		emit_signal("action_complete", self.name)

func process(_delta):
	pass
