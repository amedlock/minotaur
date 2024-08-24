extends Node3D;

@export var game_db : Node

var dungeon ;

var rng = RandomNumberGenerator.new()

var maze = []

func _ready():
	dungeon = get_parent()
	maze.resize(dungeon.WIDTH * dungeon.HEIGHT)
	for yp in range(dungeon.HEIGHT):
		for xp in range(dungeon.WIDTH):
			var n = xp + (yp * dungeon.WIDTH)
			maze[n] = MazeCell.new(xp, yp)


func randint( hi ):
	return rng.randi() % hi


class MazeCell:
	var x
	var y
	var north = null  # wall or door
	var east = null	  # wall or door
	var gate = null   # magic, war, both or null
	var item = null 
	var enemy = null
	
	var corners = []
	
	var active = false # maze building flag

	func _init(xp, yp):
		self.x = xp
		self.y = yp

	func reset():
		self.corners.clear()
		self.active = false
		self.enemy = null
		self.item = null
		self.gate = null
		self.north = null
		self.east = null

	func adjacent_to(m2:MazeCell) -> bool:
		if m2.x==x:
			return m2.y==y+1 or m2.y==y-1
		if m2.y==y:
			return m2.x==x+1 or m2.x==x-1
		return false
	
	func _to_string():
		return "MAZE:%s" % str(Vector2(x,y))
		
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
		return JSON.stringify(obj)


func valid(x,y) -> bool:
	return x>=0 and x<dungeon.WIDTH and y>=0 and y<dungeon.HEIGHT

func maze_cell(x, y) -> MazeCell:
	if not valid(x,y):
		return null
	else:
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
	from.active = true
	to.active = true
	if from.x==to.x-1:
		from.east = null
	elif from.x==to.x+1:
		to.east = null
	elif from.y==to.y-1:
		from.north = null
	elif from.y==to.y+1:
		to.north = null


func choose_random( items: Array ):
	if items.is_empty():
		return null
	return items[randint( items.size() )]
	
	
func take_random( items: Array ):
	if items.is_empty():
		return null
	var pos = randint( items.size() )
	return items.pop_at(pos)


# maze cells adjacent (NSEW) to mc
func all_adjacent(mc: MazeCell) -> Array:
	var result = []
	var n = maze_cell(mc.x,mc.y+1)
	if n:
		result.append(n)
	var s = maze_cell(mc.x,mc.y-1)
	if s:
		result.append(s)
	var e = maze_cell(mc.x+1,mc.y)
	if e:
		result.append(e)
	var w = maze_cell(mc.x-1,mc.y)
	if w:
		result.append(w)
	return result
	


func find_inactive_near( mc :MazeCell ):
	var result = []
	for x in all_adjacent(mc):
		if x.active or is_outer_maze(x):
			continue
		result.append(x)
	return result


func find_active_near(mc : MazeCell) -> MazeCell:
	var work = []
	for it in all_adjacent(mc):
		if it.active:
			work.append(it)
	return choose_random(work)


func format_num(n, sz):
	var s = str(n)
	while len(s)<sz:
		s += ' '
	return s


# add walls adjacent to outer corridor
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

func build_maze_prim():
	for mc in maze:
		mc.active = false
		add_walls(mc)
	add_outer_doors()
	
	var sx = rng.randi_range(1,5)
	var sy = rng.randi_range(1,5)
	
	var start = maze_cell(sx, sy)
	start.active = true # first active cell
		
	var frontier = find_inactive_near(start) # these are potentials
	while frontier.size()>0:
		var pick = take_random( frontier )
		var near = find_active_near(pick)
		add_path( near, pick )
		for f in find_inactive_near(pick):
			if not(f in frontier):
				frontier.append( f )

	
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
func add_more_doors():
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
	match info.level_type:
		"war": gates = ["magic", "both"]
		"magic": gates = ["war", "both"]
		_: gates = ["war", "magic"]
	maze_cell(dungeon.WIDTH-1, 0).gate = gates[0]
	maze_cell(0, dungeon.HEIGHT-1).gate = gates[1]



func add_exit(info):
	if info.depth>=99:
		return
	var exit_loc = [Vector2(3,4), Vector2(7,4), Vector2(4,3), Vector2(4,7) ]
	var result : Vector2 = choose_random( exit_loc ) 
	maze_cell(result.x, result.y).item = game_db.find_item("ladder")


func add_enemies(info, coords):
	var num = randint(6) + 12
	var allowed = game_db.find_enemies(info.level_type, info.depth)
	for _n in range(num):
		if allowed.is_empty() or coords.is_empty(): return
		var c = take_random( coords )
		var mon = take_random( allowed )
		maze_cell( c.x, c.y ).enemy = mon


func add_key( info, coords ):
	var keys = game_db.search_items("key", [], info.depth)
	if keys.is_empty():
		print_debug("Warning no 'key' items found for level ", info.depth)
	else:
		var c = choose_random( coords )
		maze_cell(c.x, c.y).item = choose_random(keys)



func add_loot( num, info, coords ):
	var names = [ "small_bag" ]
	if info.depth in [2,3,4]: 
		names.append( "bag" )
	elif info.depth in [4,5]: 
		names = [ "small_bag", "bag", "box" ]
	elif info.depth >= 6:
		names = [  "small_bag", "bag", "box", "pack", "chest" ]
	var bags = game_db.search_items( "container", names, info.depth )
	for _n in range(num):
		if coords.is_empty(): return
		var c = take_random( coords )
		maze_cell(c.x,c.y).item = choose_random( bags )


func add_money(num, info, coords):
	var allowed = game_db.search_items("money", [], info.depth )
	if allowed.is_empty(): 
		return  # no money, shouldnt happen :)
	for _n in range(num):
		if coords.is_empty(): return
		var c = take_random( coords ) 
		maze_cell(c.x, c.y).item = choose_random( allowed )


func add_other(coords):
	var food = game_db.find_item( "food" )
	for _n in range(1 + randint(3)):
		if coords.is_empty(): return
		var c = take_random( coords )
		maze_cell(c.x, c.y).item = food
	var quiver = game_db.find_item("quiver"); 
	for _n in range(1 + randint(3)):
		if coords.is_empty(): return
		var c = take_random( coords )
		maze_cell(c.x,c.y).item = quiver


func add_weapons( weap_count, armor_count, info, coords ):
	var armor = game_db.search_items( "armor", [], info.depth )
	for _n in range( armor_count ):
		var c = take_random( coords )
		maze_cell(c.x,c.y).item = take_random( armor )
	var allowed = game_db.search_items( "weapon", [], info.depth )		
	for _n in range(weap_count):
		if coords.is_empty():  return
		var c = take_random( coords )
		maze_cell(c.x,c.y).item = choose_random( allowed )
		
	var amulets = game_db.search_items("amulet", [], info.depth)
	for _n in range(6):
		var c = take_random(coords)
		maze_cell(c.x, c.y).item = choose_random(amulets)


func add_items(info, coords):
	var bags = 6 + randint(3)
	var money = 10 - bags
	var weapons = 7 + randint( 5 )
	var armor =  5 # randint(2)
	add_loot( bags, info, coords )
	add_key( info, coords )
	add_money( money, info, coords )
	add_other( coords )	
	add_weapons( weapons, armor, info, coords )


func set_mural_color( info ):
	dungeon.set_mural_color(info.level_type)


func add_minotaur(info, coords):
	if info.has_minotaur:
		var c= take_random( coords )
		maze_cell(c.x, c.y).enemy = game_db.find_enemy("minotaur")


func add_cell_corner( cx, cy ):
	var c = maze_cell( cx, cy )
	if c==null: 
		return
	var cw = maze_cell(cx-1,cy)
	var cs = maze_cell(cx, cy -1)
	var ne = false
	var se = false
	if c.north != null:
		if c.east!=null:
			ne = true
		elif cw!=null and cw.east!=null:
			ne = true
	if cs:
		if cs.north!=null or cs.east!=null:
			se = true
	if c.east!=null:
		ne = true
		se = true
	if ne and cy>0:
		c.corners.append("ne")
	if se and cx< dungeon.WIDTH-1:
		c.corners.append("se")


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
		for post in c.corners:
			if post:
				grid.set_corner(c.x, c.y, post)
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



func build_maze(level_info):
	for cell in maze:
		cell.reset()
	rng.seed = level_info.seed_number
	build_maze_prim()
	add_more_doors()
	add_exit(level_info)
	add_gates(level_info)
	var empty_cells = all_empty_cells()
	
	add_enemies(level_info, empty_cells)
	add_items( level_info, empty_cells)
	add_minotaur( level_info, empty_cells )
	add_all_corners()
	build_dungeon_grid() # build the actual geometry
	set_mural_color(level_info)	
