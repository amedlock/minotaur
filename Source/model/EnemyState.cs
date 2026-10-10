using Godot;

namespace minotaur.Source.model;

// a living enemy in the maze, EnemyInfo holds the stats it was created from
public class EnemyState(EnemyInfo info)
{
  public readonly EnemyInfo Info = info;

  public int Health = info.WarHp;
  public int Mind = info.MindHp;

  public bool IsDead => Health <= 0 || Mind <= 0;

  public void Damage(ItemInfo item)
  {
    if (item == null) return;
    var dmg = item.Stat1 * 4;
    var magic = item.Stat2 * 4;
    Health = Mathf.Max(Health - dmg, 0);
    Mind = Mathf.Max(Mind - magic, 0);
  }
}
