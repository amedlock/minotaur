extends Node


var map_cell_prefab = preload("res://data/map/map_cell.tscn")
var gate_icon = preload("res://data/map/gate_icon.tscn")


var empty_tile  	= map_tile(3,0)
var arrow_tile 		= map_tile(3,2)
var tombstone_tile 	= map_tile(1,3)

onready var walls = $Walls
onready var other = $Other
onready var marker = $marker


var dungeon ;
var player 

var lookup = {}


func _ready():
	var game = get_parent()
	dungeon = game.find_node("Dungeon")
	player = dungeon.find_node("Player")
	for y in range( 12 ):
		for x in range( 12 ):
			var col = map_cell_prefab.instance()
			self.add_child(col)
			col.position = tile_position(x,y)
			col.region_rect = empty_tile
			col.z_index =  2 
			col.name = "map_%d_%d" % [x, y]
			lookup[ index(x,y) ] = col



func clear_all():
	for n in other.get_children():
		other.remove_child(n)
		n.queue_free()
	for n in walls.get_children():
		other.remove_child(n)
		n.queue_free()



# converts grid coord to map position
func tile_position( x, y ) -> Vector2:
	var xc = 39.5 + (x * 16)
	var yc = 215.5 - (y * 16)
	return Vector2(xc, yc)


func index( x, y ): 
	return x + (y * 100);


func update():
	var loc = player.loc
	marker.visible = true
	marker.position = tile_position(loc.x, loc.y)
	if player.dead():
		marker.modulate = Color.gray
		marker.region_rect = tombstone_tile
		marker.rotation_degrees = 0
	else:
		marker.modulate = Color.black
		marker.region_rect = arrow_tile
		marker.rotation_degrees = 360-player.dir


func  map_tile(x : int, y : int) -> Rect2:
	return Rect2(x * 16, y * 16, 16, 16)


var spr_lookup = {
	"none_none": empty_tile,
	"none_door": map_tile(2,0),
	"none_wall": map_tile(2,0),
	"wall_none": map_tile(0,0),
	"wall_wall": map_tile(1,0),
	"wall_door": map_tile(1,2),	
	"door_none": map_tile(0,1),
	"door_wall": map_tile(0,2),
	"door_door": map_tile(1,1)
}

func choose_tile(c):
	var n1 = "wall" if c.north else "none"
	var n2 = "wall" if c.east else "none"
	var path = "%s_%s" % [n1, n2]
	return spr_lookup[path]
	

func add_gate(gate, x, y):
	var icon = gate_icon.instance()
	icon.modulate = gate.sprite.modulate
	other.add_child(icon)
	icon.position = tile_position(x,y)
	


func fix_up_corner(cell):
	var n = dungeon.grid.get_cell(cell.x,cell.y+1)
	if not (n and n.east):
		return
	var e = dungeon.grid.get_cell(cell.x+1,cell.y)
	if not (e and e.north ):
		return
	var fix = gate_icon.instance()
	fix.name="fix_%d_%d" % [cell.x, cell.y]
	fix.region_rect = map_tile(2,2)
	fix.position = tile_position(cell.x,cell.y)
	other.add_child(fix)

	

#rebuilds the map layout, only called when level layout changes
func update_map(depth):
	clear_all()
	find_node("Label").set_text("Level: " + str(depth) )
	for y in range( dungeon.HEIGHT ):
		for x in range( dungeon.WIDTH ):
			var c = dungeon.grid.get_cell( x, y )
			var spr = lookup[ index( x, y ) ]
			spr.region_rect = choose_tile( c )
			if not(c.north or c.east):
				fix_up_corner(c)
			if c.gate:
				add_gate(c.gate, x, y)
