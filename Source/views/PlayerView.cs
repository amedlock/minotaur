#region

using Godot;
using minotaur.Source.player;

#endregion

namespace minotaur.Source.views;

public partial class PlayerView : Node
{
  [Export] private PlayerController controller;

  public override void _Process(double delta)
  {
    if (Input.IsActionJustPressed("right"))
    {
      controller.Turn(-90);
    }
    else if (Input.IsActionJustPressed("left"))
    {
      controller.Turn(90);
    }
    else if (Input.IsActionJustPressed("forward"))
    {
      controller.MoveForward();
    }
    else if (Input.IsActionJustPressed("back"))
    {
      controller.MoveBackward();
    }
    else if (Input.IsActionJustPressed("look_left"))
    {
      controller.Glance(90);
    }
    else if (Input.IsActionJustPressed("look_right"))
    {
      controller.Glance(-90);
    }
    else if (Input.IsActionJustReleased("look_left") || Input.IsActionJustReleased("look_right"))
    {
      controller.UnGlance();
    }
    else if (Input.IsActionJustPressed("open"))
    {
      controller.OpenDoor();
    }
  }
}