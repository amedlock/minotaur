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
	var coord = player.get_coord()
	var cell_ahead = player.cell_ahead()
	var cell = grid.get_cell(coord.x, coord.y)
	var info = [
		"Cell: %s" % str(cell.debug_info()),
		"World pos: %s" % [str(player.get_coord())],
		"Facing: %s %s" % [str(player.get_facing()), str(player.dir_name())],
		"Cell Ahead: %s" % str(cell_ahead.debug_info() if cell_ahead else "")
	]
	var item = player.item_at_feet()
	if item:
		info.append("Item: %s/%s pwr:%s" % [item.kind, item.name, item.power])
		
	var wall = player.wall_ahead()
	if wall:
		info.append("Wall: %s" % wall.name )
	self.text = join_str(info)


