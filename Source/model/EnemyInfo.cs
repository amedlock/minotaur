#region

using Godot;

#endregion

namespace minotaur.Source.enemies;

public class EnemyInfo
{
  public int BaseDamage = 5;
  public Rect2I ImageRect;
  public int WarHp;
  public int MindHp;
  public int Armor;
  public int Damage;
  public int MinDepth;
  public string Name;
  public string Weapon;
  public EnemyType Type;
  public int MaxDepth;
  public Color Color;
  public int Power;
}