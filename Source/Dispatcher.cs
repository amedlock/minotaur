using Godot;
using minotaur.Source.model;
using minotaur.Source.views;

namespace minotaur.Source;

public partial class Dispatcher : Node
{
  [Export]
  public MainGame MainGame;
  
  [Export]
  public MainMenu MainMenu;

  [Export]
  public GameModel GameModel;
  
  public override void _Ready()
  {
    MainMenu.StartGame += skill => MainGame.StartGame(skill);
  }
}
