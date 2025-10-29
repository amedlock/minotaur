using System;
using Godot;

namespace minotaur.Source.enemies;

public partial class Enemy : Sprite3D
{
  public int GridX = 0;
  public int GridY = 0;

  public int Health;
  public int Mind;

  private EnemyInfo _info;

  public void Init(EnemyInfo info, RandomNumberGenerator rnd)
  {
    _info = info;
    RegionEnabled = true;
    RegionRect = _info.ImageRect;
    Health = rnd.RandiRange(_info.MinHp, _info.MaxHp);
    Mind = rnd.RandiRange(_info.MinMind, _info.MaxMind);
  }
  
  public EnemyInfo Info => _info;
}