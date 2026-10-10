#region

using Godot;
using minotaur.Source.model;

#endregion

namespace minotaur.Source.views;

public partial class Player : Node3D
{
  public AudioStreamPlayer Audio { get; private set; }

  [Export] private Dungeon _dungeon;

  [Export] private GameModel _gameModel;

  private bool _enabled;

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

  public Wall WallBehind => null;
  public Node3D WallAhead => _dungeon.GetWall(_gameModel.PlayerCoord, Direction);

  public DungeonCell CurrentCell => _dungeon.GetCell(_gameModel.PlayerCoord);
  public DungeonCell CellAhead => _dungeon.GetCell(_gameModel.PlayerCoord, Direction);
  public DungeonCell CellBehind => _dungeon.GetCell(_gameModel.PlayerCoord, RearDirection);

  public Hud Hud { get; private set; }


  public override void _Ready()
  {
    Hud = GetNode<Hud>("Camera3D/HUD");
    Audio = GetNode<AudioStreamPlayer>("Audio");
  }

  public void Disable()
  {
    Hud.Hide();
    Visible = false;
  }
}