#region

using Godot;

#endregion

namespace minotaur.Source.enemies;

public class EnemyInfo
{
  public int BaseDamage = 5;
  public Rect2I ImageRect;
  public int MaxHp;
  public int MaxMind;
  public int MinHp;
  public int MinLevel;
  public int MinMind;
  public string Name;
  public int Power;
  public EnemyType Type;
}