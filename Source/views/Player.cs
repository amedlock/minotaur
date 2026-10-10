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
    }
  }


  // update location and rotation based on gameModel
  public void Update()
  {
    RotationDegrees = new Vector3(0, _gameModel.Facing.Degrees(), 0);
    Position = _dungeon.PlayerPosition;
  }

  // if glancing, the node rotation may not match the facing direction
  public Direction Direction => _gameModel.Facing;

  public Direction RearDirection => Direction.Opposite();

  public Node3D WallBehind => _dungeon.GetWall(_gameModel.PlayerCoord, RearDirection);
  public Node3D WallAhead => _dungeon.GetWall(_gameModel.PlayerCoord, Direction);


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