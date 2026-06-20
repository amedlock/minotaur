#region

using Godot;

#endregion

namespace minotaur.Source.views;

public partial class MainMenu : Node2D
{
  private bool _enabled;

  [Signal]
  public delegate void StartGameEventHandler(int skill);
  
  [Signal]
  public delegate void QuitGameEventHandler();
  
  public bool Enabled
  {
    get => _enabled;
    set
    {
      _enabled = value;
      Visible = value;
      SetProcessInput(value);
    }
  }

  public override void _Input(InputEvent @event)
  {
    if (@event is InputEventKey {Pressed: true} eventKey  )
    {
      int skill = eventKey.Keycode switch
      {
        Key.Key1 or Key.Kp1 => 1,
        Key.Key2 or Key.Kp2 => 2,
        Key.Key3 or Key.Kp3 => 3,
        Key.Key4 or Key.Kp4 => 4,
        _ => 0
      };
      if (skill > 0)
      {
        EmitSignal(SignalName.StartGame, skill);
      }
    }
  }
}