extends "action.gd"

onready var player = get_parent()
var dungeon
var game
var item_list
var audio

var player_anim
var player_weapon
var player_item # actual item being fired

var enemy_anim 
var enemy_weapon
var enemy_audio
var enemy_item # enemy item being fired

var mx # monster grid coords
var my 

var turn_time = 0.5
var timer = 0.0  # can player attack yet?
var timer2 = 0.0 # can enemy attack yet
var enemy = null # which enemy
var retreating = false # retreat from combat?


# for turning the player to face enemy
var turning = false
var facing = null 
var delta  = null


func _ready():
	dungeon = player.get_parent().find_node("Dungeon")
	audio = player.find_node("Audio")
	game = dungeon.get_parent()	
	item_list = dungeon.find_node("ItemList")
	enemy_anim = player.find_node("EnemyAnim")
	enemy_anim.connect("finished", self, "enemy_fire_done" )
	enemy_weapon = player.find_node("EnemyWeapon")
	enemy_audio = enemy_weapon.find_node("Audio")
	player_anim = player.find_node("PlayerAnim")
	player_anim.connect("finished", self, "player_fire_done" )
	player_weapon = player.find_node("PlayerWeapon")


	
func start( enemy, eloc, dir_change ):
	if enemy==null: return
	if player.active_action: return
	mx = eloc.x
	my = eloc.y
	timer = 0
	timer2 = 0	
	player.active_action = self
	retreating = false
	self.enemy = enemy
	if dir_change!=0:
		self.facing = player.dir
		self.delta = dir_change
		turning = true
		timer = 0


func input(evt):
	if evt.type!=InputEvent.KEY or (not evt.pressed):
		return
	if self.timer>0:
		return
	if retreating==false and evt.scancode==KEY_S:
		self.retreating = true
		self.timer = 0.75 
	if evt.scancode==KEY_F:
		self.timer = 0.75
		player.attacking = true
	pass
	
func process(dt):
	if turning:
		turn_player(dt)
		return 
	timer = max( self.timer - dt, 0 )
	timer2 = max( self.timer2 - dt, 0 )			
	if player_anim.is_playing() or enemy_anim.is_playing():
		return
	if player.dead():
		complete()
	if enemy.dead():
		enemy.die()
		player.killed( enemy )
		if enemy.monster.name=="minotaur": 
			dungeon.add_final( enemy.x, enemy.y )
		complete()
	if timer==0 and player.attacking and player.has_weapon():
		player_fire()
	if timer2==0:
		enemy_fire()
	elif retreating:
		player.retreat()

	
	
func turn_player(dt):
	timer = timer+dt
	var ratio = min(timer, turn_time) / turn_time;
	player.set_dir( facing + (delta * ratio) )
	if timer >= turn_time: 
		timer = 0
		turning = false
		delta = null
		facing = null
		player.update_path()		

var broken


func get_sound_fx( item ):
	if item==null: return null;
	if item.name in ["fireball", "small_fireball"]:
		return "fireball"
	if item.name in ["lightning", "small_lightning", "scroll", "book"]:
		return "lightning"
	return null


func player_fire():
	if player.dead() or enemy.dead(): return 
	if player_anim.is_playing(): return
	player_item = player.right_hand
	if player_item==null or player_item.kind!="weapon": return
	broken = false
	var fx = get_sound_fx( player_item )
	if player_item.name in ["bow", "crossbow"]:
		if player.arrows<1: return
		player_weapon.set_region_rect( player_item.missile )	
		player.arrows -= 1
		broken = (randi() % 30) == 29
	elif player_item.name in ["scroll", "book"]:
		player_weapon.set_region_rect( player_item.missile )
		broken = (randi() % 25) == 24
	else:
		player.right_hand = null
		player_weapon.set_region_rect( player_item.img )	
	player_weapon.set_modulate( player_item.color )
	if player_item.name in  ["axe", "dagger", "fireball", "small_fireball"]:
		player_anim.play("SpinFire")
	else:	
		player_anim.play("Fire")
	if broken and dungeon.maze_number > 2:  # clear out of the players hand
		player.right_hand = null
	player.hud.update_pack()
	if fx!=null: audio.play(fx,0)
	timer = 1.5	
	

func player_fire_done():
	if get_sound_fx( player_item )==null: audio.play( "hit" )
	enemy.damage( player_item ) 	# remove it
	if broken: player_item = null
	player_item = null
	player.attacking = false
	audio.stop(0)


func choose_enemy_weapon(monster):
	var items = []
	if monster.kind in ["magic", "both"]:
		items = item_list.find_items("weapon", ["lighting", "fireball", "small_fireball"], [1,2] )
	else:
		items = item_list.find_items("weapon", ["axe", "dagger", "spear"], [1,2] )
	assert( items.size() > 0 )
	return items[ randi() % items.size() ]

				
func enemy_fire():
	enemy_item = choose_enemy_weapon(enemy.monster)
	if player.dead() or enemy.dead(): return
	if enemy_anim.is_playing(): return
	timer2 = 2
	enemy_weapon.set_region_rect( enemy_item.img ) #Rect2( Vector2(0,0), Vector2(32,32)))
	enemy_weapon.set_modulate( enemy_item.color )
	var fx = get_sound_fx( enemy_item )
	if fx!=null : enemy_audio.play( fx, 0 )
	if enemy_item.name in ["axe", "dagger", "fireball", "small_fireball"]:
		enemy_anim.play("SpinFire")
	else:
		enemy_anim.play("Fire")

func enemy_fire_done():
	if get_sound_fx( enemy_item )==null: enemy_audio.play("hit")
	player.damage( enemy, enemy_item )
	player.hud.update_stats()
	if retreating and not player.dead():
		player.retreat()
	enemy_audio.stop(0)

	