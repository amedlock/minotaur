extends Spatial

onready var anim = find_node("anim");
var raised = false


func _ready():
	raised = false;
	anim.connect("animation_finished", self, "anim_done" )
	
func is_moving(): 
	return anim.is_playing()

func activate():
	if anim.is_playing():
		return
	print("door activated: %s" % self.get_parent().debug_info() )
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
