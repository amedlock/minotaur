extends Spatial;

enum GameMode { Menu, Game, Map, GameOver, GameWon };


var mode = GameMode.Menu

onready var dungeon = $Dungeon
var player : Spatial
onready var menu = $MainMenu
onready var map_view = $MapView
onready var help = $Help

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
	mode = GameMode.Game	
				
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
	if evt is InputEventKey:
		if evt.echo or (not evt.pressed): 
			return
		if evt.scancode==KEY_F1 and dungeon.visible:
			help.visible = !help.visible
		elif evt.scancode==KEY_F10:
			get_tree().quit()
	
		
		
