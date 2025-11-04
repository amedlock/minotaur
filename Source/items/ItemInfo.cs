using Godot;
using minotaur.Source.dungeon;

namespace minotaur.Source.items;

public class ItemInfo
{
  public ItemType ItemType;
  public string Name;
  public int MinDepth = 1;
  public Color Color;
  public Rect2I Image;
  public Color color = Colors.White;
  public int Uses = 10 ; // min uses before item could break
  public bool NeedsKey = false;
  public int Stat1;
  public int Stat2;
  public Vector3 Offset = Vector3.Zero;//   # Vector3 offset for items in maze

  public bool IsWeapon => ItemType is ItemType.WarWeapon or ItemType.MagicWeapon;
  
  public bool IsBow => Name is "bow" or "crossbow";
  public bool IsBook => Name is "scroll" or "book" or "wand" or "staff";
  public bool Spins => Name is "axe" or "dagger" or "fireball" or "small_fireball";

  public bool IsWar => ItemType == ItemType.WarWeapon;
  public bool IsMagic => ItemType == ItemType.MagicWeapon;

  public bool IsAllowed(LevelType levelType)
  {
    if (IsWeapon)
    {
      return levelType switch
      {
        LevelType.Magic => IsMagic,
        LevelType.War => IsWar,
        LevelType.Both => true
      };
    }
    return true;
  }
}