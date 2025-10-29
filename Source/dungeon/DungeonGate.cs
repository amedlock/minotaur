using Godot;

namespace minotaur.Source.dungeon;

public partial class DungeonGate : Node3D
{
  private Vector3 Up = new Vector3(0f, 1f, 0f);

  public LevelType Type = LevelType.None;

  private float _minSize = 0.02f;
  private float _maxSize = 0.03f;
  
  public Sprite3D Sprite;

  public override void _Ready()
  {
    Sprite = FindChild("Sprite3D") as Sprite3D;
  }
  
}