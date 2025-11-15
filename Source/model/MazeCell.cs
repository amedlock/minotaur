#region

using minotaur.Source.dungeon;
using minotaur.Source.enemies;
using minotaur.Source.items;

#endregion

namespace minotaur.Source.model;

public class MazeCell(int x, int y)
{
  public readonly int X = x;
  public readonly int Y = y;
  public WallType East = WallType.Empty;
  public WallType North = WallType.Empty;
  
  public GateType Gate = GateType.None;

  public ItemInfo ItemInfo;

  public EnemyInfo EnemyInfo;

  public bool Used; // means this cell has been used for building, or item/enemy placement

  public void Reset()
  {
    // used by Prim algorithm
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

    if (cell.Y == Y)
    {
      return cell.X == X - 1 || cell.X == X + 1;
    }

    return false;
  }
}