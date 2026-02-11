#region

using Godot;

#endregion

namespace minotaur.Source.views;

public partial class MainMenu : Node2D
{
  private bool _enabled;
  private MainGame _game;

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

  public override void _Ready()
  {
    _game = GetParent() as MainGame;
  }

  public override void _Input(InputEvent @event)
  {
    if (@event is not InputEventKey eventKey) return;
    if (!eventKey.Pressed) return;
    switch (eventKey.Keycode)
    {
      case Key.Key1 or Key.Kp1: _game.StartGame(1); break;
      case Key.Key2 or Key.Kp2: _game.StartGame(2); break;
      case Key.Key3 or Key.Kp3: _game.StartGame(3); break;
      case Key.Key4 or Key.Kp4: _game.StartGame(4); break;
    }
  }
}