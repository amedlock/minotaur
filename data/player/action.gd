extends Node;


signal  action_complete;


func _ready():
	connect("action_complete", get_parent(), "action_complete" )


func complete():
	emit_signal("action_complete", get_name() )


func input(evt): pass

func process(dt): pass
