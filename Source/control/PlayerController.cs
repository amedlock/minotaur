#region

using System;
using Godot;
using minotaur.Source.model;
using minotaur.Source.views;

#endregion

namespace minotaur.Source.control;

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

  [Export] private Player _player;

  [Export] private MainGame _mainGame;

  private PlayerData _playerData;

  public override void _Ready()
  {
    _playerData = _gameModel.PlayerData;
  }

  public void MoveForward()
  {
    var wall = _player.WallAhead;
    if (wall is Wall or Door { Blocked: true })
    {
      return;
    }

    var nextCell = _gameModel.CellAhead;
    if (nextCell == null)
    {
      return;
    }

    if (nextCell.Enemy != null)
    {
      StartCombat(nextCell, true);
      return;
    }

    var pos = _dungeon.PlayerPosition;
    var delta = _player.Transform.Basis.Z * -GridGeometry.CellSize;
    _gameModel.PlayerState = PlayerState.Moving;
    var tween = CreateTween().TweenProperty(_player, "position", pos + delta, MoveTime);
    tween.Finished += () =>
    {
      _gameModel.PlayerState = PlayerState.Idle;
      _gameModel.PlayerCoord = nextCell.Coord;
      EnterGate();
    };
  }

  // the cell a backward step leads to, null when a wall, closed door or enemy is in the way
  private MazeCell OpenCellBehind()
  {
    if (_player.WallBehind is Wall or Door { Blocked: true })
    {
      return null;
    }

    var cell = _gameModel.CellBehind;
    return cell?.Enemy == null ? cell : null;
  }

  public void MoveBackward()
  {
    var nextCell = OpenCellBehind();
    if (nextCell == null)
    {
      return;
    }

    var pos = _dungeon.PlayerPosition;
    var delta = _player.Transform.Basis.Z * -GridGeometry.CellSize;
    _gameModel.PlayerState = PlayerState.Moving;
    var tween = CreateTween().TweenProperty(_player, "position", pos - delta, MoveTime);
    tween.Finished += () =>
    {
      _gameModel.PlayerState = PlayerState.Idle;
      _gameModel.PlayerCoord = nextCell.Coord;
      EnterGate();
    };
  }

  public void Turn(int amount)
  {
    _gameModel.PlayerState = PlayerState.Turning;
    var start = _gameModel.Facing.Degrees();
    var rot = start + amount;
    var compassRot = Mathf.PosMod(_hud.Compass.RotationDegrees - rot, 360);
    var tween = CreateTween();
    tween.TweenProperty(_player, "rotation_degrees:y", rot, TurnTime).From(start);
    tween.Parallel().TweenProperty(_hud.Compass, "rotation_degrees", compassRot, TurnTime);
    tween.Finished += () =>
    {
      _gameModel.Facing = DirectionExtensions.FromDegrees(rot);
      _gameModel.PlayerState = PlayerState.Idle;
    };
  }

  public void Glance(int amount)
  {
    var start = _gameModel.Facing.Degrees();
    _gameModel.PlayerState = PlayerState.Turning;
    var tween = CreateTween().TweenProperty(_player, "rotation_degrees:y", start + amount, GlanceTime).From(start);
    tween.Finished += () =>
    {
      _gameModel.PlayerState = PlayerState.Glance;
      _glanceAmount += amount;
    };
  }

  public void UnGlance()
  {
    if (_gameModel.PlayerState != PlayerState.Glance)
    {
      return;
    }

    var rot = _gameModel.Facing.Degrees();
    _gameModel.PlayerState = PlayerState.Turning;
    var tween = CreateTween().TweenProperty(_player, "rotation_degrees:y", rot, GlanceTime).From(rot + _glanceAmount);
    tween.Finished += () =>
    {
      _gameModel.PlayerState = PlayerState.Idle;
      _glanceAmount = 0;
    };
  }

  public void OpenDoor()
  {
    var wall = _player.WallAhead;
    if (wall is Door door) door.Activate();
  }

  public void StartCombat(MazeCell cell, bool attack)
  {
    _combatController.Start(cell, attack);
  }

  public void Attack()
  {
    _combatController.PlayerAttack = true;
  }

  // asks to leave combat by stepping back a cell, there is no retreat with nowhere to step to
  public void Retreat()
  {
    if (OpenCellBehind() == null) return;

    _combatController.PlayerRetreat = true;
  }

  // combat has ended with the enemy still alive
  public void Retreated()
  {
    MoveBackward();
  }


  public void Rest()
  {
    if (_playerData.Food < 1 || !_playerData.NeedsRest) return;

    if (_playerData.Health == _playerData.HealthMax && _playerData.Mind == _playerData.MindMax)
    {
      return;
    }

    _playerData.NeedsRest = false;
    int hpGain = Mathf.FloorToInt(_playerData.WarExp / 4.0);
    int mindGain = Mathf.FloorToInt(_playerData.MagicExp / 5.0);
    _playerData.WarExp = _playerData.WarExp % 4;
    _playerData.MagicExp = _playerData.MagicExp % 5;
    _playerData.HealthMax += hpGain;
    _playerData.MindMax += mindGain;
    _playerData.Health = Mathf.Min(_playerData.Health + Mathf.FloorToInt(_playerData.HealthMax * 2.0 / 3.0),
      _playerData.HealthMax);
    _playerData.Mind = Mathf.Min(_playerData.Mind + Mathf.FloorToInt(_playerData.MindMax * 2.0 / 3.0),
      _playerData.MindMax);
    _playerData.Food -= 1;
  }


  public void UseExit()
  {
    if (_gameModel.PlayerState == PlayerState.Idle && _gameModel.OverExit)
    {
      _player.Audio.Stream = ResourceLoader.Load<AudioStream>("res://data/sounds/descend.wav");
      _player.Audio.Play();
      _gameModel.NextLevel();
      _gameModel.PlayerState = PlayerState.Idle;
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
    UseOrTakeItem();
  }

  private void OpenContainer(MazeCell cell, ItemInfo container)
  {
    if (container.NeedsKey)
    {
      var rh = _playerData.RightHand;
      if (rh is { ItemType: ItemType.Key } && rh.Stat1 >= container.Stat1)
      {
        _gameModel.SetItem(cell.Coord, _gameModel.ChooseTreasure(container));
      }
      else
      {
        SwapWithFeet();
        return;
      }
    }
    else
    {
      _gameModel.SetItem(cell.Coord, _gameModel.ChooseTreasure(container));
    }
  }

  public void ClickRightHand()
  {
    if (_gameModel.PlayerState == PlayerState.Combat)
    {
      Attack();
    }
    else if (_player.WallAhead is not (Wall or Door { Blocked: true }))
    {
      // no fighting through a wall or a closed door
      StartCombat(_gameModel.CellAhead, true);
    }
  }

  public void SwapHands()
  {
    (_playerData.LeftHand, _playerData.RightHand) = (_playerData.RightHand, _playerData.LeftHand);
  }

  // items at the feet can only be touched while standing still, not mid-move or in combat
  private bool CanUseFeet => _gameModel.PlayerState == PlayerState.Idle;

  public void SwapWithFeet()
  {
    if (!CanUseFeet) return;

    // ladders, treasure and supplies can't be carried in a hand
    if (_gameModel.ItemAtFeet is { ItemType: ItemType.Special })
    {
      return;
    }

    (_playerData.RightHand, _gameModel.ItemAtFeet) = (_gameModel.ItemAtFeet, _playerData.RightHand);
  }

  private void RemoveItem()
  {
    _gameModel.ItemAtFeet = null;
  }


  // user wants to use/take item at their feet
  public void UseOrTakeItem()
  {
    if (!CanUseFeet) return;

    var cell = _gameModel.CurrentCell;
    var itemInfo = cell.ItemInfo;
    if (itemInfo == null)
    {
      // nothing to take, so drop what is in the right hand
      SwapWithFeet();
      return;
    }

    if (itemInfo is { ItemType: ItemType.Special })
    {
      switch (itemInfo.Use)
      {
        case "exit":
          return;
        case "win":
          _mainGame.WonGame();
          return;
        case "arrows" or "food":
          _playerData.AddSpecial(itemInfo.Use, itemInfo.Stat1);
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
        return;
    }
  }


  // stepping onto a gate takes the player to another dungeon at the same depth
  private void EnterGate()
  {
    if (!_gameModel.EnterGate()) return;

    _player.Audio.Stream = ResourceLoader.Load<AudioStream>("res://data/sounds/magic.wav");
    _player.Audio.Play();
    _playerData.NeedsRest = true;
  }

  public void WonCombat(EnemyInfo enemy)
  {
    Killed(enemy);
    _playerData.NeedsRest = true;
    _gameModel.PlayerState = PlayerState.Idle;
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