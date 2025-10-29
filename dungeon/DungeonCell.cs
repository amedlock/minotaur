using System;
using Godot;
using minotaur.enemies;
using minotaur.items;
using minotaur.player;

namespace minotaur.dungeon;

public partial class DungeonCell : Node3D
{
  private int _x = 0;
  private int _y = 0;

  // walls
  public Node3D North;
  public Node3D East;
  
  public ItemInfo ItemInfo;
  
  // contents of cell
  public Item Item;
  public Enemy Enemy;
  public DungeonGate Gate;

  private DungeonGrid _grid;


  public int X => _x;
  public int Y => _y;
  
  public int CellIndex => _grid.Index(_x, _y);

  public Vector2I GridPos => new(_x, _y);

  public override void _Ready()
  {
    _grid = GetParent() as DungeonGrid;
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
    _x = x;
    _y = y;
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

  public void SetWall(Node3D wall, Direction direction)
  {
    switch (direction)
    {
      case Direction.North: 
        North = wall;
        return;
      case Direction.East:
        East = wall;
        return;
      default :
        throw new Exception("Invalid direction: " + direction);
    }
  }
}
