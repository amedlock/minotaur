extends Spatial;

enum GameMode { Menu, Game, Map, GameOver, GameWon };


var mode = Menu

var dungeon = null
var player = null
var menu = null
var map = null
var help = null;

func _ready():
	help = find_node("Help")
	dungeon = find_node("Dungeon")
	player = find_node("Player")
	menu = find_node("MainMenu")
	map = find_node("Map")
	set_process_input( true )
	show_menu()

func start_game( skill ):
	var sn = randi() # seed num
	dungeon.init_maze( skill, sn )
	player.init( skill )
	map.update()
	show_game()
	help.show()

func game_over():
	show_map()
	player.disable()	
	mode = GameOver
			
func  show_game():
	player.enable()
	dungeon.show()
	menu.disable()
	map.hide()
	mode = Game	
				
func  show_menu():
	mode = Menu
	map.hide()
	dungeon.hide()
	player.disable()
	menu.enable()	
	
func show_map():
	map.show()
	dungeon.disable()
	#player.disable()
	mode = Map

			
func _input(evt):
	if evt.type!=InputEvent.KEY : return
	if evt.is_echo() or not evt.pressed: return
	if Input.is_key_pressed(KEY_F1) and dungeon.is_visible():
		help.toggle()
	if Input.is_key_pressed(KEY_F10):
		get_tree().quit()
	
		
		
