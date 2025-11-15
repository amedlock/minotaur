using Godot;
using minotaur.Source.hud;
using minotaur.Source.model;
using minotaur.Source.player;

namespace minotaur.Source.views;

public partial class PlayerView : Node
{
  [Export] private PlayerController _controller;
  [Export] private GameModel _gameModel;

  public override void _Process(double delta)
  {
    switch (_gameModel.PlayerState)
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
          _controller.ShowMap(false);
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
      _controller.Attack();
    }

    if (Input.IsActionJustPressed("back"))
    {
      _controller.Retreat();
    }
  }

  // Move State
  private void HandleIdle()
  {
    if (Input.IsActionJustPressed("right"))
    {
      _controller.Turn(-90);
    }
    else if (Input.IsActionJustPressed("left"))
    {
      _controller.Turn(90);
    }
    else if (Input.IsActionJustPressed("forward"))
    {
      _controller.MoveForward();
    }
    else if (Input.IsActionJustPressed("back"))
    {
      _controller.MoveBackward();
    }
    else if (Input.IsActionJustPressed("look_left"))
    {
      _controller.Glance(90);
    }
    else if (Input.IsActionJustPressed("look_right"))
    {
      _controller.Glance(-90);
    }
    else if (Input.IsActionJustReleased("look_left") || Input.IsActionJustReleased("look_right"))
    {
      _controller.UnGlance();
    }
    else if (Input.IsActionJustPressed("open"))
    {
      _controller.OpenDoor();
    }
  }

  private void HandleGlance()
  {
    if ( !Input.IsActionPressed("look_left") && !Input.IsActionPressed("look_right"))
    {
      _controller.UnGlance();
    }
  }
}