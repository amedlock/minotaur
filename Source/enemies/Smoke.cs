#region

using Godot;

#endregion

namespace minotaur.Source.enemies;

public partial class Smoke : Sprite3D
{
  private AnimationPlayer _animationPlayer;

  public override void _Ready()
  {
    _animationPlayer = GetNode<AnimationPlayer>("Animation");
    _animationPlayer.AnimationFinished += Remove;
  }

  public void Start()
  {
    Visible = true;
    _animationPlayer.Play("Puff");
  }

  private void Remove(StringName name)
  {
    GetParent().RemoveChild(this);
    QueueFree();
  }
}