#region

using Godot;
using minotaur.Source.items;

#endregion

namespace minotaur.Source.enemies;

public partial class Enemy : Sprite3D
{
  private PackedScene _smokePrefab;
  public int GridX = 0;
  public int GridY = 0;

  public int Health;
  public int Mind;

  public EnemyInfo Info { get; private set; }

  public bool IsDead => Health <= 0 || Mind <= 0;

  public void Init(EnemyInfo info, RandomNumberGenerator rnd)
  {
    Info = info;
    Modulate = info.Color;
    RegionEnabled = true;
    RegionRect = Info.ImageRect;
    Health = Info.WarHp;
    Mind = Info.MindHp;
    _smokePrefab = ResourceLoader.Load<PackedScene>("res://data/enemies/smoke.tscn");
  }

  public void Damage(ItemInfo item)
  {
    if (item == null) return;
    var dmg = item.Stat1 * 4;
    var magic = item.Stat2 * 4;
    Health = Mathf.Max(Health - dmg, 0);
    Mind = Mathf.Max(Mind - magic, 0);
  }

  public void Die()
  {
    if (!Visible) return; // might get called twice
    Visible = false;
    var smoke = (Smoke)_smokePrefab.Instantiate();
    smoke.Position = Position - new Vector3(0, 0.6f, 0);
    smoke.Visible = true;
    GetParent().AddChild(smoke);
    smoke.Start();
  }
}