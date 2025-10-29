using System;
using Godot;
using minotaur.dungeon;
using minotaur.hud;
using minotaur.items;

namespace minotaur.player;

public partial class Player : Node3D
{
  private bool _isDead = false;
  private Marker3D _startPosition;
  private PlayerState _playerState = PlayerState.Idle;

  public PlayerState PlayerState
  {
    get => _playerState;
    set => _playerState = value;
  }

  public bool IsDead => _isDead;


  // player direction in degrees
  public int Dir => (int)(Mathf.PosMod(RotationDegrees.Y, 360f));


  private Node _combat; // $combat
  private Hud _hud; //= $Camera3D/HUD
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

  public int Gold = 0;
  public int Food = 0;
  public int Arrows = 0;

  public int Skill = 0;

  //  these are protective Items from game_db.gd
  public ItemInfo Helmet = null;
  public ItemInfo Breastplate = null;
  public ItemInfo Amulet = null;
  public ItemInfo Shield = null;

  public ItemInfo Potion = null; // active potion?
  private int _potionTurns = 0; // how many turns before it vanishes


  public override void _Ready()
  {
    _dungeon = GetParent() as Dungeon;
    _combat = GetNode<Node>("combat");
    _hud = GetNode<Hud>("Camera3D/HUD");
    _audio = GetNode<AudioStreamPlayer>("Audio");
    _startPosition = _dungeon.GetNode<Marker3D>("StartPos");
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
      return cell.ItemInfo is { Name: "ladder" };
    }
  }

  //  get the world coords for the player
  public Vector2I Coord
  {
    get
    {
      var loc = (Position - _dungeon.StartPosition).Abs();
      return new Vector2I(Mathf.RoundToInt(loc.X / 3.0f), Mathf.RoundToInt(loc.Z / 3.0f));
    }

    set => Position = _dungeon.StartPosition + new Vector3(value.X * 3f, 0, -value.Y * 3f);
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

  public string DirName
  {
    get
    {
      return Direction switch
      {
        Direction.North => "north",
        Direction.East => "east",
        Direction.West => "west",
        Direction.South => "south",
        _ => ""
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

  public Item ItemAtFeet
  {
    get => _dungeon.GetCell(Coord).Item;
    set { }
  }

  public Wall WallBehind => null;
  public Node3D WallAhead => _dungeon.Grid.GetWall(Coord, Direction);
  public DungeonCell CellAhead => null;

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
      // Inventory[n] = null;
    }

    // right = game_db.find_item("bow");
    switch (skill)
    {
      case 1:
        Health = 18;
        Mind = 9;
        // self.Shield = game_db.find_item("small_shield");
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
    _hud.UpdateAll();
  }


  public void Enable()
  {
    Visible = true;
    _hud.UpdateAll();
  }

  public void Disable()
  {
    Visible = false;
  }

  public void ResetLocation()
  {
    Position = _startPosition.Position;
    RotationDegrees = new Vector3(0, 270, 0);
  }


  public void StartCombat(DungeonCell cell, bool attacking)
  {
  }


  public void SwapHands()
  {
    // var left = player.LeftHand;
    // player.LeftHand = player.RightHand;
    // player.RightHand = left;
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
}