extends Sprite

# class member variables go here, for example:
# var a = 2
# var b = "textvar"

func _ready():
	self.hide()
	
func toggle():
	if self.is_visible():
		self.hide()
	else:
		self.show()
