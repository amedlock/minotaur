using Godot;

namespace minotaur.help;

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
