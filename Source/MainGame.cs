using Godot;
using minotaur.Source.dungeon;
using minotaur.Source.help;
using minotaur.Source.map;
using minotaur.Source.menu;
using minotaur.Source.player;

namespace minotaur;

public partial class MainGame : Node3D
{
  private Player _player;
  private Dungeon _dungeon;
  private Help _help;
  private MainMenu _menu;
  private MapView _mapView;
  
  private GameMode _gameMode;
  private int _depth = 0;
  private int _skillLevel = 1;
  
  
  private RandomNumberGenerator _random = new();
  private uint _seedNum;

  public Player Player => _player;
  public Dungeon Dungeon => _dungeon;

  public override void _Ready()
  {
    _dungeon = FindChild("Dungeon") as Dungeon;
    _player = _dungeon.FindChild("Player") as Player;
    _help = FindChild("Help") as Help;
    _menu = FindChild("MainMenu") as MainMenu;
    _mapView = FindChild("MapView") as MapView;
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
    _seedNum = _random.Randi();
    _dungeon.InitMaze( skill, _seedNum );
    _player.Init(skill);
    ShowGame();
    _help.Show();
  }

  public void GameOver()
  {
    ShowMap();
    _player.Disable();
    _gameMode = GameMode.GameOver;
  }

  public void ShowGame()
  {
    _dungeon.Show();
    _player.Show();
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
    _dungeon.Hide();
    _gameMode = GameMode.Map;
  }
  
  
}