extends Spatial;


const ImageSize = Vector2(32,32);

var icons = {
	"ladder" : Rect2( Vector2(3,0), ImageSize ) ,
	"small_potion" : Rect2( Vector2(0,0), ImageSize ),
	"potion" : Rect2( Vector2(2,0), ImageSize ),
	"arrow" : Rect2( Vector2(1,0), ImageSize ),
	"small_shield" : Rect2( Vector2(4,0), ImageSize ),
	"breastplate" : Rect2( Vector2(6,0), ImageSize ),
	"flour" : Rect2( Vector2(1,1), ImageSize ),
	"quiver" : Rect2( Vector2(3,1), ImageSize ),
	"shield" : Rect2( Vector2(5,1), ImageSize ),
	"helmet" : Rect2( Vector2(7,1), ImageSize ),
	"bow" : Rect2( Vector2(0,2), ImageSize ),
	"crossbow" : Rect2( Vector2(2,2), ImageSize ),
	"axe" : Rect2( Vector2(4,2), ImageSize ),
	"dagger" : Rect2( Vector2(6,2), ImageSize ),
	"spear" : Rect2( Vector2(7,3), ImageSize ),
	"ring" : Rect2( Vector2(5,3), ImageSize ),
	"scroll" : Rect2( Vector2(3,3), ImageSize ),
	"book" : Rect2( Vector2(1,3), ImageSize ),
	"tome" : Rect2( Vector2(0,3), ImageSize ),
	"small_fireball" : Rect2( Vector2(2,4), ImageSize ),
	"fireball" : Rect2( Vector2(0,4), ImageSize ),
	"small_lightning" : Rect2( Vector2(4,4), ImageSize ),
	"lightning" : Rect2( Vector2(6,4), ImageSize ),
	"money_belt" : Rect2( Vector2(1,5), ImageSize ),
	"small_bag" : Rect2( Vector2(3,5), ImageSize ),
	"bag" : Rect2( Vector2(5,5), ImageSize ),
	"key" : Rect2( Vector2(7,5), ImageSize ),
	"box" : Rect2( Vector2(6,6), ImageSize ),
	"pack" : Rect2( Vector2(4,6), ImageSize ),
	"chest" : Rect2( Vector2(2,6), ImageSize ),
	"lamp" : Rect2( Vector2(0,6), ImageSize ),
	"ingot" : Rect2( Vector2(1,7), ImageSize ),
	"coins" : Rect2( Vector2(3,7), ImageSize ),	
	"necklace" : Rect2( Vector2(4,7), ImageSize ),	
	"chalice" : Rect2( Vector2(5,7), ImageSize ),
	"crown" : Rect2( Vector2(6,7), ImageSize ),
	"treasure" : Rect2( Vector2(7,7), ImageSize )
}

# levels of war monsters
const White = Color( 0xffffffff );
const Grey = Color( 0x999999ff );
const Orange = Color( 0xFF8400ff )

# levels of magic monsters
const Blue = Color( 0x436fa8ff )
const Pink = Color( 0xe843a0ff )
const Purple = Color( 0x5a1e96ff )
const Red = Color( 0xcc2a2aff )


#levels of hybrid monsters
const Tan =  Color( 0xdbb592ff )
const Green = Color( 0x00b300ff )
const Yellow = Color( 0xf4fc58ff )


class Item:
	var name = ""
	var kind ;
	var img ;
	var color
	var power
	var needs_key
	var missile; # bows, scrolls, etc	
	var stat1  
	var stat2



var treasure # final treasure


var items = []

# item_type = "item", "weapon", "armor", "bag", "shield", "key", "potion", "money", "treasure"

func define_item( item_type, icon_name, power, col, stat1, stat2 ):
	var it = Item.new()
	it.kind = item_type
	it.name = icon_name
	it.img = icons[icon_name]
	it.needs_key = icon_name in ["box", "pack", "chest" ]
	if icon_name in ["bow", "crossbow"]:
		it.missile = icons["arrow"]
	elif icon_name in ["scroll", "book"]:
		it.missile = icons["small_lightning"]
	it.color = col
	it.power = power
	it.stat1 = stat1
	it.stat2 = stat2
	items.append( it )
	return it



func add_containers( ):
	var container_colors = [Tan,Orange,Blue]
	for index in range(3):
		var power = index+1
		var col = container_colors[index]
		define_item( "bag", "money_belt", power, col,  0, 0 )
		define_item( "bag", "small_bag", power, col, 0, 0 )
		define_item( "bag", "bag", power, col,  0, 0 )
		define_item( "bag", "box", power, col, 0, 0 )
		define_item( "bag", "pack", power, col, 0, 0 )
		define_item( "bag", "chest", power, col, 0, 0 )

func add_potions():
	var potion_colors = [Blue, Pink, Purple ]
	for index in range(3):
		var power = index + 1
		var col = potion_colors[index]
		define_item( "item", "small_potion", power, col, 0, 0 )
		define_item( "item", "potion", power, col, 0, 0 )
		
func add_money():		
	var money_colors = [Orange, Grey, Yellow, White ]
	for index in range(4):
		var col = money_colors[index]		
		var power = index + 1
		define_item( "money", "coins", power, col,  power * 10, 0 )
		define_item( "money", "necklace", power, col, power * 15, 0 )
		define_item( "money", "ingot", power, col,  power * 20, 0 )
		define_item( "money", "lamp", power, col,  power * 25, 0 )
		define_item( "money", "chalice", power, col,  power * 35, 0 )
		define_item( "money", "crown", power, col,  power * 50, 0 )

						
var war_colors = [ Tan, Orange, Blue, Grey, Yellow, White ]

func add_war_weapons( kind, name, dmg ):
	var index = 0
	for value in dmg:
		var col = war_colors[index];
		define_item(kind, name, index+1, col, dmg[index], 0 )
		index = index + 1




var magic_colors = [ Blue, Grey, White, Pink, Red, Purple ]

func add_magic_weapons( kind, name, dmg ):
	var index = 0
	for value in dmg:
		var col = magic_colors[index];
		define_item(kind, name, index+1, col, 0, dmg[index] )
		index = index + 1



func add_weapons():
	add_war_weapons("weapon", "bow", [6,9,15,21,27,33] )
	add_war_weapons("weapon", "crossbow", [18,24,30,36,42,99] )
	add_war_weapons("weapon", "spear", [21,27,33,42,57,75] )
	add_war_weapons("weapon", "dagger", [9,15,21,30,42,57] )
	add_war_weapons("weapon", "axe", [15,21,27,36,57,66] )
	add_war_weapons("armor", "small_shield", [6, 12, 18, 24, 30, 36] )
	add_war_weapons("armor", "shield", [8, 16, 24, 32, 40, 48] )
	add_magic_weapons("weapon", "scroll", [3,5,9,13,17,21] )
	add_magic_weapons("weapon", "book", [11,15,19,23,27,65] )
	add_magic_weapons("weapon", "small_lightning", [9,13,17,23,33,43] )
	add_magic_weapons("weapon", "lightning", [17,21,29,45,55,65] )
	add_magic_weapons("weapon", "small_fireball", [5,9,13,23,33,39] )
	add_magic_weapons("weapon", "fireball", [13,17,25,33,39,48] )
	add_war_weapons( "armor", "helmet", [5,10,15,20,25,30] )
	add_war_weapons( "armor", "breastplate", [10,17,24,31,38,45] )

		
						

func add_all_items():
	define_item( "item", "quiver", 1, Tan,  6, 0 )
	define_item( "item", "flour", 1, Tan, 3, 0 )
	define_item( "item", "key", 1, Tan,  3, 0 )
	define_item( "item", "key", 2, Orange,  3, 0 )
	define_item( "item", "key", 3, Blue,  3, 0 )
	define_item( "item", "ring", 1, Blue,  0, 0 )
	define_item( "item", "ring", 2, Pink,  0, 0 )
	define_item( "item", "ring", 3, Purple,  0, 0 )
	#define_item( "item", "small_ring", 1, Blue,  0, 0 )
	#define_item( "item", "small_ring", 2, Pink,  0, 0 )
	#define_item( "item", "small_ring", 3, Purple,  0, 0 )	
	#define_item( "item", "tome", 3, Blue,  1, 0 )
	#define_item( "item", "tome", 3, Pink,  2, 0 )
	#define_item( "item", "tome", 3, Purple,  3, 0 )
	add_containers()
	add_potions()
	add_money()
	add_weapons()
	treasure = define_item("treasure", "treasure", 1, Yellow, 0, 0 )
		
	
	
var dungeon


func _ready():
	dungeon = get_parent()
	for name in icons:
		var r = icons[name]
		r.position *= 32
		icons[name] =  r
	add_all_items()
	

func find_items(kind, names, powers):
	var result = []
	for x in items:
		if x.kind!=kind: 
			continue
		if names==null or (x.name in names):
			if powers==null or (x.power in powers ):
				result.append( x )
	return result
	
func all_items( kind, name ):
	var result = []
	for n in items:
		if n.name==name and n.kind==kind: 
			result.append( n )
	return result

func find_item( name ):
	for n in items:
		if n.name==name: return n
	return null;
				
func choose_random( items ):
	assert( items.size() > 0 )
	var pos = randi() % items.size()
	return items[pos]


var level_bags = [
	["money_belt", "small_bag"],
	["bag"],
	["box"],
	["pack"],
	["chest"]
]

var level_money = [
	["coins", "necklace"],
	["ingot"],
	["lamp"],
	["chalice"],
	["crown"]
]


func get_special_loot( it, depth ):
	return choose_random(  find_items("armor", null, null ) )

func get_container_loot(it, depth):
	var result  
	if it.needs_key:
		result = get_special_loot(it,depth)
		if result!=null: return result
	var levels = [1]
	if depth <=5: levels = [depth-1,depth]					
	if randi() % 20<2: 
		result = find_items("item", ["potion", "ring"], levels )
		if result.size()>0: return choose_random( result )
	var names = []
	for n in range( level_money.size() ):
		if n > depth-1: continue
		for nl in level_money[n]:
			names.append( nl )
	result = find_items( "money", names, levels )
	return choose_random( result )
	
	
