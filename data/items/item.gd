extends Sprite3D

var item_info ; # reference to the Item instance in item_list

var magic_sound = preload("res://data/sounds/magic.wav")


func debug_info():
	return item_info.name


func configure(info):
	self.item_info = info
	set_region_rect( info.img )
	if info.color!=null:
		set_modulate( info.color )
	else:
		set_modulate( Color( 0xFFFFFFFF ) )




