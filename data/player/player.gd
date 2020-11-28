extends Spatial;


var loc = Vector2(0, 0)  # grid coordinates of player x and z
var dir = 270;
var glance_amt = 0		 # if glancing, add this angle


# these are Item references, not nodes
var left_hand ;
var right_hand ;


# item references for each slot
var inventory = {1:null, 2:null, 3:null,
				 4:null, 5:null, 6:null,
				 7:null, 8:null, 9:null }



var food : int = 6;
var arrows : int = 12;
var health : int = 20
var health_max : int = 20
var mind : int = 8
var mind_max : int = 8
var gold = 0

var war_exp = 0  # experience since last rest
var magic_exp = 0  # experience since last rest

var needs_rest = false # can we rest and recover health/mind?

var resurrected = false # has this player cheated death

var skill = 1


# these are Items from item_list.gd
var helmet = null
var breastplate = null
var ring = null

var potion = null  		# active potion?
var potion_turns = 0	# how many turns before it vanishes


# saved for retreat during combat
var last_loc
var last_dir

var start_pos  # starting position for player


# references to game, grid, map, hud, combat, audio and dungeon nodes
var dungeon
var grid
var game
var map_view
var item_list

onready var idle = $idle
onready var move = $move
onready var glance = $glance
onready var combat = $combat
onready var viewMap = $viewMap
onready var hud = $Camera/HUD
onready var audio = $Audio


var player_state = "idle"


func _ready():
	dungeon = get_parent()
	game = dungeon.get_parent()
	start_pos = dungeon.find_node("StartPos")
	grid = dungeon.find_node("Grid")
	item_list = dungeon.find_node("ItemList")
	map_view = game.find_node("MapView")
	self.disable()


func enable():
	self.show()
	hud.update()


func disable():
	self.hide()


# updates direction based on dir and glance
func set_dir( d ):
	d = 0 if d==null else int(d)
	self.dir = wrapi(d,0,360)
	update_rotation()
	hud.update_compass()


func set_glance( g ):
	self.glance_amt = g
	update_rotation()


func update_rotation():
	self.rotation_degrees = Vector3( 0, self.dir + glance_amt, 0 )


func coord_to_world(p):
	return start_pos.translation + Vector3(p.x * 3, 0, -p.y * 3)


func set_pos( v: Vector2 ):
	loc.x = v.x
	loc.y = v.y
	self.translation = coord_to_world(loc)


func reset_location():
	set_pos( Vector2(0, 0) ); last_loc = null
	set_dir( 270 ); last_dir = null


func over_exit() -> bool:
	if dungeon.current_level.depth<1:
		return false
	var cell = dungeon.grid.get_cell(loc.x, loc.y)
	return cell.item and cell.item.item_info.name=="ladder"



func is_moving():
	return player_state=="move"


func is_dead() -> bool:
	return health<1 or mind<1


func dir_name() -> String:
	if dir > 315 or dir < 45: return "north"
	if dir >= 45 and dir < 135: return "west"
	if dir >= 135 and dir < 225: return "south"
	return "east"



func _input(evt):
	match player_state:
		"idle" : idle.input(evt)
		"combat": combat.input(evt)
		"glance": glance.input(evt)
		"move": move.input(evt)
		"viewMap": viewMap.input(evt)
	if evt is InputEventKey:
		debug_keys(evt)


func _process(delta):
	match player_state:
		"idle" : idle.process(delta)
		"combat": combat.process(delta)
		"glance": glance.process(delta)
		"move": move.process(delta)
		"viewMap": viewMap.process(delta)




func script_changed():
	player_state.player = self



func use_exit():
	if over_exit():
		dungeon.use_exit()


func show_map():
	pass


func debug_keys(evt):
	if evt is InputEventKey:
		if evt.pressed and not evt.echo:
			match evt.scancode:
				KEY_F5:
					reset_location()
				KEY_F6:
					dungeon.go_next_level()
				KEY_F7:
					self.health = 1;
					self.mind = 1



func move_forward():
	var wall = wall_ahead()
	if wall and wall.is_blocked():
		return
	var cell = cell_ahead()
	if not cell:
		return
	if cell.enemy:
		start_combat(cell)
		return
	last_loc = self.loc
	last_dir = self.dir
	move.start( cell )



func move_backward():
	var wall = wall_behind()
	if wall and wall.is_blocked():
		return
	var cell = cell_behind()
	if not cell:
		return
	if cell.enemy:
		idle.turn_to(180)
		#start_combat(cell)
		return
	last_loc = self.loc
	last_dir = self.dir
	move.start( cell )


func close_doors():
	pass

func cheat_death():
	if resurrected: return false
	if gold > 500:
		resurrected = true
		for n in range(1,10): self.inventory[n] = null
		hud.update_pack()
		health = health_max
		mind = mind_max
		reset_location()
		return true
	return false


func set_item_at_feet(item):
	if is_moving():
		return null
	grid.set_item( loc.x, loc.y , item)


func item_at_feet():
	if is_moving():
		return null
	var item_node = grid.get_cell( loc.x, loc.y ).item
	return item_node.item_info if item_node else null


func has_key_for(item):
	for n in inventory.values():
		if n and n.name=="key" and n.power >= item.power:
			return true
	if right_hand and right_hand.name=="key" and right_hand.power >= item.power:
		return true
	if left_hand and left_hand.name=="key" and left_hand.power >= item.power:
		return true
	return false

func open_container(item):
	if item.needs_key:
		if not self.has_key_for(item):
			return false
	var loot = item_list.get_container_loot(item, dungeon.current_level.depth )
	if loot:
		set_item_at_feet(loot)
		return true
	return false



# can the item be used/consumed?
func take_item( item ) -> bool:
	if not item:
		return false
	match item.kind:
		"ladder": return false
		"container":
			return open_container(item)
		"treasure":
			self.win()
			self.disable()
		"money":
			gold += item.stat1
		"quiver":
			arrows += 6
		"food":
			food += 6
		_: return false
	set_item_at_feet(null)
	return true


var magic_items = ["small_ring", "ring", "tome", "potion", "small_potion"]


var magic_sound = preload("res://data/sounds/magic.wav")


func is_better(it, other):
	if it==null:
		return true
	if other==null:
		return false
	return it.stat1 < other.stat1


func use(item):
	match item.name:
		"potion", "small_potion":
			return true
		"ring":
			if is_better(self.ring,item):
				self.ring = item
				return true
		"breastplate":
			if is_better(self.breastplate, item):
				self.breastplate = item
				return true
		"helmet":
			if is_better(self.helmet, item):
				self.helmet = item
				return true
	return false


func use_item():
	if not right_hand:
		return
	if self.use(right_hand):
		right_hand = null
	elif right_hand.name=="tome": # does nothing right now :/
		if right_hand.power==1: pass
		if right_hand.power==2: pass
		if right_hand.power==3: pass
	hud.update_pack()
	hud.update_stats()



func play_sound(sample):
	audio.stream = sample
	audio.play()


func open_door():
	var w = wall_ahead()
	if w:
		w.activate()


func enter_gate(cell):
	if cell and cell.gate:
		audio.stream = load("res://data/sounds/magic.wav")
		audio.play()
		dungeon.load_gate_level(cell.gate)
		cell.set_gate(null)
		self.needs_rest = true


func can_see( wall ):
	return wall==null or (not wall.is_blocked())


func has_weapon():
	return right_hand!=null and right_hand.kind=="weapon"


func face_vector():
	dir = wrapi(dir, 0, 360);
	match int( dir / 90 ):
		0 : return Vector2(0,1)  # north
		1 : return Vector2(-1,0) # west
		2 : return Vector2(0,-1) # south
		3 : return Vector2(1,0)  # east
	assert(false)


func coord_ahead() -> Vector2:
	return loc + face_vector()

func coord_behind() -> Vector2:
	return loc + -face_vector()

func coord_right() -> Vector2:
	var f = face_vector()
	return loc + Vector2( f.y, -f.x )

func coord_left() -> Vector2:
	var f = face_vector()
	return loc + Vector2( -f.y, f.x )


func cell_ahead():
	var p = loc + face_vector()
	return dungeon.grid.get_cell(p.x, p.y)

func cell_behind():
	var p = loc + (-face_vector())
	return dungeon.grid.get_cell(p.x, p.y)

func cell_left():
	var p = coord_left()
	return dungeon.grid.get_cell(p.x, p.y)

func cell_right():
	var p = coord_right()
	return dungeon.grid.get_cell(p.x, p.y)

func wall_ahead():
	return dungeon.grid.get_wall(loc, coord_ahead() )

func wall_behind():
	return dungeon.grid.get_wall(loc, coord_behind() )



func start_combat( cell ) -> bool:
	if not (cell and cell.enemy):
		return false
	var pos = cell.grid_pos();
	var wall = dungeon.grid.get_wall(loc, pos)
	if wall and wall.is_blocked():
		return false
	var ang = int(-rad2deg( (pos - loc).angle_to(face_vector()) ))
	assert( ang in [90, 0, -90])
	if ang!=0:
		idle.turn_to(ang)
	combat.start(cell)
	return true


func end_combat(outcome,enemy):
	match outcome:
		"win":
			killed(enemy)
			enemy.die()
			needs_rest = true
			self.player_state="idle"
		"die":
			if is_dead() and not cheat_death():
				player_state ="lost"
		_: assert(false)
	hud.update()
	


func retreat():
	if last_loc==null: return false
	set_pos( last_loc ); last_loc = null
	set_dir( last_dir ); last_dir = null
	self.hud.update()
	self.in_combat = false
	return true


# tries to initiate combat ahead of player
func attack_ahead():
	combat.attacking = true
	var ahead = cell_ahead()
	if ahead==null or ahead.enemy==null:
		return
	var wall = wall_ahead()
	if can_see(wall):
		self.player_state= "combat"
		combat.start(ahead)
		reduce_potion_turn()


# start combat if monster is nearby
func check_for_monster():
	if start_combat(cell_ahead()):
		return
	if rand_range(0,99) < 40:
		return
	if start_combat(cell_left()):
		return
	if start_combat(cell_right()):
		return


func win():
	game.show_map()
	audio.stream = load("res://data/sounds/win2.wav")
	audio.play()
	self.state = "won"


func reduce_potion_turn():
	potion_turns = max( potion_turns-1, 0 )
	if potion_turns<1:
		potion = null


func rest():
	if food<1 or (not needs_rest):
		return
	if health==health_max and mind==mind_max:
		return
	needs_rest = false
	var hp_gain = (war_exp / 4)
	var mind_gain = (magic_exp / 5)
	war_exp = war_exp % 4
	magic_exp = magic_exp % 5
	health_max += hp_gain;
	mind_max += mind_gain
	health = int( min( health + (health_max * 2 / 3), health_max ))
	mind = int( min( mind + (mind_max * 2 / 3), mind_max ))
	food -= 1
	hud.update_stats()



func killed ( enemy ):
	if enemy.monster.kind=="war":
		war_exp += int(enemy.monster.power)
	elif enemy.monster.kind=="magic":
		magic_exp += int(enemy.monster.power)
	elif enemy.monster.kind=="both":
		war_exp += int(enemy.monster.power)
		magic_exp += int(enemy.monster.power)
	if enemy.monster.name=="minotaur":
		dungeon.add_final( enemy )
	if self.is_dead():
		dungeon.game.game_over()



func percentage( val , pct ):
	return (val * ( 100 - pct ) ) / 100;


func vary_amount( a, pct ):
	var p = pct[ randi() % pct.size() ]
	return a * ( 100 - p ) / 100


func apply_armor( dmg, arm ):
	var prot = percentage( dmg, arm )
	if prot==0: return dmg
	prot = vary_amount( prot, [5, 10, 15] )
	return max( dmg-prot, 0 )


func damage( monster, weap ):
	var maxdmg = int(monster.power * 8) * skill
	var dmg = vary_amount( maxdmg, [ 5, 10, 15, 20 ] )
	var war_amt = apply_armor( dmg, war_armor() )
	var mind_amt = apply_armor( dmg, mind_armor() )
	if weap.name in ["axe", "dagger", "spear"]:
		health = int( max( self.health - war_amt, 0 ))
	elif weap.name in ["fireball", "lightning", "small_fireball"]:
		mind = int( max( self.mind - mind_amt, 0 ))



func init( difficulty ):
	self.skill = difficulty
	gold = 0
	war_exp = 0
	magic_exp = 0
	food = 6
	arrows = 6 + 5 * (5 - skill) # 11-25 arrows
	resurrected = false
	for n in range(10): inventory[n] = null;
	right_hand = dungeon.item_list.find_item("bow")
	if skill==1:
		left_hand = dungeon.item_list.find_item("small_shield")
		health = 18
		mind = 12
	elif skill==2:
		health = 16
		mind = 10
	elif skill==3:
		health = 14
		mind = 8
	elif skill==4:
		health = 12
		mind = 7
	mind_max = mind
	health_max = health
	hud.update()



func war_armor():
	var result = 0
	if helmet :
		result += helmet.stat1
	if breastplate:
		result += breastplate
	if left_hand and left_hand in ["shield", "small_shield"]:
		result += left_hand.stat1
	return result

func mind_armor():
	var result = 0
	if ring:
		result += ring.stat1
	if potion:
		result += potion.stat1
	return result


func war_dmg():
	if right_hand and right_hand.kind=="weapon":
		return right_hand.stat1
	return 0

func mind_dmg():
	if right_hand and right_hand.kind=="weapon":
		return right_hand.stat2
	return 0


