using System;
using System.Collections.Generic;
using Godot;
using minotaur.Source.enemies;
using minotaur.Source.hud;
using minotaur.Source.map;
using minotaur.Source.player;
using minotaur.Source.Source.dungeon;

namespace minotaur.Source.dungeon;

public partial class Dungeon : Node3D
{
  public int Height = 12;
  public int Width = 12;
  const float CellSize = 3.0f;
  const int MaxLevel = 100;

  private GameDb _gameDb;

  public DungeonGrid Grid;
  private GameDb GameDb;

  private Player player;
  private LevelBuilder builder;
  private Hud hud;
  private AudioStreamPlayer audio;

  private int _skillLevel = 1;
  private uint _seedNumber = 1;
  private Dictionary<int, LevelInfo> _levels = new();
  private LevelInfo _currentLevel;
  private MapView _mapView;
  private Marker3D _startPosition;
  private Dictionary<string, Resource> _muralColors = new();


  public Vector3 StartPosition => _startPosition.Position;


  public LevelInfo CurrentLevel
  {
    get => _currentLevel;
  }

  public Vector3 WorldPosition(Vector2I coord)
  {
    return StartPosition + new Vector3(coord.X * CellSize, 0, coord.Y * CellSize);
  }


  public LevelInfo.GateType MuralColor
  {
    set
    {
      var mat = value switch
      {
        LevelInfo.GateType.Blue => _muralColors["blue"],
        LevelInfo.GateType.Green => _muralColors["green"],
        _ => _muralColors["tan"]
      };
      foreach (var node in GetTree().GetNodesInGroup("murals"))
      {
        node.GetNode<MeshInstance3D>("Mesh").MaterialOverride = (mat as StandardMaterial3D);
      }
    }
  }

  public override void _Ready()
  {
    var game = FindParent("Game") as MainGame;
    Grid = GetNode("Grid") as DungeonGrid;
    player = GetNode("Player") as Player;
    builder = GetNode("Builder") as LevelBuilder;
    _mapView = game.GetNode("MapView") as MapView;
    _startPosition = GetNode("StartPos") as Marker3D;
    hud = GetNode("Player/Camera3D/HUD") as Hud;
    audio = GetNode("Player/Audio") as AudioStreamPlayer;
    _muralColors["war"] = ResourceLoader.Load("res://data/dungeon/green_mat.tres");
    _muralColors["magic"] = ResourceLoader.Load("res://data/dungeon/blue_mat.tres");
    _muralColors["tan"] = ResourceLoader.Load("res://data/dungeon/tan_mat.tres");
    _muralColors["both"] = _muralColors["tan"];

    GetNode<Node3D>("ceiling").Visible = true;
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
    Grid.ClearAll();
    builder.BuildMaze(_currentLevel);
    Visible = true;
    hud.UpdateStats();
    player.ResetLocation();
    _mapView.UpdateMap(_currentLevel.Depth);
  }

  // # (skill_level: dungeon level) minotaur first appears here
  private Dictionary<int, int> _minotaurAppears = new()
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

  void AddFinal(Enemy enemy)
  {
    // Grid.GetCell(enemy.GridX, enemy.GridY).Has = _gameDb.FindItem("treasure", CurrentLevel.Depth);
  }
  

  public void LoadGateLevel(DungeonGate gate)
  {
    CurrentLevel.UsedGate = true;
    // CurrentLevel.SeedNumber = this.rng.Ranrandi()
    CurrentLevel.MagicMonsters = gate.Type != LevelType.War  ;
    CurrentLevel.WarMonsters = gate.Type != LevelType.Magic;
    CurrentLevel.ToughMonsters = gate.Type == LevelType.Both;
    Grid.ClearAll();
    builder.BuildMaze(CurrentLevel);
  }

  public Vector3 MazeOrigin => new(-18, 0, -18);

  public DungeonCell GetCell(Vector2I coord)
  {
    return Grid.GetCell(coord.X, coord.Y);
  }

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
