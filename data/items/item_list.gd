extends Spatial;


const ImageSize = Vector2(32,32);

var icons = {
	"small_potion" : Rect2( Vector2(0,0), ImageSize ),
	"arrow" : Rect2( Vector2(1,0), ImageSize ),
	"potion" : Rect2( Vector2(2,0), ImageSize ),
	"ladder" : Rect2( Vector2(3,0), ImageSize ) ,
	"small_shield" : Rect2( Vector2(4,0), ImageSize ),
	"breastplate" : Rect2( Vector2(6,0), ImageSize ),
	"bomb" :Rect2( Vector2(0,1), ImageSize ),
	"food" : Rect2( Vector2(1,1), ImageSize ),
	"quiver" : Rect2( Vector2(3,1), ImageSize ),
	"shield" : Rect2( Vector2(5,1), ImageSize ),
	"helmet" : Rect2( Vector2(7,1), ImageSize ),
	"bow" : Rect2( Vector2(0,2), ImageSize ),
	"crossbow" : Rect2( Vector2(2,2), ImageSize ),
	"axe" : Rect2( Vector2(4,2), ImageSize ),
	"dagger" : Rect2( Vector2(6,2), ImageSize ),
	"tome" : Rect2( Vector2(0,3), ImageSize ),
	"book" : Rect2( Vector2(1,3), ImageSize ),
	"scroll" : Rect2( Vector2(3,3), ImageSize ),
	"amulet" : Rect2( Vector2(5,3), ImageSize ),
	"spear" : Rect2( Vector2(7,3), ImageSize ),
	"fireball" : Rect2( Vector2(0,4), ImageSize ),
	"small_lightning" : Rect2( Vector2(1,4), ImageSize ),
	"small_fireball" : Rect2( Vector2(2,4), ImageSize ),
	"wand" : Rect2( Vector2(4,4), ImageSize ),
	"staff" : Rect2( Vector2(6,4), ImageSize ),
	"ring" : Rect2( Vector2(1,5), ImageSize ),
	"small_bag" : Rect2( Vector2(3,5), ImageSize ),
	"bag" : Rect2( Vector2(5,5), ImageSize ),
	"key" : Rect2( Vector2(7,5), ImageSize ),
	"box" : Rect2( Vector2(6,6), ImageSize ),
	"pack" : Rect2( Vector2(4,6), ImageSize ),
	"chest" : Rect2( Vector2(2,6), ImageSize ),
	"lamp" : Rect2( Vector2(0,6), ImageSize ),
	"horn" : Rect2( Vector2(1,7), ImageSize ),
	"coins" : Rect2( Vector2(3,7), ImageSize ),
	"necklace" : Rect2( Vector2(4,7), ImageSize ),
	"chalice" : Rect2( Vector2(5,7), ImageSize ),
	"crown" : Rect2( Vector2(6,7), ImageSize ),
	"treasure" : Rect2( Vector2(7,7), ImageSize )
}




class Item:
	var name = ""
	var kind = ""
	var img: Rect2
	var color = null
	var power = 0
	var uses = 10  # min uses before item could break
	var needs_key
	var stat1
	var stat2
	var offset = Vector3(0,0,0)   # Vector3 offset for items in maze

	func json() -> Dictionary:
		var fields = ['name', 'kind', 'img', 'color', 'power',
					  'offset', 'stat1', 'stat2']
		var result = {}
		for key in fields:
			var val = self.get(key)
			if val:
				result[key] = val
		return result

	func can_open_with( other ):
		if other==null or self.kind!="container":
			return false
		if not self.needs_key:
			return true
		return other.name=="key" and other.power >= self.power



var treasure 	# final treasure


# A list of every item in the game
var items = []


func define_item( item_type, icon_name, power, col, stat1, stat2 ):
	var it = Item.new()
	it.kind = item_type
	it.name = icon_name
	it.img = icons[icon_name]
	it.needs_key = item_type in ["box", "pack", "chest" ]

	it.color = col
	it.power = power
	it.stat1 = stat1
	it.stat2 = stat2
	items.append( it )
	return it


func missile_for(item):
	match item.name:
		"bow", "crossbow":
			return icons["arrow"]
		"staff", "book":
			return icons["small_fireball"]
		"wand", "scroll":
			return icons["small_lightning"]
		_:
			return icons[item.name]


var dungeon


func _ready():
	dungeon = get_parent()
	for name in icons:
		var r = icons[name]
		r.position *= 32
		icons[name] =  r
	# add_all_items()
	
	load_all_items()


# equivalent to [data[n] for n in names.split(",")]
func select_names(data: Dictionary, names: String) -> Array:
	var result = []
	for n in names.split(",", false):
		var item = data.get(n.strip_edges())
		if item == null:
			print("Missing entry: {0} in {1}".format([n, data]))
		else:
			result.push_back(item)
	return result


func load_colors(data) -> Dictionary:
	var colors = {}
	for name in data:
		colors[name] = Color(data[name])
	return colors


func add_item_types(kind, name, dmg, colors):
	var index = 0
	for value in dmg:
		var col = colors[index];
		define_item(kind, name, index+1, col, dmg[index], 0 )
		index = index + 1


func load_weapons(data, colors):
	var war_colors = select_names( colors ,"Tan, Orange, Blue, Grey, Yellow, White")
	var war = data["War"]
	for name in war:
		add_item_types("weapon", name, war[name], war_colors)
		
	var magic_colors = select_names( colors ,"Blue, Grey, White, Pink, Red, Purple")
	var magic = data["Magic"]
	for name in magic:
		add_item_types("weapon", name, magic[name], magic_colors)


func load_armor(data, colors):
	var armor_colors = select_names( colors ,"Tan, Orange, Blue, Grey, Yellow, White")
	for name in data:
		add_item_types("armor", name, data[name], armor_colors)

func load_money(data, colors):
	var money_colors = select_names(colors, "Orange, Grey, Yellow, White")
	for name in data:
		for power in range(4):
			var stat1 = data[name] * power
			define_item("money", name, power, money_colors[power], stat1, 0)

func load_containers(data, colors):
	var cont_colors = select_names(colors, "Tan,Orange,Blue")
	for name in data:
		for index in range(3):
			define_item("container", name, index+1, cont_colors[index], 0, 0)

	
func define_special(name, data, color):
	var item = data[name]
	return define_item(item[0], name, item[1], color[item[2]], item[3], item[4])
	
func load_items(data, colors):
	define_special("quiver", data, colors)
	define_special("food", data, colors)
	treasure = define_special("treasure", data, colors)
	define_special("ladder", data, colors)
	var keys = data["keys"]
	for name in keys:
		var power = keys[name]
		define_item("item", "key", power, colors[name], 3, 0)
	var amulets = data["amulets"]
	for name in amulets:
		var power = amulets[name]
		define_item("item", "amulet", power, colors[name], 0, 0)


func load_all_items():
	var src = File.new()
	src.open( 'data/items/items.json', File.READ )
	var txt = src.get_as_text() ;
	src.close()
	var data = JSON.parse(txt);
	if data.error:
		print("Error reading item list:")
		print( "On line: %d: %s" % [data.error_line, data.error_string] )
	else:
		var colors = load_colors( data.result["Colors"])
		load_weapons(data.result["Weapons"], colors)
		load_armor(data.result["Armor"], colors)
		load_containers(data.result["Containers"], colors)
		load_items(data.result["Items"], colors)
		load_money(data.result["Money"], colors)



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
		if n.name==name:
			return n
	return null;


var level_bags = [
	["small_bag"],
	["bag"],
	["box"],
	["pack"],
	["chest"]
]

var level_money = [
	["coins", "necklace"],
	["horn"],
	["lamp"],
	["chalice"],
	["crown"]
]


func choose_random_item( kind, names, powers ):
	var subset = find_items(kind, names, powers)
	if subset.size()==0:
		return null
	var pos = randi() % subset.size()
	return subset[pos]


func get_special_loot( _item, _depth ):
	return choose_random_item( "armor", null, null )


# this needs to take depth into account
func get_container_loot(item, depth):
	var result
	if item.needs_key:
		result = get_special_loot(item,depth)
		if result!=null: return result
	var levels = [1]
	if depth <=5: levels = [depth-1,depth]
	if randi() % 20<2:
		result = choose_random_item("item", ["potion", "ring"], levels )
		if result:
			return result
	var names = []
	for n in range( level_money.size() ):
		if n > depth-1: continue
		for nl in level_money[n]:
			names.append( nl )
	return choose_random_item( "money", names, levels )


