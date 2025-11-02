using System;
using System.Collections.Generic;
using Godot;
using minotaur.Source.dungeon.builder;
using minotaur.Source.enemies;
using minotaur.Source.hud;
using minotaur.Source.map;
using minotaur.Source.player;

namespace minotaur.Source.dungeon;


public partial class Dungeon : Node3D
{
  public int Height = 12;
  public int Width = 12;
  internal const float CellSize = 3.0f;
  const int MaxLevel = 100;

  private GameDb _gameDb;

  // this is the "virtual" grid of cell info
  public DungeonGrid Grid;
  private GameDb GameDb;

  // dungeon cells (Node3D) lookup
  private readonly Dictionary<int, DungeonCell> _cells = new();
  
  private Player _player;
  private LevelBuilder _builder;
  private Hud _hud;
  private AudioStreamPlayer _audio;

  private int _skillLevel = 1;
  private uint _seedNumber = 1;
  private Dictionary<int, LevelInfo> _levels = new();
  private LevelInfo _currentLevel;
  private MapView _mapView;
  private Marker3D _startPosition;
  private Dictionary<string, Resource> _muralColors = new();


  
  public override void _Ready()
  {
    var game = FindParent("Game") as MainGame;
    Grid = new DungeonGrid(Width, Height);
    _player = GetNode("Player") as Player;
    _builder = GetNode("Builder") as builder.LevelBuilder;
    _mapView = game.GetNode("MapView") as MapView;
    _startPosition = GetNode("StartPos") as Marker3D;
    _hud = GetNode("Player/Camera3D/HUD") as Hud;
    _audio = GetNode("Player/Audio") as AudioStreamPlayer;
    _muralColors["war"] = ResourceLoader.Load("res://data/dungeon/green_mat.tres");
    _muralColors["magic"] = ResourceLoader.Load("res://data/dungeon/blue_mat.tres");
    _muralColors["tan"] = ResourceLoader.Load("res://data/dungeon/tan_mat.tres");
    _muralColors["both"] = _muralColors["tan"];

    GetNode<Node3D>("ceiling").Visible = true;

    var cellPrefab = (PackedScene)ResourceLoader.Load("res://data/dungeon/cell.tscn");
    
    foreach (var y in GD.Range(Height))
    {
      foreach (var x in GD.Range(Width))
      {
        var cell = (DungeonCell)cellPrefab.Instantiate();
        AddChild(cell);
        cell.Init(x, y);
        if (cell.Position != new Vector3(x * CellSize, 0, y * CellSize))
        {
          throw new Exception("Wrong Position");
        }
        _cells[Grid.Index(x, y)] = cell;
      }
    }
  }
  
  public Vector3 StartPosition => _startPosition.Position;

  
  public MainGame Game => GetParent() as MainGame;

  public LevelInfo CurrentLevel
  {
    get => _currentLevel;
  }

  public Vector3 WorldPosition(Vector2I coord)
  {
    return StartPosition + new Vector3(coord.X * CellSize, 0, coord.Y * CellSize);
  }
  
  public GateType MuralColor
  {
    set
    {
      var mat = value switch
      {
        GateType.Magic => _muralColors["blue"],
        GateType.War => _muralColors["green"],
        _ => _muralColors["tan"]
      };
      foreach (var node in GetTree().GetNodesInGroup("murals"))
      {
        node.GetNode<MeshInstance3D>("Mesh").MaterialOverride = (mat as StandardMaterial3D);
      }
    }
  }


  public DungeonCell GetCell(int x, int y)
  {
    return Grid.Valid(x, y) ? _cells.GetValueOrDefault(Grid.Index(x, y)) : null;
  }
    
  public DungeonCell GetCell(Vector2I pos) => GetCell(pos.X, pos.Y);
  
  public DungeonCell GetCell(Vector2I pos, Direction dir)
  {
    return dir switch
    {
      Direction.North => GetCell(pos.X, pos.Y + 1),
      Direction.East => GetCell(pos.X + 1, pos.Y),
      Direction.South => GetCell(pos.X, pos.Y - 1),
      Direction.West => GetCell(pos.X - 1, pos.Y),
      _ => throw new Exception("Illegal direction")
    };
  }

  public void SetCell(int x, int y, DungeonCell cell)
  {
    var current = GetCell(x, y);
    if (current != null)
    {
      RemoveChild(current);
      current.QueueFree();
    }
    _cells[Grid.Index(x, y)] = cell;
  }

  public Node3D GetWall(Vector2I coord, Direction dir)
  {
    switch (dir)
    {
      case Direction.West: return GetWall(coord + new Vector2I(-1,0), Direction.East);
      case Direction.South: return GetWall(coord + new Vector2I(0, -1), Direction.North);
    }
    
    var cell = GetCell(coord);
    if (cell == null)
    {
      return null;
    }
    return dir switch
    {
      Direction.North => cell.North,
      Direction.East => cell.East,
      _ => null
    };
  }
  
  

  public void InitMaze(int skill, uint seedNum)
  {
    _skillLevel = skill;
    _seedNumber = seedNum;
    var rng = new RandomNumberGenerator();
    rng.Seed = seedNum;
    foreach (var num in GD.Range(1, MaxLevel + 1))
    {
      _levels[num] = CreateDungeonInfo(skill, num, rng);
    }

    _currentLevel = _levels[1];
    Grid.Reset();
    _builder.BuildMaze(_currentLevel);
    Visible = true;
    _hud.UpdateStats();
    _player.ResetLocation();
    _mapView.UpdateMap(_currentLevel.Depth);
  }

  // # (skill_level: dungeon level) minotaur first appears here
  private readonly Dictionary<int, int> _minotaurAppears = new()
  {
    [1] = 3,
    [2] = 6,
    [3] = 10,
    [4] = 15
  };

  private LevelInfo CreateDungeonInfo(int skill, int num, RandomNumberGenerator rng)
  {
    uint seed = 1386327162;
    var result = new LevelInfo(num, seed);
    result.LevelType = rng.RandWeighted([40f, 40f, 20f]) switch
    {
      0 => LevelType.War,
      1 => LevelType.Magic,
      _ => LevelType.Both
    };
    result.HasMinotaur = num >= _minotaurAppears[skill];
    return result;
  }

  public void AddFinal(Enemy enemy)
  {
    // Grid.GetCell(enemy.GridX, enemy.GridY).Has = _gameDb.FindItem("treasure", CurrentLevel.Depth);
  }
  

  public void LoadGateLevel(DungeonGate gate)
  {
    CurrentLevel.UsedGate = true;
    // CurrentLevel.SeedNumber = this.rng.Ranrandi()
    Grid.Reset();
    _builder.BuildMaze(CurrentLevel);
  }

  public Vector3 MazeOrigin => new(-18, 0, -18);

  Dictionary<Direction, WallPost> _wallPosts = new(){
    [Direction.North] = WallPost.NW,
    [Direction.East] = WallPost.NE,
    [Direction.South] = WallPost.SE,
    [Direction.West] = WallPost.SW
  };

  private Dictionary<WallPost, Vector3> _wallPostOffsets = new()
  {
    [WallPost.NW] = new Vector3(0, 0.25f, 0),
    [WallPost.NE] = new Vector3(3, 0.25f, 0),
    [WallPost.SE] = new Vector3(3, 0.25f, 3),
    [WallPost.SW] = new Vector3(0, 0.25f, 3)
  };

  private Dictionary<Direction,int> _wallAngle = new()
  {
    [Direction.North] = 0,
    [Direction.East] = 270,
    [Direction.South] = 180,
    [Direction.West] = 90
  };


  public Vector3 WallPostForWall(int cx, int cy, Direction dir)
  {
    var which = _wallPosts[dir];
    return MazeOrigin + _wallPostOffsets[which] + new Vector3(cx * CellSize, 0f, cy * CellSize);
  }

  public void UseExit()
  {
    throw new NotImplementedException();
  }
}
