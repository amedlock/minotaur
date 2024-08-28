extends Node

var player
var dungeon
var game
var audio

@export var game_db : Node

@onready var player_anim = $PlayerAnim
@onready var player_weapon = $PlayerWeapon
@onready var player_audio = $PlayerWeapon/Audio


@onready var enemy_anim = $EnemyAnim
@onready var enemy_weapon = $EnemyWeapon
@onready var enemy_audio = $EnemyWeapon/Audio


var player_item # player item being fired
var enemy_item # enemy item being fired

var enemy_cell : Node3D = null
var enemy : Node3D = null # which enemy, set in start(...)
var monster


# max time for each turn, in seconds
const Turn_Time = 2

# delay in a turn before enemy attacks
const Enemy_Delay = 0.75


# combat is a series of turns

# time in turn so far
var turn_elapsed = 0

#has player attacked this turn
var player_attack = false

#has player retreated this turn
var player_retreat = false

# enemy can only do one thing: attack
var enemy_attack = false



func reset_turn():
	turn_elapsed = 0
	player_attack = false
	enemy_attack = false
	player_retreat = false



func _ready():
	player = get_parent()
	dungeon = player.find_parent("Dungeon")
	game = player.find_parent("Game")
	audio = game.find_child("Audio")	
	enemy_anim = player.find_child("EnemyAnim")
	enemy_anim.connect("animation_finished", self.damage_player)
	enemy_weapon = player.find_child("EnemyWeapon")
	enemy_audio = enemy_weapon.find_child("Audio")
	player_anim = player.find_child("PlayerAnim")
	player_anim.connect("animation_finished", self.damage_enemy)
	player_weapon = player.find_child("PlayerWeapon")
	enemy_weapon.visible = false
	player_weapon.visible = false
	set_process(false)



func start(e_cell, _attack: bool):
	if (not e_cell.enemy) or (not e_cell.enemy.monster):
		return
	self.enemy_cell = e_cell
	self.enemy = e_cell.enemy
	player.player_state = player.PlayerState.COMBAT
	monster = enemy.monster
	enemy_weapon.visible = false
	player_weapon.visible = false
	reset_turn()
	set_process(true)
	if _attack:
		self.attack_monster()


func end_turn():
	if player_anim.is_playing() or enemy_anim.is_playing():
		return
	if player.is_dead():
		game.game_over()
		self.set_process(false)
	elif enemy.is_dead():
		enemy_cell.set_enemy( null )
		self.set_process(false)
		player.won_combat(enemy)
	elif player_retreat:
		player.retreat()
		self.set_process(false)
	else:
		reset_turn()


func _process(delta):
	turn_elapsed += delta
	if turn_elapsed >= Turn_Time or player.is_dead():
		end_turn()
		return

	if (not enemy_attack): # and (turn_elapsed >= Enemy_Delay):
		enemy_attack = true
		enemy_fire()

	if not (player_retreat or player_attack):
		if Input.is_action_just_pressed("back"):
			player_retreat = true

func attack_monster():
	if player_attack or player_retreat:
		return
	player_attack = true
	player_fire()


var broken


func get_sound_fx( item ):
	if item==null: return null;
	if item.name in ["fireball", "small_fireball"]:
		return load("res://data/sounds/fireball.wav")
	if item.name in ["wand", "staff", "scroll", "book"]:
		return load("res://data/sounds/lightning.wav")
	return null


func can_attack():
	return player.right_hand!=null and player.right_hand.kind=="weapon"

func player_fire():
	player_item = player.right_hand
	if player_item==null or player_item.kind!="weapon": 
		return
	broken = false
	var missile = game_db.missile_for(player_item)
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

	if fx!=null:
		audio.stream = fx
		audio.play()
	if broken and dungeon.current_level.depth > 2:  # clear out of the players hand
		player.right_hand = null
	player.hud.update_pack()


func damage_player(_anim):
	if enemy_item:
		player.damage(monster, enemy_item)

	
func damage_enemy(_anim):
	enemy.damage( player_item )
	if enemy.is_dead():
		enemy.die()
	if broken:
		player_item = null # remove it
		player.right_hand = null
		player.hud.update()


func choose_enemy_weapon():
	var items = []
	if monster.kind in ["magic", "both"]:
		items = game_db.search_items("weapon", ["lighting", "fireball", "small_fireball"], 1 )
	else:
		items = game_db.search_items("weapon", ["axe", "dagger", "spear"], 1 )
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
	player.damage( monster, enemy_item )
	player.hud.update_stats()
