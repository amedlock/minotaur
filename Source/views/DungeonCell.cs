#region

using System;
using Godot;
using minotaur.Source.model;

#endregion

namespace minotaur.Source.views;

public partial class DungeonCell : Node3D
{
  public Enemy Enemy;
  public DungeonGate Gate;


  // contents of cell
  public Item Item;

  // walls
  public Node3D North;
  public Node3D East;

  public int X { get; private set; }

  public int Y { get; private set; }

  public ItemInfo ItemInfo => Item == null ? null : Item.Info;
  public Vector2I Coord => new(X, Y);

  public void Init(int x, int y)
  {
    X = x;
    Y = y;
    Name = $"Cell_{X}_{Y}";
    Position = GridGeometry.CellOrigin(Coord);
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
      Enemy = null;
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
}