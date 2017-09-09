extends Spatial;

const Up = Vector3(0,1,0)

export( String, "blue", "tan", "green" ) var kind = "tan"

func _ready():
	set_process(true)
	

func _process(dt):
	self.rotate( Up, dt * 0.5 )