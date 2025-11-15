#region

using Godot;
using Godot.Collections;
using minotaur.Source.dungeon;
using minotaur.Source.model;
using minotaur.Source.player;

#endregion

namespace minotaur.Source.views;

public partial class MapView : Node2D
{
  private static readonly Rect2 EmptyTile = MapTile(3, 0);
  private static readonly Rect2 ArrowTile = MapTile(3, 2);
  private static readonly Rect2 TombstoneTile = MapTile(1, 3);

  private static readonly Dictionary<StringName, Rect2> SpriteLookup = new()
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

  [Export]
  private Dungeon _dungeon;

  [Export]
  private Player _player;

  [Export]
  private GameModel _gameModel;

  
  private PackedScene _gateIcon;

  private Dictionary<int, Sprite2D> _lookup = new();
  private PackedScene _mapCellPrefab;
  private Sprite2D _marker;
  private Node2D _other;
  private Node2D _walls;


  private DungeonGrid grid => _gameModel.Grid;
  
  private static Rect2 MapTile(int x, int y)
  {
    return new Rect2(x * 16, y * 16, 16, 16);
  }

  // converts grid coord to map position
  private static Vector2I TilePosition(int x, int y)
  {
    var xc = (int)(39.5f + x * 16);
    var yc = (int)(215.5f - y * 16);
    return new Vector2I(xc, yc);
  }

  private static int _index(int x, int y)
  {
    return x + y * 100;
  }


  public override void _Ready()
  {
    // var game = GetParent() as MainGame;
    // _dungeon = game.GetNode("Dungeon") as Dungeon;
    // _player = _dungeon.GetNode("Player") as Player;
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
    foreach (var n in _other.GetChildren())
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
  public void UpdateMap(LevelInfo levelInfo)
  {
    ClearAll();
    var label = (Label)FindChild("Label");
    label.Text = $"Level: {levelInfo.Depth}";

    foreach (var y in GD.Range(_gameModel.Grid.Height))
    {
      foreach (var x in GD.Range(_gameModel.Grid.Width))
      {
        var cell = grid.Cell(x, y);
        var spr = _lookup[_index(x, y)];
        spr.RegionRect = ChooseTile(cell);
        // one of the walls is present, but not the other
        if (cell.North == WallType.Empty || cell.East == WallType.Empty) FixUpCorner(cell);
        if (cell.Gate != GateType.None) AddGate(cell.Gate, x, y);
      }
    }

  }

  public Rect2 ChooseTile(MazeCell cell)
  {
    return SpriteLookup["none_none"];
    // StringName n1 = nameof(cell.North.WallType);
    // StringName n2 = nameof(cell.East.WallType);
    // var path = $"{n1}_{n2}";
    // return _spriteLookup[path];
  }

  public void AddGate(GateType gate, int x, int y)
  {
    var icon = (Sprite2D)_gateIcon.Instantiate();
    icon.Modulate = gate switch
    {
      GateType.War => Colors.DarkGreen,
      GateType.Magic => Colors.DarkBlue,
      _ => Colors.Tan
    };
    _other.AddChild(icon);
    icon.Position = TilePosition(x, y);
  }

  private static void FixUpCorner(MazeCell cell)
  {
    if (cell is { East: WallType.Empty }) return;

    if (cell is { North: WallType.Empty })
    {
    }
    // var fix = _gateIcon.Instantiate() as Sprite2D;
    // fix.Name = $"fix_{cell.X}_{cell.Y}";
    // fix.RegionRect = MapTile(2, 2);
    // fix.Position = TilePosition(cell.X, cell.Y);
    // _other.AddChild(fix);
  }
}