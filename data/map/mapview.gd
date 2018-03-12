extends Node

var dungeon ;

var map_cell = preload("res://data/map/map_cell.tscn")


var empty = Rect2( 48, 0, 16, 16 )
var north_wall = Rect2( 0, 0, 16, 16 )
var wall_both = Rect2( 16, 0, 16, 16 )
var east_wall = Rect2( 32, 0, 16, 16 )


var north_door = Rect2( 0, 16, 16, 16 )
var both_door = Rect2( 16, 16, 16, 16 )
var east_door = Rect2( 32, 16, 16, 16 )

var door_wall = Rect2( 0, 32, 16, 16 )
var wall_door = Rect2( 16, 32, 16, 16 )

onready var rows = find_node("Rows")

var marker

var lookup = {}

func index( x, y ): return x + (y * 100);



func update_player( x,y, r ):
	var pos = Vector2(39, 41) + Vector2( x * 16, y * 16 )
	self.marker.position =  pos 
	self.marker.rotation_degrees = 360-r


func _ready():
	dungeon = get_parent().find_node("Dungeon")
	marker = find_node("marker")
	assert( marker!=null )
	for y in range( 12 ):
		var r = rows.find_node("Row" + str(y) )
		for x in range( 12 ):
			var spr = r.find_node( "Col" + str(x) )
			spr.z_index =  2 
			lookup[ index(x ,y ) ] = spr
			

func choose_walltype( c, wallnum, doornum ): 
	if c == dungeon.WallType.Wall: 
		return wallnum
	elif c==dungeon.WallType.Door:
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


func update_map(depth):
	find_node("Label").set_text("Level: " + str(depth) )
	for y in range( 12 ):
		for x in range( 12 ):
			var c = dungeon.get_cell( x, y )
			var spr = lookup[ index( x, y ) ]
			var n = choose_walltype( c.north, 1, 2 )
			var e = choose_walltype( c.east, 4, 8 )
			spr.set_region_rect(  cell_types[ n + e ] )
