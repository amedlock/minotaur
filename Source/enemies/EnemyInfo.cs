using Godot;
using minotaur.Source.dungeon;

namespace minotaur.Source.enemies;

public class EnemyInfo
{
  public string Name;
  public EnemyType Type;
  public int MinLevel;
  public int Power;
  public int MinHp;
  public int MaxHp;
  public int MinMind;
  public int MaxMind;
  public int BaseDamage = 5;
  public Rect2I ImageRect;
}