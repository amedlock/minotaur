extends Node

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

var enemy_cell : Spatial = null
var enemy : Spatial = null # which enemy, set in start(...)
var monster


var attacking = false
var retreat = false

const Enemy_Delay = 2
const Player_Delay = 1


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
	set_process(false)



func start(e_cell, attack: bool):
	if (not e_cell.enemy) or (not e_cell.enemy.monster):
		return
	self.enemy_cell = e_cell
	self.enemy = e_cell.enemy
	assert( self.enemy != null )
	player.player_state = "combat"
	attacking = attack
	retreat = false
	monster = enemy.monster
	enemy_weapon.visible = false
	player_weapon.visible = false
	set_process(true)
	


func _process(_delta):
	if Input.is_action_just_pressed("attack"):
		attacking = true
	elif Input.is_action_just_pressed("back"):
		retreat = true
		
	player_turn()
	enemy_turn()
	check_combat_over()


func check_combat_over():
	if player.is_dead():
		player.end_combat("die", enemy_cell.enemy)
		return
	elif enemy.is_dead():
		player.end_combat("win", enemy_cell.enemy)
		return

func player_turn():
	if not player_timer.is_stopped() or player_anim.is_playing():
		return
	if retreat:
		player.retreat()
	elif attacking:
		attacking = false
		player_fire()


func enemy_turn():
	if enemy_timer.is_stopped() and not enemy_anim.is_playing():
		enemy_fire()
		
		
func animating():
	return player_anim.is_playing() or enemy_anim.is_playing()




var broken


func get_sound_fx( item ):
	if item==null: return null;
	if item.name in ["fireball", "small_fireball"]:
		return load("res://data/sounds/fireball.wav")
	if item.name in ["wand", "staff", "scroll", "book"]:
		return load("res://data/sounds/lightning.wav")
	return null


func player_fire():
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
	print("Player Fire done")
	enemy.damage( player_item )
	if broken:
		player_item = null # remove it


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
	player.damage( monster, enemy_item )
	player.hud.update_stats()



