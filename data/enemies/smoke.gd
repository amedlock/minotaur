extends Sprite3D



func start():
	self.visible = true
	$Animation.play("Puff")
	await $Animation.animation_finished
	self.get_parent().remove_child(self)
	self.queue_free()
