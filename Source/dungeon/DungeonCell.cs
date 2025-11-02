using Godot;
using minotaur.Source.dungeon.builder;
using minotaur.Source.enemies;
using minotaur.Source.items;
using minotaur.Source.player;

namespace minotaur.Source.dungeon;

public partial class DungeonCell : Node3D
{
  // walls
  public Node3D North;
  public Node3D East;
  
  
  // contents of cell
  public Item Item;
  public Enemy Enemy;
  public DungeonGate Gate;

  private Dungeon _dungeon;
  internal MazeCell MazeCell;

  public int X { get; private set; }

  public int Y { get; private set; }

  public Vector2I GridPos => new(X, Y);

  public EnemyInfo EnemyInfo => Enemy == null ? null : Enemy.Info;

  public ItemInfo ItemInfo => Item == null ? null : Item.Info;

  public override void _Ready()
  {
    _dungeon = GetParent() as Dungeon;
  }

  public void PlayerEnters(Player player)
  {
    if (Gate != null)
    {
      player.EnterGate(Gate);
      return;
    }
  }
  

  // returns wall between the two DungeonCells, if any
  public Node3D CheckWall(DungeonCell other)
  {
    if (other == null)
    {
      return null;
    }
    
    if (other.Y == Y)
    {
      if (other.X != X)
      {
        return other.X > X ? East : other.East;
      }
    }
    else if (other.X == X)
    {
      if (other.Y != Y)
      {
        return other.Y > Y ? North : other.North;
      }
    }
    return null;
  }


  public void Init(int x, int y)
  {
    X = x;
    Y = y;
    Position = new Vector3(x * Dungeon.CellSize, 0, y * Dungeon.CellSize);
    ClearAll();
  }

  public void ClearAll()
  {
    foreach (var ch in GetChildren())
    {
      RemoveChild(ch);
      ch.QueueFree();
    }

    Gate = null;
    Item = null;
    Enemy = null;
    North = null;
    East = null;
  }

  public void RemoveEnemy()
  {
    if (Enemy != null)
    {
      RemoveChild(Enemy);
      Enemy.QueueFree();
    }
  }
}
