extends Spatial;


enum Movement { Forward, Back, TurnLeft, TurnRight, MoveLeft, MoveRight, LookLeft, LookRight }

const MoveDir = {
  0 : Vector2(0,-1),
  90  : Vector2(-1,0),
  180  : Vector2(0,1),
  270  : Vector2(1,0)
}



var loc = Vector2(0, 0)  # coordinates of player x and z
var dir = 270;

const eye_height = 1.1
var grid_start ;

var enemy_near = [null,null,null,null] ; # danger close!

var wall_ahead = null;  # set by dungeon.update_path_info()
var wall_behind = null;
var wall_left = null
var wall_right = null

var outer_wall_ahead = false ;
var outer_wall_behind = false;

var left_hand;
var right_hand;

var inventory = {1:null, 2:null, 3:null, 4:null, 5:null, 6:null, 7:null, 8:null, 9:null }

# references to game, hud and dungeon nodes
var dungeon = null;
var hud = null;
var audio = null;
var game  = null;
var map 
var item_list


var last_loc
var last_dir
var active_action = null


func enable():
	self.show()
	self.set_process_input( true )
	update_stats()


func disable():
	self.hide()
	self.set_process_input( false )


func _ready():
	game = get_parent()
	map = game.find_node("Map")
	audio = find_node("Audio")
	dungeon = game.find_node("Dungeon")
	item_list = dungeon.find_node("ItemList")
	hud = self.find_node( "HUD" )
	grid_start = dungeon.maze_origin + Vector3( 1.5, 1.25, 1.5 )
	set_process(true);
	set_process_input(true)
	self.hide()


func set_dir( d ):	
	d = 0 if d==null else int(d)
	while d < 0: d += 360
	dir = d % 360
	set_rotation( Vector3( 0, deg2rad(dir), 0 ) )
	hud.update_compass()
	map.update_player(loc.x, loc.y, dir)

func coord_to_world(v2):
	return grid_start + Vector3( v2.x * 3, 0 , v2.y * 3)

func set_pos( cx, cy ):
	loc.x = cx
	loc.y = cy
	self.set_translation( coord_to_world( loc )  );	
	map.update_player(cx, cy, dir)
	

func move_dir():
	while dir<0: dir += 360	
	dir = dir % 360
	dir = int( dir / 90 ) * 90
	return MoveDir[dir]

func reset_location():
	set_pos( 0, 0 ); last_loc = null
	set_dir( 270 ); last_dir = null
	update_path()

func over_exit():
	if dungeon.maze_number <1 : return false
	var info = dungeon.current_level_info()
	return info.exit.x == loc.x and info.exit.y==loc.y


func is_fighting(): return active_action and active_action.get_name()=="Combat"

func is_moving(): return active_action and active_action.get_name()=="Move"

func dead(): return health<1 or mind<1


func _input(evt):
	if active_action:
		active_action.input( evt )
		return	
	if not (evt is InputEventKey and evt.pressed): return
	if evt.scancode==KEY_Q:
		find_node("Glance").start( 90 )
		return
	elif evt.scancode==KEY_E:
		find_node("Glance").start(  -90 )
		return
	if evt.scancode==KEY_TAB:
		find_node("ViewMap").start()
	if evt.scancode==KEY_X and over_exit():
		dungeon.use_exit(loc.x, loc.y)
	if Input.is_key_pressed(KEY_R):
		rest()
	if Input.is_key_pressed(KEY_T):
		use_item()
	if Input.is_key_pressed(KEY_F5):
		reset_location()		
	if Input.is_key_pressed(KEY_F6):
		var li = dungeon.current_level_info()
		dungeon.use_exit( li.exit.x, li.exit.y )
	if Input.is_key_pressed(KEY_SPACE):
		open_door()		
	if Input.is_key_pressed(KEY_W):
		find_node("Move").forward( loc, move_dir() )
	elif Input.is_key_pressed(KEY_S):
		find_node("Move").backward( loc, move_dir() )
	elif Input.is_key_pressed(KEY_A):
		find_node("Turn").start( dir, 90 )
	elif Input.is_key_pressed(KEY_D):
		find_node("Turn").start( dir, -90 )
	elif Input.is_key_pressed(KEY_F):
		self.attack()

func _process(dt):
	if active_action:
		active_action.process(dt)


func action_complete( kind ):
	active_action = null
	if kind=="Move":
		update_path()
		if check_for_gate(): return
		else: check_for_monster()
	if kind=="Turn":
		update_path()
	if kind=="Glance":
		pass
	if kind=="Combat":
		needs_rest = true
		update_path()
		if dead() and not cheat_death(): 
			game.game_over()


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


func update_path():
	var last = self.wall_ahead
	dungeon.update_path_info(loc, move_dir() )
	if last!=null and last!=wall_ahead: last.player_moved()
	update_stats()

func open_item(it):
	var valuable = item_list.get_container_loot(it, dungeon.current_depth() )
	dungeon.set_cell_item( loc.x, loc.y, valuable )


var magic_items = ["small_ring", "ring", "tome", "potion", "small_potion"]

func use_item():
	if not right_hand:
		return
	var fx = null
	if right_hand.name in magic_items:
		fx = load("res://data/sounds/magic.wav")
	if right_hand.name=="small_potion":
		if right_hand.power in [1,3]:
			health = health_max
		if right_hand.power in [2,3]:
			mind = mind_max
		right_hand = null
	elif right_hand.name=="potion":
		if right_hand.power in [1,3]:
			health_max += 10
		if right_hand.power in [2,3]:
			mind_max += 5
		right_hand = null
	elif right_hand.name=="tome":
		if right_hand.power==1: pass
		if right_hand.power==2: pass
		if right_hand.power==3: pass
	elif right_hand.name=="key":
		var feet = dungeon.item_at_feet()
		if feet and feet.needs_key and right_hand.power >= feet.power:
			open_item( feet )
	elif right_hand.name=="ring":
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
	if fx!=null: 
		audio.stream = fx
		audio.play()

func open_door():
	if wall_ahead!=null:
		wall_ahead.activate()

func can_see( wall ):
	if wall==null: return true
	return not wall.is_blocked()

func has_weapon():
	return right_hand!=null and right_hand.kind=="weapon"


func coord_ahead(): return loc + move_dir()
func coord_behind(): return loc + -move_dir()
func coord_right(): 
	var f = coord_ahead()
	return Vector2( f.y, f.x )

func coord_left(): 
	var f = coord_ahead()
	return Vector2( f.y, -f.x )


func check_for_monster():
	var combat = find_node("Combat")
	if enemy_near[0] and can_see( wall_ahead ):
		combat.start( enemy_near[0], coord_ahead(), 0 )
	if randi() % 4 < 2: return
	if enemy_near[1] and can_see( wall_right ):  
		combat.start( enemy_near[1], coord_right(), -90  )
	if enemy_near[3] and can_see( wall_left ):  
		combat.start( enemy_near[3] , coord_left(), 90 )

	
func win():
	game.show_map()
	audio.stream = load("res://data/sounds/win2.wav")
	audio.play()

func attack():
	if is_fighting():
		self.attacking = true 
	elif active_action!=null:
		return
	elif enemy_near[0] and can_see( wall_ahead ):
		var combat = find_node("Combat")
		var coord = self.loc + MoveDir[self.dir]
		combat.start( enemy_near[0], coord, 0 )		

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
	update_path()
	if active_action: active_action.complete()
	return true


func check_for_gate():
	var c = dungeon.get_cell( loc.x, loc.y )
	if c==null or c.gate<0: return false
	transport_player( c.gate )
	audio.stream = load("res://data/sounds/magic.wav")
	audio.play()
	return true



func transport_player(g):
	if loc.x==dungeon.WIDTH-1 and loc.y==0:
		dungeon.load_next_level()
	elif loc.x==0 and loc.y==dungeon.HEIGHT-1:
		dungeon.load_prev_level()
	reset_location()
	if g==dungeon.GateType.Green:
		health_max += int(health_max / 2)
		mind_max = int(mind_max/2)
	if g==dungeon.GateType.Blue:
		health_max = int(health_max/2)
		mind_max += int(mind_max / 2)
	mind = min( mind, mind_max )
	health = min(health,health_max)
	needs_rest = true
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


func init( skill ):
	self.skill = skill
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
		mind = 6
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

func update_stats():
	hud.update_stats()
	hud.update_pack()

