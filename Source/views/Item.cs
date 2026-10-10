#region

using Godot;
using minotaur.Source.model;

#endregion

namespace minotaur.Source.views;

public partial class Item : Sprite3D
{
  private ItemInfo _info;

  public ItemInfo Info
  {
    get => _info;
    set
    {
      _info = value;
      if (value != null)
      {
        RegionEnabled = true;
        RegionRect = value.Image;
        Modulate = value.Color;
        Visible = true;
      }
      else
      {
        Visible = false;
      }
    }
  }
}