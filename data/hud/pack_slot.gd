extends Area2D;


@export var slot_num: int;


var hud;
@onready var sprite = $Sprite2D

func _ready():
	hud = find_parent("HUD")
	connect("input_event", Callable(self, "clicked_slot"))
	sprite.hide()
	
func clicked_slot(_view, evt, _shape_idx ):
	if evt is InputEventMouseButton and evt.pressed:
		hud.pack_slot_clicked( slot_num, evt.button_index )

func set_item( it ):
	if it==null: 
		sprite.hide()
	else:
		sprite.set_scale( Vector2(2, 2 ) )
		sprite.set_region_rect( it.img )
		sprite.set_modulate( it.color )
		sprite.show()
