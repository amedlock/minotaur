extends Object

enum CmdState { READY, RUN, STOP, FAIL };



class Cmd:
	var context
	var state = CmdState.READY
	
	func _init(ctx):
		self.context = ctx
	
	func reset(context):
		pass
	
	func exec(context):
		pass



class CmdList:
	var items = []
	
	func add( cmd: Cmd ):
		self.items.append(cmd)
	
	func exec(context):
		for c in self.items:
			c.exec(context)



class CmdQueue:
	var items = []
	
	func push( cmd: Cmd ):
		self.items.append(cmd)
		
	func exec( context ):
		if len(items):
			var hd = items.remove(0)
			hd.exec(context)

