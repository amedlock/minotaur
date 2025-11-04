using System;
using System.Collections.Generic;
using Godot;
using minotaur.Source.dungeon;
using minotaur.Source.enemies;
using minotaur.Source.hud;
using minotaur.Source.items;

namespace minotaur.Source.player;

public partial class Player : Node3D
{
  private bool _isDead = false;
  private Marker3D _startPosition;
  private PlayerState _playerState = PlayerState.Idle;

  // grid coords
  private Vector2I _coord = new(0, 0);
  
  // facing direction in degrees
  private int _facing = 90;

  public PlayerState PlayerState
  {
    get => _playerState;
    set => _playerState = value;
  }

  public bool IsDead => _isDead;


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

  private Node _combat; // $combat
  private AudioStreamPlayer _audio; //= $Audio
  private Dungeon _dungeon;

  public int WarArmor = 0;
  public int MindArmor = 0;

  public int WarDamage = 0;
  public int MindDamage = 0;
  public int WarExp = 0;
  public int MagicExp = 0;
  public int Health = 0;
  public int HealthMax = 0;
  public int Mind = 0;
  public int MindMax = 0;

  public bool Resurrected = false;
  public bool NeedsRest = false;

  public int Gold = 0;
  public int Food = 0;
  public int Arrows = 0;

  public int Skill = 0;

  //  these are protective Items from game_db.gd
  public ItemInfo Helmet = null;
  public ItemInfo Breastplate = null;
  public ItemInfo Amulet = null;
  public ItemInfo Shield = null;

  private Dictionary<int, ItemInfo> _slots = new();

  public ItemInfo RightHand { get; set; }
  public ItemInfo LeftHand { get; set; }

  public void SwapHands()
  {
    (LeftHand, RightHand) = (RightHand, LeftHand);
  }
  
  public ItemInfo GetSlot(int slot)
  {
    return _slots[slot % 9];
  }

  public void SetSlot(int slot, ItemInfo item)
  {
    _slots[slot % 9] = item;
  }
  
  public ItemInfo ItemAtFeet => _dungeon.GetCell(Coord).ItemInfo;

  public ItemInfo Potion = null; // active potion?
  private int _potionTurns = 0; // how many turns before it vanishes


  public override void _Ready()
  {
    _dungeon = GetParent() as Dungeon;
    _combat = GetNode<Node>("combat");
    Hud = GetNode<Hud>("Camera3D/HUD");
    _audio = GetNode<AudioStreamPlayer>("Audio");
    _startPosition = _dungeon.GetNode<Marker3D>("StartPos");
    Position = _startPosition.Position + (new Vector3(_coord.X, 0, _coord.Y) * 3f);
    RotationDegrees = new Vector3(0, Dir, 0);
  }

  public Vector3 CoordToWorld(Vector2I coord)
  {
    return _startPosition.Position + new Vector3(coord.X * 3f, 0, -coord.Y * 3);
  }

  public bool OverExit
  {
    get
    {
      if (_dungeon.CurrentLevel == null)
      {
        return false;
      }

      var cell = _dungeon.GetCell(Coord);
      return cell.Item is { Info: { Name: "ladder" } };
    }
  }

  //  get the world coords for the player
  public  Vector2I Coord
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

  public Hud Hud { get; private set; }


  public void Init(int skill)
  {
    Skill = skill;
    Gold = 0;
    WarExp = 0;
    MagicExp = 0;
    Food = 10 - skill;
    Arrows = 9 - skill;
    Resurrected = false;
    foreach (var n in GD.Range(10))
    {
      _slots[n] = null;
    }

    RightHand = _dungeon.GameDb.FindItem("bow");
    switch (skill)
    {
      case 1:
        Health = 18;
        Mind = 9;
        LeftHand = _dungeon.GameDb.FindItem("small_shield");
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
  }



  public void AttackAhead()
  {
  }

  public void SwapItems()
  {
  }

  public void UseOrTakeItem()
  {
    var item = ItemAtFeet;
    if (item == null)
    {
      return;
    }
  }

  public void OpenDoor()
  {
    var wall = WallAhead;
    if (wall is Door door)
    {
      door.Activate();
    }
  }

  public void Rest()
  {
    throw new NotImplementedException();
  }

  public void UseExit()
  {
    if (_playerState == PlayerState.Idle && OverExit)
    {
      _dungeon.UseExit();
      GetNode<PlayerInput>("PlayerControl").Reset();
    }
  }

  public void EnterGate(DungeonGate dungeonGate)
  {
    _audio.Stream = ResourceLoader.Load<AudioStream>("res://data/sounds/magic.wav");
    _dungeon.LoadGateLevel(dungeonGate);
    GetNode<PlayerInput>("PlayerControl").Reset();
    this.NeedsRest = true;
  }

  public void WonCombat(Enemy enemy)
  {
    Killed(enemy);
    NeedsRest = true;
    PlayerState = PlayerState.Idle;
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

    if (enemy.Info.Name == "minotaur")
    {
      _dungeon.AddFinal(enemy);
    }

    if (IsDead)
    {
      _dungeon.Game.GameOver();
    }
  }

  // vary amount by +/- percent
  private int VaryAmount(int amount, int percent)
  {
    float variance = amount * (100f / percent);
    return (int)((amount - variance) + GD.RandRange(0, 2 * variance));
  }

  private float Percentage(int amount, int percent)
  {
    return (amount * (100f - percent)) / 100f;
  }
  
  private int ApplyArmor(int damage, int armor)
  {
    var prot = (int)Percentage(damage, armor);
    if (prot == 0)
    {
      return damage;
    }
    prot = VaryAmount(prot, 15);
    return Mathf.Max(damage - prot, 1);
  }
  

  public void Damage(Enemy enemy, ItemInfo item)
  {
    var maxDamage = item.Stat1 * Skill;
    var damage = Skill switch
    {
      1 => VaryAmount(maxDamage, 5),
      2 => VaryAmount(maxDamage, 10),
      3 => VaryAmount(maxDamage, 15),
      4 => VaryAmount(maxDamage, 20)
    };
    var warAmount = ApplyArmor(damage, WarArmor);
    var mindAmount = ApplyArmor(damage, MindArmor);
    if (item.IsWar)
    {
      Health = Mathf.Clamp(Health - warAmount, 0, HealthMax);
    }

    if (item.IsMagic)
    {
      Mind = Mathf.Clamp(Mind - mindAmount, 0, MindMax);
    }
  }

 
}