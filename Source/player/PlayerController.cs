#region

using System;
using Godot;
using minotaur.Source.dungeon;
using minotaur.Source.enemies;
using minotaur.Source.hud;
using minotaur.Source.items;
using minotaur.Source.model;
using minotaur.Source.views;

#endregion

namespace minotaur.Source.player;

public partial class PlayerController : Node
{
  private const float MoveTime = 0.75f;
  private const float TurnTime = 0.3f;
  private const float GlanceTime = 0.25f;

  private int _glanceAmount;

  [Export] private CombatController _combatController;

  [Export] private Dungeon _dungeon;

  [Export] private GameModel _gameModel;

  [Export] private Hud _hud;

  [Export] private MapView _mapView;

  [Export] private Player _player;
  
  [Export]
  private MainGame _mainGame;

  private PlayerData _playerData;
  public Hud Hud => _hud;

  public override void _Ready()
  {
    _playerData = _gameModel.PlayerData;
  }


  public void Init(int skill)
  {
    _gameModel.Skill = skill;
    _gameModel.Facing = 270;
    _playerData.Gold = 0;
    _playerData.WarExp = 0;
    _playerData.MagicExp = 0;
    _playerData.Food = 10 - skill;
    _playerData.Arrows = 9 - skill;
    _playerData.Resurrected = false;
    _playerData.ClearSlots();

    _playerData.RightHand = _gameModel.GameDb.FindItem("bow");
    switch (skill)
    {
      case 1:
        _playerData.Health = 18;
        _playerData.Mind = 9;
        _playerData.LeftHand = _gameModel.GameDb.FindItem("small_shield");
        break;
      case 2:
        _playerData.Health = 16;
        _playerData.Mind = 7;
        break;
      case 3:
        _playerData.Health = 14;
        _playerData.Mind = 7;
        break;
      case 4:
        _playerData.Health = 12;
        _playerData.Mind = 6;
        break;
      default:
        throw new Exception("Invalid skill: " + skill);
    }

    _playerData.MindMax = _playerData.Mind;
    _playerData.HealthMax = _playerData.Health;
    _hud.UpdateAll();
    _player.Update();
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
    var pos = _dungeon.PlayerPosition;
    var delta = _player.Transform.Basis.Z * -3;
    _gameModel.PlayerState = PlayerState.Moving;
    var tween = CreateTween().TweenProperty(_player, "position", pos + delta, MoveTime);
    tween.Finished += () =>
    {
      _gameModel.PlayerState = PlayerState.Idle;
      _gameModel.PlayerCoord = nextCell.Coord;
      _player.Update();
      _hud.UpdateAll();
    };
  }

  public void MoveBackward()
  {
    var wall = _player.WallBehind;
    if (wall != null && wall.Blocked) return;

    var nextCell = _player.CellBehind;

    var pos = _dungeon.PlayerPosition;
    var delta = _player.Transform.Basis.Z * -3f;
    _gameModel.PlayerState = PlayerState.Moving;
    var tween = CreateTween().TweenProperty(_player, "position", pos - delta, MoveTime);
    tween.Finished += () =>
    {
      _gameModel.PlayerState = PlayerState.Idle;
      _gameModel.PlayerCoord = nextCell.Coord;
      _player.Update();
      _hud.UpdateAll();
    };
  }

  public void Turn(int amount)
  {
    _gameModel.PlayerState = PlayerState.Turning;
    var rot = _player.Dir + amount;
    var dir = Mathf.PosMod(rot, 360);
    var compassRot = Mathf.PosMod(_hud.Compass.RotationDegrees - rot, 360);
    var tween = CreateTween();
    tween.TweenProperty(_player, "Dir", rot, TurnTime);
    tween.Parallel().TweenProperty(_hud.Compass, "rotation_degrees", compassRot, TurnTime);
    tween.Finished += () =>
    {
      _player.Dir = dir;
      _gameModel.PlayerState = PlayerState.Idle;
    };
  }

  public void Glance(int amount)
  {
    var rot = _player.Dir + amount;
    _gameModel.PlayerState = PlayerState.Turning;
    var tween = CreateTween().TweenProperty(_player, "rotation_degrees:y", rot, GlanceTime);
    tween.Finished += () =>
    {
      _gameModel.PlayerState = PlayerState.Glance;
      _player.Dir = Mathf.PosMod(rot, 360);
      _glanceAmount += amount;
    };
  }

  public void UnGlance()
  {
    if (_gameModel.PlayerState != PlayerState.Glance)
    {
      return;
    }

    var rot = _player.Dir - _glanceAmount;
    _gameModel.PlayerState = PlayerState.Turning;
    var tween = CreateTween().TweenProperty(_player, "rotation_degrees:y", rot, GlanceTime);
    tween.Finished += () =>
    {
      _gameModel.PlayerState = PlayerState.Idle;
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
    _combatController.Start(cell, attack);
  }

  public void Attack()
  {
  }

  public void Retreat()
  {
  }


  public void Rest()
  {
    if (_playerData.Food < 1 || !_playerData.needsRest) return;

    if (_playerData.Health == _playerData.HealthMax && _playerData.Mind == _playerData.MindMax)
    {
      return;
    }

    _playerData.needsRest = false;
    var hpGain = Mathf.FloorToInt(_playerData.WarExp / 4.0);
    var mindGain = Mathf.FloorToInt(_playerData.MagicExp / 5.0);
    _playerData.WarExp = _playerData.WarExp % 4;
    _playerData.MagicExp = _playerData.MagicExp % 5;
    _playerData.HealthMax += hpGain;
    _playerData.MindMax += mindGain;
    _playerData.Health = Mathf.Min(_playerData.Health + Mathf.FloorToInt(_playerData.HealthMax * 2.0 / 3.0),
      _playerData.HealthMax);
    _playerData.Mind = Mathf.Min(_playerData.Mind + Mathf.FloorToInt(_playerData.MindMax * 2.0 / 3.0),
      _playerData.MindMax);
    _playerData.Food -= 1;
    _hud.UpdateAll();
  }


  public void UseExit()
  {
    if (_gameModel.PlayerState == PlayerState.Idle && _gameModel.OverExit)
    {
      _player.Audio.Stream = ResourceLoader.Load<AudioStream>("res://data/sounds/descend.wav");
      _player.Audio.Play();
      _gameModel.NextLevel();
      _dungeon.BuildLevel();
      _mapView.UpdateMap(_gameModel.CurrentLevel);
      _player.Update();
      _hud.UpdateAll();
      _gameModel.PlayerState = PlayerState.Idle;
    }
  }


  public void ShowMap(bool visible)
  {
    if (visible)
    {
      _mapView.Show();
      _player.Hide();
    }
    else
    {
      _mapView.Hide();
      _player.Show();
    }
  }

  public void ClickSlot(int slotNum, bool rightClick)
  {
    if (rightClick)
    {
      var item = _playerData.GetSlot(slotNum);
      _playerData.SetSlot(slotNum, _playerData.RightHand);
      _playerData.RightHand = item;
    }
    else
    {
      var item = _playerData.GetSlot(slotNum);
      _playerData.SetSlot(slotNum, _playerData.LeftHand);
      _playerData.LeftHand = item;
    }
  }

  public void ClickFeet()
  {
    var item = _gameModel.ItemAtFeet;
    UseOrTakeItem();
  }


  private void OpenContainer(MazeCell cell, ItemInfo container)
  {
    if (container.NeedsKey)
    {
      var rh = _playerData.RightHand;
      if (rh is { ItemType: ItemType.Key } && rh.Stat1 >= container.Stat1)
      {
        cell.ItemInfo = _gameModel.ChooseTreasure(container);
      }
      else
      {
        SwapWithFeet();
        return;
      }
    }
    else
    {
      cell.ItemInfo = _gameModel.ChooseTreasure(container);
    }
    _dungeon.CurrentCell.SetItem(cell.ItemInfo);
    _hud.UpdatePack();
  }

  public void ClickRightHand()
  {
    if (_gameModel.PlayerState == PlayerState.Combat)
    {
      _combatController.PlayerAttack = true;
    }
    else
    {
      StartCombat(_player.CellAhead, true);
    }
  }

  public void SwapHands()
  {
    (_playerData.LeftHand, _playerData.RightHand) = (_playerData.RightHand, _playerData.LeftHand);
    _hud.UpdatePack();
  }

  public void SwapWithFeet()
  {
    (_playerData.RightHand, _gameModel.ItemAtFeet) = (_gameModel.ItemAtFeet, _playerData.RightHand);
    _dungeon.CurrentCell.SetItem(_gameModel.ItemAtFeet);
    _hud.UpdatePack();
  }

  private void RemoveItem()
  {
    _gameModel.CurrentCell.ItemInfo = null;
    _dungeon.CurrentCell.RemoveItem();
    _hud.UpdateAll();
  }
  

  // user wants to use/take item at their feet
  public void UseOrTakeItem()
  {
    var cell = _gameModel.CurrentCell;
    var itemInfo = cell.ItemInfo;
    if (itemInfo == null)
    {
      return;
    }
    
    if (itemInfo is { ItemType: ItemType.Special })
    {
      switch (itemInfo.Name)
      {
        case "ladder": 
          return;
        case "treasure": 
          _mainGame.WonGame();
          return;
        case "quiver" or "food":
          _playerData.AddSpecial(itemInfo.Name, itemInfo.Stat1);
          RemoveItem();
          return;
      }
      return;
    }


    switch (itemInfo.ItemType)
    {
      case ItemType.Ladder:
        return;
      
      case ItemType.Money or ItemType.Treasure:
        _playerData.Gold += itemInfo.Stat1;
        RemoveItem();
        return;
      
      case ItemType.Container:
        OpenContainer(cell, itemInfo);
        return;
      
      case ItemType.Armor or ItemType.MagicArmor:
        _playerData.AddArmor(itemInfo);
        RemoveItem();
        return;

      default:
        (_playerData.RightHand, _gameModel.ItemAtFeet) = (_gameModel.ItemAtFeet, _playerData.RightHand);
        _dungeon.CurrentCell.SetItem(_gameModel.ItemAtFeet);
        _hud.UpdatePack();
        return;
    }
  }

  
  public void EnterGate(DungeonGate dungeonGate)
  {
    _player.Audio.Stream = ResourceLoader.Load<AudioStream>("res://data/sounds/magic.wav");
    _gameModel.LoadGateLevel(dungeonGate);
    _playerData.needsRest = true;
  }

  public void WonCombat(EnemyInfo enemy)
  {
    Killed(enemy);
    _playerData.needsRest = true;
    _gameModel.PlayerState = PlayerState.Idle;
    _hud.UpdateAll();
  }

  private void Killed(EnemyInfo enemy)
  {
    // switch (enemy.Info.Type)
    // {
    //   case EnemyType.War:
    //     WarExp += enemy.Info.WarHp;
    //     break;
    //   case EnemyType.Magic:
    //     MagicExp += enemy.Info.MindHp;
    //     break;
    //   case EnemyType.Both:
    //     WarExp += (enemy.Info.WarHp * 2 ) / 3;
    //     MagicExp += (enemy.Info.MindHp * 2 ) / 3;
    //     break;
    // }

    // if (enemy.Info.Name == "minotaur")
    // {
    //   _dungeon.AddFinal(enemy);
    // }

    // if (IsDead)
    // {
    //   _dungeon.Game.GameOver();
    // }
  }
  
}