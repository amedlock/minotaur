extends Spatial;


var WIDTH = 12;
var HEIGHT = 12;

var item_prefab = preload("res://data/items/item_prefab.tscn")
var enemy_prefab = preload("res://data/enemies/enemy_prefab.tscn")


var corner = preload("res://data/dungeon/wall_corner.scn")

enum GateType { Empty, Tan, Green, Blue }

var secret # secret doors

var maze_walls = null;

enum WallDir  { North, South, East, West }
enum WallPost  { NW, NE, SE, SW }
enum WallType { None, Wall, Door, SecretDoor, Gate };

class LevelInfo:
	var skill = 0
	var mazenumber = null  # 1..99
	var seed_number = null
	var next_seed = null
	var used_gate = false;
	var type = null
	var has_minotaur = null
	var start = null
	var exit = null
	var gate = GateType.Empty
	var depth = 1;


class Cell:
	var x = 0
	var y = 0
	var gate = 0
	var north = None
	var east = None
	var enemy = null ; # an enemy
	var item = null ; # an item
	
	func reset():
		self.item = null
		self.enemy = null
		self.gate = -1
		self.north = None
		self.east = None


var skill_level = 1
var level_info = {}  # for maze 1 , others built from this one 
var maze_number = 0;   # row by row, level by level ( 1..N mazes )

var total_levels = { 1: 3, 2:6, 3:10 }


func init_maze( skill, seednum ):
	skill_level = skill
	print("Skill=", skill_level )
	print("Seednum=", seednum )
	seed( seednum )
	for num in range(0, 100 ):
		make_dungeon_info( skill, num+1)
	maze_number = 1
	builder.build_maze()
	hud.update_stats()
	self.show()
	player.reset_location()
	player.map.update_map(current_depth())

func enable():
	self.show()
	
func disable():
	self.hide()	


func calc_maze_index( x, y ): return int(x) + (int(y) * WIDTH)

func next_maze_type(n):
	var cur = level_info[n]
	var nxt = level_info[n+1]
	if nxt.depth!=cur.depth: nxt = level_info[1]
	return nxt.type

func prev_maze_type(n):
	var prev = level_info[skill_level+1];
	if level_info.has( n-1 ):
		prev = level_info[ n-1 ];
	return prev.type

func use_exit(x,y):
	var i = level_info[ maze_number ]
	if i.exit.x!=x or i.exit.y!=y: return
	if skill_level==1:
		if maze_number==1: 
			maze_number = 3
		else: 
			maze_number+=1
	if skill_level==2:
		if maze_number in [1,2]: maze_number = 4
		elif maze_number==3: maze_number = 5
		elif maze_number in [4,5]: maze_number = 6
		else: maze_number+=1
	if skill_level==3:
		if maze_number in [1,2]: maze_number = 5
		elif maze_number in [2,3]: maze_number = 6
		elif maze_number in [5,6]: maze_number = 7
		else: maze_number+=1
	print("Level=", maze_number, "Depth=", current_depth() )
	audio.play("descend")
	builder.build_maze()
	player.map.update_map(current_depth())
	player.reset_location()

func check_for_gate(x, y):
	var info = current_level_info()

func randint( hi ):
	if hi < 1: return 0
	var result = randi() % hi
	assert( result < hi )
	return result;


func get_depth( skill, num ):
	if skill==1:
		if num in [1,2]: return 1
		else: return num - 1
	elif skill==2:
		if num<4: return 1
		elif num<6: return 2
		else: return num - 3
	elif skill==3:
		if num<5: return 1
		if num < 7: return 2
		return num - 4

func make_dungeon_info(skill, num):
	var result = LevelInfo.new()
	result.skill = skill
	result.mazenumber = num;
	result.type = [ Tan, Tan, Green, Blue ][ randint(4) ];
	result.has_minotaur = num >= total_levels[skill]
	result.start = Vector2(2 + randint( 4 ),2 + randint( 4 ) )
	if level_info.has( num-1 ):
		result.seed_number = level_info[num-1].next_seed
	else:
		result.seed_number = randi()
	seed( result.seed_number )
	result.next_seed = randi()
	result.depth = get_depth( skill, num )
	level_info[num] = result

func current_level_info():	return get_level_info( maze_number )

func current_depth():
	if level_info.has( maze_number ): return level_info[maze_number].depth
	else: return 1

func get_level_info(num):
	if level_info.has(num) : 
		return level_info[num]
	return level_info[ num % 99 ]


func load_next_level():
	current_level_info().used_gate = true
	maze_number += 1
	if skill_level==1 and maze_number>2: maze_number = 1
	if skill_level==2 and maze_number>3: maze_number = 1
	if skill_level==3 and maze_number>4: maze_number = 1
	builder.build_maze()

func load_prev_level():
	current_level_info().used_gate = true
	maze_number -= 1
	if maze_number==0:
		if skill_level==1 : maze_number = 2
		if skill_level==2 : maze_number = 3
	builder.build_maze()


var player ;
var builder ;
var hud ;
var audio
var enemy_list
var item_list

func _ready():
	randomize()
	enemy_list = find_node("Enemies")
	item_list = find_node("ItemList")
	builder = find_node("Builder")
	find_node("ceil").show()
	player = get_parent().find_node("Player")
	audio = player.find_node("Audio")
	hud = player.find_node("HUD")
	create_grid()
	


var grid = []
var walls = {}
var item_lookup = {}
var enemy_lookup = {}

func make_cell( x, y ):
	var c = Cell.new()
	c.x = x
	c.y = y
	grid[ calc_maze_index(c.x, c.y) ] = c;


func get_cell( cx, cy ):
	if cx>WIDTH-1 or cx<0: return null
	if cy>HEIGHT-1 or cy<0: return null
	var i = int(cx + (cy * WIDTH))	
	return grid[i]

func world_pos( cx, cy ):
	return maze_origin + Vector3( cx*3, 0, cy * 3 )

func set_cell_item( cx, cy, it ):
	var c = get_cell( cx, cy )
	if c==null: return;
	if c.item==it: return ;
	c.item==it
	if it!=null:
		add_item( cx, cy , it )
	else:
		remove_item( cx, cy )
	

func item_at_feet():
	var c = get_cell( player.loc.x, player.loc.y )
	if c==null: return null
	return c.item


func get_wall( cx, cy, dir ):
	if dir==South: return get_wall( cx, cy+1, North )
	if dir==West:  return get_wall( cx-1, cy, East );
	var c = get_cell( cx, cy )
	if dir==null or c==null: return null
	if dir==North: return c.north
	if dir==East: return c.east


func set_wall( cx, cy, dir, type ):
	if dir==South: return set_wall( cx, cy+1, North, type )
	if dir==West : return set_wall( cx-1, cy, East, type )
	var c = get_cell(cx, cy)
	if c==null or dir==null: 
		return null
	if dir==North: 
		c.north = type
	elif dir==East : 
		c.east = type




func create_grid():
	grid = []
	grid.resize( WIDTH * HEIGHT )
	maze_walls = find_node("maze_walls")
	for yp in range(0,HEIGHT):
		for xp in range(0,WIDTH):
			var c = Cell.new()
			c.x = xp
			c.y = yp
			grid[ calc_maze_index(c.x, c.y) ] = c



func find_enemy( x, y ):
	var index = calc_maze_index( x, y )
	if enemy_lookup.has( index ): 
		return enemy_lookup[index]
	return null


func get_wall_between( p1, p2 ):
	var c = Vector2( int(p1.x), int(p1.y) )
	var d = Vector2( int(p2.x), int(p2.y) )
	if not is_adjacent( c, d ):
		assert( false )
	if c.y==d.y:
		if c.x > d.x: return get_wall_node( d.x, d.y, East )
		else: return get_wall_node( c.x, c.y, East )
	else:
		if p1.y > p2.y: return get_wall_node( c.x, c.y, North );
		else: return get_wall_node( d.x, d.y, North );

# responsible for setting wall_ahead,outer_wall_XXXX for player object
func update_path_info(loc, dirvec):
	var ahead = loc + dirvec
	var behind = loc - dirvec
	var l_side = loc + Vector2( dirvec.y, -dirvec.x )
	var r_side = loc + Vector2( -dirvec.y, dirvec.x )
	player.wall_ahead = get_wall_between( loc, ahead )
	player.wall_behind = get_wall_between( loc, behind )
	player.wall_left = get_wall_between( loc, l_side )
	player.wall_right = get_wall_between( loc, r_side )
	player.outer_wall_ahead = get_cell( ahead.x, ahead.y )==null
	player.outer_wall_behind = get_cell( behind.x, behind.y )==null
	player.enemy_near[0] = find_enemy( ahead.x, ahead.y )
	player.enemy_near[2] = find_enemy( behind.x, behind.y )	
	player.enemy_near[1] = find_enemy( r_side.x, r_side.y )
	player.enemy_near[3] = find_enemy( l_side.x, l_side.y )


func get_wall_name( x, y, walldir ):
	var num = wall_num( walldir )
	return "cell_" + str(x) + "_" + str(y) + "_" + str(num)


func wall_num( dir ):
	if dir==North: return 1
	if dir==East: return 2
	if dir==South: return 3
	if dir==West: return 4
	return dir

func wall_index( x, y, dir ):
	return int( dir + (( x + (y * WIDTH * HEIGHT ) ) * 5 ))

func get_wall_node(cx, cy, dir):
	if dir==South: return get_wall_node( cx, cy+1, North )
	if dir==West: return get_wall_node( cx-1, cy, East )
	if get_cell( cx, cy )==null: return null
	var index = wall_index( cx, cy, dir )
	if not walls.has(index):
		return null
	return walls[index]
	#return maze_walls.find_node( get_wall_name( cx, cy, dir ) )
	
	
func wall_valid( cx, cy, num ):
	if num<0 or num>4:
		return false
	if cx<0 or cy<0 or cx>WIDTH-1 or cy>HEIGHT-1: 
		return false
	if cx==0 and num==4: return false
	if cx==WIDTH-1 and num==2: return false
	return true
	
	
var wall_post = {
	WallDir.North: WallPost.NW,
	WallDir.East: WallPost.NE,
	WallDir.South: WallPost.SE,
	WallDir.West: WallPost.SW

}	

var wall_post_offset = {
	WallPost.NW: Vector3( 0, 0.25, 0 ),
	WallPost.NE: Vector3( 3, 0.25, 0 ),
	WallPost.SE: Vector3( 3, 0.25, 3 ),
	WallPost.SW: Vector3( 0, 0.25, 3 )
}

var wall_angle = {
	North: 0,
	East: 270,
	South: 180,
	West: 90
}

const maze_origin = Vector3( -18, 0, -18 )

func wall_post_for_wall( cx, cy, walldir ):
	var which = wall_post[walldir]
	return  maze_origin + wall_post_offset[ which ] + Vector3( cx*3, 0, cy*3 )
	
	
func add_final( x, y ):
	set_cell_item( x, y, item_list.treasure )

func add_item( x, y, item ):
	if item==null:
		remove_item( x,y )
		return
	var index = calc_maze_index(x,y)
	var node = null
	if not item_lookup.has( index ):
		node = item_prefab.instance()
		item_list.add_child( node )
		item_lookup[index] = node
	else:
		node = item_lookup[index]
	node.set_region_rect( item.img )
	node.item = item
	node.set_translation( world_pos( x, y ) + Vector3( 1.5, 0.6, 1.5 ) )
	node.color = item.color
	if item.color!=null:
		node.set_modulate( item.color )
	else:
		node.set_modulate( Color( 0xFFFFFFFF ) )
	get_cell( x, y ).item = item
	

func remove_item( x, y ):
	var index = calc_maze_index(x,y)
	if not item_lookup.has( index ):
		return
	var node = item_lookup[index]
	item_list.remove_child( node );
	get_cell( x, y ).item = null
	item_lookup.erase( index )	
	


func add_enemy( x, y, enemy ):
	var cell = get_cell( x, y )
	if cell==null: return
	cell.enemy = enemy
	var index = calc_maze_index(x,y)
	var node
	if not enemy_lookup.has( index ):
		node = enemy_prefab.instance()
		enemy_lookup[index] = node
		enemy_list.add_child( node )
	else:
		node = enemy_lookup[ index ]
	node.set_region_rect( enemy.img )
	node.set_translation( world_pos( x, y ) + Vector3( 1.5, 0.9, 1.5 ) )
	node.set_modulate( enemy.color )
	node.health = enemy.health
	node.mind = enemy.mind
	node.monster = enemy
	node.x = x
	node.y = y
	
func remove_enemy( x, y ):
	var cell = get_cell( x,y  )
	if 	cell==null: return
	var index = int( (y * WIDTH) + x )
	if not enemy_lookup.has( index ):
		return
	var node = enemy_lookup[index]
	enemy_list.remove_child( node )
	enemy_lookup[index] = null
	

func add_corner( cx, cy, wallpost ):
	var offset =maze_origin + wall_post_offset[ wallpost ] + Vector3( cx*3, 0, cy*3 )
	var it = corner.instance();
	it.set_translation( offset )
	maze_walls.add_child( it )


var blue_gate = preload( "res://data/gate/blue_gate.scn")
var green_gate = preload( "res://data/gate/green_gate.scn")
var tan_gate = preload( "res://data/gate/green_gate.scn")

func add_gate_node( x, y, prefab ):
	var p = world_pos( x, y ) + Vector3(1.5,1.3,1.5 )
	var ent = prefab.instance()
	ent.set_translation( p )
	maze_walls.add_child( ent )
	



# adds a wall to the dungeon, num = which side
# sides are 1 = North, 2 = East, 3 = South, 4 = West
# num can only be 1 or 2 as cells only have walls on the north and east sides
# 3 and 4 are recursively called to the next cell south or west respectively
func add_wall(cx, cy, kind, walldir ): 
	assert( walldir in [North,South,East,West] )
	var post = wall_post_for_wall( cx, cy, walldir ) 	
	if not wall_valid( cx, cy, walldir ):
		return
	else:
		var w = kind.instance()
		var name = get_wall_name( cx, cy, walldir )
		w.set_name( name )				
		w.set_translation( post )
		w.set_rotation_deg( Vector3(0,  wall_angle[ walldir ], 0 ) )
		maze_walls.add_child( w )
		walls[ wall_index(cx, cy, walldir ) ] = w



func add_neighbors( items, frontier, seen ):
	for it in items:
		if not frontier.has(it):
			seen.append( it )
			frontier.append( it )

func is_adjacent( c1 , c2 ):
	if c1.x==c2.x:
		return (c1.y == c2.y+1) or (c1.y == c2.y-1)
	elif c1.y==c2.y:
		return (c1.x == c2.x+1) or (c1.x == c2.x-1)
	return false


func clear_maze():
	walls.clear()
	item_lookup.clear()
	enemy_lookup.clear()
	for ch in maze_walls.get_children():
		maze_walls.remove_child( ch )
	for ch in enemy_list.get_children():
		enemy_list.remove_child( ch )
	for ch in item_list.get_children():
		item_list.remove_child( ch )
	for cell in grid:
		cell.reset()



	