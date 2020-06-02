extends Spatial;


var enemy_list ;
var item_list;
var dungeon ;

var rng = RandomNumberGenerator.new()


func _ready():
	dungeon = get_parent()
	enemy_list = dungeon.find_node("Enemies")
	item_list = dungeon.find_node("ItemList")


func randint( hi ):
	return rng.randi() % hi


class MazeCell:
	var x
	var y
	var north = null
	var east = null
	var gate = null
	var item = null 
	var enemy = null

	func _init(xp, yp):
		self.x = xp
		self.y = yp

	func adjacent_to(m2:MazeCell)->bool:
		if m2:
			if m2.x==x:
				return abs(m2.y-y)==1
			if m2.y==y:
				return abs(m2.x-x)==1
		return false
	
	func json() -> String:
		var obj= {'coord':"%s,%s" % [x, y] }
		if enemy:
			obj['enemy'] = enemy.json()
		if item:
			obj['item'] = item.json()
		for key in ['north', 'east', 'gate']:
			var val = self.get(key)
			if val!=null:
				obj[key] = val
		return to_json(obj)

var maze = []

func maze_cell(x, y) -> MazeCell:
	if x<0 or x>=dungeon.WIDTH:
		return null
	if y<0 or y>=dungeon.HEIGHT:
		return null
	return maze[x + (y * dungeon.WIDTH)]



func clear_outer_wall():
	for y in range(dungeon.HEIGHT):
		for x in range(dungeon.WIDTH):
			var mc = maze_cell(x,y)
			if x==0 or x==dungeon.WIDTH-1:
				mc.north = null
			if y==0 or y==dungeon.HEIGHT-1:
				mc.east= null


func add_outer_doors():
	maze_cell(3,0).north = "door"
	maze_cell(8,0).north  = "door"
	maze_cell(3,dungeon.HEIGHT-2).north = "door"
	maze_cell(8,dungeon.HEIGHT-2).north = "door" 
	maze_cell(0,3).east = "door"
	maze_cell(0,8).east ="door" 
	maze_cell(dungeon.WIDTH-2,3).east = "door" 
	maze_cell(dungeon.WIDTH-2,8).east  = "door" 



func add_path( from, to ):
	if from.x==to.x:
		if from.y==to.y-1:
			from.north = null
		else:
			to.north= null
	else:
		if from.x==to.x-1:
			from.east = null
		else:
			to.east = null


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


func update_frontier( mc :MazeCell, seen, frontier ):
	var n = maze_cell(mc.x,mc.y+1)
	var s = maze_cell(mc.x, mc.y-1)
	var e = maze_cell(mc.x+1, mc.y)
	var w = maze_cell(mc.x-1, mc.y)
	for mc in [n,s,e,w]:
		if not mc:
			continue
		if mc in seen:
			continue
		if not is_outer_maze(mc):
			frontier.append(mc)


func find_adjacent(mc : MazeCell, seen) -> MazeCell:
	var work = []
	for it in seen:
		if it.adjacent_to(mc):
			work.append(it)
	return choose_random(work)


func format_num(n, sz):
	var s = str(n)
	while len(s)<sz:
		s += ' '
	return s

func write_maze(name: String):
	var f = File.new()
	f.open(name, File.WRITE)
	for y in range(dungeon.WIDTH):
		f.store_string("\n%s " % format_num(y,3))
		for x in range(dungeon.HEIGHT):
			var mc = maze_cell(x,y)
			if mc.north:
				if mc.east:
					f.store_string("+ ")
				else:
					f.store_string("- ")
			elif mc.east:
				f.store_string("| ")
			else:
				f.store_string(". ")
	f.close()


func add_walls(mc : MazeCell):
	if mc.x==dungeon.WIDTH-1 or mc.y==dungeon.HEIGHT-1:
		return
	if mc.y==0:
		if mc.x > 0:
			mc.north = "wall"
	elif mc.x==0:
		if mc.y > 0:
			mc.east = "wall"
	else:
		mc.north = "wall"
		mc.east = "wall"

# cur	-> current active cell, picked from frontier
# seen 	-> already processed and in the maze
# frontier -> neighbors to all seen cells



func build_maze_prim(info):
	for mc in maze:
		add_walls(mc)
	clear_outer_wall()
	add_outer_doors()
	write_maze("maze1.txt")
	var prim_out = File.new(); 
	prim_out.open("prim.txt", File.WRITE)
	
	var start = maze_cell(info.start.x, info.start.y)
	var seen 	 =[start]		# seen cells are part of maze
	var frontier =[]		# these are potentials
	update_frontier(start, seen, frontier)
	while frontier.size()>0:
		var pick = take_random( frontier )
		var near = find_adjacent(pick, seen)
		var coords =[ pick.x, pick.y, near.x, near.y]
		prim_out.store_string("Path from %s,%s -> %s,%s\n" % coords)
		add_path( pick, near )
		seen.append( pick )
		update_frontier(pick, seen, frontier)
	prim_out.close()
	write_maze("maze2.txt")
	test_maze()	


	
func check_wall( x, y, dir ):
	if dir=="west": return check_wall(x-1, y, "east")
	if dir=="south": return check_wall( x, y-1, "north" )
	var c = maze_cell( x,y )
	if c==null: 
		return false
	if dir=="north": 
		return c.north == "wall"
	if dir=="east": 
		return c.east=="wall"
		
# prims algo makes maze too twisty, add some strategic doorways
func more_doors():
	if check_wall( 3,3, "east" ):  
		maze_cell( 3, 3 ).east  = "door" 
	if check_wall( 8,4, "north" ):  
		maze_cell( 8, 4 ).north = "door"
	if check_wall( 3,8, "north" ):  
		maze_cell( 3, 8 ).north = "door" 
	if check_wall( 8,9, "north" ):  
		maze_cell( 8, 9 ).north = "door"



func add_gates(info ):
	if info.depth > 2: return
	if info.used_gate==true: return
	var gates = []
	match info.m_type:
		"war": gates = ["magic", "both"]
		"magic": gates = ["war", "both"]
		"both": gates = ["magic", "war"]
	maze_cell(dungeon.WIDTH-1, 0).gate = gates[0]
	maze_cell(0, dungeon.HEIGHT-1).gate = gates[1]



func add_exit():
	var exit_loc = [Vector2(3,4), Vector2(7,4), Vector2(4,3), Vector2(4,7) ]
	var result : Vector2 = choose_random( exit_loc ) 
	maze_cell(result.x, result.y).item = item_list.find_item("ladder")


func add_enemies(info, coords):
	var num = randint(6) + 12
	var powers = []
	match info.depth:
		[1,2]: powers = [1]
		[3,4]: powers = [1,2]
		[5,6]: powers = [2,3]
		_: powers= [3]
	var kinds  = []
	if info.war_monsters:
		kinds.append("war")
	if info.magic_monsters:
		kinds.append("magic")
	if info.special_monsters:
		kinds.append("both")
	var allowed = enemy_list.find_enemies( kinds, powers, info.depth )
	for _n in range(num):
		if allowed.empty() or coords.empty(): return
		var c = take_random( coords )
		var mon = choose_random( allowed )
		maze_cell( c.x, c.y ).enemy = mon


func add_key( info, coords ):
	var powers ;
	match info.depth:
		1,2 : powers = [1]
		3,4 : powers = [1,2]
		5,6 : powers = [2,3]
		_: powers = [3]
	var keys = item_list.find_items("item",["key"], powers )
	assert( keys.empty()==false )
	var c = choose_random( coords )
	maze_cell(c.x, c.y).item = choose_random(keys)



func add_loot( num, info, coords ):
	var powers = [1]
	var names = [ "small_bag" ]
	if info.depth in [2,3,4]: 
		powers = [1,2]
		names.append( "bag" )
	elif info.depth in [4,5]: 
		names = [ "small_bag", "bag", "box" ]
		powers = [1,2,3]
	elif info.depth >= 6:
		names = [  "small_bag", "bag", "box", "pack", "chest" ]
		powers = [2,3]
	var bags = item_list.find_items( "container", names, powers )
	for _n in range(num):
		if coords.empty(): return
		var c = take_random( coords )
		maze_cell(c.x,c.y).item = choose_random( bags )


func add_money(num, info, coords):
	var restrict = { "coins":1, "necklace":1 ,"lamp":2, "horn":3, "chalice":4, "crown":6} 
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
	for _n in range(num):
		if coords.empty(): return
		var c = take_random( coords ) 
		maze_cell(c.x, c.y).item = choose_random( allowed )


func add_other(coords):
	var food = item_list.find_item( "food" )
	for _n in range(randint(5)):
		if coords.empty(): return
		var c = take_random( coords )
		maze_cell(c.x, c.y).item = food
	var quiver = item_list.find_item("quiver"); 
	for _n in range(1 + randint(3)):
		if coords.empty(): return
		var c = take_random( coords )
		maze_cell(c.x,c.y).item = quiver


func add_weapons( num, info, coords ):
	var powers = [1,2]
	if info.depth>2: powers.append( 3 )
	if info.depth>4: powers.append( 4 )
	if info.depth>6: powers.append( 5 )
	if info.depth>8: powers.append( 6 )
	var armor = item_list.find_items( "armor", null, powers )
	for _n in range( randi() % 3 ):
		var c = take_random( coords )
		maze_cell(c.x,c.y).item = take_random( armor )
	var allowed = item_list.find_items( "weapon", null, powers )		
	for _n in range(num):
		if coords.empty():  return
		var c = take_random( coords )
		maze_cell(c.x,c.y).item = choose_random( allowed )


func add_items(info, coords):
	var bags = 6 + randint(3)
	var money = 10 - bags
	var weapons = 7 + randint( 5 )
	add_loot( bags, info, coords )
	add_key( info, coords )
	add_money( money, info, coords )
	add_other( coords )	
	add_weapons( weapons, info, coords )


func set_mural_color( info ):
	dungeon.set_mural_color(info.m_type)


func add_minotaur(info, coords):
	if info.has_minotaur:
		var c= take_random( coords )
		maze_cell(c.x, c.y).enemy = enemy_list.minotaur


func add_cell_corner( cx, cy ):
	var c = maze_cell( cx, cy )
	if c==null: return
	var cw = maze_cell(cx-1,cy ) 
	var ne = false
	var se = false
	if c.north!=null: 
		ne = true
	if cw!=null and cw.north!=null: 
		ne = true
	if c.east!=null: 
		ne = true
		se = true
	if ne and cy>0: 
		dungeon.grid.add_corner( cx, cy, "ne" )
	if se and cx < dungeon.WIDTH-1: 
		dungeon.grid.add_corner( cx, cy, "se" )
		
func add_all_corners():
	for x in range(0,dungeon.WIDTH-1):
		for y in range(0,dungeon.HEIGHT-1):
			add_cell_corner( x, y )
	

# this actually builds the grid cells through the Grid node
func build_dungeon_grid():
	var grid = dungeon.grid
	for c in maze:
		if c.north:
			grid.set_wall(c, "north", c.north)
		if c.east:
			grid.set_wall(c, "east", c.east)
		if c.item:
			grid.set_item(c.x, c.y, c.item)
		if c.enemy:
			grid.set_enemy(c.x, c.y, c.enemy )
		if c.gate:
			grid.set_gate(c.x, c.y, c.gate)


func is_outer_maze(c):
	return c.x==0 or c.y==0 or c.x==dungeon.WIDTH-1 or c.y==dungeon.HEIGHT-1


func all_empty_cells():
	var coords =[]
	for xc in range(1,dungeon.WIDTH-1):
		for yc in range(1,dungeon.HEIGHT-1):
			var mc = maze_cell(xc, yc)
			if mc.item==null:
				coords.append( Vector2(xc,yc) )
	return coords;


func test_maze():
	var max_x = dungeon.WIDTH-1
	var max_y = dungeon.HEIGHT-1
	for x in range(1,max_x):
		assert(maze_cell(x,10).north)
		assert(maze_cell(x,0).north)
	for y in range(1,max_y):
		assert(maze_cell(10,y).east)
		assert(maze_cell(0,y).east)



func build_maze():
	var info = dungeon.current_level
	maze.resize(dungeon.WIDTH * dungeon.HEIGHT)
	for yp in range(dungeon.HEIGHT):
		for xp in range(dungeon.WIDTH):
			var n = xp + (yp * dungeon.WIDTH)
			maze[n] = MazeCell.new(xp, yp)
	rng.seed = info.seed_number
	var types = ["war", "magic", "both"]
	info.m_type = choose_random(types)
	build_maze_prim(info)
	more_doors()
	add_exit()
	add_gates(info)
	var empty_cells = all_empty_cells()	
	add_enemies(info, empty_cells)
	add_items( info, empty_cells)
	add_minotaur( info, empty_cells )
	build_dungeon_grid() # build the actual geometry
	set_mural_color(info)	
	var f = File.new()
	f.open("map_cell.json", File.WRITE)
	f.store_string("[")
	var first = true
	for m in maze:
		if first:
			first = false
		else:
			f.store_string(",\n")
		f.store_string( m.json() )
	f.store_string("]")
	f.close()
	maze.clear()
	
	
