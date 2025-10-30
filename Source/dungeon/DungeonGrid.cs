using System;
using System.Collections.Generic;
using Godot;
using minotaur.Source.player;
using minotaur.Source.Source.dungeon;

namespace minotaur.Source.dungeon;

public partial class DungeonGrid : Node3D
{
  private const int Width = 12;
  private const int Height = 12;
  public float CellSize = 3.0f;

  private Dictionary<int,DungeonCell> _cells = new();

  private List<Node3D> _gates = new();

  private Dungeon _dungeon;
  private Node3D _outerWall;
  private PackedScene _cellPrefab;

  public override void _Ready()
  {
    _dungeon = GetParent() as Dungeon;
    _outerWall = _dungeon.FindChild("outer_wall") as Node3D;
    _cellPrefab = ResourceLoader.Load<PackedScene>("res://data/dungeon/cell.tscn");
  }

  public bool Valid(int x, int y) => (x >= 0) && (x < Width) && (y >= 0) && (y < Height);
  public int Index(int x, int y) => x + (y * Width);

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

  
  public DungeonCell GetCell(int x, int y)
  {
    return Valid(x, y) ? _cells[Index(x, y)] : null;
  }

  public void ClearAll()
  {
    foreach (DungeonCell cell in _cells.Values)
    {
      RemoveChild(cell);
      cell.QueueFree();
    }
    _cells.Clear();
  }

  public DungeonCell AddCell(int cellX, int cellY)
  {
    if (!Valid(cellX, cellY))
    {
      throw new Exception("Illegal cell position");
    }
    var index = Index(cellX, cellY);
    if (!_cells.TryGetValue(index, out DungeonCell result))
    {
      result = (DungeonCell)_cellPrefab.Instantiate();
      result.Name = $"{cellX}_{cellY}";
      AddChild(result);
      result.Init(cellX, cellY);
      result.Position = new Vector3(cellX * CellSize, 0, -(cellY * CellSize));
      _cells[index] = result;
    }

    
    return result;
  }

  public Node3D GetWall(Vector2I coord, Direction dir)
  {
    switch (dir)
    {
      case Direction.West: return GetWall(coord + new Vector2I(-1,0), Direction.East);
      case Direction.South: return GetWall(coord + new Vector2I(0, -1), Direction.North);
    }
    
    var cell = _dungeon.GetCell(coord);
    if (cell == null)
    {
      return null;
    }
    return dir switch
    {
      Direction.North => cell.North,
      Direction.East => cell.East
    };
  }
}