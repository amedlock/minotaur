using minotaur.Source.enemies;
using minotaur.Source.items;

namespace minotaur.Source.dungeon.builder;

public class MazeCell(int x, int y)
{
  public readonly int X = x;
  public readonly int Y = y;

  public ItemInfo ItemInfo;
  public EnemyInfo EnemyInfo;

  public WallType North = WallType.Empty;
  public WallType East = WallType.Empty;
  public GateType Gate = GateType.None;

  public bool Used; // means this cell has been used for building, or item/enemy placement

  public void Reset()
  {
    Used = false;
    // fill in all walls until maze algo removes some or makes into doors
    North = WallType.Wall;
    East = WallType.Wall;
    ItemInfo = null;
    EnemyInfo = null;
    Gate = GateType.None;
  }

  public bool AdjacentTo(MazeCell cell)
  {
    if (cell.X == X)
    {
      return cell.Y == Y - 1 || cell.Y == Y + 1;
    }
    else if (cell.Y == Y)
    {
      return cell.X == X - 1 || cell.X == X + 1;
    }

    return false;
  }
}