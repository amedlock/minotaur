extends Node2D;

var hp_disp ;
var mind_disp ;
var armor_disp;
var damage_disp;
var gold_disp;

var food_disp;
var level_disp;
var arrow_disp;
var left_hand_sprite ;
var at_feet_sprite;
var right_hand_sprite;

var pack ;
var hands
var stats
var player ;
var item_list;
var dungeon ;

var pack_slots = ["Slot1", "Slot2", "Slot3", "Slot4", "Slot5", 
					"Slot6", "Slot7", "Slot8", "Slot9" ]

func _ready():
	var game = get_tree().get_root().get_node("Game")
	assert( game != null )
	player = game.find_node("Player")
	dungeon = game.find_node("Dungeon")
	item_list = dungeon.find_node("ItemList")
	stats = find_node("Stats")
	hands = find_node("Hands")
	hp_disp = stats.find_node("HPDisplay" )
	mind_disp = stats.find_node("MindDisplay" )
	armor_disp = stats.find_node("ArmorDisplay" )
	damage_disp = stats.find_node("DamageDisplay" )
	gold_disp = stats.find_node("GoldDisplay")
	level_disp = hands.find_node("LevelDisplay" )
	arrow_disp = hands.find_node("ArrowsDisplay")
	food_disp = hands.find_node("FoodDisplay")	
	left_hand_sprite = hands.find_node("Left").find_node("Sprite")
	right_hand_sprite = hands.find_node("Right").find_node("Sprite")
	at_feet_sprite = hands.find_node("Feet").find_node("Sprite")
	pack = find_node( "Pack" )
	hands.get_node("background/Feet").connect("input_event", self, "clicked_feet" )
	hands.get_node("background/Left").connect("input_event", self, "clicked_left" )
	hands.get_node("background/Right").connect("input_event", self, "clicked_right" )
	


func calc_sprite_scale( src_w, src_h, dest_w, dest_h ):
	var wr = float(dest_w) / float(src_w);
	var hr = float(dest_h) / float(src_h)
	return Vector2(wr, hr)

onready var compass = find_node("Compass")
				
func update_compass():
	compass.rotation_degrees = -player.dir 
								

func update_stats( ):
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

	
func check_money():
	if player.left_hand and player.left_hand.kind=="money":
		player.gold += player.left_hand.stat1
		player.left_hand = null
	if player.right_hand and player.right_hand.kind=="money":
		player.gold += player.right_hand.stat1
		player.right_hand = null

func update_pack():
	check_money()
	update_hand_slot( left_hand_sprite, player.left_hand )
	update_hand_slot( right_hand_sprite, player.right_hand )
	var at_feet = player.item_at_feet()
	update_hand_slot( at_feet_sprite, at_feet )
	update_damage()
	var index = 1
	for name in pack_slots:
		var p = pack.find_node( name )
		if player.inventory.has( index ):
			p.set_item( player.inventory[index] )
		else:
			p.set_item( null )
		index += 1


func update_hand_slot( dest, item ):
	if not item:
		dest.hide()
	else:
		dest.set_scale( calc_sprite_scale( 32, 32, 50, 50 ) )
		dest.set_region_rect( item.img )
		dest.set_modulate( item.color if item.color!=null else Color( 0xffffffff ) )
		dest.show()



func update():
	update_compass()
	update_pack()
	update_stats()


# events 

func pack_slot_clicked( slot , button ):
	var cur = player.inventory[ slot ]
	if button==BUTTON_RIGHT:
		player.inventory[ slot ] = player.right_hand
		player.right_hand = cur
	elif button==BUTTON_LEFT:
		player.inventory[ slot ] = player.left_hand
		player.left_hand = cur
	player.attacking = false
	update_stats()
	update_pack()



func clicked_feet( _viewport, event, _shape_idx ):
	if not(event is InputEventMouseButton) or not event.pressed: 
		return
	if player.is_moving(): 
		return 
	var item = player.item_at_feet()
	if item.name=="ladder":
		return
	if item and player.take_item(item):
		pass
	elif event.button_index == BUTTON_LEFT:
		var left_item = player.left_hand
		player.left_hand = item
		player.set_item_at_feet(left_item)
	elif event.button_index == BUTTON_RIGHT:
		var right_item = player.right_hand
		player.right_hand = item
		player.set_item_at_feet(right_item)
	player.attacking = false
	update()


# alternate attack method, press F otherwise
func clicked_right( _viewport, event, _shape_idx ):
	if not (event is InputEventMouseButton): 
		return
	if !event.pressed: 
		return;
	if event.button_index == BUTTON_LEFT:
		player.attack()
	elif event.button_index == BUTTON_RIGHT:
		var left = player.left_hand
		player.left_hand = player.right_hand
		player.right_hand = left
		player.attacking = false
	update()


# alternate attack method?
func clicked_left( _viewport, event, _shape_idx ):
	if not (event is InputEventMouseButton): 
		return
	if !event.pressed: 
		return;
	if event.button_index == BUTTON_LEFT:
		pass
	elif event.button_index==BUTTON_RIGHT:
		var left = player.left_hand
		player.left_hand = player.right_hand
		player.right_hand = left
		player.attacking = false
	update()
	


