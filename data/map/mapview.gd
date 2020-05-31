extends Node


var map_cell = preload("res://data/map/map_cell.tscn")
var gate_icon = preload("res://data/map/gate_icon.tscn")


func  map_tile(x : int, y : int) -> Rect2:
	return Rect2(x * 16, y * 16, 16, 16)


var empty = map_tile(3,0)
var north_wall = map_tile(0,0)
var wall_both = map_tile(1,0)
var east_wall = map_tile(2,0)


var north_door = map_tile(0,1)
var both_door = map_tile(1,1)
var east_door = map_tile(2,1)

var door_wall = map_tile(0,2)
var wall_door = map_tile(1,2)


var arrow_tile = map_tile(3,2)
var tombstone_tile = map_tile(1,3)

onready var rows = $Rows
onready var gates = $Gates
onready var marker = $marker


var dungeon ;
var player 

var lookup = {}

const blue_color = Color("#00369d")
const grey_color = Color("#909090")


func _ready():
	var game = get_parent()
	dungeon = game.find_node("Dungeon")
	player = dungeon.find_node("Player")
	for y in range( 12 ):
		var r = rows.find_node("Row" + str(y) )
		for x in range( 12 ):
			var spr = r.find_node( "Col" + str(x) )
			spr.z_index =  2 
			lookup[ index(x ,y ) ] = spr



# converts grid coord to map position
func tile_position( x, y ) -> Vector2:
	return Vector2(39, 217) + Vector2( x * 16, -(y * 16) )

func index( x, y ): 
	return x + (y * 100);


func update():
	var loc = player.loc
	marker.position = tile_position(loc.x, loc.y)
	if player.dead():
		marker.modulate = grey_color
		marker.region_rect = tombstone_tile
		marker.rotation_degrees = 0
	else:
		marker.modulate = blue_color
		marker.region_rect = arrow_tile
		marker.rotation_degrees = 360-player.dir


func choose_walltype( c, wallnum, doornum ): 
	if c==null:
		return 0
	elif c.name =="wall": 
		return wallnum
	elif c.name =="door":
		return doornum
	else:
		return 0


var cell_types = {
	0 : empty,
	1 : north_wall,
	2 : north_door,
	4 : east_wall,
	5 : wall_both,
	6 : door_wall,
	8 : east_door,
	9 : wall_door,
	10 : both_door
}

func add_gate(gate, x, y):
	var icon = gate_icon.instance()
	icon.modulate = gate.sprite.modulate
	gates.add_child(icon)
	icon.position = tile_position(x,y)
	


func update_map(depth):
	for n in gates.get_children():
		gates.remove_child(n)
		n.queue_free()
	find_node("Label").set_text("Level: " + str(depth) )
	for y in range( 12 ):
		for x in range( 12 ):
			var c = dungeon.grid.get_cell( x, y )
			var spr = lookup[ index( x, y ) ]
			var n = choose_walltype( c.north, 1, 2 )
			var e = choose_walltype( c.east, 4, 8 )
			spr.set_region_rect(  cell_types[ n + e ] )
			if c.gate:
				add_gate(c.gate, x, y)
