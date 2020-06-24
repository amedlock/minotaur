extends Node

# This is the "view map" state

var player
var game

func _ready():
	player = get_parent()
	game = player.find_parent("Game")

	
func input(evt):
	if evt is InputEventKey and evt.pressed==false and evt.scancode==KEY_TAB:
		game.show_game()
		emit_signal("action_complete", self.name)

