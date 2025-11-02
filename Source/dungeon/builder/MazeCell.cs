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

  public bool Active;

  public void Reset()
  {
    Active = false;
    North = WallType.Empty;
    East = WallType.Empty;
    ItemInfo = null;
    EnemyInfo = null;
    Gate = GateType.None;
  }

}