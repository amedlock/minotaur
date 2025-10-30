using minotaur.Source.items;

namespace minotaur.Source.dungeon;

public class LevelInfo(int depth, uint seedNumber)
{
  public enum GateType
  {
    Empty, Tan, Green, Blue
  }
  
  public bool UsedGate = false;
  public LevelType LevelType = LevelType.War;
  public bool HasMinotaur = false;
  public GateType _gateType = GateType.Empty;
  
  public int Depth => depth;
  public uint SeedNumber => seedNumber;
  
  public bool HasItem(ItemInfo itemInfo)
  {
    return itemInfo.NeedsKey;
  }
}