extends Node3D;


# levels of war monsters
const White = Color( 0xffffffff );
const Grey = Color( 0x999999ff );
const Orange = Color( 0xFF8400ff )

# levels of magic monsters
const Blue = Color( 0x436fa8ff )
const Pink = Color( 0xe843a0ff )
const Purple = Color( 0x5a1e96ff )

#levels of hybrid monsters
const Tan = Color( 0xdbb592ff )
const Green = Color( 0x00b300ff )
const Yellow = Color( 0xf4fc58ff )


const Red = Color( 0xcc2a2aff )

const ImageSize = Vector2(32,32);


var icons : Dictionary 

class Enemy:
	var name
	var kind
	var power
	var min_hp
	var max_hp
	var min_mind
	var max_mind
	var img : Rect2

	func json() -> Dictionary:
		return {'name':name, 'kind':kind, 'power': power,
				'health':min_hp, 'mind': min_mind}


# registry of all the enemies
var monsters = {
	'war':{},
	'magic':{},
	'both':{},
	'special':{}
};


var dungeon
var item_list

var minotaur


func _ready():
	dungeon = get_parent()
	item_list = dungeon.find_child("Items");
	load_enemies()
	minotaur = monsters.special.minotaur



func add_enemy(kind, e_name, icon_name, min_lvl, hp1, hp2, mind1, mind2):
	assert( kind in monsters.keys() )
	assert( not(e_name in monsters[kind] ) )
	var e = Enemy.new()
	e.name = e_name
	e.kind = kind
	e.img = icons[icon_name]
	e.power = min_lvl
	e.min_hp = hp1
	e.max_hp = hp2
	e.min_mind = mind1
	e.max_mind = mind2
	monsters[kind][e_name] = e
	return e



func find_enemies(info, powers):
	var kinds = ["both"]
	if info.war_monsters:
		kinds.append("war")
	if info.magic_monsters:
		kinds.append("magic")		
	var result =[]
	for k in kinds:
		var mgroup = monsters[k]
		for e_name in mgroup:
			var e = mgroup[e_name]
			if e.power in powers:
				result.append( e )
	return result


func load_icons(data) -> Dictionary:
	var result = {}
	for i_name in data:
		var v = data[i_name]
		result[i_name] = Rect2( Vector2(v[0] * 32, v[1] * 32), ImageSize )
	return result



# load enemies from external JSON file
# name : icon, min_lvl, min_hp, max_hp, min_mind, max_mind
func load_enemies():
	var monster_list = FileAccess.open('data/enemies/monsters.json', FileAccess.READ)
	if monster_list.get_error():
		print("Could not open enemies json")
		return
	var txt = monster_list.get_as_text() ;
	monster_list.close()
	var parser = JSON.new()
	if parser.parse(txt) != OK:
		print("Error reading monster list:")
		print( "On line: %d: %s" % [parser.error_line, parser.error_string] )
	var data = parser.data
	icons = load_icons(data["icons"])
	for kind in data:
		if ["comment", "icons"].has(kind):
			continue
		var mlist = data[kind]
		for enemy_name in mlist:
			var stats = mlist[enemy_name]
			add_enemy(kind, enemy_name, stats[0], int(stats[1]), \
				int(stats[2]), int(stats[3]), int(stats[4]), int(stats[5]) )
