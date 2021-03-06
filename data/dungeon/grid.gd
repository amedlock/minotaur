extends Spatial

var width = 12
var height = 12

var cells = []

var gates = []

var cell_prefab = preload("res://data/dungeon/cell.tscn")


var outer_wall 

var cell_size

var dungeon


func _ready():
	dungeon = get_parent()
	outer_wall = dungeon.find_node("outer_wall")



func index(x,y) -> int:
	return x + (y * width)


# Called when the node enters the scene tree for the first time.
func configure(w: int, h: int, cell_sz: float):
	self.width = w
	self.height = h
	self.cell_size = cell_sz
	var total = width * height
	cells.resize(total)
	for yp in range(0,height):
		for xp in range(0,width):
			var index = xp + (yp * width)
			var node = cell_prefab.instance()
			self.add_child(node)
			node.configure(xp, yp)
			node.name = "Cell_%d_%d" % [xp, yp]
			node.translation = Vector3(xp * cell_sz, 0, -(yp * cell_sz))
			cells[index] = node


func reset_all():
	for c in cells:
		c.reset()


func get_cell(x : int, y: int):
	if x<0 or x>=width:
		return null
	if y<0 or y>=height:
		return null
	return cells[x + (y * width)]


func get_wall(p : Vector2, p2: Vector2):
	if p.x>p2.x: # west
		return get_wall(p2, p)
	elif p.y>p2.y: # south
		return get_wall(p2, p)
		
	var cur = get_cell(int(p.x), int(p.y))
	if cur==null:
		return outer_wall
	
	if p2.x > p.x:
		return cur.east
	elif p2.y > p.y:
		return cur.north
	return null


func set_corner(x, y, which):
	var cell = get_cell(x,y)
	if cell:
		cell.set_corner(which)


func clear_corner(x,y,which):
	var cell = get_cell(x,y)
	if cell:
		cell.clear_corner(which)


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

