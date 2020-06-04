extends Sprite3D;

var monster;  # reference to the Enemy object from enemy_list.gd  monsters[]
var health;  # hit points
var mind ;   # magic hit points
# var drops; # item dropped when killed, from item_list.gd  items[]

var x      # grid coordinates
var y 

var smoke = preload("smoke.tscn")

var cell

func _ready():
	cell = self.get_parent()


func configure(xp, yp, enemy):
	monster = enemy
	x = xp
	y = yp
	health = enemy.health
	mind = enemy.mind
	self.name = enemy.name
	set_region_rect( monster.img )
	self.translation.y = 0.9


func damage( item ):
	if item!=null:
		self.health = max( self.health-item.stat1, 0 )
		self.mind = max( self.mind-item.stat2, 0 )

func dead():
	return mind < 1 or health<1
	
func die():
	if not self.visible: 
		return # this might get called twice
	self.hide()	
	var sm = smoke.instance()
	sm.translation =  self.translation - Vector3( 0, 0.6, 0 ) 
	cell.add_child( sm )
	sm.start()
	cell.set_enemy( null )


