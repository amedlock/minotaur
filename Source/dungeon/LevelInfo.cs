using minotaur.items;

namespace minotaur.dungeon;

public class LevelInfo(int depth, uint seedNumber)
{
  public enum GateType
  {
    Empty, Tan, Green, Blue
  }
  
  
  public bool MagicMonsters = false;
  public bool WarMonsters = false;
  public bool ToughMonsters = false;
  
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