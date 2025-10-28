using System.Collections.Generic;
using Godot;

namespace minotaur.dungeon;

public partial class DungeonGrid : Node3D
{
  private const int Width = 12;
  private const int Height = 12;
  public float CellSize;

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
  public int Index(int x, int y) => Valid(x,y) ?  x + (y * Width) : -1;

  
  public DungeonCell GetCell(int x, int y)
  {
    int n = Index(x, y);
    return n < 0 ? null : _cells[Index(x, y)];
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
    var index = Index(cellX, cellY);
    var result = _cells.ContainsKey(index) ? _cells[index] : null;
    if (result == null)
    {
      result = new  DungeonCell();
      result.Name = $"{cellX}_{cellY}";
      AddChild(result);
      result.Init(cellX, cellY);
      _cells[index] = result;
    }
    return result;
  }
}