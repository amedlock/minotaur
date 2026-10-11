#region

using System;
using Godot;
using minotaur.Source.model;

#endregion

namespace minotaur.Source.control;

public partial class CombatController : Node
{
  private const float TurnTime = 2f;

  // player weapon broken?
  private bool _broken;

  [Export] private GameModel _gameModel;

  [Export] private PlayerController _playerController;

  private PlayerData _playerData;

  private EnemyState _enemy;

  private AnimationPlayer _enemyAnim;
  private AnimationPlayer _playerAnim;


  private AudioStreamPlayer _enemyAudio;
  private MazeCell _enemyCell;
  private ItemInfo _enemyItem;
  private double _enemyTurn;
  private Sprite2D _enemyWeapon;

  // weapon sounds by name
  private readonly System.Collections.Generic.Dictionary<string, AudioStream> _sounds = new();

  [Export] private MainGame _mainGame;


  // has player attacked this turn
  private bool _playerAttack;
  private AudioStreamPlayer _playerAudio;

  private ItemInfo _playerItem;

  // the player has asked to retreat, it happens once the enemy has had a shot
  private bool _playerRetreat;

  // an enemy shot has landed since this fight started
  private bool _enemyHasFired;

  private Sprite2D _playerWeapon;

  public bool PlayerAttack
  {
    set => _playerAttack = value;
  }

  public bool PlayerRetreat
  {
    set => _playerRetreat = value;
  }

  public override void _Ready()
  {
    _playerData = _gameModel.PlayerData;

    _playerAnim = GetNode<AnimationPlayer>("PlayerAnim");
    _playerAnim.AnimationFinished += name => DamageEnemy();
    _playerWeapon = GetNode<Sprite2D>("PlayerWeapon");
    _playerAudio = GetNode<AudioStreamPlayer>("PlayerWeapon/Audio");

    _enemyAnim = GetNode<AnimationPlayer>("EnemyAnim");
    _enemyAnim.AnimationFinished += name => DamagePlayer();
    _enemyWeapon = GetNode<Sprite2D>("EnemyWeapon");
    _enemyAudio = GetNode<AudioStreamPlayer>("EnemyWeapon/Audio");

    foreach (var pair in _gameModel.GameDb.Sounds)
    {
      _sounds[pair.Key] = ResourceLoader.Load<AudioStream>(pair.Value);
    }

    _playerWeapon.Visible = false;
    _enemyWeapon.Visible = false;
    SetProcess(false);
  }

  public void Start(MazeCell cell, bool attack)
  {
    if (cell?.Enemy == null)
    {
      return;
    }

    GD.Print("Starting Combat");
    _gameModel.PlayerState = PlayerState.Combat;
    _enemyCell = cell;
    // coming back to the enemy just retreated from does not restart its attack timer
    if (_enemy != cell.Enemy)
    {
      _enemyTurn = 0;
    }

    _enemy = cell.Enemy;
    _enemyItem = _gameModel.FindWeapon(_enemy.Info);
    _playerRetreat = false;
    _enemyHasFired = false;
    _enemyWeapon.Visible = false;
    _playerWeapon.Visible = false;
    SetProcess(true);
    if (attack)
    {
      AttackMonster();
    }
  }


  public override void _Process(double delta)
  {
    if (_gameModel.PlayerState != PlayerState.Combat)
    {
      SetProcess(false);
      return;
    }

    if (_playerData.IsDead)
    {
      _mainGame.GameOver();
      SetProcess(false);
      return;
    }

    // enemy is dead, stay in combat until weapons in flight have landed
    if (_enemy is { IsDead: true })
    {
      _playerAttack = false;
      if (!_enemyAnim.IsPlaying() && !_playerAnim.IsPlaying())
      {
        _playerController.WonCombat(_enemy.Info);
      }

      return;
    }

    _enemyTurn += delta;

    if (_playerRetreat)
    {
      // no more attacks from either side, the retreat waits for the weapons in flight to land
      _playerAttack = false;
      if (_enemyHasFired)
      {
        if (!_enemyAnim.IsPlaying() && !_playerAnim.IsPlaying())
        {
          Retreat();
        }

        return;
      }
    }
    else if (_playerAttack)
    {
      AttackMonster();
    }

    if (_enemy == null || _enemy.IsDead)
    {
      return;
    }

    if (_enemyTurn >= TurnTime && !_enemyAnim.IsPlaying())
    {
      EnemyFire();
      _enemyTurn = 0;
    }
  }

  // ends combat with the enemy still alive, it keeps the damage it has taken
  private void Retreat()
  {
    _playerRetreat = false;
    SetProcess(false);
    _gameModel.PlayerState = PlayerState.Idle;
    _playerController.Retreated();
  }

  private void AttackMonster()
  {
    _playerAttack = false;
    PlayerFire();
  }

  private AudioStream GetSoundFx(ItemInfo item)
  {
    if (item?.Sound == null)
    {
      return null;
    }

    return _sounds.TryGetValue(item.Sound, out var stream) ? stream : null;
  }


  private void PlayerFire()
  {
    _playerItem = _playerData.RightHand;
    if (_playerItem is not { IsWeapon: true }) return;

    if (_playerItem.Ammo == "arrows")
    {
      if (_playerData.Arrows < 1) return;
      _playerData.Arrows -= 1;
    }

    _broken = _playerItem.BreakChance > 0 && GD.Randi() % _playerItem.BreakChance == 0;
    if (_gameModel.CurrentLevel.Depth <= 2) // don't break on first 2 levels
    {
      _broken = false;
    }

    if (_playerItem.SingleUse)
    {
      _playerData.RightHand = null; //clear out of the players hand
    }

    _playerWeapon.RegionRect = _playerItem.Projectile;
    _playerWeapon.Modulate = _playerItem.Color;
    if (_playerItem.Spin)
      _playerAnim.Play("SpinFire");
    else
      _playerAnim.Play("Fire");

    var fx = GetSoundFx(_playerItem);
    if (fx != null)
    {
      _playerAudio.Stream = fx;
      _playerAudio.Play();
    }
  }


  private void DamageEnemy()
  {
    if (_playerItem == null)
    {
      return;
    }

    _enemy.Damage(_playerItem);
    _playerItem = null;
    if (_broken)
    {
      GD.Print("Weapon broke");
      _playerData.RightHand = null;
    }

    if (_enemy.IsDead)
    {
      // combat ends in _Process once any weapon still in flight has landed
      _gameModel.KillEnemy(_enemyCell);
    }
  }

  private void DamagePlayer()
  {
    // a weapon already in flight still lands after the enemy has died
    if (_enemy == null || _enemyItem == null) return;
    _gameModel.DamagePlayer(_enemyItem);
    _enemyHasFired = true;
  }


  private void EnemyFire()
  {
    _enemyWeapon.RegionRect = _enemyItem.Image;
    _enemyWeapon.Modulate = _enemyItem.Color;
    var fx = GetSoundFx(_enemyItem);
    if (fx != null)
    {
      _enemyAudio.Stream = fx;
      _enemyAudio.Play();
    }

    if (_enemyItem.Spin)
      _enemyAnim.Play("SpinFire");
    else
      _enemyAnim.Play("Fire");
  }
}