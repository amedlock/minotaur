using Godot;

namespace minotaur.dungeon;

public partial class Wall : Node3D
{
  public bool Blocked => true;
  public bool Moving => false;
}