using Godot;

namespace minotaur.dungeon;

public partial class Wall : Node3D
{
  public WallType WallType = WallType.Wall;
  
  public bool Blocked => true;
  public bool Moving => false;
}