#region

using Godot;
using minotaur.Source.dungeon;

#endregion

namespace minotaur.Source.items;

public class ItemInfo
{
  public Color color = Colors.White;
  public Color Color;
  public Rect2I Image;
  public ItemType ItemType;
  public int MinDepth = 1;
  public string Name;
  public bool NeedsKey = false;
  public Vector3 Offset = Vector3.Zero; //   # Vector3 offset for items in maze
  public int Stat1;
  public int Stat2;
  public int Uses = 10; // min uses before item could break

  public bool IsWeapon => ItemType is ItemType.WarWeapon or ItemType.MagicWeapon;

  public bool IsBow => Name is "bow" or "crossbow";
  public bool IsBook => Name is "book" or "staff";

  public bool IsScroll => Name is "scroll" or "wand";

  public bool Spins => Name is "axe" or "dagger" or "fireball" or "small_fireball";

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