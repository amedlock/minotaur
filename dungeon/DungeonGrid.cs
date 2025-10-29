using System.Collections.Generic;
using Godot;
using minotaur.player;

namespace minotaur.dungeon;

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
  public int Index(int x, int y) => Valid(x,y) ?  x + (y * Width) : -1;

  public DungeonCell GetCell(Vector2I pos) => GetCell(pos.X, pos.Y);
  
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
    return dir switch
    {
      Direction.North => GetCell(coord).North,
      Direction.East => GetCell(coord).East,
      Direction.South => GetCell(new Vector2I(coord.X, coord.Y - 1)).North,
      Direction.West => GetCell(new Vector2I(coord.X - 1, coord.Y)).East,
      _ => null
    };
  }
}