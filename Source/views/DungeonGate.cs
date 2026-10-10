#region

using Godot;
using minotaur.Source.model;

#endregion

namespace minotaur.Source.views;

public partial class DungeonGate : Node3D
{
  public Sprite3D Sprite;
  public LevelType Type = LevelType.None;

  public override void _Ready()
  {
    Sprite = (Sprite3D)FindChild("Sprite3D");
  }
}