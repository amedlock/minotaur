extends Spatial;


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


# sprite sheet cell offset calc
func ss_cell(x,y):
	return Rect2( Vector2(x * 32, y * 32), ImageSize )



var icons = {
	"skeleton" : ss_cell(0,0),
	"skeleton_shield":  ss_cell(1,0),
	"cloaked_skel":  ss_cell(2,0),
	"ghoul":  ss_cell(3,0),
	"wraith": ss_cell(4,0),
	"vampire" : ss_cell(5,0),
	"wraith_shield":  ss_cell(6,0),
	"lich":  ss_cell(7,0),
	"giant":  ss_cell(0,1),
	"giant_shield":  ss_cell(1,1),
	"dwarf":  ss_cell(2,1),
	"dwarf_warrior":  ss_cell(3,1),
	"dwarf_shield":  ss_cell(4,1),
	"knight" : ss_cell(5,1),
	"enchantress" :  ss_cell(6,1),
	"wizard" :  ss_cell(7,1),
	"spider":  ss_cell(0,2),
	"wasp":  ss_cell(1,2),
	"hornet":  ss_cell(2,2),	
	"scorpion" :  ss_cell(3,2),
	"wolf" : ss_cell(4,2),
	# 5,2
	# 6,2
	"golem": ss_cell(7,2),
	"green_dragon":  ss_cell(0,3),
	"red_dragon":  ss_cell(1,3),
	"drake":  ss_cell(2,3),	
	"giant_frog": ss_cell(3,3),
	"snake":  ss_cell(4,3),
	"undead_dragon": ss_cell(5,3),
	"evil_eye" :  ss_cell(6,2),
	"minotaur":  ss_cell(7,3)
}

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
	item_list = dungeon.find_node("Items");
	minotaur = add_enemy( "special", "Minotaur", "minotaur", 6, 60, 90, 40, 60 )
	load_enemies()
	minotaur = monsters.special.minotaur



func add_enemy(kind, name, icon_name, min_lvl, hp1, hp2, mind1, mind2):
	assert( kind in monsters.keys() )
	assert( not(name in monsters[kind] ) )
	var e = Enemy.new()
	e.name = name
	e.img = icons[icon_name]
	e.power = min_lvl
	e.min_hp = hp1
	e.max_hp = hp2
	e.min_mind = mind1
	e.max_mind = mind2
	monsters[kind][name] = e
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
		for name in mgroup:
			var e = mgroup[name]
			if e.power in powers:
				result.append( e )
	return result




# load enemies from external JSON file
# name : icon, min_lvl, min_hp, max_hp, min_mind, max_mind
func load_enemies():
	var monster_list = File.new()
	monster_list.open( 'data/enemies/monsters.json', File.READ )
	var txt = monster_list.get_as_text() ;
	monster_list.close()
	var data = JSON.parse(txt);
	if data.error:
		print("Error reading monster list:")
		print( "On line: %d: %s" % [data.error_line, data.error_string] )
	else:
		for kind in data.result:
			if kind=="comment": continue
			var mlist = data.result[kind]
			for name in mlist:				
				var stats = mlist[name]
				add_enemy(kind, name, stats[0], int(stats[1]),
						  int(stats[2]), int(stats[3]),
						  int(stats[4]), int(stats[5]) )
