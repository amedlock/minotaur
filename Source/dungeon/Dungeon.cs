#region

using System;
using System.Collections.Generic;
using Godot;
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
  
  [Export]
  private LevelBuilder _builder;

  [Export] 
  private Hud _hud;

  [Export] 
  private MapView _mapView;

  [Export] 
  private Player _player;

  [Export] 
  private GameModel gameModel;

  [Export] 
  private Marker3D _startPosition;

  private Node3D _walls;
  private Node3D _cells;

  public override void _Ready()
  {
    _muralColors["war"] = ResourceLoader.Load("res://data/dungeon/green_mat.tres");
    _muralColors["magic"] = ResourceLoader.Load("res://data/dungeon/blue_mat.tres");
    _muralColors["tan"] = ResourceLoader.Load("res://data/dungeon/tan_mat.tres");
    _muralColors["both"] = _muralColors["tan"];

    GetNode<Node3D>("ceiling").Visible = true;

    _cells = new Node3D();
    _cells.Name = "Cells";
    _cells.Position = GetNode<Marker3D>("GridOrigin").Position;
    AddChild(_cells);

    var gridOrigin = (Marker3D)FindChild("GridOrigin");
    var cellPrefab = (PackedScene)ResourceLoader.Load("res://data/dungeon/cell.tscn");
    foreach (var y in GD.Range(gameModel.Grid.Height))
    {
      foreach (var x in GD.Range(gameModel.Grid.Width))
      {
        var cell = (DungeonCell)cellPrefab.Instantiate();
        Cells.Add(cell);
        _cells.AddChild(cell);
        cell.Init(x, y);
      }
    }
  }

  public void BuildLevel()
  {
    ClearAll();
    var levelInfo = gameModel.CurrentLevel;
    var grid = gameModel.Grid;
    _builder.Build(this, levelInfo, grid);
  }


  public void ClearAll()
  {
    Cells.ForEach(cell => cell.ClearAll());
  }


  private Dictionary<Direction, int> _wallAngle = new()
  {
    [North] = 0,
    [East] = 270,
    [South] = 180,
    [West] = 90
  };


  public Vector3 StartPosition => _startPosition.Position;


  public MainGame Game { get; private set; }

  public DungeonGrid Grid
  {
    get => gameModel.Grid;
  }

  public LevelInfo CurrentLevel { get; private set; }
  public Vector3 PlayerPosition {
    get
    {
      var coord = gameModel.PlayerCoord;
      return StartPosition + new Vector3(coord.X * 3, 0, coord.Y * -3);
    } 
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
        node.GetNode<MeshInstance3D>("Mesh").MaterialOverride = mat as StandardMaterial3D;
    }
  }

  public Vector3 MazeOrigin => new(-18, 0, -18);
  public List<DungeonCell> Cells { get; } = new();
  public DungeonCell CurrentCell => Cells[gameModel.GridIndex];


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
    // var next = CurrentLevel.Depth + 1;
    // if (!_levels.ContainsKey(next)) return;
    //
    // CurrentLevel = _levels[next];
    // _audio.Stream = ResourceLoader.Load<AudioStream>("res://data/sounds/descend.wav");
    // _audio.Play();
    // Grid.Reset();
    // _builder.Build(this, gameModel.CurrentLevel, gameModel.Grid);
    // _mapView.UpdateMap(CurrentLevel);
    // _player.ResetLocation();
    // _hud.UpdateStats();
  }



  public Vector3 WallPostForWall(int cx, int cy, Direction dir)
  {
    var which = _wallPosts[dir];
    return MazeOrigin + _wallPostOffsets[which] + new Vector3(cx * CellSize, 0f, cy * CellSize);
  }
}