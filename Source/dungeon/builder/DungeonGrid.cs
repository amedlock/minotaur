using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;
using minotaur.Source.player;

namespace minotaur.Source.dungeon.builder;

public class DungeonGrid
{
  public readonly float CellSize = 3.0f;
  public readonly int Width, Height;
  private List<MazeCell> _items;

  public IEnumerable<MazeCell> Cells => _items;
  public IEnumerable<MazeCell> EmptyCells => _items.Where(c => c.ItemInfo == null && c.EnemyInfo == null);

  public bool Valid(int x, int y) => (x >= 0) && (x < Width) && (y >= 0) && (y < Height);
  public int Index(int x, int y) => x + (y * Width);

  public MazeCell Cell(int x, int y) => Valid(x, y) ? _items[Index(x, y)] : null;
  public MazeCell Cell(Vector2I coord) => Cell(coord.X, coord.Y);

  public MazeCell Cell(Vector2I coord, Direction dir)
  {
    return dir switch
    {
      Direction.North => Cell(coord.X, coord.Y - 1),
      Direction.South => Cell(coord.X, coord.Y + 1),
      Direction.East => Cell(coord.X + 1, coord.Y),
      Direction.West => Cell(coord.X - 1, coord.Y),
      _ => null
    };
  }

  public DungeonGrid(int width, int height)
  {
    Width = width;
    Height = height;
    _items = new(width * height);
    foreach (var y in GD.Range(height))
    {
      foreach (var x in GD.Range(width))
      {
        _items.Add(new MazeCell(x, y));
      }
    }
  }


  public void Reset()
  {
    foreach (var cell in _items)
    {
      cell.Reset();
    }
  }
}
