#region

using System.Collections.Generic;
using Godot;
using minotaur.Source.dungeon;
using minotaur.Source.player;

#endregion

namespace minotaur.Source.model;

public partial class GameModel : Node
{
  private DungeonGrid _grid;
  private PlayerData _playerData;
  public GameDb GameDb { get; private set; }

  public int Depth { get; }

  public LevelInfo CurrentLevel => Levels[Depth];

  public List<LevelInfo> Levels = new();

  public DungeonGrid Grid => _grid;
  public PlayerData PlayerData => _playerData;
  
  public override void _Ready()
  {
    _playerData = new PlayerData();
    GameDb = new GameDb();
    _grid = new DungeonGrid(12, 12);
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
}