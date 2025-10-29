using Godot;
using minotaur.items;

namespace minotaur.hud;

public partial class PackSlot : Area2D
{

  [Export]
  public int SlotNumber;
  
  private Hud _hud;
  private Sprite2D _sprite;

  public override void _Ready()
  {
    _hud = FindParent("HUD") as Hud;
    _sprite = FindChild("Sprite2D") as Sprite2D;
    InputEvent += (viewport, @event, idx) => ClickedSlot(viewport, @event, idx);
    _sprite.Hide();
  }

  public void ClickedSlot(Node viewport, InputEvent inputEvent, long shape_idx)
  {
    if (inputEvent is InputEventMouseButton { Pressed: true, ButtonIndex: var index })
    {
      _hud.PackSlotClicked( SlotNumber, index );    
    }
  }


  public void SetItem(ItemInfo itemInfo)
  {
    if (itemInfo == null)
    {
      _sprite.Hide();
    }
    else
    {
      _sprite.Scale = new Vector2(2,2);
      _sprite.RegionRect = itemInfo.Image;
      _sprite.RegionEnabled = true;
      _sprite.Modulate = itemInfo.Color;
      _sprite.Show();
    }
  }
}
