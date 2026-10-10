#region

using Godot;

#endregion

namespace minotaur.Source.model;

public class ItemInfo
{
  public Color Color;
  public Rect2I Image;
  public ItemType ItemType;
  public int MinDepth = 1;
  public string Name;
  public bool NeedsKey = false;
  public int Stat1;
  public int Stat2;

  public bool IsWeapon => ItemType is ItemType.WarWeapon or ItemType.MagicWeapon;

  // what a special item does when used: "arrows", "food", "exit" or "win"
  public string Use;

  // weapon behaviour, read from game_info.json

  // what firing uses up, "arrows" or null
  public string Ammo;

  // icon shown in flight
  public Rect2I Projectile;

  // uses the spinning animation in flight
  public bool Spin;

  // name of the sound played when fired, or null
  public string Sound;

  // leaves the hand when fired
  public bool SingleUse;

  // breaks 1 time in this many shots, 0 never breaks
  public int BreakChance;

  public bool IsWar => ItemType == ItemType.WarWeapon;
  public bool IsMagic => ItemType == ItemType.MagicWeapon;

  public bool IsAllowed(LevelType levelType)
  {
    if (IsWeapon)
      return levelType switch
      {
        LevelType.Magic => IsMagic,
        LevelType.War => IsWar,
        LevelType.Both => true,
        _ => false
      };
    return true;
  }
}