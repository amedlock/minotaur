extends Node3D

@onready var anim = find_child("anim");
var raised = false


func _ready():
	raised = false;
	anim.connect("animation_finished", Callable(self, "anim_done"))
	
func is_moving(): 
	return anim.is_playing()

func activate():
	if anim.is_playing():
		return
	if raised:
		anim.play("Lower")
	else:
		anim.play("Raise")


func is_blocked():
	return not self.raised or anim.is_playing()

func anim_done(which):
	raised = which=="Raise"

func player_moved():
	if raised:
		self.activate()

var timer : Timer;

func add_timer():
	timer = Timer.new()
	self.add_child(timer)
	timer.one_shot = true
	timer.connect("timeout", Callable(self, "remove_timer"))
	timer.start(5.0)
	

func remove_timer():
	if self.raised:
		anim.play("Lower")
	if timer:
		self.remove_child(timer)
		timer = null



