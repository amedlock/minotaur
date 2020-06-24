extends Sprite3D;

var monster;  # reference to the Enemy object from enemy_list.gd  monsters[]
var health;  # hit points
var mind    # magic hit points

#var drops 

const smoke = preload("smoke.tscn")

var cell : Spatial

func _ready():
	cell = self.get_parent()


func roll_die(sides: int, num : int) -> int:
	var total = 0
	var m = sides+1
	for n in num:
		total += (randi() % m)
	return total


func configure(enemy, _dungeon):
	#var skill = dungeon.skill_level
	#var depth = dungeon.current_level.depth
	monster = enemy
	health = rand_range(enemy.min_hp, enemy.max_hp)
	mind = rand_range(enemy.min_mind, enemy.max_mind)
	self.name = enemy.name
	set_region_rect( monster.img )
	self.translation.y = 0.9


func damage( item ):
	if item!=null:
		self.health = max( self.health-item.stat1, 0 )
		self.mind = max( self.mind-item.stat2, 0 )

func is_dead():
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


