#region

using System;
using Godot;
using minotaur.Source.dungeon;
using minotaur.Source.enemies;
using minotaur.Source.hud;
using minotaur.Source.items;
using minotaur.Source.model;

#endregion

namespace minotaur.Source.player;

public partial class CombatController : Node
{
  private const float TurnTime = 2f;
  private const float EnemyDelay = 0.75f;

  // player weapon broken?
  private bool _broken;
  
  [Export]
  private GameModel _gameModel;
  
  [Export]
  private PlayerController _playerController;

  [Export]
  private Hud _hud;
  
  private PlayerData _playerData;
  
  private Enemy _enemy;

  private AnimationPlayer _enemyAnim;


  struct CombatState
  {
    public float timer;
    public bool attack;
    public bool animating;
    public bool playerDead;
    public bool enemyDead;
    public bool gameOver;
    public bool doAttack;
    public bool doRetreat;
    public bool endCombat;

    public void Reset()
    {
      attack = false;
      timer = 0f;
      animating = false;
      playerDead = false;
      enemyDead = false;
      gameOver = false;
      doAttack = false;
      doRetreat = false;
      endCombat = false;
    }
  }
  
  
  private CombatState _combatState;
  
  
  
  //  enemy can only do one thing: attack
  private bool _enemyAttack;
  private AudioStreamPlayer _enemyAudio;
  private DungeonCell _enemyCell;
  private EnemyInfo _enemyInfo;
  private ItemInfo _enemyItem;
  private double _enemyTurn;
  private Sprite2D _enemyWeapon;

  private AudioStream _fireballSound;
  private AudioStream _lightningSound;
 
  [Export]
  private MainGame _mainGame;
  
  private AnimationPlayer _playerAnim;

  // has player attacked this turn
  private bool _playerAttack;
  private AudioStreamPlayer _playerAudio;

  private ItemInfo _playerItem;

  // has player retreated this turn
  private bool _playerRetreat;

  private Sprite2D _playerWeapon;

  // time in turn so far
  private double _turnElapsed = 0.75f;

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

  public void Start(DungeonCell dungeonCell, bool attack)
  {
    if (dungeonCell.Enemy is not { Info: not null })
    {
      return;
    }

    GD.Print("Starting Combat");
    _gameModel.PlayerState = PlayerState.Combat;
    _enemyCell = dungeonCell;
    _enemy = _enemyCell.Enemy;
    _enemyInfo = _enemy.Info;
    _enemyItem = _gameModel.FindWeapon(_enemy);
    _enemyWeapon.Visible = false;
    _playerWeapon.Visible = false;
    _combatState.Reset();
    SetProcess(true);
    if (attack)
    {
      AttackMonster();
    }
  }


  public void EndCombat()
  {
    _gameModel.PlayerState = PlayerState.Idle;
    SetProcess(false);
    if (_playerData.IsDead)
    {
      _mainGame.GameOver();
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

    if (_combatState.timer > 0)
    {
      _combatState.timer = Math.Max(_combatState.timer - (float)delta, 0f);
    }

    _enemyTurn += delta;
    _combatState.attack = _combatState.attack || Input.IsActionJustPressed("attack");

    if (_combatState.timer <= 0)
    {
      if (_playerAttack)
      {
        AttackMonster();
        _combatState.timer = 0;
      }
      else if (_playerRetreat)
      {
        Retreat();
      }
    }
    if (_enemy==null || _enemy.IsDead)
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

    if (_broken && _gameModel.CurrentLevel.Depth > 2) // don't break on first 2 levels
    {
      _playerData.RightHand = null; //clear out of the players hand
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
      _playerController.Hud.UpdatePack();
    }
    
    if (_enemy.IsDead)
    {
      _enemy.Die();
      _playerController.WonCombat(_enemyInfo);
      _enemyCell.RemoveEnemy();
    }
  }

  private void DamagePlayer()
  {
    if (_enemy == null || _enemy.IsDead || _enemyItem == null) return;
    _gameModel.DamagePlayer(_enemy, _enemyItem);
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