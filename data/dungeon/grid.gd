extends Spatial

var width
var height

var cells = []

var gates = []

var cell_prefab = preload("res://data/dungeon/cell.tscn")


var outer_wall 

var cell_size

var dungeon


func _ready():
	dungeon = get_parent()
	outer_wall = dungeon.find_node("outer_wall")


# Called when the node enters the scene tree for the first time.
func configure(w: int, h: int, cell_sz: float):
	self.width = w
	self.height = h
	self.cell_size = cell_sz
	var total = width * height
	cells.resize(total)
	var cell_file = File.new()
	cell_file.open("cell_file.txt", File.WRITE)
	cell_file.store_string("start = %s\n" % str(dungeon.translation))
	for yp in range(0,height):
		for xp in range(0,width):
			var index = xp + (yp * width)
			var node = cell_prefab.instance()
			self.add_child(node)
			node.configure(xp, yp)
			node.name = "Cell_%d_%d" % [xp, yp]
			node.translation = Vector3(xp * cell_sz, 0, -(yp * cell_sz))
			cells[index] = node
			cell_file.store_string("Cell %d,%d = %s\n" % [xp, yp, str(node.translation)])			
	cell_file.close()


func reset_all():
	for c in cells:
		c.reset()


func get_cell(x : int, y: int):
	if x<0 or x>=width:
		return null
	if y<0 or y>=height:
		return null
	return cells[x + (y * width)]


func get_wall(p : Vector2, dir: Vector2):
	if dir.x==-1: # west
		return get_wall( Vector2(p.x-1, p.y), -dir)
	elif dir.y==-1: # south
		return get_wall( Vector2(p.x, p.y-1), -dir)
	var cur = get_cell(p.x, p.y)
	if cur==null:
		return outer_wall
	var p2 = p + dir
	var cell = get_cell(p2.x, p2.y)
	if cell==null:
		return outer_wall
	else:
		if dir.x==1:
			return cur.east
		elif dir.y==1:
			return cur.north
	return null


func add_corner(x, y, which):
	var cell = get_cell(x,y)
	if cell:
		cell.add_corner(which)


func set_gate( x,y , kind):
	var cell = get_cell(x,y)
	if not cell:
		return
	cell.set_gate(kind)
	

func set_item(x, y, item):
	var cell = get_cell(x,y)
	if cell!=null:
		cell.set_item(item)


func set_enemy(x, y, enemy):
	var cell = get_cell(x,y)
	if x==6 and y==1:
		print("Stop")
	if cell:
		cell.set_enemy(enemy)



func set_wall(p, dir, kind):
	if dir=="west":
		return set_wall(Vector2(p.x-1, p.y), "east", kind)
	elif dir=="south":
		return set_wall(Vector2(p.x, p.y-1), "north", kind)
	var cell = get_cell(p.x,p.y)
	if cell:
		cell.set_wall(dir, kind)


func set_door(x, y, dir):
	if dir=="west":
		set_door(x-1, y, "east")
	elif dir=="south":
		set_door(x, y-1, "north")
	else:
		var cell = get_cell(x,y)
		if cell:
			cell.set_door(dir)

