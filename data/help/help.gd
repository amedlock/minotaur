extends Sprite

func _ready():
	self.hide()
	
func toggle():
	if self.is_visible():
		self.hide()
	else:
		self.show()
