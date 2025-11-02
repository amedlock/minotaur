using Godot;
using minotaur.Source.items;

namespace minotaur.Source.enemies;

public partial class Enemy : Sprite3D
{
  public int GridX = 0;
  public int GridY = 0;

  public int Health;
  public int Mind;

  private EnemyInfo _info;
  private PackedScene _smokePrefab;

  public void Init(EnemyInfo info, RandomNumberGenerator rnd)
  {
    _info = info;
    RegionEnabled = true;
    RegionRect = _info.ImageRect;
    Health = rnd.RandiRange(_info.MinHp, _info.MaxHp);
    Mind = rnd.RandiRange(_info.MinMind, _info.MaxMind);
    _smokePrefab = ResourceLoader.Load<PackedScene>("res://data/enemies/smoke.tscn");
  }
  
  public EnemyInfo Info => _info;
  
  public bool IsDead => Health <= 0 || Mind <= 0;

  public void Damage(ItemInfo item)
  {
    if (item == null) return;
    Health = Mathf.Max(Health - item.Stat1, 0);
    Mind = Mathf.Max(Mind - item.Stat2, 0);
  }

  public void Die()
  {
    if (!Visible)
    {
      return; // might get called twice
    }
    Visible = false;
    var smoke = (Smoke)_smokePrefab.Instantiate();
    smoke.Position = Position - new Vector3(0, 0.6f, 0);
    smoke.Visible = true;
    GetParent().AddChild(smoke);
    smoke.Start();
  }
}