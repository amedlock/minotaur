#region

using Godot;
using minotaur.Source.model;
using minotaur.Source.views;

#endregion

namespace minotaur.Source;

// initial Controller for the game
public partial class MainGame : Node3D
{
  // views
  [Export] private MainMenu _menu;

  [Export] private Dungeon _dungeon;

  [Export] private Player _player;

  [Export] private MapView _mapView;

  [Export] private Help _help;

  // game model
  [Export] GameModel _gameModel;

  private AudioStreamPlayer _audio;

  public override void _Ready()
  {
    _audio = new AudioStreamPlayer();
    AddChild(_audio);
    ShowMenu();
  }

  public override void _Input(InputEvent @event)
  {
    switch (_gameModel.GameMode)
    {
      case GameMode.Dungeon:
        // the map is shown while the key is held
        if (@event.IsActionPressed("view map") && _gameModel.PlayerState == PlayerState.Idle)
        {
          ShowMap();
        }
        else if (@event.IsActionPressed("show help"))
        {
          _help.Toggle();
        }

        break;

      case GameMode.Map:
        if (@event.IsActionReleased("view map"))
        {
          HideMap();
        }

        break;
    }

    if (@event.IsActionPressed("quit game"))
    {
      GetTree().Quit();
    }
  }

  public void StartGame(int skill)
  {
    uint seedNum = 0xdeadbeef; // _random.Randi();
    _gameModel.Init(skill, seedNum);
    _gameModel.CreateLevel(1);
    ShowGame();
    _help.Show();
  }

  public void GameOver()
  {
    ShowMap();
    _player.Disable();
    _gameModel.GameMode = GameMode.GameOver;
  }

  public void WonGame()
  {
    _audio.Stream = ResourceLoader.Load<AudioStream>("res://data/sounds/win2.wav");
    _audio.Play();
    ShowMap();
    _player.Disable();
    _gameModel.GameMode = GameMode.GameWon;
  }

  public void ShowGame()
  {
    _dungeon.Show();
    _player.Enabled = true;
    _menu.Enabled = false;
    _mapView.Hide();
    _gameModel.GameMode = GameMode.Dungeon;
  }

  public void ShowMenu()
  {
    _gameModel.GameMode = GameMode.Menu;
    _mapView.Hide();
    _dungeon.Hide();
    _player.Hide();
    _menu.Enabled = true;
  }

  public void ShowMap()
  {
    _mapView.Show();
    _player.Hide();
    _gameModel.GameMode = GameMode.Map;
  }

  public void HideMap()
  {
    _mapView.Hide();
    _player.Show();
    _gameModel.GameMode = GameMode.Dungeon;
  }
}