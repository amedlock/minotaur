extends Node3D;

const Up = Vector3(0,1,0)

@export var kind = "war" # ( String, "magic", "both", "war" )

var min_size = 0.02
var max_size = 0.03


@onready var sprite = $Sprite3D

#func _process(dt):
#	sprite.pixel_size = wrapf( sprite.pixel_size + (dt*0.005), min_size, max_size)
