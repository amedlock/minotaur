#region

using System;
using System.Collections.Generic;
using Godot;
using minotaur.Source.dungeon;
using minotaur.Source.enemies;
using minotaur.Source.items;
using minotaur.Source.player;

#endregion

namespace minotaur.Source.model;

public partial class GameModel : Node
{
  private DungeonGrid _grid;
  private PlayerData _playerData;
  public GameDb GameDb { get; private set; }

  public LevelRandomizer randomizer;

  public int Depth = 1;
  public int Skill = 1;

  // player location
  public int playerX = 0;
  public int playerY = 0;
  private int _facing = 90;

  public int Facing
  {
    get => _facing;
    set => _facing = value;
  }
  
  public int GridIndex => Grid.Index(playerX, playerY);
  
  public LevelInfo CurrentLevel => Levels[Depth];

  public MazeCell CurrentCell => _grid.Cell(playerX, playerY);

  public List<LevelInfo> Levels = new();

  public DungeonGrid Grid => _grid;
  public PlayerData PlayerData => _playerData;
  public PlayerState PlayerState { get; set; }

  public bool OverExit => ItemAtFeet is { Name: "ladder" };
  
  public ItemInfo ItemAtFeet
  {
    get => CurrentCell.ItemInfo;
    set => CurrentCell.ItemInfo = value;
  }

  public Vector2I PlayerCoord
  {
    get => new Vector2I(playerX, playerY);
    set
    {
      playerX = value.X;
      playerY = value.Y;
    }
  }

  public override void _Ready()
  {
    _playerData = new PlayerData();
    GameDb = new GameDb();
    _grid = new DungeonGrid(12, 12);
    randomizer = new LevelRandomizer(_grid, GameDb);
  }

  

  // generate levelInfo for the dungeon using the skill and seed
  public void Init(int skill, uint seed)
  {
    var rng = new RandomNumberGenerator();
    rng.Seed = seed;

    Levels.Clear();
    foreach (var index in GD.Range(100))
    {
      var levelType = rng.RandWeighted([40f, 40f, 20f]) switch
      {
        0 => LevelType.War,
        1 => LevelType.Magic,
        _ => LevelType.Both
      };
      var seedNum = rng.Randi();
      var levelInfo = new LevelInfo(skill, index + 1, seedNum, levelType);
      Levels.Add(levelInfo);
    }
  }
  
  public void CreateLevel(int depth)
  {
    int index = depth - 1;
    if (index >= Levels.Count)
    {
      throw new ArgumentException("Invalid depth");
    }
    
    var levelInfo = Levels[index];
    randomizer.BuildLevel(levelInfo);
  }

  
  public void LoadGateLevel(DungeonGate gate)
  {
    CurrentLevel.UsedGate = true;
    // CurrentLevel.SeedNumber = this.rng.Ranrandi()
    Grid.Reset();
  }
  
  
  // vary amount by +/- percent
  private int VaryAmount(int amount, int percent)
  {
    var variance = amount * (percent / 100f);
    return (int)(amount - variance + GD.RandRange(0, 2 * variance));
  }

  private float Percentage(int amount, int percent)
  {
    return amount * (100f - percent) / 100f;
  }

  private int ApplyArmor(int damage, int armor)
  {
    var prot = (int)Percentage(damage, armor);
    if (prot == 0) return damage;
    prot = VaryAmount(prot, 15);
    return Mathf.Max(damage - prot, 1);
  }


  public void DamagePlayer(Enemy enemy, ItemInfo item)
  {
    var maxDamage = item.Stat1;
    var damage = Skill switch
    {
      1 => VaryAmount(maxDamage, 5),
      2 => VaryAmount(maxDamage, 10),
      3 => VaryAmount(maxDamage, 15),
      _ => VaryAmount(maxDamage, 20)
    };
    var warAmount = ApplyArmor(damage, _playerData.WarArmor);
    var mindAmount = ApplyArmor(damage, _playerData.MindArmor);
    if (item.IsWar) _playerData.Health = Mathf.Clamp(_playerData.Health - warAmount, 0, _playerData.HealthMax);

    if (item.IsMagic) _playerData.Mind = Mathf.Clamp(_playerData.Mind - mindAmount, 0, _playerData.MindMax);
  }

  public void NextLevel()
  {
    if (Depth >= 99)
    {
      return;
    }
    Depth++;
    randomizer.BuildLevel(Levels[Depth]);
    playerX = 0;
    playerY = 0;
    _facing = 90;
  }
}