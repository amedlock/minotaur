using Godot;
using minotaur.Source.hud;
using minotaur.Source.model;
using minotaur.Source.player;

namespace minotaur.Source.views;

public partial class PlayerView : Node
{
  [Export] private PlayerController controller;
  [Export] private GameModel gameModel;
  [Export] private Hud hud;

  public override void _Process(double delta)
  {
    switch (gameModel.PlayerState)
    {
      case PlayerState.Idle:
      {
        HandleIdle(); break;
      }
      case PlayerState.Combat:
      {
        HandleCombat(); break;
      }
      case PlayerState.Glance:
      {
        HandleCombat(); break;
      }
      case PlayerState.ViewMap:
      {
        if (Input.IsActionJustPressed("view map"))
        {
          controller.ShowMap(false);
        }
        break;
      }
      case PlayerState.Won:
      {
        // todo
        break;
      }
    }
  }

  private void HandleCombat()
  {
    // check hud for attack
    if (Input.IsActionJustPressed("attack"))
    {
      controller.Attack();
    }

    if (Input.IsActionJustPressed("back"))
    {
      controller.Retreat();
    }
  }

  // Move State
  private void HandleIdle()
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

  private void HandleGlance()
  {
    if ( !Input.IsActionPressed("look_left") && !Input.IsActionPressed("look_right"))
    {
      controller.UnGlance();
    }
  }
}