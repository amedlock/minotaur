using Godot;

namespace minotaur.items;

public partial class Item : Node
{
  public ItemInfo Info;

  public void Init(ItemInfo itemInfo)
  {
    Info = itemInfo;
  }
}