#region

using System;
using System.Collections.Generic;
using Godot;
using minotaur.Source.dungeon.builder;
using minotaur.Source.enemies;
using minotaur.Source.hud;
using minotaur.Source.model;
using minotaur.Source.player;
using static minotaur.Source.dungeon.Direction;
using MapView = minotaur.Source.views.MapView;

#endregion

namespace minotaur.Source.dungeon;

public partial class Dungeon : Node3D
{
  private const float CellSize = 3.0f;
  private const int MaxLevel = 100;

  // dungeon cells (Node3D) lookup
  private readonly Dictionary<int, LevelInfo> _levels = new();

  // # (skill_level: dungeon level) minotaur first appears here
  private readonly Dictionary<int, int> _minotaurAppears = new()
  {
    [1] = 3,
    [2] = 6,
    [3] = 10,
    [4] = 15
  };

  private readonly Dictionary<string, Resource> _muralColors = new();

  private readonly Dictionary<WallPost, Vector3> _wallPostOffsets = new()
  {
    [WallPost.NW] = new Vector3(0, 0.25f, 0),
    [WallPost.NE] = new Vector3(3, 0.25f, 0),
    [WallPost.SE] = new Vector3(3, 0.25f, 3),
    [WallPost.SW] = new Vector3(0, 0.25f, 3)
  };

  private readonly Dictionary<Direction, WallPost> _wallPosts = new()
  {
    [North] = WallPost.NW,
    [East] = WallPost.NE,
    [South] = WallPost.SE,
    [West] = WallPost.SW
  };

  private AudioStreamPlayer _audio;
  private LevelBuilder _builder;

  [Export]
  private Hud _hud;
  
  [Export]
  private MapView _mapView;
  
  [Export]
  private Player _player;
  
  [Export]
  private GameModel gameModel;
  
  
  private uint _seedNumber = 1;

  private int _skillLevel = 1;
  
  [Export]
  private Marker3D _startPosition;

  private Dictionary<Direction, int> _wallAngle = new()
  {
    [North] = 0,
    [East] = 270,
    [South] = 180,
    [West] = 90
  };

  // this is the "virtual" grid of cell info
  // public DungeonGrid Grid;
  public int Height = 12;
  public int Width = 12;

  public Vector3 StartPosition => _startPosition.Position;


  public MainGame Game { get; private set; }

  public DungeonGrid Grid
  {
    get => gameModel.Grid;
  }

  public LevelInfo CurrentLevel { get; private set; }

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
        node.GetNode<MeshInstance3D>("Mesh").MaterialOverride = mat as StandardMaterial3D;
    }
  }

  public Vector3 MazeOrigin => new(-18, 0, -18);
  public List<DungeonCell> Cells { get; } = new();


  public override void _Ready()
  {
    _muralColors["war"] = ResourceLoader.Load("res://data/dungeon/green_mat.tres");
    _muralColors["magic"] = ResourceLoader.Load("res://data/dungeon/blue_mat.tres");
    _muralColors["tan"] = ResourceLoader.Load("res://data/dungeon/tan_mat.tres");
    _muralColors["both"] = _muralColors["tan"];

    GetNode<Node3D>("ceiling").Visible = true;

    // var tcell = ResourceLoader.Load<PackedScene>("res://data/test_cell.tscn").Instantiate() as Node3D;
    // // tcell.Position = new Vector3(18f, 0, -18f);
    // _player.AddChild(tcell);

    var gridOrigin = (Marker3D)FindChild("GridOrigin");
    var cellPrefab = (PackedScene)ResourceLoader.Load("res://data/dungeon/cell.tscn");
    foreach (var y in GD.Range(Height))
    foreach (var x in GD.Range(Width))
    {
      var cell = (DungeonCell)cellPrefab.Instantiate();
      Cells.Add(cell);
      gridOrigin.AddChild(cell);
      cell.Init(x, y);
    }
  }

  public Vector3 WorldPosition(Vector2I coord)
  {
    return StartPosition + new Vector3(coord.X * CellSize, 0, coord.Y * CellSize);
  }

  public DungeonCell GetCell(int x, int y)
  {
    return gameModel.Grid.Valid(x, y) ? Cells[Grid.Index(x, y)] : null;
  }

  public DungeonCell GetCell(Vector2I pos)
  {
    return GetCell(pos.X, pos.Y);
  }

  public DungeonCell GetCell(Vector2I pos, Direction dir)
  {
    return dir switch
    {
      North => GetCell(pos.X, pos.Y + 1),
      East => GetCell(pos.X + 1, pos.Y),
      South => GetCell(pos.X, pos.Y - 1),
      West => GetCell(pos.X - 1, pos.Y),
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

    Cells[Grid.Index(x, y)] = cell;
  }

  public Node3D GetWall(Vector2I coord, Direction dir)
  {
    switch (dir)
    {
      case West: return GetWall(coord + new Vector2I(-1, 0), East);
      case South: return GetWall(coord + new Vector2I(0, -1), North);
    }

    var cell = GetCell(coord);
    if (cell == null) return null;
    return dir switch
    {
      North => cell.North,
      East => cell.East,
      _ => null
    };
  }


  public void InitMaze(int skill, uint seedNum)
  {
    // _skillLevel = skill;
    // _seedNumber = seedNum;
    // var rng = new RandomNumberGenerator();
    // rng.Seed = seedNum;
    // foreach (var num in GD.Range(1, MaxLevel + 1))
    // {
    //   _levels[num] = CreateDungeonInfo(skill, num, rng);
    // }
    //
    // CurrentLevel = _levels[1];
    // // Grid.Reset();
    // // _builder.BuildMaze(CurrentLevel);
    // Visible = true;
    // _hud.UpdateStats();
    // _player.ResetLocation();
    // _mapView.UpdateMap(CurrentLevel);
  }

  public void AddFinal(Enemy enemy)
  {
    // Grid.GetCell(enemy.GridX, enemy.GridY).Has = _gameDb.FindItem("treasure", CurrentLevel.Depth);
  }


  public void WonGame()
  {
    _audio.Stream = ResourceLoader.Load<AudioStream>("res://data/sounds/win2.wav");
    _audio.Play();
    Game.WonGame();
  }

  public void NextLevel()
  {
    var next = CurrentLevel.Depth + 1;
    if (!_levels.ContainsKey(next)) return;

    CurrentLevel = _levels[next];
    _audio.Stream = ResourceLoader.Load<AudioStream>("res://data/sounds/descend.wav");
    _audio.Play();
    Grid.Reset();
    _builder.BuildMaze(CurrentLevel);
    _mapView.UpdateMap(CurrentLevel);
    _player.ResetLocation();
    _hud.UpdateStats();
  }

  public void LoadGateLevel(DungeonGate gate)
  {
    CurrentLevel.UsedGate = true;
    // CurrentLevel.SeedNumber = this.rng.Ranrandi()
    Grid.Reset();
    _builder.BuildMaze(CurrentLevel);
  }


  public Vector3 WallPostForWall(int cx, int cy, Direction dir)
  {
    var which = _wallPosts[dir];
    return MazeOrigin + _wallPostOffsets[which] + new Vector3(cx * CellSize, 0f, cy * CellSize);
  }
}