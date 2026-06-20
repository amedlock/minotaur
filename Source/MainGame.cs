#region

using System;
using System.Collections.Generic;
using Godot;
using minotaur.Source.dungeon;
using minotaur.Source.help;
using minotaur.Source.model;
using minotaur.Source.player;
using minotaur.Source.views;
using MainMenu = minotaur.Source.views.MainMenu;
using MapView = minotaur.Source.views.MapView;

#endregion

namespace minotaur.Source;

// initial Controller for the game
public partial class MainGame : Node3D
{
  private GameMode _gameMode;

  [Export] public Dispatcher Dispatcher;

  // views
  [Export] private MainMenu _menu;

  [Export] private Dungeon _dungeon;

  [Export] private Player _player;
  [Export] private PlayerController _playerController;

  [Export] private GameOver _gameOver;

  [Export] private MapView _mapView;

  [Export] 
  private Help _help;
  
  // game model
  [Export] GameModel _gameModel;

  
  public override void _Ready()
  {
    ShowMenu();
  }

  public override void _Input(InputEvent @event)
  {
    switch (_gameMode)
    {
      case GameMode.Dungeon:
        if (Input.IsActionJustPressed("view map"))
        {
          ShowMap();
        }

        if (Input.IsActionJustPressed("show help"))
        {
          _help.Toggle();
        }
        break;

      case GameMode.Map:
        if (Input.IsActionJustPressed("view map"))
        {
          ShowGame();
        }
        break;
    }

    if (Input.IsActionJustPressed("quit game"))
    {
      GetTree().Quit();
    }
  }

  public void StartGame(int skill)
  {
    uint seedNum = 0xdeadbeef; // _random.Randi();
    _gameModel.Init(skill, seedNum);
    _gameModel.CreateLevel(1);
    _dungeon.BuildLevel();
    _playerController.Init(skill);
    ShowGame();
    _help.Show();
  }

  public void GameOver()
  {
    ShowMap();
    _player.Disable();
    _gameMode = GameMode.GameOver;
  }

  public void WonGame()
  {
    ShowMap();
    _player.Disable();
    _gameMode = GameMode.GameWon;
  }

  public void ShowGame()
  {
    _dungeon.Show();
    _player.Enable();
    _menu.Enabled = false;
    _mapView.Hide();
    _gameMode = GameMode.Dungeon;
  }

  public void ShowMenu()
  {
    _gameMode = GameMode.Menu;
    _mapView.Hide();
    _dungeon.Hide();
    _player.Hide();
    _menu.Enabled = true;
  }

  public void ShowMap()
  {
    _mapView.Show();
    _player.Hide();
    _gameMode = GameMode.Map;
  }
}