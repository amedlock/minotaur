extends Sprite3D;

var monster;  # reference to the Enemy object from enemy_list.gd  monsters[]
var health;  # hit points
var mind ;   # magic hit points
var drops; # item dropped when killed, from item_list.gd  items[]

var x      # grid coordinates
var y 

var smoke = preload("smoke.tscn")

var enemies

func _ready():
	enemies = self.get_parent()

func damage( item ):
	if item!=null:
		self.health = max( self.health-item.stat1, 0 )
		self.mind = max( self.mind-item.stat2, 0 )

func dead():
	return mind < 1 or health<1
	
func die():
	if not self.is_visible(): return # this might get called twice
	self.hide()	
	var sm = smoke.instance()
	sm.show()
	enemies.add_child( sm )
	sm.set_translation( self.get_translation() - Vector3( 0, 0.6, 0 ) )
	var anim = sm.find_node("Animation")	
	assert( sm.is_visible() )
	anim.connect("finished", self, "remove_me", [sm] )
	anim.play( "puff" )			
	enemies.get_parent().remove_enemy( x, y )
		
func remove_me(sm):
	sm.queue_free()
