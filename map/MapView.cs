using Godot;
using Godot.Collections;
using minotaur.dungeon;
using minotaur.player;

namespace minotaur.map;

public partial class MapView : Node2D
{
  private PackedScene _mapCellPrefab;
  private PackedScene _gateIcon;
  private Node2D _walls;
  private Node2D _other;
  private Sprite2D _marker;

  private Dungeon _dungeon;
  private Player _player;

  private static readonly Rect2 EmptyTile = MapTile(3, 0);
  private static readonly Rect2 ArrowTile = MapTile(3, 2);
  private static readonly Rect2 TombstoneTile = MapTile(1, 3);

  private Dictionary<int, Sprite2D> _lookup = new();

  private static Dictionary<StringName, Rect2> _spriteLookup = new()
  {
    ["none_none"] = EmptyTile,
    ["none_door"] = MapTile(2, 0),
    ["none_wall"] = MapTile(2, 0),
    ["wall_none"] = MapTile(0, 0),
    ["wall_wall"] = MapTile(1, 0),
    ["wall_door"] = MapTile(1, 2),
    ["door_none"] = MapTile(0, 1),
    ["door_wall"] = MapTile(0, 2),
    ["door_door"] = MapTile(1, 1)
  };
  

  private static Rect2 MapTile(int x, int y)
  {
    return new Rect2(x * 16, y * 16, 16, 16);
  }

  // converts grid coord to map position
  private static Vector2I TilePosition(int x, int y)
  {
    var xc = (int)(39.5f + (x * 16));
    var yc = (int)(215.5f - (y * 16));
    return new Vector2I(xc, yc);
  }

  private static int _index(int x, int y)
  {
    return x + y * 100;
  }


  public override void _Ready()
  {
    var game = GetParent() as MainGame;
    _dungeon = game.GetNode("Dungeon") as Dungeon;
    _player = _dungeon.GetNode("Player") as Player;
    _walls = GetNode("Walls") as Node2D;
    _other = GetNode("Other") as Node2D;
    _marker = GetNode("marker") as Sprite2D;
    _mapCellPrefab = ResourceLoader.Load<PackedScene>("res://data/map/map_cell.tscn");
    _gateIcon = ResourceLoader.Load<PackedScene>("res://data/map/gate_icon.tscn");

    foreach (var y in GD.Range(12))
    {
      foreach (var x in GD.Range(12))
      {
        var col = _mapCellPrefab.Instantiate() as Sprite2D;
        AddChild(col);
        col.Position = TilePosition(x, y);
        col.RegionRect = EmptyTile;
        col.ZIndex = 2;
        col.Name = $"map_{x}_{y}";
        _lookup[_index(x, y)] = col;
      }
    }
  }


  public override void _Process(double delta)
  {
    var loc = _player.Coord;
    _marker.Visible = true;
    _marker.Position = TilePosition(loc.X, loc.Y);
    if (_player.IsDead)
    {
      _marker.Modulate = Colors.Gray;
      _marker.RegionRect = TombstoneTile;
      _marker.RotationDegrees = 0;
    }
    else
    {
      _marker.Modulate = Colors.Black;
      _marker.RegionRect = ArrowTile;
      _marker.RotationDegrees = Mathf.PosMod(360 - _player.Dir, 360);
    }
  }


  public void ClearAll()
  {
    foreach( var n in _other.GetChildren())
    {
      _other.RemoveChild(n);
      n.QueueFree();
    }

    foreach (var n in _walls.GetChildren())
    {
      _walls.RemoveChild(n);
      n.QueueFree();
    }
  }

  // rebuilds the map layout, only called when level layout changes
  public void UpdateMap(int depth)
  {
    ClearAll();
    var label = FindChild("Label") as Label;
    label.Text = $"Level: {depth}";
    foreach (var y in GD.Range(_dungeon.Height))
    {
      foreach (var x in GD.Range(_dungeon.Width))
      {
        var c = _dungeon.Grid.GetCell(x, y);
        var spr = _lookup[_index(x, y)];
        spr.RegionRect = ChooseTile(c);
        // one of the walls is present, but not the other
        if (c.North == null || c.East == null)
        {
          FixUpCorner(c);
        }
        if (c.Gate!=null)
        {
          AddGate(c.Gate, x, y);
        }
      }
    }
  }

  public Rect2 ChooseTile(DungeonCell cell)
  {
    return _spriteLookup["none_none"];
    // StringName n1 = nameof(cell.North.WallType);
    // StringName n2 = nameof(cell.East.WallType);
    // var path = $"{n1}_{n2}";
    // return _spriteLookup[path];
  }

  public void AddGate(DungeonGate gate, int x, int y)
  {
    var icon = _gateIcon.Instantiate() as Sprite2D;
    icon.Modulate = gate.Sprite.Modulate;
    _other.AddChild(icon);
    icon.Position = TilePosition(x, y);
  }

  private void FixUpCorner(DungeonCell cell)
  {
    var n = _dungeon.Grid.GetCell(cell.X, cell.Y + 1);
    // if (n is not { East: true })
    // {
    //   return;
    // }

    var e = _dungeon.Grid.GetCell(cell.X + 1, cell.Y);
    // if (e is not { North: true })
    // {
    //   return;
    // }
    // var fix = _gateIcon.Instantiate() as Sprite2D;
    // fix.Name = $"fix_{cell.X}_{cell.Y}";
    // fix.RegionRect = MapTile(2, 2);
    // fix.Position = TilePosition(cell.X, cell.Y);
    // _other.AddChild(fix);
  }

}