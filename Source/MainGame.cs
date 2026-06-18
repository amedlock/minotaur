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
  private int _depth;

  private GameMode _gameMode;
  

  // views
  [Export] private MainMenu menu;

  [Export] private Dungeon dungeon;

  [Export] private Player player;
  [Export] private PlayerController playerController;

  [Export] private GameOver gameOver;

  [Export] private MapView _mapView;

  [Export] 
  private Help _help;
  
  // game model
  [Export] GameModel gameModel;

  
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
    gameModel.Init(skill, seedNum);
    gameModel.CreateLevel(1);
    dungeon.BuildLevel();
    playerController.Init(skill);
    ShowGame();
    _help.Show();
  }

  public void GameOver()
  {
    ShowMap();
    player.Disable();
    _gameMode = GameMode.GameOver;
  }

  public void WonGame()
  {
    ShowMap();
    player.Disable();
    _gameMode = GameMode.GameWon;
  }

  public void ShowGame()
  {
    dungeon.Show();
    player.Enable();
    menu.Enabled = false;
    _mapView.Hide();
    _gameMode = GameMode.Dungeon;
  }

  public void ShowMenu()
  {
    _gameMode = GameMode.Menu;
    _mapView.Hide();
    dungeon.Hide();
    player.Hide();
    menu.Enabled = true;
  }

  public void ShowMap()
  {
    _mapView.Show();
    player.Hide();
    _gameMode = GameMode.Map;
  }
}