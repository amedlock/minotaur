extends Node2D;

var hp_disp ;
var mind_disp ;
var armor_disp;
var damage_disp;
var gold_disp;

var food_disp;
var level_disp;
var arrow_disp;
var left_hand ;
var at_feet;
var right_hand;

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
	left_hand = hands.find_node("Left").find_node("Sprite")
	right_hand = hands.find_node("Right").find_node("Sprite")
	at_feet = hands.find_node("Feet").find_node("Sprite")
	pack = find_node( "Pack" )
	



func calc_sprite_scale( src_w, src_h, dest_w, dest_h ):
	var wr = float(dest_w) / float(src_w);
	var hr = float(dest_h) / float(src_h)
	return Vector2(wr, hr)

onready var compass = find_node("Compass")
				
func update_compass():
	compass.set_rotd( player.dir )
								

func update_stats( ):
	level_disp.set_text( "Level: " + str( dungeon.current_depth() ) )
	arrow_disp.set_text( "Arrows: " + str( player.arrows ) )
	food_disp.set_text( "Food: " + str( player.food ) )
	gold_disp.set_text( str( player.gold ) )
	hp_disp.set_text(  str( player.health ) + "/" + str( player.health_max ) )
	mind_disp.set_text( str( player.mind ) + "/" + str( player.mind_max ) )
	update_damage()


func update_damage():
	armor_disp.set_text( str( player.war_armor() ) + "/" + str(player.mind_armor()) )
	damage_disp.set_text( str( player.war_dmg() ) + "/" + str( player.mind_dmg() ) )

	

func consume( it ):
	if it==null: return false
	if it.kind=="money":
		player.gold += it.stat1
		return true
	if it.name=="quiver":
		player.arrows += 6
		return true
	if it.name=="flour":
		player.food += 6
		return true
	return false

func check_money():
	if player.left_hand and player.left_hand.kind=="money":
		player.gold += player.left_hand.stat1
		player.left_hand = null		
	if player.right_hand and player.right_hand.kind=="money":
		player.gold += player.right_hand.stat1
		player.right_hand = null		

func update_pack():
	check_money()
	update_hand_slot( left_hand, player.left_hand )
	update_hand_slot( right_hand, player.right_hand )
	update_hand_slot( at_feet, dungeon.item_at_feet() )
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
	if item==null:
		dest.hide()
	else:
		dest.set_scale( calc_sprite_scale( 32, 32, 50, 50 ) )
		dest.set_region_rect( item.img )
		dest.set_modulate( item.color if item.color!=null else Color( 0xffffffff ) )
		dest.show()





# events 

func pack_slot_clicked( slot , button ):
	var cur = player.inventory[ slot ]
	if consume(cur):
		player.inventory[slot] = null
		update_stats()
	elif button==BUTTON_RIGHT:
		player.inventory[ slot ] = player.right_hand
		player.right_hand = cur
	elif button==BUTTON_LEFT:
		player.inventory[ slot ] = player.left_hand
		player.left_hand = cur
	player.attacking = false
	update_pack()

func _on_Feet_input_event( viewport, event, shape_idx ):
	if player.is_moving(): return 
	if event.type!=InputEvent.MOUSE_BUTTON: return
	if !event.pressed: return;
	var feet = dungeon.item_at_feet()
	if feet and feet.kind=="bag" and feet.needs_key==false:
		player.open_item( feet )
		update_pack()
		return
	if feet and feet.kind=="treasure":
		player.win()
	if consume(feet):
		feet= null
		update_stats()
	elif event.button_index == BUTTON_LEFT:
		var cur = player.left_hand
		player.left_hand = feet
		feet= cur
	elif event.button_index == BUTTON_RIGHT:
		var cur = player.right_hand
		player.right_hand = feet
		feet = cur;
	dungeon.set_cell_item( player.loc.x, player.loc.y, feet )
	player.attacking = false
	update_pack()

# alternate attack method, press F otherwise
func _on_Right_input_event( viewport, event, shape_idx ):
	if event.type!=InputEvent.MOUSE_BUTTON: return
	if !event.pressed: return;
	if event.button_index == BUTTON_LEFT:
		player.attack()
	elif event.button_index == BUTTON_RIGHT:
		var left = player.left_hand
		player.left_hand = player.right_hand
		player.right_hand = left
		player.attacking = false
	update_pack()


# alternate attack method?
func _on_Left_input_event( viewport, event, shape_idx ):
	if event.type!=InputEvent.MOUSE_BUTTON: return
	if !event.pressed: return;
	if event.button_index == BUTTON_LEFT:
		pass
	elif event.button_index==BUTTON_RIGHT:
		var left = player.left_hand
		player.left_hand = player.right_hand
		player.right_hand = left
		player.attacking = false
	update_pack()
	
	

