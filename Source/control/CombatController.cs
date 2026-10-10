#region

using System;
using Godot;
using minotaur.Source.model;
using minotaur.Source.views;

#endregion

namespace minotaur.Source.control;

public partial class CombatController : Node
{
  private const float TurnTime = 2f;

  // player weapon broken?
  private bool _broken;

  [Export] private GameModel _gameModel;

  [Export] private PlayerController _playerController;

  [Export] private Hud _hud;

  [Export] private Dungeon _dungeon;

  private PlayerData _playerData;

  private EnemyState _enemy;

  private AnimationPlayer _enemyAnim;
  private AnimationPlayer _playerAnim;


  private AudioStreamPlayer _enemyAudio;
  private MazeCell _enemyCell;
  private ItemInfo _enemyItem;
  private double _enemyTurn;
  private Sprite2D _enemyWeapon;

  private AudioStream _fireballSound;
  private AudioStream _lightningSound;

  [Export] private MainGame _mainGame;


  // has player attacked this turn
  private bool _playerAttack;
  private AudioStreamPlayer _playerAudio;

  private ItemInfo _playerItem;

  // has player retreated this turn
  private bool _playerRetreat;

  private Sprite2D _playerWeapon;

  public bool PlayerAttack
  {
    set => _playerAttack = value;
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

    _fireballSound = ResourceLoader.Load<AudioStream>("res://data/sounds/fireball.wav");
    _lightningSound = ResourceLoader.Load<AudioStream>("res://data/sounds/lightning.wav");

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
    _enemy = cell.Enemy;
    _enemyItem = _gameModel.FindWeapon(_enemy.Info);
    _enemyTurn = 0;
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

    if (_playerAttack)
    {
      AttackMonster();
    }
    else if (_playerRetreat)
    {
      Retreat();
    }

    if (_enemy == null || _enemy.IsDead)
    {
      return;
    }

    if (_enemyTurn >= TurnTime)
    {
      EnemyFire();
      _enemyTurn = 0;
    }
  }

  private void Retreat()
  {
    SetProcess(false);
    _gameModel.PlayerState = PlayerState.Idle;
  }

  private void AttackMonster()
  {
    _playerAttack = false;
    PlayerFire();
  }

  private AudioStream GetSoundFx(ItemInfo item)
  {
    if (item == null)
    {
      return null;
    }

    if (item.Name is "fireball" or "small_fireball")
    {
      return _fireballSound;
    }

    if (item.Name is "wand" or "staff" or "scroll" or "book")
    {
      return _lightningSound;
    }

    return null;
  }


  private void PlayerFire()
  {
    _playerItem = _playerData.RightHand;
    if (_playerItem is not { IsWeapon: true }) return;

    _broken = false;
    var fx = GetSoundFx(_playerItem);
    if (_playerItem.IsBow)
    {
      if (_playerData.Arrows < 1) return;
      _playerWeapon.RegionRect = _gameModel.GameDb.FindIcon("arrow");
      _playerData.Arrows -= 1;
      _broken = GD.Randi() % 30 == 29;
    }
    else if (_playerItem.IsScroll)
    {
      _playerWeapon.RegionRect = _gameModel.GameDb.FindIcon("fireball");
      _broken = GD.Randi() % 25 == 24;
    }
    else if (_playerItem.IsBook)
    {
      _playerWeapon.RegionRect = _gameModel.GameDb.FindIcon("small_lightning");
      _broken = GD.Randi() % 25 == 24;
    }
    else
    {
      _playerWeapon.RegionRect = _playerItem.Image;
      _playerData.RightHand = null;
    }

    _playerWeapon.Modulate = _playerItem.Color;
    if (_playerItem.Spins)
      _playerAnim.Play("SpinFire");
    else
      _playerAnim.Play("Fire");

    if (fx != null)
    {
      _playerAudio.Stream = fx;
      _playerAudio.Play();
    }

    if (_gameModel.CurrentLevel.Depth <= 2) // don't break on first 2 levels
    {
      _broken = false;
    }

    _hud.UpdatePack();
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
      _hud.UpdatePack();
    }

    if (_enemy.IsDead)
    {
      // combat ends in _Process once any weapon still in flight has landed
      _enemyCell.Enemy = null;
      _dungeon.KillEnemy(_enemyCell.Coord);
    }
  }

  private void DamagePlayer()
  {
    // a weapon already in flight still lands after the enemy has died
    if (_enemy == null || _enemyItem == null) return;
    _gameModel.DamagePlayer(_enemyItem);
    _hud.UpdateStats();
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

    if (_enemyItem.Spins)
      _enemyAnim.Play("SpinFire");
    else
      _enemyAnim.Play("Fire");
  }
}