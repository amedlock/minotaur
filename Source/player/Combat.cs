using System.Collections.Generic;
using Godot;
using minotaur.Source.dungeon;
using minotaur.Source.enemies;
using minotaur.Source.items;

namespace minotaur.Source.player;

public partial class Combat : Node
{
  private Player _player;
  private Dungeon _dungeon;
  private MainGame _mainGame;
  private GameDb _gameDb;

  private AnimationPlayer _playerAnim;
  private Sprite2D _playerWeapon;
  private AudioStreamPlayer _playerAudio;

  private AnimationPlayer _enemyAnim;
  private Sprite2D _enemyWeapon;
  private AudioStreamPlayer _enemyAudio;

  private AudioStream _fireballSound;
  private AudioStream _lightningSound;
  
  private ItemInfo _playerItem;
  private ItemInfo _enemyItem;
  private DungeonCell _enemyCell;
  private Enemy _enemy;
  private EnemyInfo _enemyInfo;

  private List<ItemInfo> _enemyWeapons;
  
  private const float TurnTime = 2f;
  private const float EnemyDelay = 0.75f;

  // time in turn so far
  private double _turnElapsed = 0.75f;

  // has player attacked this turn
  private bool _playerAttack = false;

  // has player retreated this turn
  private bool _playerRetreat = false;

  //  enemy can only do one thing: attack
  private bool _enemyAttack = false;
  
  // player weapon broken?
  private bool _broken = false;
  

  public override void _Ready()
  {
    _player = (Player)GetParent();
    _dungeon = (Dungeon)_player.GetParent();
    _mainGame = (MainGame)_dungeon.GetParent();
    _gameDb = (GameDb)_mainGame.GetNode("GameDB");
    
    _playerAnim = GetNode<AnimationPlayer>("PlayerAnim");
    _playerWeapon = GetNode<Sprite2D>("PlayerWeapon");
    _playerAudio = GetNode<AudioStreamPlayer>("PlayerWeapon/Audio");

    _enemyAnim = GetNode<AnimationPlayer>("EnemyAnim");
    _enemyWeapon = GetNode<Sprite2D>("EnemyWeapon");
    _enemyAudio = GetNode<AudioStreamPlayer>("EnemyWeapon/Audio");

    _fireballSound = ResourceLoader.Load<AudioStream>("res://data/sounds/fireball.wav");
    _lightningSound = ResourceLoader.Load<AudioStream>("res://data/sounds/lightning.wav");
    
    _playerWeapon.Visible = false;
    _enemyWeapon.Visible = false;
    SetProcess(false);
  }

  private void ResetTurn()
  {
    _turnElapsed = 0;
    _playerAttack = false;
    _enemyAttack = false;
    _playerRetreat = false;
  }


  public void Start(DungeonCell dungeonCell, bool attack)
  {
    if (dungeonCell.Enemy is not { Info: not null })
    {
      return;
    }

    _player.PlayerState = PlayerState.Combat;
    _enemyCell = dungeonCell;
    _enemy = _enemyCell.Enemy;
    _enemyInfo = _enemy.Info;
    _enemyWeapons = _gameDb.FindWeapons(_enemyInfo.Type, _dungeon.CurrentLevel);
    _enemyWeapon.Visible = false;
    _playerWeapon.Visible = false;
    ResetTurn();
    SetProcess(true);
    if (attack)
    {
      AttackMonster();
    }
  }

  public void EndTurn()
  {
    if (_playerAnim.IsPlaying() || _enemyAnim.IsPlaying())
    {
      return;
    }

    if (_player.IsDead)
    {
      _mainGame.GameOver();
      SetProcess(false);
      return;
    }

    if (_enemy.IsDead)
    {
      _player.WonCombat(_enemy);
      _enemyCell.RemoveEnemy();
      SetProcess(false);
      return;
    }

    if (_playerRetreat)
    {
      SetProcess(false);
    }
    else
    {
      ResetTurn();
    }
  }


  public override void _Process(double delta)
  {
    _turnElapsed += delta;
    if (_turnElapsed >= TurnTime || _player.IsDead)
    {
      EndTurn();
      return;
    }

    if (!_enemyAttack)
    {
      _enemyAttack = true;
      EnemyFire();
    }

    if (!(_playerRetreat || _playerAttack))
    {
      if (Input.IsActionJustPressed("back"))
      {
        _playerRetreat = true;
      }
    }
  }

  private void AttackMonster()
  {
    if (_playerAttack || _playerRetreat)
    {
      return;
    }

    _playerAttack = true;
    PlayerFire();
  }

  private AudioStream GetSoundFx(ItemInfo item)
  {
    if (item == null)
    {
      return null;
    }
    if (item.Name.Contains("fireball")) // "fireball", "small_fireball"
    {
      return _fireballSound;
    }

    if (item.Name.MatchN("wand|staff|scroll|book")) // "wand", "staff", "scroll", "book"
    {
      return _lightningSound;
    }

    return null;
  }
  

  private void PlayerFire()
  {
    _playerItem = _player.RightHand;
    if (_playerItem is not { IsWeapon: true })
    {
      return;
    }

    _broken = false;
    var missile = _gameDb.FindMissile(_playerItem) ?? _playerItem;
    var fx = GetSoundFx(_playerItem);
    if (_playerItem.IsBow){
      if (_player.Arrows < 1)
      {
        return;
      }
      _playerWeapon.RegionRect = missile.Image;
      _player.Arrows -= 1;
      _broken = (GD.Randi() % 30) == 29;
    }
    else if (_playerItem.IsBook)
    {
      _playerWeapon.RegionRect = missile.Image;
      _broken = (GD.Randi() % 25) == 24;
    }
    else
    {
      _player.RightHand = null;
      _playerWeapon.RegionRect = _playerItem.Image;	
    }
    _playerWeapon.Modulate = _playerItem.Color;

    if (_playerItem.Spins)
    {
      _playerAnim.Play("SpinFire");
    }
    else
    {
      _playerAnim.Play("Fire");
    }

    if (fx != null)
    {
      _playerAudio.Stream = fx;
      _playerAudio.Play();
    }

    if (_broken && _dungeon.CurrentLevel.Depth > 2)
    {
      _player.RightHand = null;  //clear out of the players hand
    }

    _player.Hud.UpdatePack();
  }
  

  private void DamageEnemy()
  {
    if (_playerItem == null)
    {
      return;
    }

    _enemy.Damage(_playerItem);
    _playerItem = null;
    if (_enemy.IsDead)
    {
      _enemy.Die();
    }

    if (_broken)
    {
      _player.RightHand = null;
    }
  }
  
  

  private void EnemyFire()
  {
    _enemyItem = _enemyWeapons[(int)(GD.Randi() % _enemyWeapons.Count)];
    _enemyWeapon.RegionRect = _enemyItem.Image;
    _enemyWeapon.Modulate = _enemyItem.Color;
    var fx = GetSoundFx(_enemyItem);
    if (fx != null)
    {
      _enemyAudio.Stream = fx;
      _enemyAudio.Play();
    }

    if (_enemyItem.Spins)
    {
      _enemyAnim.Play("SpinFire");
    }
    else
    {
      _enemyAnim.Play("Fire");
    }

    _player.Damage(_enemy, _enemyItem);
    _player.Hud.UpdatePack();
  }
  
}