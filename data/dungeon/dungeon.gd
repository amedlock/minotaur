extends Spatial;


# width and height of the dungeon map
const WIDTH = 12;
const HEIGHT = 12;
const CELL_SIZE = 3.0;
const MAX_LEVEL = 100;


enum GateType { Empty, Tan, Green, Blue }

onready var enemy_list = $Enemies     # all enemies are children of this node

onready var item_list = $ItemList  	  # all items are enemies of this node


enum WallDir  { North, South, East, West } # four movement/wall directions

enum WallPost  { NW, NE, SE, SW }
enum WallType { None, Wall, Door, SecretDoor, Gate }

enum MazeType { War, Magic, Both }


# just the info for the level, no spatials here
class LevelInfo:
	var depth = null    			# 1..100
	var seed_number = null   		# seed used to generate dungeon
	var used_gate = false    		# used a gate to come here
	var war_monsters = false		# has war monsters
	var magic_monsters = false 		# has magic monsters
	var special_monsters = false 	# has magic monsters
	var has_minotaur = false		# minotaur on this level?
	var start = null				# starting coord
	var gate = GateType.Empty   	# what gate type is on this level


var skill_level = 1   # 1,2,3,4

var seed_number = 0xdeadd00d  # starting seed number

var level_info = {}  # for maze 1 , others built from this one 

var current_level : LevelInfo;

var minotaur_appears = { 1: 3, 2:6, 3:10, 4:16 } # you can go deeper but minotaur appears here


var player ;
onready var builder = $Builder;
var hud ;
var audio

onready var grid = $Grid

func _ready():
	find_node("ceiling").show()
	player = get_parent().find_node("Player")
	audio = player.find_node("Audio")
	hud = player.find_node("HUD")
	self.translation = maze_origin
	grid.configure(WIDTH,HEIGHT,CELL_SIZE)


func init_maze( skill, seednum ):
	skill_level = skill
	seed_number= seednum
	var rng = RandomNumberGenerator.new()
	rng.seed = seednum
	for num in range(1, MAX_LEVEL+1):
		level_info[num] = make_dungeon_info( skill, num, rng )
	current_level = level_info[1]
	grid.reset_all()
	builder.build_maze(current_level)
	hud.update_stats()
	enable()
	player.reset_location()
	player.map_view.update_map(current_level.depth)


func enable():
	self.show()
	
func disable():
	self.hide()	

var mural_colors = {
	"war":preload("res://data/dungeon/green_mat.tres"),
	"magic" :preload("res://data/dungeon/blue_mat.tres"),
	"both"  :preload("res://data/dungeon/tan_mat.tres")
}

func set_mural_color(kind):
	var mat = mural_colors[kind]
	for m in find_node("floor").get_children():
		if m.is_in_group("murals"):
			m.get_node("Mesh").set_surface_material( 0, mat )
	



func go_next_level():
	var next = current_level.depth+1
	if not level_info.has(next):
		return
	current_level = level_info[next]
	audio.stream = load("res://data/sounds/descend.wav")
	audio.play()
	grid.reset_all()
	builder.build_maze(current_level)
	player.map_view.update_map(current_level.depth)
	player.reset_location()
	player.hud.update()



func use_exit():
	var item = player.item_at_feet()
	if item==null or item.name!="ladder":
		return false
	go_next_level()
	return true



func make_dungeon_info(skill:int, num: int, rng: RandomNumberGenerator) -> LevelInfo:
	var result = LevelInfo.new()
	result.depth = num;
	result.seed_number = rng.randi()
	var monster_rng = rng.randi_range(0,100)
	result.war_monsters = monster_rng < 40 or monster_rng > 80
	result.magic_monsters = monster_rng >= 40 
	result.has_minotaur = num >= minotaur_appears[skill]
	return result



func load_gate_level(gate):
	current_level.used_gate = true
	current_level.seed_number = randi()
	current_level.magic_monsters = gate.kind != 'war'
	current_level.war_monsters = gate.kind != 'magic'
	grid.reset_all()
	builder.build_maze()


func world_pos( cx, cy ):
	return maze_origin + Vector3( cx*3, 0, cy * 3 )
	


func cell_at_offset(loc,x,y):
	return grid.get_cell( loc.x + x, loc.y + y )


func find_walls(loc):
	var result = [
		cell_at_offset(loc,0,1),
		cell_at_offset(loc,1,0),
		cell_at_offset(loc,0,-1),
		cell_at_offset(loc,-1,0) ]

	match player.dir:
		0: return result
		1: return result
		2: return result
		3: return result


func quick_rotate_90(v: Vector2, amt: int):
	while amt > 0:
		var tmp = v.y
		v.y = -v.y
		v.x = tmp
		amt -= 1
	return v

	
var wall_post = {
	"north": WallPost.NW,
	"east": WallPost.NE,
	"south": WallPost.SE,
	"west": WallPost.SW

}	

var wall_post_offset = {
	WallPost.NW: Vector3( 0, 0.25, 0 ),
	WallPost.NE: Vector3( 3, 0.25, 0 ),
	WallPost.SE: Vector3( 3, 0.25, 3 ),
	WallPost.SW: Vector3( 0, 0.25, 3 )
}

var wall_angle = {
	"north": 0,
	"east": 270,
	"south": 180,
	"west": 90
}

const maze_origin = Vector3( -18, 0, -18 )

func wall_post_for_wall( cx, cy, walldir ):
	var which = wall_post[walldir]
	return  maze_origin + wall_post_offset[ which ] + Vector3( cx*3, 0, cy*3 )


func add_final( x, y ):
	grid.get_cell(x, y).item = item_list.treasure








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
	grid.reset_all()




