#region

using Godot;

#endregion

namespace minotaur.Source.views;

public partial class OuterWall : Node3D
{
  public bool Blocked = true;
  public bool Moving = false;
}