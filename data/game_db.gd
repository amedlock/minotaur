extends Node

const InfoTree = preload("res://data/info_tree.gd").InfoTree


class Enemy:
	var name
	var kind
	var min_level
	var power
	var min_hp
	var max_hp
	var min_mind
	var max_mind
	var base_damage = 5
	var img : Rect2



class Item:
	var name = ""
	var kind = ""
	var img: Rect2
	var color = null
	var min_level = 1
	var uses = 10  # min uses before item could break
	var needs_key
	var stat1
	var stat2
	var offset = Vector3(0,0,0)   # Vector3 offset for items in maze

	func damage():
		return stat1


const ImageSize = Vector2(32,32);

var colors = {}
var icons = {}
var enemies = []
var items = []
var final_treasure : Item

const war_colors = ["Tan", "Orange", "Blue", "Grey", "Yellow", "White"]
const magic_colors = ["Blue", "Grey", "White", "Pink", "Red", "Purple"]
const armor_colors = war_colors
const money_colors = ["Orange", "Grey", "Yellow", "White"]
const container_colors = ["Tan", "Orange", "Blue"]


func _init() -> void:
	load_game_info()


func add_enemy(e_name : String, kind : String, stats : Array, power : int):
	assert( stats.size()==6 )
	var enemy = Enemy.new()
	enemy.name = e_name
	enemy.kind = kind
	enemy.img = icons[stats[0]]
	enemy.min_level = int(stats[1])
	enemy.power = enemy.min_level * 2
	enemy.min_hp = int(stats[2])
	enemy.max_hp = int(stats[3])
	enemy.min_mind = int(stats[4])
	enemy.max_mind = int(stats[5])
	enemies.append(enemy)


func add_item(i_name, kind, icon, color, min_lvl, stat1, stat2):
	var item = Item.new()
	item.name = i_name
	item.kind = kind
	item.img = icon
	item.color = color
	item.min_level = int(min_lvl)
	item.stat1 = int(stat1)
	item.stat2 = int(stat2)
	item.needs_key = (i_name in ["box", "pack", "chest"])
	items.append(item)

func load_game_info():
	var t = InfoTree.new("game_info")
	t.load_json("data/game_info.json")
	load_colors( t.find_child("colors"))
	load_icons( t.find_child("item_icons"))
	load_icons( t.find_child("enemy_icons"))
	load_enemies( t.find_child("enemies"))
	load_all_items( t.find_child("items"))

func load_colors(t):
	for col in t.children():
		colors[col.key] = Color(col.value)
	

func load_icons(t):
	for icon in t.children():
		assert(icon.key not in icons)
		var coord = Vector2(icon.value[0] * 32, icon.value[1] * 32)
		icons[icon.key] = Rect2(coord, ImageSize)
	

func load_enemies(t):
	for e in t.find_child("war").children():
		var n = 0
		for col in war_colors:
			var stats = e.value
			add_enemy(e.key, "war", stats, n)
			n += 1
	for e in t.find_child("magic").children():
		var n = 0
		for col in magic_colors:
			var stats = e.value
			add_enemy(e.key, "magic", stats, n)
			n += 1


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


func load_all_items(node):
	load_specials(node.find_child("specials"))
	var weapons = node.find_child("weapons")
	load_weapons(weapons, "war", war_colors)
	load_weapons(weapons, "magic", magic_colors)
	load_armor(node.find_child("armor"))
	load_key_items(node.find_child("keys"), icons["key"], "key")
	load_key_items(node.find_child("amulets"), icons["amulet"], "amulet")
	load_containers(node.find_child("containers"))
	load_money(node.find_child("money"))


func load_key_items(node, icon, i_name):
	for key_item in node.children():
		add_item(i_name, i_name, icon, colors[key_item.key], key_item.value, key_item.value, 0)


func load_weapons(weapons, kind, color_names):
	for weap in weapons.find_child(kind).children():
		var n = 0
		for damage in weap.value:
			var min_lvl = (n * 2)+ 1
			var icon = icons[weap.key]
			var col = colors[ color_names[n] ]
			add_item(weap.key,"weapon", icon, col, min_lvl, damage, 0)
			n += 1


func load_armor(armor):
	for a in armor.children():
		var min_lvl = 1
		for n in range(a.value.size()):
			var amount = a.value[n]
			var col = armor_colors[n]
			add_item(a.key, "armor", icons[a.key], colors[col], min_lvl, amount, 0)
			min_lvl += 2



func load_containers(t):
	for cont in t.children():
		var min_lvl = cont.value * 2
		for col in container_colors:			
			add_item(cont.key, "container", icons[cont.key], colors[col], min_lvl, cont.value, 0 )


func load_money(t):
	for m in t.children():
		var n = 0
		for col in money_colors:
			var min_lvl = int(m.value[0]) + (n * 2)
			var amount = int(m.value[1]) * (n+1)
			add_item(m.key, "money", icons[m.key], colors[col], min_lvl, amount, 0 )
			n+=1


func load_specials(specials):
	for it in specials.children():
		var info = it.value
		var icon = icons[info[0]]
		var col = colors[info[2]]
		add_item(it.key, "special", icon, col, 1, info[3], info[4])


func check_in_list(val: Variant, valid: Array) -> bool:
	if valid==null or valid.is_empty():
		return true
	return valid.has(val)


func find_item(item_name: String):
	for it in items:
		if it.name==item_name:
			return it
	return null


func search_items(kind: String, names: Array, level: int) -> Array:
	var result = []
	for it in items:
		if it.kind != kind:
			continue
		if check_in_list(it.name, names) and level >= it.min_level:
			result.append(it)
	return result


func find_enemy(m_name : String, power = 0):
	for it in enemies:
		if it.name==m_name and (power<0 or power==it.power):
			return it
	return null


func search_enemies(kinds: Array, powers: Array) -> Array:
	var result = []
	for e in enemies:
		if check_in_list(e.kind, kinds) and check_in_list(e.power, powers):
			result.append(e)
	return result


# returns enemies of type with min_level >= depth
# will return strongest enemies of a particular name
func find_enemies(type: String, depth: int) -> Array:
	var result = {}
	for e in enemies:
		if e.min_level > depth:
			continue
		if not(type=="both" or e.kind==type):
			continue
		if not result.has(e.name):
			result[e.name] = e
		else:
			var curr = result.get(e.name)
			if curr.min_level < e.min_level:
				result[e.name] = e
	return result.values()


func choose_random_item( kind, names, depth ):
	var subset = search_items(kind, names, depth)
	if subset.size()==0:
		return null
	var pos = randi() % subset.size()
	return subset[pos]


func get_special_loot( _item, depth ):
	return choose_random_item( "armor", [], depth )


# this needs to take depth into account
func get_container_loot(item, depth):
	if item.needs_key:
		return get_special_loot(item,depth)
	if randi() % 20<2:
		return choose_random_item("item", ["potion", "ring"], depth )
	var result = search_items("money", [], depth)
	assert( not result.is_empty())
	var n = randi_range(0, result.size()-1)
	return result[n]
