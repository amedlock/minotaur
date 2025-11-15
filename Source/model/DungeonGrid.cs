#region

using System.Collections.Generic;
using System.Linq;
using Godot;
using minotaur.Source.dungeon;

#endregion

namespace minotaur.Source.model;

public class DungeonGrid
{
  private readonly List<MazeCell> _items;
  public readonly int Width, Height;

  public GateType GateType { get; set; }

  public DungeonGrid(int width, int height)
  {
    Width = width;
    Height = height;
    _items = new List<MazeCell>(width * height);
    foreach (var y in GD.Range(height))
    {
      foreach (var x in GD.Range(width))
      {
        _items.Add(new MazeCell(x, y));
      }
    }
  }

  public int Count => Width * Height;

  public IEnumerable<MazeCell> Cells => _items;
  public IEnumerable<MazeCell> EmptyCells => _items.Where(c => c.ItemInfo == null && c.EnemyInfo == null);


  public bool Valid(int x, int y)
  {
    return x >= 0 && x < Width && y >= 0 && y < Height;
  }

  public int Index(int x, int y)
  {
    return x + y * Width;
  }

  public MazeCell Cell(int x, int y)
  {
    return Valid(x, y) ? _items[Index(x, y)] : null;
  }

  public MazeCell Cell(Vector2I coord)
  {
    return Cell(coord.X, coord.Y);
  }

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


  public void Reset()
  {
    foreach (var cell in _items)
    {
      cell.Reset();
    }
  }


  public IEnumerable<WallPost> WallPosts(int x, int y)
  {
    var result = new List<WallPost>();
    var cell = Cell(x, y);
    if (cell == null) return result;

    if (cell.North != WallType.Empty)
    {
      result.Add(WallPost.NW);
      result.Add(WallPost.NE);
    }

    if (cell.East != WallType.Empty)
    {
      result.Add(WallPost.NE);
      result.Add(WallPost.SE);
    }

    return result.Distinct();
  }
}