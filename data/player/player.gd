extends Spatial;

enum PlayerState {
	IDLE,  		# default state
	TURNING,
	MOVING,
	GLANCING,
	COMBAT,
	WAITING,
	WON,
	LOST
}


var player_state = PlayerState.IDLE;



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

onready var combat = $combat
onready var hud = $Camera/HUD
onready var audio = $Audio



func _ready():
	dungeon = get_parent()
	game = dungeon.get_parent()
	start_pos = dungeon.find_node("StartPos")
	grid = dungeon.find_node("Grid")
	item_list = dungeon.find_node("ItemList")
	map_view = game.find_node("MapView")
	reset_location()
	self.player_state = PlayerState.IDLE
	self.disable()


func enable():
	self.show()	
	hud.update()


func disable():
	self.hide()



func _input(evt):
	if $PlayerControl.tween.is_active():
		return
		
	match player_state:
		PlayerState.IDLE:
			if Input.is_action_just_pressed("forward"):
				$PlayerControl.move_forward()
			elif Input.is_action_just_pressed("left"):
				$PlayerControl.turn_player(90)
			elif Input.is_action_just_pressed("right"):
				$PlayerControl.turn_player(-90)
			elif Input.is_action_just_pressed("back"):
				$PlayerControl.move_back()
			elif Input.is_action_just_pressed("look_left"):
				$PlayerControl.glance(90)
			elif Input.is_action_just_pressed("look_right"):
				$PlayerControl.glance(-90)
			elif Input.is_action_just_pressed("open"):
				self.open_door()
			elif Input.is_action_just_pressed("rest"):
				self.rest()
			elif Input.is_action_just_pressed("use"):
				self.use_item()
			elif Input.is_action_just_pressed("descend"):
				self.use_exit()
							
		PlayerState.TURNING: 
			pass
			
		PlayerState.MOVING:
			# allow rest here?
			pass
			
		PlayerState.GLANCING:
			if !Input.is_action_pressed("look_left") and !Input.is_action_pressed("look_right"):
				$PlayerControl.unglance()
			
		PlayerState.COMBAT:
			pass
			
		PlayerState.WAITING:
			pass
			
			
	if evt is InputEventKey:
		debug_keys(evt)



func reset_location():
	self.translation = start_pos.translation
	self.rotation_degrees.y = 270


func coord_to_world(p : Vector2) -> Vector3:
	return start_pos.translation + Vector3(p.x * 3, 0, -p.y * 3)


# get the world coords for the player
func get_coord() -> Vector2:
	var loc = (self.translation - start_pos.translation).abs()
	return Vector2(round(loc.x / 3.0), round(loc.z / 3.0))


#player direction in degrees
func get_dir() -> int:
	var val = int( fposmod(rotation_degrees.y, 360))
	if not val in [0,90,180,270]:
		print_debug("Dir=" + str(val))	
	return val


func get_forward_vector() -> Vector2:
	var bz = transform.basis.z
	return Vector2(round(bz.x), round(bz.z))


func over_exit() -> bool:
	if dungeon.current_level.depth<1:
		return false
	var coord = get_coord()
	var cell = dungeon.grid.get_cell(coord.x, coord.y)
	return cell.item and cell.item.item_info.name=="ladder"



func is_dead() -> bool:
	return health<1 or mind<1


func dir_name() -> String:
	match get_dir():
		0: return "north"
		90: return "west"
		180: return "south"
		_: return "east"







func use_exit():
	if player_state==PlayerState.IDLE and over_exit():
		if dungeon.use_exit():
			$PlayerControl.reset()


func show_map():
	game.show_map()


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



func cheat_death():
	if resurrected: 
		return false
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
	var coord = get_coord()
	grid.set_item( coord.x, coord.y , item)


func item_at_feet():
	var coord = get_coord()
	var item_node = grid.get_cell( coord.x,coord.y ).item
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
		$PlayerControl.reset()


func can_see( wall ):
	return wall==null or (not wall.is_blocked())


func has_weapon():
	return right_hand!=null and right_hand.kind=="weapon"


func face_vector() -> Vector2:
	var fwd = -transform.basis.z.normalized()
	return  Vector2(round(fwd.x), -round(fwd.z))


func coord_ahead() -> Vector2:
	return get_coord() + face_vector()

func coord_behind() -> Vector2:
	return get_coord() - face_vector()

func coord_right() -> Vector2:
	var fv = face_vector()
	return get_coord() + Vector2(fv.y, -fv.x)

func coord_left() -> Vector2:
	var fv = face_vector()
	return get_coord() - Vector2(fv.y, -fv.x)

func current_cell():
	var p = get_coord()
	return dungeon.grid.get_cell(p.x, p.y)


func cell_ahead():
	var p = coord_ahead()
	return dungeon.grid.get_cell(p.x, p.y)

func cell_behind():
	var p = coord_behind()
	return dungeon.grid.get_cell(p.x, p.y)

func cell_left():
	var p = coord_left()
	return dungeon.grid.get_cell(p.x, p.y)

func cell_right():
	var c = coord_right()
	return dungeon.grid.get_cell(c.x, c.y)

func wall_ahead() -> Spatial:
	var c = get_coord()
	return dungeon.grid.get_wall(c.x, c.y, get_dir())

func wall_behind() -> Spatial:
	var c = get_coord()
	var rdir = posmod(get_dir()-180, 360)
	return dungeon.grid.get_wall(c.x, c.y, rdir)



func start_combat( cell ) -> bool:
	if player_state!=PlayerState.IDLE:
		return false
	return (cell and cell.enemy)
#	var pos = cell.grid_pos();
#	var wall = dungeon.grid.get_wall(loc, pos)
#	if wall and wall.is_blocked():
#		return false
#	var ang = int(-rad2deg( (pos - loc).angle_to(face_vector()) ))
#	assert( ang in [90, 0, -90])
#	if ang!=0:
#		idle.turn_to(ang)
#		idle.connect('completed', combat, 'start', [cell], CONNECT_ONESHOT)
#	return true


func end_combat(outcome,enemy):
	match outcome:
		"win":
			killed(enemy)
			enemy.die()
			needs_rest = true
			self.player_state=PlayerState.IDLE
		"die":
			if is_dead() and not cheat_death():
				self.player_state = PlayerState.LOST
		_: assert(false)
	hud.update()
	


func retreat():
	$PlayerControl.flee()


# tries to initiate combat ahead of player
func attack_ahead():
	if player_state != PlayerState.IDLE:
		return
	var ahead = cell_ahead()
	if ahead==null or ahead.enemy==null:
		return
	var wall = wall_ahead()
	if can_see(wall):
		combat.attacking = true
		self.player_state= PlayerState.COMBAT
		combat.start(ahead, true)
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
	self.player_state = PlayerState.WON


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
	health = int( min( health + int(health_max * 2.0 / 3.0), health_max ))
	mind = int( min( mind + int(mind_max * 2.0 / 3.0), mind_max ))
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


