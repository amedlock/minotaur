#region

using System;
using System.Collections.Generic;
using Godot;

#endregion

namespace minotaur.Source.model;

public partial class GameModel : Node
{
  public GameDb GameDb { get; private set; }

  internal LevelRandomizer Randomizer;

  // current dungeon level, the first level is depth 1
  public int Depth = 1;
  public int Skill = 1;

  // player location
  public int PlayerX;
  public int PlayerY;

  public Direction Facing { get; set; } = Direction.East;

  public LevelInfo CurrentLevel => LevelAt(Depth);

  public MazeCell CurrentCell => Grid.Cell(PlayerX, PlayerY);

  public MazeCell CellAhead => Grid.Cell(PlayerCoord, Facing);

  public MazeCell CellBehind => Grid.Cell(PlayerCoord, Facing.Opposite());

  public List<LevelInfo> Levels = new();

  public DungeonGrid Grid { get; private set; }

  public PlayerData PlayerData { get; private set; }

  // which screen is up
  public GameMode GameMode { get; set; } = GameMode.Menu;

  // what the player is doing while in the dungeon
  public PlayerState PlayerState { get; set; }

  public bool OverExit => ItemAtFeet is { Name: "ladder" };

  public ItemInfo ItemAtFeet
  {
    get => CurrentCell.ItemInfo;
    set => CurrentCell.ItemInfo = value;
  }

  public Vector2I PlayerCoord
  {
    get => new(PlayerX, PlayerY);
    set
    {
      PlayerX = value.X;
      PlayerY = value.Y;
    }
  }

  // Levels is indexed from 0, depth starts at 1
  private LevelInfo LevelAt(int depth)
  {
    return depth >= 1 && depth <= Levels.Count ? Levels[depth - 1] : null;
  }

  public override void _Ready()
  {
    PlayerData = new PlayerData();
    GameDb = new GameDb();
    Grid = new DungeonGrid(GridGeometry.GridWidth, GridGeometry.GridHeight);
    Randomizer = new LevelRandomizer(Grid, GameDb);
  }


  // generate levelInfo for the dungeon using the skill and seed
  public void Init(int skill, uint seed)
  {
    var rng = new RandomNumberGenerator();
    rng.Seed = seed;

    Levels.Clear();
    foreach (int index in GD.Range(100))
    {
      var levelType = rng.RandWeighted([40f, 40f, 20f]) switch
      {
        0 => LevelType.War,
        1 => LevelType.Magic,
        _ => LevelType.Both
      };
      uint seedNum = rng.Randi();
      var levelInfo = new LevelInfo(skill, index + 1, seedNum, levelType);
      Levels.Add(levelInfo);
    }

    Skill = skill;
    Facing = Direction.East;
    PlayerData.Gold = 0;
    PlayerData.WarExp = 0;
    PlayerData.MagicExp = 0;
    PlayerData.Food = 10 - skill;
    PlayerData.Arrows = 9 - skill;
    PlayerData.Resurrected = false;
    PlayerData.ClearSlots();

    PlayerData.RightHand = GameDb.FindItem("bow");
    switch (skill)
    {
      case 1:
        PlayerData.Health = 18;
        PlayerData.Mind = 9;
        PlayerData.LeftHand = GameDb.FindItem("small_shield");
        break;
      case 2:
        PlayerData.Health = 16;
        PlayerData.Mind = 7;
        break;
      case 3:
        PlayerData.Health = 14;
        PlayerData.Mind = 7;
        break;
      case 4:
        PlayerData.Health = 12;
        PlayerData.Mind = 6;
        break;
      default:
        throw new Exception("Invalid skill: " + skill);
    }

    PlayerData.MindMax = PlayerData.Mind;
    PlayerData.HealthMax = PlayerData.Health;
  }

  public void CreateLevel(int depth)
  {
    if (depth < 1 || depth > Levels.Count)
    {
      throw new ArgumentException("Invalid depth");
    }

    Depth = depth;
    var levelInfo = LevelAt(depth);
    Randomizer.BuildLevel(levelInfo);
  }


  public void LoadGateLevel(LevelType levelType)
  {
    CurrentLevel.UsedGate = true;
    uint seed = CurrentLevel.SeedNumber >> 16 | (CurrentLevel.SeedNumber & 0xffff) << 16;
    var levelInfo = new LevelInfo(Skill, Depth, seed, levelType);
    Levels[Depth - 1] = levelInfo;
    Randomizer.BuildLevel(levelInfo);
  }

  // vary amount by +/- percent
  private int VaryAmount(int amount, int percent)
  {
    float variance = amount * (percent / 100f);
    return (int)(amount - variance + GD.RandRange(0, 2 * variance));
  }

  // TEMP: scales enemy damage down for playtesting until the stats rework,
  // the mirror of the x4 on player damage in EnemyState.Damage
  private const int EnemyDamageDivisor = 4;

  // armor blocks that percent of the damage
  private int ApplyArmor(int damage, int armor)
  {
    var prot = (int)(damage * armor / 100f);
    if (prot == 0) return damage;
    prot = VaryAmount(prot, 15);
    return Mathf.Max(damage - prot, 1);
  }


  public void DamagePlayer(ItemInfo item)
  {
    int maxDamage = Mathf.Max(item.Stat1 / EnemyDamageDivisor, 1);
    int damage = Skill switch
    {
      1 => VaryAmount(maxDamage, 5),
      2 => VaryAmount(maxDamage, 10),
      3 => VaryAmount(maxDamage, 15),
      _ => VaryAmount(maxDamage, 20)
    };
    int warAmount = ApplyArmor(damage, PlayerData.WarArmor);
    int mindAmount = ApplyArmor(damage, PlayerData.MindArmor);
    if (item.IsWar) PlayerData.Health = Mathf.Clamp(PlayerData.Health - warAmount, 0, PlayerData.HealthMax);

    if (item.IsMagic) PlayerData.Mind = Mathf.Clamp(PlayerData.Mind - mindAmount, 0, PlayerData.MindMax);
  }

  public void NextLevel()
  {
    if (Depth >= Levels.Count)
    {
      return;
    }

    Depth++;
    Randomizer.BuildLevel(LevelAt(Depth));
    PlayerX = 0;
    PlayerY = 0;
    Facing = Direction.East;
  }

  public ItemInfo ChooseTreasure(ItemInfo container)
  {
    return GameDb.FindItem("coins");
  }

  public ItemInfo FindWeapon(EnemyInfo enemyInfo)
  {
    return GameDb.FindWeapon(enemyInfo.Weapon, enemyInfo.Power, Depth);
  }
}