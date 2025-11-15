#region

using Godot;
using minotaur.Source.dungeon;
using minotaur.Source.hud;

#endregion

namespace minotaur.Source.player;

public partial class PlayerController : Node
{
  private const float MoveTime = 0.75f;
  private const float TurnTime = 0.3f;
  private const float GlanceTime = 0.25f;

  private int _glanceAmount = 0;
  
  [Export] private Combat _combat;

  [Export] private Dungeon _dungeon;

  [Export] private Hud _hud;

  [Export] private Player _player;

  public override void _Ready()
  {
    // _player = GetParent<Player>();
    // _dungeon = _player.GetParent<Dungeon>();
    // _hud = _player.GetNode<Hud>("Camera3D/HUD");
  }

  public void MoveForward()
  {
    var wall = _player.WallAhead;
    if (wall is Wall or Door { Blocked: true })
    {
      return;
    }
    var nextCell = _player.CellAhead;
    if (nextCell is { Enemy: not null })
    {
      StartCombat(nextCell, true);
      return;
    }

    // _player.PrevCoord = _player.Coord;
    // _player.CanRetreat = true;
    var pos = _player.Position;
    var delta = _player.Transform.Basis.Z * -3;
    _player.PlayerState = PlayerState.Moving;
    var tween = CreateTween().TweenProperty(_player, "position", pos + delta, MoveTime);
    tween.Finished += () =>
    {
      _player.PlayerState = PlayerState.Idle;
      _player.Coord = nextCell.Coord;
      _hud.UpdateAll();
    };
  }

  public void MoveBackward()
  {
    var wall = _player.WallBehind;
    if (wall != null && wall.Blocked) return;

    var nextCell = _player.CellBehind;

    var pos = _player.Position;
    var pvec = _player.Transform.Basis.Z * -3f;
    _player.PlayerState = PlayerState.Moving;
    var tween = CreateTween().TweenProperty(_player, "position", pos - pvec, MoveTime);
    tween.Finished += () =>
    {
      _hud.UpdateAll();
      _player.Coord = nextCell.Coord;
      _player.PlayerState = PlayerState.Idle;
    };
  }

  public void Turn(int amount)
  {
    _player.PlayerState = PlayerState.Turning;
    var rot = _player.Dir + amount;
    var dir = Mathf.PosMod(rot, 360);
    var compassRot = Mathf.PosMod(_hud.Compass.RotationDegrees - rot, 360);
    var tween = CreateTween();
    tween.TweenProperty(_player, "Dir", rot, TurnTime);
    tween.Parallel().TweenProperty(_hud.Compass, "rotation_degrees", compassRot, TurnTime);
    tween.Finished += () =>
    {
      _player.Dir = dir;
      _player.PlayerState = PlayerState.Idle;
    };
  }

  public void Glance(int amount)
  {
    var rot = _player.Dir + amount;
    _player.PlayerState = PlayerState.Turning;
    var tween = CreateTween().TweenProperty(_player, "rotation_degrees:y", rot, GlanceTime);
    tween.Finished += () =>
    {
      _player.PlayerState = PlayerState.Glance;
      _player.Dir = Mathf.PosMod(rot, 360);
      _glanceAmount += amount; 
    };
  }

  public void UnGlance()
  {
    if (_player.PlayerState != PlayerState.Glance)
    {
      return;
    }
    var rot = _player.Dir - _glanceAmount;
    _player.PlayerState = PlayerState.Turning;
    var tween = CreateTween().TweenProperty(_player, "rotation_degrees:y", rot, GlanceTime);
    tween.Finished += () =>
    {
      _player.PlayerState = PlayerState.Idle;
      _glanceAmount = 0;
      _player.Dir = Mathf.PosMod(rot, 360);
    };
  }

  public void OpenDoor()
  {
    var wall = _player.WallAhead;
    if (wall is Door door) door.Activate();
  }

  public void StartCombat(DungeonCell cell, bool attack)
  {
    _combat.Start(cell, attack);
  }

  public void Attack()
  {
  }

  public void Retreat()
  {
  }
}