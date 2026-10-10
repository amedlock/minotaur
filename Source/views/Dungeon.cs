#region

using System.Collections.Generic;
using Godot;
using minotaur.Source.control;
using minotaur.Source.model;
using static minotaur.Source.model.Direction;

#endregion

namespace minotaur.Source.views;

public partial class Dungeon : Node3D
{
  private readonly Dictionary<string, Resource> _muralColors = new();

  [Export] private LevelBuilder _builder;

  [Export] private GameModel gameModel;

  [Export] private Marker3D _startPosition;

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

    var cellPrefab = (PackedScene)ResourceLoader.Load("res://data/dungeon/cell.tscn");
    foreach (int y in GD.Range(gameModel.Grid.Height))
    {
      foreach (int x in GD.Range(gameModel.Grid.Width))
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


  public Vector3 StartPosition => _startPosition.Position;

  public DungeonGrid Grid => gameModel.Grid;

  public Vector3 PlayerPosition
  {
    get
    {
      return StartPosition + GridGeometry.CellOrigin(gameModel.PlayerCoord);
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

  public List<DungeonCell> Cells { get; } = new();
  public DungeonCell CurrentCell => Cells[gameModel.GridIndex];

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
    return GetCell(pos + dir.Offset());
  }

  public Node3D GetWall(Vector2I coord, Direction dir)
  {
    switch (dir)
    {
      case West: return GetWall(coord + West.Offset(), East);
      case South: return GetWall(coord + South.Offset(), North);
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
}
