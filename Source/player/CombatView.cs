#region

using Godot;

#endregion

namespace minotaur.Source.player;

public partial class CombatView : Node
{
  [Export] private PlayerController _controller;


  public void Activate()
  {
  }


  public override void _Process(double delta)
  {
    if (Input.IsActionJustPressed("attack")) _controller.Attack();

    if (Input.IsActionJustPressed("back")) _controller.Retreat();
  }
}