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
	var m_type : String 			# "war", "magic", "both" 
	var seed_number = null   		# seed used to generate dungeon
	var used_gate = false    		# used a gate to come here
	var war_monsters = false		# has war monsters
	var magic_monsters = false 		# has magic monsters
	var special_monsters = false 	# has magic monsters
	var has_minotaur = false		# minotaur on this level?
	var start = null				# starting coord
	var gate = GateType.Empty   	# what gate type is on this level


var rng = RandomNumberGenerator.new()

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
	rng.seed = seednum
	for num in range(1, MAX_LEVEL+1):
		level_info[num] = make_dungeon_info( skill, num )
	current_level = level_info[1]
	builder.build_maze()
	hud.update_stats()
	enable()
	player.reset_location()
	player.map.update_map(current_level.depth)


func enable():
	self.show()
	
func disable():
	self.hide()	

func get_cell(x, y):
	return grid.get_cell(x,y)


func go_next_level():
	var next = current_level.depth+1
	if not level_info.has(next):
		return
	current_level = level_info[next]
	audio.stream = load("res://data/sounds/descend.wav")
	audio.play()
	builder.build_maze()
	player.map.update_map(current_level.depth)
	player.reset_location()

func use_exit():
	var item = self.item_at_feet()
	if item==null or item.name!="exit":
		return
	go_next_level()


func randint( hi ):
	if hi < 1: return 0
	var result = rng.randi() % hi
	return result;




func make_dungeon_info(skill:int, num: int) -> LevelInfo:
	var result = LevelInfo.new()
	result.depth = num;
	var monster_rng = randint(100)
	result.war_monsters = monster_rng < 40 or monster_rng > 80
	result.magic_monsters = monster_rng >= 40 
	result.has_minotaur = num >= minotaur_appears[skill]
	result.start = Vector2(2 + randint( 4 ),2 + randint( 4 ) )
	result.seed_number = rng.randi()
	return result



func enter_gate():
	current_level.used_gate = true
	builder.build_maze()



func world_pos( cx, cy ):
	return maze_origin + Vector3( cx*3, 0, cy * 3 )
	

func item_at_feet():
	if player.is_moving(): return null
	var c = grid.get_cell( player.loc.x, player.loc.y )
	if c!=null: 
		return c.item
	return null
	


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


func wall_num( dir ):
	if dir=="north": return 1
	if dir=="east": return 2
	if dir=="south": return 3
	if dir=="west": return 4
	return dir

func wall_index( x, y, dir ):
	return int( dir + (( x + (y * WIDTH * HEIGHT ) ) * 5 ))

	
func wall_valid( cx, cy, num ):
	if num<0 or num>4:
		return false
	if cx<0 or cy<0 or cx>WIDTH-1 or cy>HEIGHT-1: 
		return false
	if cx==0 and num==4: return false
	if cx==WIDTH-1 and num==2: return false
	return true
	
	
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




