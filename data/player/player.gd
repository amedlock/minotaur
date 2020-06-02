extends Spatial;


var loc = Vector2(0, 0)  # coordinates of player x and z
var dir = 270;


# these are Item references, not nodes
var left_hand ;
var right_hand ;

var inventory = {1:null, 2:null, 3:null, 4:null, 5:null, 6:null, 7:null, 8:null, 9:null }


var last_loc
var last_dir
var active_action = null


# references to game, hud and dungeon nodes
var dungeon = null;
var grid = null
var game  = null;
var map_view
var item_list

var start_pos  # starting position for player

onready var hud = $Camera/HUD
onready var audio = $Audio
onready var glance = $Glance
onready var combat = $Combat
onready var turn = $Turn
onready var move = $Move
onready var show_map = $ShowMap


func _ready():
	dungeon = get_parent()
	game = dungeon.get_parent()
	start_pos = dungeon.find_node("StartPos")
	grid = dungeon.find_node("Grid")
	item_list = dungeon.find_node("ItemList")	
	map_view = game.find_node("MapView")
	self.hide()
	# connect all "action" nodes
	for n in self.get_children():
		if n.is_in_group("action"):
			n.connect("action_complete", self, "action_complete")


func enable():
	self.show()
	self.set_process_input( true )
	hud.update()


func disable():
	self.hide()
	self.set_process_input( false )


func set_dir( d ):
	d = 0 if d==null else int(d)
	d = wrapi(d,0,360)
	self.rotation_degrees = Vector3( 0, d, 0 )
	self.dir = d
	hud.update_compass()


func coord_to_world(p):
	return start_pos.translation + Vector3(p.x * 3, 0, -p.y * 3)


func set_pos( cx, cy ):
	loc.x = cx
	loc.y = cy
	self.translation = coord_to_world(loc)

	

func reset_location():
	set_pos( 0, 0 ); last_loc = null
	set_dir( 270 ); last_dir = null


func over_exit() -> bool:
	if dungeon.current_level.depth <1 : 
		return false
	var cell = dungeon.grid.get_cell(loc.x, loc.y)
	return cell.item and cell.item.name=="exit"


func is_fighting(): 
	return active_action and active_action.get_name()=="Combat"


func is_moving(): 
	return active_action and active_action.get_name()=="Move"


func dead(): 
	return health<1 or mind<1



func dir_name() -> String:
	if dir > 315 or dir < 45: return "north"
	if dir >= 45 and dir < 135: return "west"
	if dir >= 135 and dir < 225: return "south"
	return "east"



func _input(evt):
	if active_action:
		active_action.input(evt)
		return
	if not (evt is InputEventKey and evt.pressed): 
		return		
	match evt.scancode:
		KEY_Q: glance.start(90)
		KEY_E: glance.start(-90)
		KEY_TAB: show_map.start()
		KEY_X: 
			if over_exit():
				dungeon.use_exit()
		KEY_R: rest()
		KEY_T: use_item()
		KEY_F5: reset_location()
		KEY_F6: dungeon.go_next_level()
		KEY_SPACE: open_door()
		KEY_W: move.forward()
		KEY_S: move.backward()
		KEY_A: turn.start( 90 )
		KEY_D: turn.start( -90 )
		KEY_F: self.attack()



func _process(delta):
	if active_action:
		active_action.process(delta)


func action_complete( kind ):
	active_action = null
	match kind:
		"Move":
			dungeon.grid.get_cell(loc.x, loc.y).on_enter(game.player)
		"Turn": pass
		"Glance": pass
		"die": pass
		"win": 
			needs_rest = true
			if dead() and not cheat_death(): 
				game.game_over()
	hud.update()


func cheat_death():
	if resurrected: return false
	if gold > 500:
		resurrected = true
		for n in range(1,10): self.inventory[n] = null
		hud.update_pack()
		health = health_max
		mind = mind_max
		active_action = null
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
	return item_node.item if item_node else null


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
func take_item( item ):
	if item:
		if item.kind=="container" and open_container(item):
			return true
		match item.kind:
			"treasure": win()
			"money": 	
				gold += item.stat1
			"quiver": 	arrows += 6
			"food":		food += 6
			_: return false
		set_item_at_feet(null)
		return true
	return false

var magic_items = ["small_ring", "ring", "tome", "potion", "small_potion"]


var magic_sound = preload("res://data/sounds/magic.wav")


func use_item():
	if not right_hand:
		return
	var at_feet = item_at_feet()
	if right_hand.name=="small_potion":
		if right_hand.power in [1,3]:
			health = health_max
		if right_hand.power in [2,3]:
			mind = mind_max
		play_sound(magic_sound)
		right_hand = null
	elif right_hand.name=="potion":
		if right_hand.power in [1,3]:
			health_max += 10
		if right_hand.power in [2,3]:
			mind_max += 5
		play_sound(magic_sound)
		right_hand = null
	elif right_hand.name=="tome": # does nothing right now :/
		if right_hand.power==1: pass
		if right_hand.power==2: pass
		if right_hand.power==3: pass
	elif right_hand.name=="key":
		if at_feet and at_feet.can_open_with(right_hand):
			open_container(at_feet)
	elif right_hand.name=="ring":  # make armor items actually wearable and not just used
		if right_hand.power in [1,3]:
			m_armor += 5
		elif right_hand.power in [2,3]:
			m_armor += 5
		right_hand = null
	elif right_hand.name=="ring":
		if right_hand.power in [1,3]:
			m_armor += 5
		elif right_hand.power in [2,3]:
			m_armor += 7
		right_hand = null
	elif right_hand.name in [ "helmet", "breastplate" ]: 
		armor = max( armor, right_hand.stat1 )
		m_armor = max( m_armor, right_hand.stat2 )
		right_hand= null
	hud.update_pack()
	hud.update_stats()



func play_sound(sample):
	audio.stream = sample
	audio.play()
	

func open_door():
	var w = wall_ahead()
	if w:
		w.activate()


func enter_gate(kind:String):
	audio.stream = load("res://data/sounds/magic.wav")
	audio.play()
	var cell = grid.get_cell(loc.x, loc.y)
	cell.set_gate(null)
	print("Entered gate:" + kind)


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


func cell_ahead():
	var p = loc + face_vector()
	return dungeon.grid.get_cell(p.x, p.y)

func wall_ahead():
	return dungeon.grid.get_wall(loc, face_vector() )

func wall_behind():
	return dungeon.grid.get_wall(loc, -face_vector() )

func coord_ahead() -> Vector2: 
	return loc + face_vector() 
	
func coord_behind() -> Vector2: 
	return loc + -face_vector() 
	
func coord_right() -> Vector2: 
	var f = face_vector()
	return Vector2( f.y, -f.x )

func coord_left() -> Vector2: 
	var f = face_vector()
	return Vector2( -f.y, f.x )


func start_combat( pos: Vector2, ang : int ) -> bool:
	var cell = dungeon.grid.get_cell(pos.x ,pos.y)
	if cell and cell.enemy:
		combat.fight(cell.enemy, ang)
		return true
	return false


func check_for_monster():
	if start_combat(coord_ahead(), 0):
		return
	if randi() % 4 < 2: 
		return
	if start_combat(coord_left(), -90):
		return
	if start_combat(coord_right(), 90):
		return

	
func win():
	game.show_map()
	audio.stream = load("res://data/sounds/win2.wav")
	audio.play()

func attack():
	if is_fighting():
		self.attacking = true 
		return
	elif active_action!=null:
		return
	var fwd = coord_ahead()
	var cell_ahead = dungeon.grid.get_cell( fwd.x, fwd.y )
	if cell_ahead==null or cell_ahead.enemy==null:
		return
	var wall = dungeon.grid.get_wall(loc, fwd)
	if can_see(wall):
		combat.start( cell_ahead, fwd, 0 )

func rest():
	if food<1 or (not needs_rest): return
	needs_rest = false
	health_max += (war_exp / 2)
	war_exp = war_exp % 2
	mind_max += (mind_exp / 2)
	mind_exp = mind_exp % 2
	var health_gain = food * 3
	var mind_gain = food * 2	
	health = min( health+health_gain, health_max )
	mind = min( mind + mind_gain, mind_max )
	food -= 1	
	hud.update_stats()



func killed ( enemy ):
	if enemy.monster.kind=="war":
		war_exp += enemy.monster.power
	elif enemy.monster.kind=="magic":
		mind_exp += enemy.monster.power
	elif enemy.monster.kind=="both":
		war_exp += enemy.monster.power 
		mind_exp += enemy.monster.power 


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


func damage( enemy, weap ):
	var maxdmg = enemy.monster.max_damage
	if skill==1: maxdmg /= 4
	elif skill==2: maxdmg /=2
	var dmg = vary_amount( maxdmg, [ 5, 10, 15, 20 ] )
	var war_amt = apply_armor( dmg, war_armor() )
	var mind_amt = apply_armor( dmg, mind_armor() )
	if weap.name in ["axe", "dagger", "spear"]:
		health = max( self.health - war_amt, 0 )
	elif weap.name in ["fireball", "lightning", "small_fireball"]:
		mind = max( self.mind - mind_amt, 0 )

func mark_last():
	last_loc = loc
	last_dir = dir

func retreat():
	if last_loc==null: return false
	set_pos( last_loc.x, last_loc.y ); last_loc = null
	set_dir( last_dir ); last_dir = null
	if active_action: 
		active_action.complete()
	return true



func load_gate_level(gate):
	print("Gate:", gate.kind)


func transport_player(cell):
	match cell.gate.kind:
		"green":
			health_max += int(health_max / 2)
			mind_max = int(mind_max/2)	
		"blue":
			health_max = int(health_max/2)
			mind_max += int(mind_max / 2)	
	mind = min( mind, mind_max )
	health = min(health,health_max)
	needs_rest = true			
	load_gate_level(cell.gate)
	reset_location()
	hud.update_stats()

var food = 6;
var arrows = 12;
var health = 20
var health_max = 20
var mind = 8
var mind_max = 8
var armor = 0       # this is base war armor
var m_armor = 0     # this is base magic armor
var gold = 0

var war_exp = 0  # experience since last rest
var mind_exp = 0  # experience since last rest

var needs_rest = false # can we rest and recover health/mind?

var resurrected = false # has this player cheated death

var attacking = false  # is the player attempting to attack?

var skill = 1


func init( difficulty ):
	self.skill = difficulty
	gold = 0
	armor = 0
	m_armor = 0
	war_exp = 0
	mind_exp = 0
	food = 6
	arrows = 6 + 6 * (4 - skill)
	resurrected = false	
	for n in range(10): inventory[n] = null;
	right_hand = dungeon.item_list.find_item("bow")
	if skill==1: left_hand = dungeon.item_list.find_item("small_shield")
	if skill==1: 
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



func war_armor(): 
	var result = armor 
	if left_hand and left_hand.name in ["shield", "small_shield"]:
		result += left_hand.stat1
	return result

func mind_armor(): 
	var result = m_armor 
	if left_hand and left_hand.name in ["shield", "small_shield"]:
		result += left_hand.stat2
	return result

func war_dmg():
	if right_hand and right_hand.kind=="weapon":
		return right_hand.stat1
	return 0

func mind_dmg():
	if right_hand and right_hand.kind=="weapon":
		return right_hand.stat2
	return 0


