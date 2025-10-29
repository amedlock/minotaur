using Godot;

namespace minotaur.Source.enemies;

public partial class Enemy : Sprite3D
{
  public int GridX = 0;
  public int GridY = 0;
  public EnemyInfo Info { get; set; }
}