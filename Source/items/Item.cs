using Godot;

namespace minotaur.Source.items;

public partial class Item : Sprite3D
{
  public ItemInfo Info;

  public void Init(ItemInfo itemInfo)
  {
    Info = itemInfo;
    RegionEnabled = true;
    RegionRect = itemInfo.Image;
  }
}