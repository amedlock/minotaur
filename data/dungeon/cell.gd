extends Spatial


var x = 0
var y = 0

var north 
var east 

var item  
var enemy 
var gate 

var wall_prefab = preload("res://data/dungeon/dungeon_wall.tscn")
var corner_prefab = preload("res://data/dungeon/wall_corner.tscn")

var door_prefab = preload("res://data/door/door_prefab.tscn")

var item_prefab = preload("res://data/items/item_prefab.tscn")
var enemy_prefab = preload("res://data/enemies/enemy_prefab.tscn")


var dungeon
var grid 


# Called when the node enters the scene tree for the first time.
func _ready():
	grid = self.get_parent()


func configure(xp, yp):
	self.x = xp
	self.y = yp


func get_index() -> int:
	return x + (y * grid.width)


func on_enter(_player):
	print("Player entered")


var green_gate = preload("res://data/gate/green_gate.tscn")
var blue_gate = preload("res://data/gate/blue_gate.tscn")
var tan_gate = preload("res://data/gate/tan_gate.tscn")

func set_gate(kind):
	var node 
	match kind:
		"war": node = green_gate.instance()
		"magic": node = blue_gate.instance()
		"both": node = tan_gate.instance()
	self.add_child(node)
	self.gate = node


# check for a wall between this cell and another
func check_wall(c):
	if c==null:
		return null
	if c.y==self.y:
		if c.x == self.x+1:
			return east
		elif c.x == self.x-1:
			return c.east
	if c.x==self.x:
		if c.y == self.y+1:
			return self.north
		elif c.y == self.y-1:
			return c.north
	return null


func free_child(it):
	if it!=null:
		self.remove_child(it)
		it.queue_free()

func reset():
	if gate:
		free_child(gate)
		gate = null
	if north:
		free_child(north)
		north = null
	if item!=null:
		free_child(item)
		item = null
	if self.enemy!=null:
		free_child(enemy)
		enemy = null




var wall_angle = {
	"north": 0,
	"east": 270,
	"south": 180,
	"west": 90
}



func set_wall(dir, kind):
	var prev = get(dir)
	if prev:
		remove_child(prev)
		prev.queue_free()
	var node = null
	match kind:
		"door": node = door_prefab.instance()
		"wall": node = wall_prefab.instance()
	node.rotation_degrees = Vector3(0, wall_angle[dir], 0 )
	node.add_to_group("wall")
	self.add_child(node)
	self.set(dir, node)



func set_item(new_item):
	if item!=null:
		self.remove_child(item)
		item.queue_free();
	item = null
	if new_item!=null:
		var node = item_prefab.instance()
		self.add_child( node )
		node.translation = Vector3( 1.5, 0.6, 1.5 ) + new_item.offset
		node.configure(new_item)
		item = node
	

func set_enemy(e):
	if enemy!=null:
		self.remove_child(enemy)
		enemy.queue_free()
	self.enemy = null
	if e!=null:
		var node = enemy_prefab.instance()
		self.add_child( node )
		node.configure(x, y, e)
		node.translation = Vector3( 1.5, 0.9, 1.5 )
		self.enemy = node


var wall_post_offset = {
	"nw": Vector3( 0, 0.25, 0 ),
	"ne": Vector3( 3, 0.25, 0 ),
	"se": Vector3( 3, 0.25, 3 ),
	"sw": Vector3( 0, 0.25, 3 )
}

func add_corner( which ):
	var it = corner_prefab.instance();
	self.add_child(it)
	it.translation = wall_post_offset[which]
