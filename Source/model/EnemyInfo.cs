#region

using Godot;

#endregion

namespace minotaur.Source.model;

public class EnemyInfo
{
  public Rect2I ImageRect;
  public int WarHp;
  public int MindHp;
  public int Armor;
  public int WeaponLevel;
  public int MinDepth;
  public string Name;
  public string Weapon;
  public EnemyType Type;
  public int MaxDepth;
  public Color Color;
  public int Power;
}