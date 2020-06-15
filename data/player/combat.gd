extends Node

const Enemy_Delay = 2
const Player_Delay = 1

var player
var dungeon
var game
var item_list
var audio

onready var player_anim = $PlayerAnim
onready var player_timer = $PlayerTimer
onready var player_weapon = $PlayerWeapon
onready var player_audio = $PlayerWeapon/Audio


onready var enemy_anim = $EnemyAnim
onready var enemy_timer = $EnemyTimer
onready var enemy_weapon = $EnemyWeapon
onready var enemy_audio = $EnemyWeapon/Audio


var player_item # player item being fired
var enemy_item # enemy item being fired

var cell : Spatial = null
var enemy : Spatial = null # which enemy, set in start(...)
var monster 

var retreating = false # has player started a retreat


func _ready():
	player = get_parent()
	dungeon = player.get_parent()
	game = player.get_parent()
	audio = game.find_node("Audio")
	item_list = dungeon.find_node("ItemList")
	enemy_anim = player.find_node("EnemyAnim")
	enemy_anim.connect("animation_finished", self, "enemy_fire_done" )
	enemy_weapon = player.find_node("EnemyWeapon")
	enemy_audio = enemy_weapon.find_node("Audio")
	player_anim = player.find_node("PlayerAnim")
	player_anim.connect("animation_finished", self, "player_fire_done" )
	player_weapon = player.find_node("PlayerWeapon")
	enemy_weapon.visible = false
	player_weapon.visible = false



func start(e_cell):
	self.cell = e_cell
	self.enemy = e_cell.enemy
	monster = enemy.monster
	enemy_weapon.visible = false
	player_weapon.visible = false
	player.action = null
	retreating = false


func player_turn():
	if enemy.dead():
		return
	if player_anim.is_playing():
		return
	match player.action:
		"attack": 
			player_fire()
		"retreat":
			player.action  = null
			retreating = true
			player_timer.start(Enemy_Delay)
	

func enemy_turn():
	if player.dead():
		return
	if enemy_anim.is_playing():
		return
	enemy_fire()
	enemy_timer.start(Enemy_Delay)



# called by player.gd
func next_turn():
	if player.tween.is_active():
		return
	if player.dead():
		player.end_combat("lost", enemy)
		return
	if enemy.dead():
		enemy.die()
		player.end_combat("won", enemy)
		return

	if enemy_timer.is_stopped():
		enemy_turn()
	
	if player_timer.is_stopped():
		if retreating:
			player.retreat()
		else:
			player_turn()


var broken


func get_sound_fx( item ):
	if item==null: return null;
	if item.name in ["fireball", "small_fireball"]:
		return load("res://data/sounds/fireball.wav")
	if item.name in ["wand", "staff", "scroll", "book"]:
		return load("res://data/sounds/lightning.wav")
	return null


func player_fire():
	if not player_timer.is_stopped() or player_anim.is_playing():
		return
	player.action  = null
	player_item = player.right_hand
	if player_item==null or player_item.kind!="weapon": 
		return
	broken = false
	var missile = item_list.missile_for(player_item)
	var fx = get_sound_fx( player_item )
	if player_item.name in ["bow", "crossbow"]:
		if player.arrows<1: return
		player_weapon.set_region_rect( missile )	
		player.arrows -= 1
		broken = (randi() % 30) == 29
	elif player_item.name in ["scroll", "book", "wand", "staff"]:
		player_weapon.set_region_rect( missile )
		broken = (randi() % 25) == 24
	else:
		player.right_hand = null
		player_weapon.set_region_rect( player_item.img )	
	player_weapon.set_modulate( player_item.color )
	if player_item.name in  ["axe", "dagger", "fireball", "small_fireball"]:
		player_anim.play("SpinFire")
	else:	
		player_anim.play("Fire")
	if broken and dungeon.current_level.depth > 2:  # clear out of the players hand
		player.right_hand = null
	player.hud.update_pack()
	if fx!=null:
		audio.stream = fx
		audio.play()
	player_timer.start(Player_Delay)


func player_fire_done(_which):
	audio.stream = preload("res://data/sounds/hit.wav")
	audio.play()
	enemy.damage( player_item ) 	# remove it
	if broken:
		player_item = null


func choose_enemy_weapon():
	var items = []
	if monster.kind in ["magic", "both"]:
		items = item_list.find_items("weapon", ["lighting", "fireball", "small_fireball"], [1,2] )
	else:
		items = item_list.find_items("weapon", ["axe", "dagger", "spear"], [1,2] )
	assert( items.size() > 0 )
	return items[ randi() % items.size() ]


func enemy_fire():
	enemy_item = choose_enemy_weapon()
	enemy_weapon.set_region_rect( enemy_item.img )
	enemy_weapon.set_modulate( enemy_item.color )
	var fx = get_sound_fx( enemy_item )
	if fx!=null:
		enemy_audio.stream = fx
		enemy_audio.play()
	if enemy_item.name in ["axe", "dagger", "fireball", "small_fireball"]:
		enemy_anim.play("SpinFire")
	else:
		enemy_anim.play("Fire")
	enemy_timer.start(Enemy_Delay)
		

func enemy_fire_done(_which):
	enemy_audio.stream = load("res://data/sounds/hit.wav")
	enemy_audio.play()
	player.damage( enemy, enemy_item )
	player.hud.update_stats()



	
