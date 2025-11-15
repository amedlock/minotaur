#region

using System;
using Godot;
using minotaur.Source.enemies;
using minotaur.Source.items;
using minotaur.Source.model;
using minotaur.Source.player;

#endregion

namespace minotaur.Source.dungeon;

public partial class DungeonCell : Node3D
{
  private Dungeon _dungeon;
  public Enemy Enemy;
  public DungeonGate Gate;


  // contents of cell
  public Item Item;

  internal MazeCell MazeCell;

  // walls
  public Node3D North;
  public Node3D East;
  
  public int X { get; private set; }

  public int Y { get; private set; }

  public Vector2I GridPos => new(X, Y);

  public EnemyInfo EnemyInfo => Enemy == null ? null : Enemy.Info;

  public ItemInfo ItemInfo => Item == null ? null : Item.Info;
  public Vector2I Coord => new(X, Y);

  public override void _Ready()
  {
    _dungeon = GetParent() as Dungeon;
  }

  public void PlayerEnters(Player player)
  {
    if (Gate != null) player.EnterGate(Gate);
  }


  // returns wall between the two DungeonCells, if any
  public Node3D CheckWall(DungeonCell other)
  {
    if (other == null) return null;

    if (other.Y == Y)
    {
      if (other.X != X) return other.X > X ? East : other.East;
    }
    else if (other.X == X)
    {
      if (other.Y != Y) return other.Y > Y ? North : other.North;
    }

    return null;
  }


  public void Init(int x, int y)
  {
    X = x;
    Y = y;
    Name = $"Cell_{X}_{Y}";
    Position = new Vector3(x * 3f, 0, y * -3f);
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

  public void AddWall(Node3D node, WallType wallType, Direction direction)
  {
    AddChild(node);
    switch (direction)
    {
      case Direction.North:
        North = node;
        break;
      case Direction.East:
        East = node;
        break;
      default:
        throw new Exception("Invalid wall direction");
    }
  }

  public void RemoveItem()
  {
    if (Item != null)
    {
      Item.Visible = false;
      RemoveChild(Item);
      Item.QueueFree();
    }
  }

  public void SetItem(ItemInfo itemInfo)
  {
    RemoveItem();
    Item = new Item();
    Item.Info = itemInfo;
    AddChild(Item);
  }
}