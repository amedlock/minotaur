#region

using Godot;

#endregion

namespace minotaur.Source.help;

public partial class Help : Node2D
{
  public override void _Ready()
  {
    Visible = false;
  }

  public void Toggle()
  {
    Visible = !Visible;
  }
}