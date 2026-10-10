#region

using Godot;

#endregion

namespace minotaur.Source.views;

//  debug output window
public partial class Debug : Label
{
  public override void _Ready()
  {
    SetProcess(false);
    Visible = false;
  }


  public override void _Process(double delta)
  {
    if (Input.IsActionJustPressed("debug"))
    {
      Visible = !Visible;
      SetProcess(Visible);
    }
  }
}
