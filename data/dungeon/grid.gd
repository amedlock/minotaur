extends Spatial

var width = 12
var height = 12

var cells = []

var gates = []

var cell_prefab = preload("res://data/dungeon/cell.tscn")


onready var dungeon = get_parent()

onready var outer_wall = dungeon.find_node("outer_wall")

var cell_size : float


func index(x,y) -> int:
	return x + (y * width)


# Called when the node enters the scene tree for the first time.
func configure(w: int, h: int, cell_sz: float):
	self.width = w
	self.height = h
	self.cell_size = cell_sz
	cells.resize(width * height)
	for yp in range(0,height):
		for xp in range(0,width):
			var node = cell_prefab.instance()
			self.add_child(node)
			node.configure(xp, yp)
			node.name = "Cell_%d_%d" % [xp, yp]
			node.translation = Vector3(xp * cell_sz, 0, -(yp * cell_sz))
			cells[self.index(xp,yp)] = node


func reset_all():
	for c in cells:
		c.reset()


func get_cell(x : int, y: int):
	if x<0 or x>=width:
		return null
	if y<0 or y>=height:
		return null
	return cells[x + (y * width)]


func get_wall(x: int, y: int, ang: int) -> Spatial:
	assert(ang in [0, 90, 180, 270])

	if ang==90:
		var prev = get_cell(x-1, y)
		if prev==null:
			return outer_wall
		return prev.east
	elif ang==180:
		var prev = get_cell(x, y-1)
		if prev==null:
			return outer_wall
		return prev.north
	elif ang==270:
		return get_cell(x, y).east
	else:
		return get_cell(x, y).north


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
	if cell:
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

