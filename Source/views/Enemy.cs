#region

using Godot;
using minotaur.Source.model;

#endregion

namespace minotaur.Source.views;

public partial class Enemy : Sprite3D
{
  public EnemyInfo Info { get; private set; }

  public void Init(EnemyInfo info)
  {
    Info = info;
    Modulate = info.Color;
    RegionEnabled = true;
    RegionRect = Info.ImageRect;
  }

  public void Die()
  {
    if (!Visible)
    {
      return; // might get called twice
    }

    Visible = false;
    var smoke = (Smoke)ResourceLoader.Load<PackedScene>("res://data/enemies/smoke.tscn").Instantiate();
    smoke.Position = Position - new Vector3(0, 0.6f, 0);
    smoke.Visible = true;
    GetParent().AddChild(smoke);
    smoke.Start();
  }
}