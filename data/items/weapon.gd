extends Sprite;


var  spins = false

var anim ;


func   fire():
	if spins:
		anim.play("attack")
	else:
		anim.play("spinning")

