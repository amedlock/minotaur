#region

using System.Collections.Generic;
using Godot;

#endregion

namespace minotaur.Source.model;

// named PlayerData to avoid confusion with Player Node
public class PlayerData
{
  public int Arrows;
  public int Food;

  public int Gold;
  public int Health;
  public int HealthMax;

  public int Mind;
  public int MindMax;

  public int WarExp;
  public int MagicExp;

  public bool Resurrected;

  public bool IsDead => Health <= 0 || Mind <= 0;

  public ItemInfo LeftHand { get; set; }

  public ItemInfo RightHand { get; set; }

  public bool NeedsRest;

  public int WarArmor;
  public int MindArmor;
  public int WarDamage = 0;
  public int MindDamage = 0;

  private List<ItemInfo> _slots = [null, null, null, null, null, null, null, null, null];

  public void ClearSlots()
  {
    foreach (var n in GD.Range(_slots.Count))
    {
      _slots[n] = null;
    }
  }

  public ItemInfo SetSlot(int slotNum, ItemInfo item)
  {
    return _slots[slotNum - 1] = item;
  }

  public ItemInfo GetSlot(int slotNum)
  {
    return _slots[slotNum - 1];
  }

  public void AddArmor(ItemInfo itemInfo)
  {
    switch (itemInfo)
    {
      case { ItemType: ItemType.MagicArmor }:
        MindArmor = Mathf.Min(MindArmor, itemInfo.Stat1);
        break;
      case { ItemType: ItemType.Armor }:
        WarArmor = Mathf.Min(WarArmor, itemInfo.Stat1);
        break;
    }
  }

  public void AddSpecial(string name, int amount)
  {
    switch (name)
    {
      case "quiver":
        Arrows += amount;
        return;
      case "food":
        Food += amount;
        return;
    }
  }
}