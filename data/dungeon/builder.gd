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
	
	var corners = []
	
	var active = false # maze building flag

	func _init(xp, yp):
		self.x = xp
		self.y = yp

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
	if info.war_monsters:
		if info.magic_monsters:
			gates = ["magic", "war"]
		else:
			gates = ["both", "magic"]
	else:
		gates = ["both", "war"]
	maze_cell(dungeon.WIDTH-1, 0).gate = gates[0]
	maze_cell(0, dungeon.HEIGHT-1).gate = gates[1]



func add_exit():
	var exit_loc = [Vector2(3,4), Vector2(7,4), Vector2(4,3), Vector2(4,7) ]
	var result : Vector2 = choose_random( exit_loc ) 
	maze_cell(result.x, result.y).item = item_list.find_item("ladder")


func add_enemies(info, coords):
	var num = randint(6) + 12
	var kinds  = []
	if info.war_monsters:
		kinds.append("war")
	if info.magic_monsters:
		kinds.append("magic")
	if info.special_monsters:
		kinds.append("both")
	var power = []
	match info.depth:
		1: power = [1,2]
		2: power = [1,2,3]
		3: power = [2,3,4]
		4: power = [3,4,5]
		5: power = [4,5,6]
		6: power = [5,6]
		7: power = [6,7]
		_: power = [6,7,8]
	var allowed = enemy_list.find_enemies( info, power )
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
	if info.war_monsters:
		if info.magic_monsters:
			dungeon.set_mural_color("both")
		else:
			dungeon.set_mural_color("war")
	else:
		dungeon.set_mural_color("magic")


func add_minotaur(info, coords):
	if info.has_minotaur:
		var c= take_random( coords )
		maze_cell(c.x, c.y).enemy = enemy_list.minotaur


func add_cell_corner( cx, cy ):
	var c = maze_cell( cx, cy )
	if c==null: 
		return
	var cw = maze_cell(cx-1,cy)
	var cs = maze_cell(cx, cy -1)
	var ne = false
	var se = false
	if c.north and not c.east:
		ne = true
	if c.north or (cw and cw.north):
		ne = true
	if cs:
		if cs.north or cs.east:
			se = true
	if c.east:
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
	maze.clear()
	maze.resize(dungeon.WIDTH * dungeon.HEIGHT)
	for yp in range(dungeon.HEIGHT):
		for xp in range(dungeon.WIDTH):
			var n = xp + (yp * dungeon.WIDTH)
			maze[n] = MazeCell.new(xp, yp)
	rng.seed = level_info.seed_number
	var monster_kind = rng.randi_range(0,100)
	level_info.war_monsters = monster_kind < 40 or monster_kind > 80
	level_info.magic_monsters = monster_kind >=40
	build_maze_prim()
	add_more_doors()
	add_exit()
	add_gates(level_info)
	var empty_cells = all_empty_cells()	
	add_enemies(level_info, empty_cells)
	add_items( level_info, empty_cells)
	add_minotaur( level_info, empty_cells )
	add_all_corners()
	build_dungeon_grid() # build the actual geometry
	set_mural_color(level_info)	
	maze.clear()
	
	
