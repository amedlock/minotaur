using Godot;
using minotaur.Source.items;

namespace minotaur.Source.hud;

public partial class PackSlot : Area2D
{
  [Export] public int SlotNumber;

  private Hud _hud;
  private Sprite2D _sprite;
  private ItemInfo _itemInfo;

  public override void _Ready()
  {
    _hud = (Hud)FindParent("HUD");
    _sprite = FindChild("Sprite2D") as Sprite2D;
    InputEvent += (viewport, @event, idx) => _hud.PackSlotClicked(SlotNumber, @event);
    _sprite.Hide();
  }

  public ItemInfo Item
  {
    get => _itemInfo;
    set
    {
      if (_itemInfo == value)
      {
        return;
      }
      if (value == null)
      {
        _sprite.Hide();
      }
      else
      {
        _sprite.Scale = new Vector2(2, 2);
        _sprite.RegionRect = value.Image;
        _sprite.RegionEnabled = true;
        _sprite.Modulate = value.Color;
        _sprite.Show();
      }
    }
  }
}
