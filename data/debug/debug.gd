extends Label

# debug output window

var player
var game
var dungeon
var grid


func _ready():
	player = find_parent("Player")
	game = player.find_parent("Game")
	dungeon = game.find_node("Dungeon")
	grid = dungeon.find_node("Grid")


func join_str( items: Array) -> String:
	var result = ""
	for n in range(len(items)):
		if n > 0:
			result += '\n'
		result += str(items[n])
	return result

func _process(_delta):
	var coord = Vector2( int(player.loc.x), int(player.loc.y))
	var ahead = player.coord_ahead()
	var loc = player.loc
	var info = [
		"Position: %s" % str(coord),
		"World pos: %s" % [str(player.translation)],
		"Cell: %s" % grid.get_cell(loc.x, loc.y).debug_info() ,
		"Facing: %s %s" % [str(player.dir), str(player.dir_name())],
		"Ahead: %s" % str(ahead)
	]
	var item = player.item_at_feet()
	if item:
		info.append("Item: %s/%s pwr:%s" % [item.kind, item.name, item.power])
		
	var wall = player.wall_ahead()
	if wall:
		info.append("Wall: %s" % wall.name )
	self.text = join_str(info)

