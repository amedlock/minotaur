extends Sprite3D


@onready var anim = $Animation


func start():
	self.visible = true
	anim.play("Puff")
	await anim.animation_finished
	self.get_parent().remove_child(self)
	self.queue_free()


