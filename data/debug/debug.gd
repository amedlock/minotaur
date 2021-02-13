extends Label

# debug output window

var player
var game
var dungeon
var grid

var enabled = false

func _ready():
	player = find_parent("Player")
	game = player.find_parent("Game")
	dungeon = game.find_node("Dungeon")
	grid = dungeon.find_node("Grid")
	visible = enabled


func _input(event):
	if event is InputEventKey and event.pressed:
		if event.scancode==KEY_F2:
			enabled = !enabled
			visible = enabled
		


func join_str( items: Array) -> String:
	var result = ""
	for n in range(len(items)):
		if n > 0:
			result += '\n'
		result += str(items[n])
	return result

func _process(_delta):
	if not enabled:
		return
	var coord = Vector2( int(player.loc.x), int(player.loc.y))
	var loc = player.loc
	var cell_ahead = player.cell_ahead()
	var cell = grid.get_cell(loc.x, loc.y)
	var info = [
		"Position: %s, state: %s" % [ str(coord), player.player_state ],
		"Cell: %s" % str(cell.debug_info()),
		"World pos: %s" % [str(player.translation)],
		"Facing: %s %s" % [str(player.dir), str(player.dir_name())],
		"Cell Ahead: %s" % str(cell_ahead.debug_info() if cell_ahead else "")
	]
	var item = player.item_at_feet()
	if item:
		info.append("Item: %s/%s pwr:%s" % [item.kind, item.name, item.power])
		
	var wall = player.wall_ahead()
	if wall:
		info.append("Wall: %s" % wall.name )
	self.text = join_str(info)

