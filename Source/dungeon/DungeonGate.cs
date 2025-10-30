using Godot;

namespace minotaur.Source.dungeon;

public partial class DungeonGate : Node3D
{
  public LevelType Type = LevelType.None;

  private float _minSize = 0.02f;
  private float _maxSize = 0.03f;
  
  public Sprite3D Sprite;

  public override void _Ready()
  {
    Sprite = (Sprite3D)FindChild("Sprite3D");
  }
  
}