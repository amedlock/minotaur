extends Sprite3D

var item ; # reference to the Item instance in item_list
var color ;


var magic_sound = preload("res://data/sounds/magic.wav")


func configure(item_info):
	self.item = item_info
	color = item_info.color
	set_region_rect( item_info.img )
	if item_info.color!=null:
		set_modulate( item_info.color )
	else:
		set_modulate( Color( 0xFFFFFFFF ) )




