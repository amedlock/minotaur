extends Script


# Information Tree
class InfoTree:
	var key : String
	var value : Variant
	var _children : Dictionary
	
	func _init(k, val = null):
		self.key = k
		self.value = val

	func is_leaf() -> bool:
		return self._children==null or self._children.is_empty()

	# create a child node
	func create(k, val = null):
		var result = InfoTree.new(k, val)
		if self._children==null:
			self._children = {k: result}
		else:
			self._children[k] = result
		return result
	
	func keys():
		return self._children.keys() if self._children else []
	
	func children():
		return self._children.values() if self._children else []
	
	func find_child(k):
		return self._children.get(k, null) if self._children else null

	func get_value(k):
		var node = self.find_child(k)
		return node.value if node!=null else null

	func filter(fn):
		if self._children:
			return self._children.values().filter( fn )
		else:
			return []

	func load_json(filename: String):
		var src = FileAccess.open(filename, FileAccess.READ)
		if src.get_error():
			print("Could not open json file: ", filename)
			return
		var txt = src.get_as_text() ;
		src.close()
		var parser = JSON.new();
		var json = parser.parse(txt)
		if json!=OK:
			print("Error reading json file:", filename)
			print( "On line: %d: %s" % [parser.get_error_line(), parser.get_error_message()] )
		self.create_from_json( parser.data )

	func create_from_json(data):
		if data==null:
			return
		if data is Dictionary:
			for k in data.keys():
				var child = self.create(k)
				child.create_from_json(data.get(k))
		else:
			self.value = data
