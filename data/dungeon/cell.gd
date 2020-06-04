extends Spatial


var x = 0
var y = 0

# walls
var north : Spatial
var east  : Spatial

var item : Spatial 
var enemy : Spatial
var gate : Spatial 

var wall_prefab = preload("res://data/dungeon/dungeon_wall.tscn")
var corner_prefab = preload("res://data/dungeon/wall_corner.tscn")

var door_prefab = preload("res://data/door/door_prefab.tscn")

var item_prefab = preload("res://data/items/item_prefab.tscn")
var enemy_prefab = preload("res://data/enemies/enemy_prefab.tscn")


var grid


# Called when the node enters the scene tree for the first time.
func _ready():
	grid = self.get_parent()


func debug_info() -> String:
	var args = [
		self.name, 
		Vector2(x,y),
		self.item.debug_info() if self.item else "none",
		self.enemy.name if self.enemy else "none",
		self.north.name if self.north else "none",
		self.east.name if self.east else "none"
	]
	return "(%s %s - item:%s en:%s, doors:%s, %s)" % args


func configure(xp, yp):
	self.x = xp
	self.y = yp


func get_index() -> int:
	return x + (y * grid.width)


func on_enter(player):
	if self.gate:
		player.enter_gate(self.gate.kind)


var green_gate = preload("res://data/gate/green_gate.tscn")
var blue_gate = preload("res://data/gate/blue_gate.tscn")
var tan_gate = preload("res://data/gate/tan_gate.tscn")

func set_gate(kind):
	if self.gate:
		free_child(gate)
		self.gate = null
	var node 
	match kind:
		"war": node = green_gate.instance()
		"magic": node = blue_gate.instance()
		"both": node = tan_gate.instance()
		_: return
	node.translation = Vector3(1.5, 0.25, -1.5)
	self.add_child(node)
	self.gate = node


# check for a wall between this cell and another
func check_wall(c):
	if c==null:
		return null
	if c.y==self.y:
		if c.x > self.x:
			return east
		else:
			assert( c.x < self.x )
			return c.east
	if c.x==self.x:
		if c.y > self.y:
			return self.north
		else:
			return c.north
	return null


func free_child(it):
	if it!=null:
		self.remove_child(it)
		it.queue_free()

func reset():
	gate = null
	north = null
	east = null
	item = null
	enemy = null
	for x in self.get_children():
		self.remove_child(x)
		x.queue_free()



var wall_angle = {
	"north": 180,
	"east": 270	
}


const wall_offset = Vector3( 3, 0.25, -3)

func set_wall(dir, kind):
	assert( dir in ["north", "east"])
	var prev = get(dir)
	if prev:
		remove_child(prev)
		prev.queue_free()
	var node = null
	match kind:
		"door": node = door_prefab.instance()
		"wall": node = wall_prefab.instance()
	node.add_to_group(kind)
	self.add_child(node)
	node.name = kind
	node.translation = wall_offset
	match dir:
		"north" : node.rotation_degrees = Vector3(0,180,0)
		"east" : node.rotation_degrees = Vector3(0,270,0)
	self.set(dir, node)



func set_item(new_item):
	if item!=null:
		self.remove_child(item)
		item.queue_free();
	item = null
	if new_item!=null:
		var node = item_prefab.instance()
		self.add_child( node )
		node.translation = Vector3( 1.5, 0.55, -1.5 ) + new_item.offset
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
		node.translation = Vector3( 1.5, 0.9, -1.5 )
		self.enemy = node


var wall_post_offset = {
	"nw": Vector3( 0, 0.25, 0 ),
	"ne": Vector3( 3, 0.25, 0 ),
	"se": Vector3( 3, 0.25, 3 ),
	"sw": Vector3( 0, 0.25, 3 )
}

func clear_corner( which ):
	var n = find_node("corner_" + which, false, false)
	if n:
		remove_child(n)
		n.queue_free()


func set_corner( which ):
	var cur = find_node("corner_" + which)
	if cur:
		return
	if which in wall_post_offset:
		var it = corner_prefab.instance();
		it.translation = wall_post_offset[which]
		it.name = "corner_" + which
		self.add_child(it)
	
