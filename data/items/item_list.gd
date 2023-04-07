extends Node3D;


const ImageSize = Vector2(32,32);

var icons : Dictionary 


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
		var fields = ['name', 'kind', 'img', 'color', 'power', 'offset', 'stat1', 'stat2']
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
	for i_name in icons:
		var r = icons[i_name]
		r.position *= 32
		icons[i_name] =  r
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
	for c_name in data:
		colors[c_name] = Color(data[c_name])
	return colors


func load_icons(data) -> Dictionary:
	var result = {}
	for i_name in data:
		var v = data[i_name]
		result[i_name] = Rect2(Vector2(v[0]*32, v[1]*32), ImageSize)
	return result


func add_item_types(kind, i_name, dmg, colors):
	var index = 0
	for value in dmg:
		var col = colors[index];
		define_item(kind, i_name, index+1, col, dmg[index], 0 )
		index = index + 1


func load_weapons(data, colors):
	var war_colors = select_names( colors ,"Tan, Orange, Blue, Grey, Yellow, White")
	var war = data["War"]
	for w_name in war:
		add_item_types("weapon", w_name, war[w_name], war_colors)
		
	var magic_colors = select_names( colors ,"Blue, Grey, White, Pink, Red, Purple")
	var magic = data["Magic"]
	for w_name in magic:
		add_item_types("weapon", w_name, magic[w_name], magic_colors)


func load_armor(data, colors):
	var armor_colors = select_names( colors ,"Tan, Orange, Blue, Grey, Yellow, White")
	for w_name in data:
		add_item_types("armor", w_name, data[w_name], armor_colors)

func load_money(data, colors):
	var money_colors = select_names(colors, "Orange, Grey, Yellow, White")
	for w_name in data:
		for power in range(4):
			var stat1 = data[w_name] * power
			define_item("money", w_name, power, money_colors[power], stat1, 0)

func load_containers(data, colors):
	var cont_colors = select_names(colors, "Tan,Orange,Blue")
	for w_name in data:
		for index in range(3):
			define_item("container", w_name, index+1, cont_colors[index], 0, 0)

	
func define_special(w_name, data, color):
	var item = data[w_name]
	return define_item(item[0], w_name, item[1], color[item[2]], item[3], item[4])
	
func load_items(data, colors):
	define_special("quiver", data, colors)
	define_special("food", data, colors)
	treasure = define_special("treasure", data, colors)
	define_special("ladder", data, colors)
	var keys = data["keys"]
	for w_name in keys:
		var power = keys[w_name]
		define_item("item", "key", power, colors[w_name], 3, 0)
	var amulets = data["amulets"]
	for w_name in amulets:
		var power = amulets[w_name]
		define_item("item", "amulet", power, colors[w_name], 0, 0)


func load_all_items():
	var src = FileAccess.open('data/items/items.json', FileAccess.READ)
	if src.get_error():
		print("Could not open items file")
		return
	var txt = src.get_as_text() ;
	src.close()
	var parser = JSON.new();
	var json = parser.parse(txt)
	if json!=OK:
		print("Error reading item list:")
		print( "On line: %d: %s" % [parser.get_error_line(), parser.get_error_message()] )
	var data = parser.data
	var colors = load_colors( data["Colors"])
	icons = load_icons(data["Icons"])
	load_weapons(data["Weapons"], colors)
	load_armor(data["Armor"], colors)
	load_containers(data["Containers"], colors)
	load_items(data["Items"], colors)
	load_money(data["Money"], colors)



func find_items(kind, names, powers):
	var result = []
	for x in items:
		if x.kind!=kind:
			continue
		if names==null or (x.name in names):
			if powers==null or (x.power in powers ):
				result.append( x )
	return result

func all_items( kind, i_name ):
	var result = []
	for n in items:
		if n.name==i_name and n.kind==kind:
			result.append( n )
	return result

func find_item( i_name ):
	for n in items:
		if n.name==i_name:
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


