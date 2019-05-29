extends Spatial;


enum WallDir  { North, South, East, West }
enum WallType { None, Wall, Door, SecretDoor, Gate };
enum WallPost  { NW, NE, SE, SW }

var wall = preload("res://data/dungeon/dungeon_wall.tscn")
var corner = preload("res://data/dungeon/wall_corner.tscn")
var arch = preload("res://data/door/dungeon_arch.tscn")
var door = preload("res://data/door/door_prefab.tscn")
var exit = preload("res://data/trapdoor/trapdoor.tscn")


var green_mtl = preload("res://data/dungeon/green_mat.tres")
var blue_mtl = preload("res://data/dungeon/blue_mat.tres")
var tan_mtl = preload("res://data/dungeon/tan_mat.tres")

var enemy_list ;
var item_list;
var dungeon ;

func _ready():
	dungeon = get_parent()
	enemy_list = dungeon.find_node("Enemies")
	item_list = dungeon.find_node("ItemList")

func get_cell( x, y ): return dungeon.get_cell(x,y)


func randint( hi ):
	var result = randi() % hi
	assert( result < hi )
	return result;



func build_outer_wall():
	for xp in range(1,dungeon.WIDTH-1):
		dungeon.set_wall( xp, 1, WallDir.North, WallType.Wall );
		dungeon.set_wall( xp, dungeon.HEIGHT-1, WallDir.North, WallType.Wall )
	for yp in range(1,dungeon.HEIGHT-1):
		dungeon.set_wall( 0, yp, WallDir.East, WallType.Wall )
		dungeon.set_wall( dungeon.WIDTH-2, yp, WallDir.East, WallType.Wall )


func add_starting_point():
	dungeon.set_wall( 3, 1, WallDir.North, WallType.Door );
	dungeon.set_wall( 8, 1, WallDir.North, WallType.Door );
	dungeon.set_wall( 3, dungeon.HEIGHT-1, WallDir.North, WallType.Door );
	dungeon.set_wall( 8, dungeon.HEIGHT-1, WallDir.North, WallType.Door );
	dungeon.set_wall( 0, 3, WallDir.East, WallType.Door );
	dungeon.set_wall( 0, 8, WallDir.East, WallType.Door );
	dungeon.set_wall( dungeon.WIDTH-2, 3, WallDir.East, WallType.Door );
	dungeon.set_wall( dungeon.WIDTH-2, 8, WallDir.East, WallType.Door );
	return Vector2(5, 1)


	
func fill_inner():
	for x in range(1,dungeon.WIDTH-1):
		for y in range(1,dungeon.HEIGHT-1):
			var c = dungeon.get_cell(x,y)
			if y > 1:
				c.north = WallType.Wall
			if x < dungeon.WIDTH-2:
				c.east = WallType.Wall


func get_neighbors( cx, cy ):
	var result = []
	if cx > 1: result.append( get_cell( cx-1, cy ) )
	if cx < dungeon.WIDTH-2: result.append( get_cell( cx+1, cy ) )
	if cy > 1: result.append( get_cell( cx, cy-1 ) )
	if cy < dungeon.HEIGHT-2: result.append( get_cell( cx, cy+1 ) )
	return result;	



func path_dir( from, to ):
	if from.x==to.x:
		if from.y < to.y : return WallDir.South
		if from.y > to.y : return WallDir.North
	elif from.y==to.y:
		if from.x < to.x: return WallDir.East
		elif from.x > to.x: return WallDir.West
	return WallDir.None

func add_path( from, to ):
	var dir = path_dir( from, to )
	var kind = WallType.None
	if randint(10) > 8 :
		kind = WallType.Door
	if dir==WallDir.East:
		from.east = kind
	elif dir==WallDir.West:
		to.east = kind
	elif dir==WallDir.North:
		from.north = kind
	elif dir==WallDir.South:
		to.north = kind
	else:
		assert(false)
	

func choose_random( items ):
	assert( items.size() > 0 )
	var pos = randint( items.size() )
	return items[pos]
	
func take_random( items ):
	assert( items.size() > 0 )
	var pos = randint( items.size() )
	var it = items[pos]
	items.remove( pos )
	return it


func is_adjacent( x, y, x2, y2 ):
	if x==x2:
		return (y == y2+1) or (y == y2-1)
	elif y==y2:
		return (x == x2+1) or (x == x2-1)
	return false


func find_adjacent( x, y, seen ):
	var work = []
	for it in seen:
		if is_adjacent( it.x, it.y, x, y ):
			work.append( it )
	var p = work[ randint( work.size() ) ]
	assert( is_adjacent( p.x, p.y, x, y ) )
	return p



func build_maze_prim(info):
	fill_inner()
	var seen = [get_cell(info.start.x, info.start.y)]
	var frontier = get_neighbors( info.start.x, info.start.y )
	while frontier.size()>0:
		var pick = take_random( frontier )
		var adj = find_adjacent( pick.x, pick.y, seen )
		add_path( pick, adj )
		seen.append(pick)
		for it in get_neighbors( pick.x, pick.y ):
			if frontier.has( it ) or seen.has(it):
				continue
			frontier.append( it )
	
func check_wall( x, y, dir ):
	if dir==WallDir.West: return check_wall(x-1, y, WallDir.East)
	if dir==WallDir.South: return check_wall( x, y+1, WallDir.North )
	var c = dungeon.get_cell( x,y )
	if c==null: return false
	if dir==WallDir.North: return c.north == WallType.Wall
	if dir==WallDir.East: return c.east==WallType.Wall
		
# prims algo makes maze too twisty, add some strategic doorways
func more_doors():
	if check_wall( 3,3, WallDir.East ):  dungeon.set_wall( 3, 3, WallDir.East, WallType.Door )
	if check_wall( 8,4, WallDir.North ):  dungeon.set_wall( 8, 4, WallDir.North, WallType.Door )
	if check_wall( 3,8, WallDir.North ):  dungeon.set_wall( 3, 8, WallDir.North, WallType.Door )
	if check_wall( 8,9, WallDir.North ):  dungeon.set_wall( 8, 9, WallDir.North, WallType.Door )
	
	

func add_gates(info):
	if info.depth > 1: return
	if info.used_gate==true: return
	get_cell( dungeon.WIDTH-1, 0 ).gate = dungeon.prev_maze_type( info.mazenumber )
	get_cell( 0, dungeon.HEIGHT-1 ).gate = dungeon.next_maze_type( info.mazenumber )
	
	
var exit_loc = [Vector2(3,4), Vector2(7,4), Vector2(4,3), Vector2(4,7) ]
	
func add_exit(info):
	var which = randint(4)
	info.exit = choose_random( exit_loc )

func add_enemies(info, coords):
	var num = randint(5) + 12
	var green = dungeon.GateType.Green
	var blue = dungeon.GateType.Blue
	var _tan = dungeon.GateType.Tan;
	var powers = []
	if info.depth in [1,2]: powers = [1]
	elif info.depth in [3,4]: powers = [1,2]
	elif info.depth in [5,6]: powers = [2,3]
	elif info.depth >= 7: powers = [3]
	var kinds  = []
	if info.type==_tan and (info.depth > 5): kinds.append("both")
	elif info.type==blue: kinds.append("magic")
	elif info.type==green: kinds.append( "war" )
	else: kinds = kinds + ["magic", "war" ]
	var allowed = enemy_list.find_enemies( kinds, powers, info.depth )
	for n in range(num):
		if allowed.empty() or coords.empty(): return
		var c = take_random( coords )
		var mon = choose_random( allowed )
		get_cell( c.x, c.y ).enemy = mon


func add_key( info, coords ):
	var powers = [1] if info.depth < 3 else [2]
	if info.depth>4 : powers = [2,3]
	var keys = item_list.find_items("item",["key"], powers )
	assert( keys.empty()==false )
	var c = choose_random( coords )
	dungeon.get_cell( c.x, c.y ).item = choose_random( keys )


func add_bags( num, info, coords ):
	var powers = [1]
	var names = [ "money_belt", "small_bag" ]
	if info.depth in [2,3,4]: 
		powers = [1,2]
		names.append( "bag" )
	elif info.depth in [4,5]: 
		names = [ "money_belt", "small_bag", "bag", "box" ]
		powers = [1,2,3]
	elif info.depth >= 6:
		names = [ "money_belt", "small_bag", "bag", "box", "pack", "chest" ]
		powers = [2,3]
	var bags = item_list.find_items( "bag", names, powers )
	for n in range(num):
		if coords.empty(): return
		var it = choose_random( bags )
		var c = take_random( coords )
		get_cell( c.x, c.y ).item = it 

func add_money(num, info, coords):
	var restrict = { "coins":1, "necklace":1 ,"ingot":2, "lamp":3, "chalice":4, "crown":6} 
	var names = []
	for mname in restrict:
		if info.depth >= restrict[mname]:
			names.append( mname )
	var powers = [1,2];
	if info.depth in [3,4]: powers = [2,3]
	elif info.depth in [5,6, 7]: powers = [3,4]
	elif info.depth > 7: powers = [4]
	var allowed = item_list.find_items("money", names, powers )
	if allowed.empty(): return  # shouldnt happen
	for n in range(num):
		if coords.empty(): return
		var it = choose_random( allowed )
		var c = take_random( coords ) 
		dungeon.get_cell( c.x, c.y ).item = it 


func add_other( info, coords):
	var flour = item_list.find_item( "flour" )
	for n in range(randint(5)):
		if coords.empty(): return
		var c = take_random( coords )
		dungeon.get_cell( c.x, c.y ).item = flour
	var quiver = item_list.find_item("quiver"); 
	for n in range(1 + randint(3)):
		if coords.empty(): return
		var c = take_random( coords )
		dungeon.get_cell( c.x, c.y ).item = quiver
		

func add_weapons( num, info, coords ):
	var powers = [1,2]
	if info.depth>2: powers.append( 3 )
	if info.depth>4: powers.append( 4 )
	if info.depth>6: powers.append( 5 )
	if info.depth>8: powers.append( 6 )
	var armor = item_list.find_items( "armor", null, powers )
	for n in range( randi() % 3 ):
		var c = take_random( coords )
		dungeon.set_cell_item( c.x, c.y, take_random( armor ) )
	var allowed = item_list.find_items( "weapon", null, powers )		
	for n in range(num):
		if coords.empty():  return
		var c = take_random( coords )
		get_cell( c.x, c.y ).item = choose_random( allowed )


func add_items(info, coords):
	var bags = 6 + randint(3)
	var money = 10 - bags
	var weapons = 7 + randint( 5 )
	add_bags( bags, info, coords )
	add_key( info, coords )
	add_money( money, info, coords )
	add_other( info, coords )	
	add_weapons( weapons, info, coords )


var mural_mat = { 1: tan_mtl, 2: green_mtl, 3: blue_mtl }

func add_murals( type ):
	var flr = dungeon.find_node("floor")
	var mat = mural_mat[type]
	for m in flr.get_children():
		if m.is_in_group("murals"):
			m.get_node("Mesh").set_surface_material( 0, mat )
			
				
func add_minotaur(info, coords):
	if info.depth < dungeon.total_levels[info.skill]:
		return
	var c= take_random( coords )
	dungeon.get_cell( c.x, c.y ).enemy = enemy_list.minotaur


func build_cell( info, cx, cy ):
	var c = get_cell( cx, cy )
	if c==null: 
		return
	if c.north==WallType.Wall: dungeon.add_wall( cx, cy, wall, WallDir.North )
	elif c.north==WallType.Door: dungeon.add_wall( cx, cy, door, WallDir.North )
	
	if c.east==WallType.Wall: dungeon.add_wall( cx, cy, wall, WallDir.East )
	elif c.east==WallType.Door: dungeon.add_wall( cx, cy, door, WallDir.East )
	
	if c.gate==dungeon.GateType.Green: dungeon.add_gate_node( cx, cy, dungeon.green_gate )
	if c.gate==dungeon.GateType.Blue: dungeon.add_gate_node( cx, cy, dungeon.blue_gate )
	if c.gate==dungeon.GateType.Tan: dungeon.add_gate_node( cx, cy, dungeon.tan_gate)
	if c.item!=null:
		dungeon.add_item( cx, cy, c.item )
	if c.enemy!=null:
		dungeon.add_enemy( cx, cy, c.enemy )
	if info.exit.x==cx and info.exit.y==cy: 
		var e = exit.instance()
		e.set_translation( dungeon.world_pos(cx, cy ) + Vector3( 1.5, 0.2, 1.5 ) )
		dungeon.maze_walls.add_child( e )

func add_cell_corner( cx, cy ):
	var c = get_cell( cx, cy )
	if c==null: return
	var cw = get_cell(cx-1,cy ) 
	var ne = false
	var se = false
	if c.north!=WallType.None: 
		ne = true
	if cw!=null and cw.north!=null: 
		ne = true
	if c.east!=WallType.None: 
		ne = true
		se = true
	if ne and cy>0: dungeon.add_corner( cx, cy, WallPost.NE )
	if se and cx < dungeon.WIDTH-1: dungeon.add_corner( cx, cy, WallPost.SE )
		
func add_all_corners():
	for x in range(0,dungeon.WIDTH-1):
		for y in range(0,dungeon.HEIGHT-1):
			add_cell_corner( x, y )
	

func make_empty_cells(info):
	var coords =[]
	var ex = int(info.exit.x)
	var ey = int(info.exit.y)
	for xc in range(1,dungeon.WIDTH-1):
		for yc in range(1,dungeon.HEIGHT-1):
			if not(ex==xc and ey==yc):
				coords.append( Vector2(xc,yc) )
	return coords;


func build_maze():
	var num = dungeon.maze_number
	assert( num > 0 )
	dungeon.clear_maze()
	var info = dungeon.level_info[num]
	seed( info.seed_number )
	build_outer_wall()	
	var coord = add_starting_point()
	build_maze_prim(info)
	more_doors()
	add_all_corners()
	add_exit(info)
	add_gates(info)
	var empty_cells = make_empty_cells(info)	
	add_enemies(info, empty_cells)
	add_items( info, empty_cells)
	add_minotaur( info, empty_cells )
	for cx in range(0,dungeon.WIDTH):
		for cy in range( 0, dungeon.HEIGHT ):
			build_cell( info, cx, cy )
	add_murals(info.type)			
