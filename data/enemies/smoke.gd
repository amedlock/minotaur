extends Sprite3D


@onready var anim = $Animation


func start():
	self.visible = true
	anim.connect("animation_finished", Callable(self, "done"))
	anim.connect("animation_changed", Callable(self, "changed"))
	anim.play("Puff")


func done(_name):
	self.get_parent().remove_child(self)
	self.queue_free()
