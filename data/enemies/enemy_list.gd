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

func ss_cell(x,y):
	return Rect2( Vector2(x * 32, y * 32), ImageSize )



var icons = {
	"skeleton" : ss_cell(0,0),
	"skeleton_shield":  ss_cell(1,1),
	"cloaked_skel":  ss_cell(2,0),
	"cloaked_shield":  ss_cell(3,1),
	"giant":  ss_cell(4,0),
	"giant_shield":  ss_cell(5,1),
	"dwarf":  ss_cell(6,0),
	"dwarf_shield":  ss_cell(7,1),
	"wraith": ss_cell(0,2),
	"wraith_shield":  ss_cell(2,2),
	"snake":  ss_cell(6,2),
	"alligator":  ss_cell(7,3),
	"dragon":  ss_cell(1,3),
	"giant_ant":  ss_cell(3,3),
	"scorpion" :  ss_cell(4,2),
	"minotaur":  ss_cell(5,3)
}

class Enemy:
	var name
	var kind
	var power
	var img = null
	var health = 10
	var mind = 10
	var max_damage

	func dead():
		return health<1 or mind<1

	func json() -> Dictionary:
		return {'name':name, 'kind':kind, 'power': power,
				'health':health, 'mind': mind, 'max_damage': max_damage}


# registry of all the enemies
var monsters = []



#enemy type = "war" "magic" or "both"
func add_enemy( name, power, type, health, max_dmg ):
	var e = Enemy.new()
	e.name = name
	e.power = power
	e.kind = type
	e.img = icons[name]
	e.health = health
	e.mind = health * 2 / 3
	e.max_damage = max_dmg
	monsters.append( e )
	return e



func add_enemies():
	add_enemy( "skeleton", 1, "war", 6, 9 );
	add_enemy( "skeleton", 2, "war", 12, 14 );
	add_enemy( "skeleton", 3, "war", 18, 20 );
	add_enemy( "skeleton_shield", 1, "war", 8 ,10)
	add_enemy( "skeleton_shield", 2, "war", 16, 16 );
	add_enemy( "skeleton_shield", 3, "war", 24, 22 );
	add_enemy( "cloaked_skel", 1, "war", 9, 11 );
	add_enemy( "cloaked_skel", 2, "war", 18, 17 );
	add_enemy( "cloaked_skel", 3, "war", 27, 23 );
	add_enemy( "cloaked_shield", 1, "war", 10,  5 );
	add_enemy( "cloaked_shield", 2, "war", 20,  10 );
	add_enemy( "cloaked_shield", 3, "war", 30,  15 );
	add_enemy( "giant", 1, "war", 30 , 15 );
	add_enemy( "giant", 2, "war", 45,  20 );
	add_enemy( "giant", 3, "war", 60,  27 );
	add_enemy( "giant_shield", 1, "war", 42, 17 );
	add_enemy( "giant_shield", 2, "war", 55, 22 );
	add_enemy( "giant_shield", 3, "war", 65, 31 );
	add_enemy( "dwarf", 1,"magic",  10, 3 );
	add_enemy( "dwarf", 2,"magic",  17, 11 );
	add_enemy( "dwarf", 3,"magic",  24, 15 );
	add_enemy( "dwarf_shield", 1,  "magic",12, 9 );
	add_enemy( "dwarf_shield", 2,  "magic", 20, 13 );
	add_enemy( "dwarf_shield", 3, "magic", 28, 17 );
	add_enemy( "scorpion", 1,"magic", 9,  7 );
	add_enemy( "scorpion", 2, "magic", 18,  13 );
	add_enemy( "scorpion", 3, "magic", 27,  17 );
	add_enemy( "giant_ant", 1, "magic", 6,  5 );
	add_enemy( "giant_ant", 2, "magic", 12,  10 );
	add_enemy( "giant_ant", 3, "magic", 20,  15 );
	add_enemy( "snake", 1, "magic", 11,  9 );
	add_enemy( "snake", 2, "magic", 22, 16 );
	add_enemy( "snake", 3, "magic", 33,  23 );
	add_enemy( "alligator", 1, "magic", 18,  12 );
	add_enemy( "alligator", 2, "magic", 25,  20 );
	add_enemy( "alligator", 3, "magic", 32,  28 );
	add_enemy( "dragon", 1, "magic", 30, 15 );
	add_enemy( "dragon", 2, "magic", 45, 25 );
	add_enemy( "dragon", 3, "magic",  60, 35 );
	add_enemy( "wraith", 1, "both", 20, 15 );
	add_enemy( "wraith", 2, "both", 40, 25 );
	add_enemy( "wraith", 3, "both", 60, 35 );
	add_enemy( "wraith_shield", 1, "both", 35, 17 );
	add_enemy( "wraith_shield", 2, "both", 45, 27 );
	add_enemy( "wraith_shield", 3, "both", 70,  38 );

var items
var dungeon
var minotaur

var first_appears = {
	"wraith": 4,
	"wraith_shield": 5,
	"giant": 3,
	"giant_shield": 3,
	"dragon" : 6,
	"dwarf" : 2,
	"dwarf_shield" : 2,
	"alligator" : 3,
	"cloaked_shield": 2
}



func _ready():
	dungeon = get_parent()
	items = dungeon.find_node("Items");
	minotaur = add_enemy( "minotaur", 1, "special", 75, 50 )
	add_enemies()



func find_enemies(kinds, powers, depth):
	var result =[]
	for m in monsters:
		if first_appears.has( m.name ) and first_appears[m.name] > depth:
			continue
		elif m.kind in kinds and m.power in powers:
			result.append( m )
	return result



