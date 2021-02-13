extends Area2D;

var slots = []  # what is in each slot (0-8)

var war_colors = {
	"Wood": Color( 0xb7b666 ),
	"Iron": Color( 0xbc7812 ),
	"Steel": Color( 0x1d89b7 ),
	"Silver": Color( 0x92aab5  ),
	"Gold": Color( 0xe7ed50 ),
	"Platinum" : Color( 0xffffff )
}

var magic_colors = {
	"Blue": Color( 0x5b97f7 ),  # blue
	"Grey": Color( 0xadb2ba ),  # grey
	"White": Color( 0xffffff ), # white
	"Pink": Color( 0xf25cdb ),  # pink
	"Red": Color( 0xd30031 ),	# red
	"Purple" : Color( 0x9500d1 ) # purple
}

var item_sheet = preload("res://data/items/item_sheet.png" )

onready var hands_node = get_parent().find_node("Hands")

func scale_sprite( spr, img , w, h ):
	var img_size = img.get_size()
	var scale = Vector2((w / img_size.x), (h / img_size.y))
	spr.set_texture( img )
	spr.set_scale( scale )
	

func _ready():
	for n in range(1,10):
		var which = find_node("slot" + str(n) )
		slots.append( which )

