#region

using Godot;

#endregion

namespace minotaur.Source.dungeon;

public partial class DungeonGate : Node3D
{
  private float _maxSize = 0.03f;

  private float _minSize = 0.02f;

  public Sprite3D Sprite;
  public LevelType Type = LevelType.None;

  public override void _Ready()
  {
    Sprite = (Sprite3D)FindChild("Sprite3D");
  }
}