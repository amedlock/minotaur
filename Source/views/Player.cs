#region

using Godot;
using minotaur.Source.dungeon;
using minotaur.Source.hud;
using minotaur.Source.model;
using minotaur.Source.player;

#endregion

namespace minotaur.Source.views;

public partial class Player : Node3D
{
  private AudioStreamPlayer _audio;
  
  public AudioStreamPlayer Audio => _audio;

  // facing direction in degrees
  private PlayerState _playerState = PlayerState.Idle;

  [Export] private Marker3D _startPosition;

  [Export] private Dungeon _dungeon;

  [Export] private GameModel _gameModel;
  
  private PlayerData _playerData;

  private bool _enabled = false;

  public bool Enabled
  {
    get => _enabled;
    set
    {
      _enabled = value;
      Visible = value;
      SetProcessInput(value);
      Hud.Visible = value;
      if (value)
      {
        Hud.UpdateAll();
      }
    }
  }

  

  // update location and rotation based on gameModel
  public void Update()
  {
    var rot = _gameModel.Facing;
    RotationDegrees = new Vector3(0, rot, 0);
    var coord = _gameModel.PlayerCoord;
    Position = _dungeon.PlayerPosition;
  }
  
  // player direction in degrees
  // if glancing may not be in sync with Node RotationDegrees.Y
  public int Dir
  {
    get => _gameModel.Facing;
    set
    {
      _gameModel.Facing = Mathf.PosMod(value, 360);
      RotationDegrees = new Vector3(0, _gameModel.Facing, 0);
    }
  }

  public Direction Direction
  {
    get
    {
      return Dir switch
      {
        0 => Direction.North,
        90 => Direction.West,
        180 => Direction.South,
        _ => Direction.East
      };
    }
  }

  public Direction RearDirection
  {
    get
    {
      return Dir switch
      {
        0 => Direction.South,
        90 => Direction.East,
        180 => Direction.North,
        _ => Direction.West
      };
    }
  }


  public Vector2I ForwardVector
  {
    get
    {
      var bz = Transform.Basis.Z;
      return new Vector2I(Mathf.RoundToInt(bz.X), Mathf.RoundToInt(bz.Z));
    }
  }

  public Wall WallBehind => null;
  public Node3D WallAhead => _dungeon.GetWall(_gameModel.PlayerCoord, Direction);

  public DungeonCell CurrentCell => _dungeon.GetCell(_gameModel.PlayerCoord);
  public DungeonCell CellAhead => _dungeon.GetCell(_gameModel.PlayerCoord, Direction);
  public DungeonCell CellBehind => _dungeon.GetCell(_gameModel.PlayerCoord, RearDirection);

  public Hud Hud { get; private set; }


  public override void _Ready()
  {
    _playerData = _gameModel.PlayerData;
    Hud = GetNode<Hud>("Camera3D/HUD");
    _audio = GetNode<AudioStreamPlayer>("Audio");
  }
  
  public Vector3 CoordToWorld(Vector2I coord)
  {
    return _startPosition.Position + new Vector3(coord.X * 3f, 0, -coord.Y * 3);
  }

  public void Enable()
  {
    Visible = true;
    Hud.Show();
    Hud.UpdateAll();
  }

  public void Disable()
  {
    Hud.Hide();
    Visible = false;
  }
  

}