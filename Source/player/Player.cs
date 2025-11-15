#region

using System;
using System.Collections.Generic;
using Godot;
using minotaur.Source.dungeon;
using minotaur.Source.enemies;
using minotaur.Source.hud;
using minotaur.Source.items;
using minotaur.Source.model;

#endregion

namespace minotaur.Source.player;

public partial class Player : Node3D
{
  private readonly Dictionary<int, ItemInfo> _slots = new();
  private AudioStreamPlayer _audio;

  private Combat _combat;

  // grid coords
  private Vector2I _coord = new(0, 0);
  

  // facing direction in degrees
  private int _facing = 90;
  private PlayerState _playerState = PlayerState.Idle;
  private int _potionTurns; // how many turns before it vanishes
  
  public ItemInfo Amulet = null;
  public int Arrows;
  public ItemInfo Breastplate = null;
  public int Food;

  [Export]
  private Marker3D _startPosition;
  
  [Export]
  private Dungeon _dungeon;
  
  [Export] 
  private GameModel gameModel;

  public int Gold;
  public int Health;
  public int HealthMax;

  //  these are protective Items from game_db.gd
  public ItemInfo Helmet = null;
  public int MagicExp;
  public int Mind;
  public int MindArmor;
  public int MindDamage = 0;
  public int MindMax;
  public bool NeedsRest;

  public ItemInfo Potion = null; // active potion?

  public bool Resurrected;
  public ItemInfo Shield = null;

  public int Skill;

  public int WarArmor;

  public int WarDamage = 0;
  public int WarExp;

  public bool IsDead => Health <= 0 || Mind <= 0;


  // player direction in degrees
  // if glancing may not be in sync with Node RotationDegrees.Y
  public int Dir
  {
    get => _facing;
    set
    {
      _facing = Mathf.PosMod(value, 360);
      RotationDegrees = new Vector3(0, _facing, 0);
    }
  }

  public ItemInfo RightHand { get; set; }
  public ItemInfo LeftHand { get; set; }

  public ItemInfo ItemAtFeet => _dungeon.GetCell(Coord).ItemInfo;

  public bool OverExit
  {
    get
    {
      if (_dungeon.CurrentLevel == null) return false;

      var cell = _dungeon.GetCell(Coord);
      return cell.Item is { Info: { Name: "ladder" } };
    }
  }

  //  get the world coords for the player
  public Vector2I Coord
  {
    get => _coord;
    set
    {
      Position = _dungeon.StartPosition + new Vector3(value.X * 3f, 0, -value.Y * 3f);
      _coord = value;
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
  public Node3D WallAhead => _dungeon.GetWall(Coord, Direction);

  public DungeonCell CurrentCell => _dungeon.GetCell(Coord);
  public DungeonCell CellAhead => _dungeon.GetCell(_coord, Direction);
  public DungeonCell CellBehind => _dungeon.GetCell(_coord, RearDirection);

  public Hud Hud { get; private set; }

  public void SwapHands()
  {
    (LeftHand, RightHand) = (RightHand, LeftHand);
  }

  public ItemInfo GetSlot(int slot)
  {
    return _slots[slot % 9];
  }

  public ItemInfo SetSlot(int slotNum, ItemInfo item)
  {
    slotNum = slotNum % 9;
    var prev = _slots[slotNum];
    _slots[slotNum % 9] = item;
    return prev;
  }


  public override void _Ready()
  {
    // _dungeon = GetParent<Dungeon>();
    _combat = GetNode<Combat>("combat");
    Hud = GetNode<Hud>("Camera3D/HUD");
    _audio = GetNode<AudioStreamPlayer>("Audio");
    // _startPosition = _dungeon.GetNode<Marker3D>("StartPos");
    Position = _startPosition.Position + new Vector3(_coord.X, 0, _coord.Y) * 3f;
    RotationDegrees = new Vector3(0, Dir, 0);
  }

  public Vector3 CoordToWorld(Vector2I coord)
  {
    return _startPosition.Position + new Vector3(coord.X * 3f, 0, -coord.Y * 3);
  }


  public void Init(int skill)
  {
    Skill = skill;
    Gold = 0;
    WarExp = 0;
    MagicExp = 0;
    Food = 10 - skill;
    Arrows = 9 - skill;
    Resurrected = false;
    foreach (var n in GD.Range(10)) _slots[n] = null;

    RightHand = gameModel.GameDb.FindItem("bow");
    switch (skill)
    {
      case 1:
        Health = 18;
        Mind = 9;
        LeftHand = gameModel.GameDb.FindItem("small_shield");
        break;
      case 2:
        Health = 16;
        Mind = 7;
        break;
      case 3:
        Health = 14;
        Mind = 7;
        break;
      case 4:
        Health = 12;
        Mind = 6;
        break;
      default:
        throw new Exception("Invalid skill: " + skill);
    }

    MindMax = Mind;
    HealthMax = Health;
    Hud.UpdateAll();
  }


  public void Enable()
  {
    Visible = true;
    Hud.UpdateAll();
  }

  public void Disable()
  {
    Visible = false;
  }

  public void ResetLocation()
  {
    Position = _startPosition.Position;
    Dir = 270;
  }


  public void StartCombat(DungeonCell cell, bool attacking)
  {
    _combat.Start(cell, attacking);
  }

  public bool AttackAhead()
  {
    if (_playerState == PlayerState.Combat)
    {
      _combat.PlayerAttack = true;
    }
    else
    {
      if (WallAhead is Wall or Door { Blocked: true }) return false;
      var ahead = CellAhead;
      if (ahead is { Enemy: not null })
      {
        StartCombat(ahead, true);
        return true;
      }
    }

    return false;
  }

  public void SwapItems()
  {
    (LeftHand, RightHand) = (RightHand, LeftHand);
    Hud.UpdateAll();
  }

  // user wants to use/take item at their feet
  public void UseOrTakeItem()
  {
    var cell = CurrentCell;
    var itemInfo = cell.Item?.Info;
    if (itemInfo == null) return;

    switch (itemInfo.ItemType)
    {
      case ItemType.Money:
      {
        Gold += itemInfo.Stat2;
        cell.RemoveItem();
        return;
      }

      case ItemType.Armor:
      {
        WarArmor = Mathf.Max(WarArmor, itemInfo.Stat1);
        cell.RemoveItem();
        return;
      }
      case ItemType.MagicArmor:
      {
        MindArmor = Mathf.Max(MindArmor, itemInfo.Stat1);
        cell.RemoveItem();
        return;
      }
      case ItemType.Ladder:
      {
        _dungeon.NextLevel();
        return;
      }
      case ItemType.Special:
      {
        _dungeon.WonGame();
        return;
      }

      case ItemType.Container:
      {
        var inHand = LeftHand;
        if (inHand is { ItemType: ItemType.Key } && inHand.Stat1 >= itemInfo.Stat2) OpenContainer(cell);
        return;
      }

      default:
        SwapItems();
        break;
    }
  }

  // left clicked right inventory slot
  public void AttackOrUseItem()
  {
    if (_playerState == PlayerState.Combat)
    {
      _combat.PlayerAttack = true;
      return;
    }

    if (AttackAhead()) return;

    if (RightHand is { ItemType: ItemType.Key })
      if (ItemAtFeet is { ItemType: ItemType.Container })
        OpenContainer(CurrentCell);
  }


  private void OpenContainer(DungeonCell cell)
  {
    cell.RemoveItem();
    // TODO: use some random loot here
    cell.SetItem(gameModel.GameDb.FindItem("coins"));
  }


  public void OpenDoor()
  {
    var wall = WallAhead;
    if (wall is Door door) door.Activate();
  }

  public void Rest()
  {
    if (Food < 1 || !NeedsRest) return;

    if (Health == HealthMax && Mind == MindMax) return;

    NeedsRest = false;
    var hpGain = Mathf.FloorToInt(WarExp / 4.0);
    var mindGain = Mathf.FloorToInt(MagicExp / 5.0);
    WarExp = WarExp % 4;
    MagicExp = MagicExp % 5;
    HealthMax += hpGain;
    MindMax += mindGain;
    Health = Mathf.Min(Health + Mathf.FloorToInt(HealthMax * 2.0 / 3.0), HealthMax);
    Mind = Mathf.Min(Mind + Mathf.FloorToInt(MindMax * 2.0 / 3.0), MindMax);
    Food -= 1;
    Hud.UpdateAll();
  }

  public void UseExit()
  {
    if (_playerState == PlayerState.Idle && OverExit)
    {
      _dungeon.NextLevel();
      ResetLocation();
      _playerState = PlayerState.Idle;
    }
  }

  public void EnterGate(DungeonGate dungeonGate)
  {
    _audio.Stream = ResourceLoader.Load<AudioStream>("res://data/sounds/magic.wav");
    _dungeon.LoadGateLevel(dungeonGate);
    NeedsRest = true;
  }

  public void WonCombat(Enemy enemy)
  {
    Killed(enemy);
    NeedsRest = true;
    // PlayerState = PlayerState.Idle;
    Hud.UpdateAll();
  }

  private void Killed(Enemy enemy)
  {
    switch (enemy.Info.Type)
    {
      case EnemyType.War:
        WarExp += enemy.Info.Power;
        break;
      case EnemyType.Magic:
        MagicExp += enemy.Info.Power;
        break;
      case EnemyType.Both:
        WarExp += enemy.Info.Power;
        MagicExp += enemy.Info.Power;
        break;
    }

    if (enemy.Info.Name == "minotaur") _dungeon.AddFinal(enemy);

    if (IsDead) _dungeon.Game.GameOver();
  }

}