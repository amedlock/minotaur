using Godot;

namespace minotaur.Source.items;

public partial class Item : Node
{
  public ItemInfo Info;

  public void Init(ItemInfo itemInfo)
  {
    Info = itemInfo;
  }
}