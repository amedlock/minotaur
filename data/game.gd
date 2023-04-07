extends Node3D;

enum GameMode { Menu, Dungeon, Map, GameOver, GameWon };


var mode = GameMode.Menu

@onready var dungeon = $Dungeon
@onready var player = $Dungeon/Player
@onready var menu = $MainMenu
@onready var map_view = $MapView
@onready var help = $Help

func _ready():
	show_menu()

func start_game( skill ):
	player = dungeon.player
	var seednum = randi() # seed num
	dungeon.init_maze( skill, seednum )
	player.init( skill )
	show_game()
	help.show()

func game_over():
	map_view.update()
	show_map()
	player.disable()	
	mode = GameMode.GameOver
			
func  show_game():
	dungeon.show()
	menu.disable()
	map_view.hide()
	mode = GameMode.Dungeon	
				
func  show_menu():
	mode = GameMode.Menu
	map_view.hide()
	dungeon.hide()
	menu.enable()	
	
func show_map():
	map_view.update()
	map_view.show()
	dungeon.disable()
	mode = GameMode.Map


func _input(evt):
	match mode:
		GameMode.Dungeon:
			if Input.is_action_just_pressed("view map"):
				show_map()
		GameMode.Map:
			if Input.is_action_just_released("view map"):
				show_game()
		_:
				pass
	if evt is InputEventKey:
		if evt.echo or (not evt.pressed): 
			return
		if evt.keycode==KEY_F1 and dungeon.visible:
			help.visible = !help.visible
		elif evt.keycode==KEY_F10:
			get_tree().quit()

