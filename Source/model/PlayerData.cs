#region

using System.Collections.Generic;
using minotaur.Source.items;

#endregion

namespace minotaur.Source.player;

// named PlayerData to avoid confusion with Player Node
public class PlayerData
{
  public int Arrows;
  public int Food;

  public  int Gold;
  public  int Health;
  public  int HealthMax;
  
  public  int Mind;
  public int MindMax;

  private ItemInfo helmet;
  private ItemInfo breastplate;
  private ItemInfo hauberk;


  public ItemInfo LeftHand { get; set; }

  public ItemInfo RightHand { get; set; }

  private bool needsRest;
  private bool resurrected;

  private ItemInfo ring;

  public int WarArmor => 0;
  public int MindArmor => 0;
  public int WarDamage => 0;
  public int MindDamage => 0;

  private List<ItemInfo> slots = [null, null, null, null, null, null, null, null, null];

  public ItemInfo GetSlot(int slotNum)
  {
    return slots[slotNum-1];
  }
}
