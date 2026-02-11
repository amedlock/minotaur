#region

using System.Collections.Generic;
using Godot;
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

  public int Mind;
  public int MindMax;

  public int WarExp ;
  public int MagicExp ;
  
  public bool Resurrected ;

  public bool IsDead => Health <= 0 || Mind <= 0;
  
  private ItemInfo helmet;
  private ItemInfo breastplate;
  private ItemInfo hauberk;


  public ItemInfo LeftHand { get; set; }

  public ItemInfo RightHand { get; set; }

  public bool needsRest;
  private bool resurrected;

  private ItemInfo ring;

  public int WarArmor = 0;
  public int MindArmor = 0;
  public int WarDamage = 0;
  public int MindDamage = 0;

  private List<ItemInfo> slots = [null, null, null, null, null, null, null, null, null];


  public void ClearSlots()
  {
    foreach (var n in GD.Range(slots.Count))
    {
      slots[n] = null;
    }
  }
  
  public ItemInfo SetSlot(int slotNum, ItemInfo item)
  {
    return slots[slotNum-1] = item;
  }
  
  public ItemInfo GetSlot(int slotNum)
  {
    return slots[slotNum-1];
  }

  public void AddArmor(ItemInfo itemInfo)
  {
    switch (itemInfo)
    {
      case { ItemType: ItemType.MagicArmor }:
        MindArmor = Mathf.Min( MindArmor, itemInfo.Stat1);
        break;
      case { ItemType: ItemType.Armor }:
        WarArmor = Mathf.Min( WarArmor, itemInfo.Stat1);
        break;
    }
  }

  public void AddSpecial(string name, int amount)
  {
    switch (name)
    {
      case "quiver":
        Arrows += amount;
        return ;
      case "food":
        Food += amount;
        return ;
    }
  }
}
