#region

using System;
using System.Collections.Generic;
using Godot;

#endregion

namespace minotaur.Source.model;

// named PlayerData to avoid confusion with Player Node
public class PlayerData
{
  // a number shown on the HUD has changed
  public event Action StatsChanged;

  // a hand or pack slot has changed
  public event Action InventoryChanged;

  private int _arrows;
  private int _food;
  private int _gold;
  private int _health;
  private int _healthMax;
  private int _mind;
  private int _mindMax;
  private int _warArmor;
  private int _mindArmor;

  private ItemInfo _leftHand;
  private ItemInfo _rightHand;

  public int Arrows
  {
    get => _arrows;
    set => SetStat(ref _arrows, value);
  }

  public int Food
  {
    get => _food;
    set => SetStat(ref _food, value);
  }

  public int Gold
  {
    get => _gold;
    set => SetStat(ref _gold, value);
  }

  public int Health
  {
    get => _health;
    set => SetStat(ref _health, value);
  }

  public int HealthMax
  {
    get => _healthMax;
    set => SetStat(ref _healthMax, value);
  }

  public int Mind
  {
    get => _mind;
    set => SetStat(ref _mind, value);
  }

  public int MindMax
  {
    get => _mindMax;
    set => SetStat(ref _mindMax, value);
  }

  public int WarArmor
  {
    get => _warArmor;
    set => SetStat(ref _warArmor, value);
  }

  public int MindArmor
  {
    get => _mindArmor;
    set => SetStat(ref _mindArmor, value);
  }

  public int WarExp;
  public int MagicExp;

  public bool Resurrected;

  public bool IsDead => Health <= 0 || Mind <= 0;

  public ItemInfo LeftHand
  {
    get => _leftHand;
    set => SetItem(ref _leftHand, value);
  }

  public ItemInfo RightHand
  {
    get => _rightHand;
    set => SetItem(ref _rightHand, value);
  }

  public bool NeedsRest;

  // damage of the weapon in the right hand
  public int WarDamage => RightHand is { IsWar: true } ? RightHand.Stat1 : 0;
  public int MindDamage => RightHand is { IsMagic: true } ? RightHand.Stat1 : 0;

  private List<ItemInfo> _slots = [null, null, null, null, null, null, null, null, null];

  private void SetStat(ref int field, int value)
  {
    if (field == value) return;
    field = value;
    StatsChanged?.Invoke();
  }

  private void SetItem(ref ItemInfo field, ItemInfo value)
  {
    if (field == value) return;
    field = value;
    InventoryChanged?.Invoke();
  }

  public void ClearSlots()
  {
    foreach (var n in GD.Range(_slots.Count))
    {
      _slots[n] = null;
    }

    InventoryChanged?.Invoke();
  }

  public ItemInfo SetSlot(int slotNum, ItemInfo item)
  {
    if (_slots[slotNum - 1] == item) return item;
    _slots[slotNum - 1] = item;
    InventoryChanged?.Invoke();
    return item;
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

  // a war gate halves mind and adds twice the loss to health, a magic gate halves health and adds
  // half the loss to mind. Only the maximum is raised, resting fills it
  public void ExchangeHp(GateType gate)
  {
    switch (gate)
    {
      case GateType.War:
      {
        int loss = MindMax / 2;
        MindMax -= loss;
        Mind = Mathf.Min(Mind, MindMax);
        HealthMax += loss * 2;
        return;
      }
      case GateType.Magic:
      {
        int loss = HealthMax / 2;
        HealthMax -= loss;
        Health = Mathf.Min(Health, HealthMax);
        MindMax += loss / 2;
        return;
      }
    }
  }

  // use is what the special item gives, "arrows" or "food"
  public void AddSpecial(string use, int amount)
  {
    switch (use)
    {
      case "arrows":
        Arrows += amount;
        return;
      case "food":
        Food += amount;
        return;
    }
  }
}
