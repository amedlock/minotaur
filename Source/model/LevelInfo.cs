#region

using minotaur.Source.dungeon;
using minotaur.Source.items;

#endregion

namespace minotaur.Source.model;

public class LevelInfo
{
  public LevelType LevelType = LevelType.None;
  public GateType GateType = GateType.None;
  public bool HasMinotaur;
  public bool UsedGate = false;

  public int Depth { get; }
  public uint SeedNumber { get; }

  public LevelInfo(int skill, int depth, uint seedNumber, LevelType levelType)
  {
    Depth = depth;
    SeedNumber = seedNumber;
    LevelType = levelType;

    HasMinotaur = skill switch
    {
      1 => depth >= 3,
      2 => depth >= 6,
      3 => depth >= 10,
      _ => depth >= 15
    };
  }


  public bool HasItem(ItemInfo itemInfo)
  {
    return itemInfo.NeedsKey;
  }
}