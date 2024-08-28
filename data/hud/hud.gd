extends Node2D;

@onready var hp_disp = $Stats/HPDisplay;
@onready var mind_disp =$Stats/MindDisplay;
@onready var armor_disp = $Stats/ArmorDisplay;
@onready var damage_disp = $Stats/DamageDisplay;
@onready var gold_disp = $Stats/GoldDisplay;

@onready var food_disp = $Hands/FoodDisplay;
@onready var level_disp = $Hands/LevelDisplay;
@onready var arrow_disp = $Hands/ArrowsDisplay;
@onready var shield_sprite  = $Hands/background/Left/Sprite2D;
@onready var at_feet_sprite = $Hands/background/Feet/Sprite2D;
@onready var right_hand_sprite = $Hands/background/Right/Sprite2D;

@onready var pack = $Pack;

var armor_slots = {}


var player ;
var dungeon ;

@onready var compass = find_child("Compass")

var pack_slots = ["Slot1", "Slot2", "Slot3", "Slot4", "Slot5", 
					"Slot6", "Slot7", "Slot8", "Slot9" ]


func _ready():
	var game = get_tree().get_root().get_node("Game")
	assert( game != null )
	assert( compass != null )
	player = game.find_child("Player", true, false)
	dungeon = game.find_child("Dungeon", true, false )
	$Hands/background/Feet.connect("input_event", Callable(self, "clicked_feet"))
	#$Hands/background/Left.connect("input_event", Callable(self, "clicked_left"))
	$Hands/background/Right.connect("input_event", Callable(self, "clicked_right"))
	armor_slots['helmet'] = $ArmorItems/HelmetSprite
	armor_slots['breastplate'] = $ArmorItems/BreastplateSprite
	armor_slots['amulet'] = $ArmorItems/AmuletSprite
	armor_slots['shield'] = $Hands/background/Left/Sprite2D
	armor_slots['hand'] = $Hands/background/Right/Sprite2D
	armor_slots['feet'] = $Hands/background/Feet/Sprite2D

func calc_sprite_scale( src_w, src_h, dest_w, dest_h ):
	var wr = float(dest_w) / float(src_w);
	var hr = float(dest_h) / float(src_h)
	return Vector2(wr, hr)



func update_stats():
	level_disp.set_text( "Level: " + str( dungeon.current_level.depth ) )
	arrow_disp.set_text( "Arrows: " + str( player.arrows ) )
	food_disp.set_text( "Food: " + str( player.food ) )
	gold_disp.set_text( str( player.gold ) )
	hp_disp.set_text(  str( player.health ) + "/" + str( player.health_max ) )
	mind_disp.set_text( str( player.mind ) + "/" + str( player.mind_max ) )
	update_damage()


func update_damage():
	armor_disp.set_text( str( player.war_armor() ) + "/" + str(player.mind_armor()) )
	damage_disp.set_text( str( player.war_dmg() ) + "/" + str( player.mind_dmg() ) )


func update_pack():
	set_slot_item("hand", player.right_hand)
	var at_feet = player.item_at_feet()
	set_slot_item("feet", at_feet)
	set_slot_item("shield", player.shield)
	update_damage()
	var index = 1
	for i_name in pack_slots:
		var p = pack.find_child( i_name )
		if player.inventory.has( index ):
			p.set_item( player.inventory[index] )
		else:
			p.set_item( null )
		index += 1


func update():
	update_pack()
	update_stats()


func set_slot_item(which: String, item):
	var target = armor_slots[which]
	if item==null:
		target.visible = false
	else:
		target.set_scale( calc_sprite_scale( 32, 32, 50, 50 ) )
		target.set_modulate( item.color if item.color!=null else Color( 0xffffffff ) )
		target.region_rect = item.img
		target.visible = true
		target.region_enabled = true


# events 

func pack_slot_clicked( slot, mbutton ):
	var cur = player.inventory[ slot ]
	match mbutton:
		MOUSE_BUTTON_LEFT:
			player.inventory[ slot ] = player.item_at_feet()
			player.set_item_at_feet(cur)
		MOUSE_BUTTON_RIGHT:
			player.inventory[ slot ] = player.right_hand
			player.right_hand = cur
	update_stats()
	update_pack()



func clicked_feet( _viewport, event, _shape_idx ):
	if not(event is InputEventMouseButton) or not event.pressed: 
		return
	player.use_or_take_item()
	update()


# alternate attack method, press F otherwise
func clicked_right( _viewport, event, _shape_idx ):
	if not (event is InputEventMouseButton and event.pressed):
		return
	if event.button_index == MOUSE_BUTTON_LEFT:
		player.attack_ahead()
	elif event.button_index == MOUSE_BUTTON_RIGHT:
		pass
	update()


# alternate attack method?
func clicked_left( _viewport, event, _shape_idx ):
	if not (event is InputEventMouseButton): 
		return
	if !event.pressed: 
		return;
	if event.button_index == MOUSE_BUTTON_LEFT:
		pass
	elif event.button_index==MOUSE_BUTTON_RIGHT:
		var left = player.left_hand
		player.left_hand = player.right_hand
		player.right_hand = left
	update()
	
