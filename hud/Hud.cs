using System;
using System.Collections.Generic;
using Godot;
using minotaur.dungeon;
using minotaur.items;
using minotaur.player;

namespace minotaur.hud;

public partial class Hud : Node2D
{
  private Label _hpDisplay;
  private Label _mindDisplay;
  private Label _armorDisplay;
  private Label _damageDisplay;
  private Label _goldDisplay;
  private Label _foodDisplay;
  private Label _levelDisplay;
  private Label _arrowsDisplay;
  private Sprite2D _shieldSprite;
  private Sprite2D _atFeetSprite;
  private Sprite2D _rightHandSprite;

  private Node2D _pack;
  private Player _player;
  private Dungeon _dungeon;


  private Sprite2D _helmetSprite;
  private Sprite2D _armorSprite;
  private Sprite2D _breastPlateSprite;
  private Sprite2D _amuletSprite;

  private List<string> pack_slots =
  [
    "Slot1", "Slot2", "Slot3", "Slot4", "Slot5",
    "Slot6", "Slot7", "Slot8", "Slot9"
  ];

  public Sprite2D Compass;


  public override void _Ready()
  {
    Compass = FindChild("Compass") as Sprite2D;
    _hpDisplay = FindChild("HPDisplay") as Label;
    _mindDisplay = FindChild("MindDisplay") as Label;
    _armorDisplay = FindChild("ArmorDisplay") as Label;
    _damageDisplay = FindChild("DamageDisplay") as Label;
    _goldDisplay = FindChild("GoldDisplay") as Label;
    _foodDisplay = FindChild("FoodDisplay") as Label;
    _levelDisplay = FindChild("LevelDisplay") as Label;
    _arrowsDisplay = FindChild("ArrowsDisplay") as Label;
    _shieldSprite = GetNode("Hands/background/Left/Sprite2D") as Sprite2D;
    _atFeetSprite = GetNode("Hands/background/Feet/Sprite2D") as Sprite2D;
    _rightHandSprite = GetNode("Hands/background/Right/Sprite2D") as Sprite2D;

    _helmetSprite = GetNode("ArmorItems/HelmetSprite") as Sprite2D;
    _breastPlateSprite = GetNode("ArmorItems/BreastplateSprite") as Sprite2D;
    _amuletSprite = GetNode("ArmorItems/AmuletSprite") as Sprite2D;

    MainGame game = FindParent("Game") as MainGame;
    _dungeon = game.GetNode("Dungeon") as Dungeon;
    _player = _dungeon.GetNode("Player") as Player;

    foreach (var node in FindChildren("Hands/background"))
    {
      var area2d = (Area2D)node;
      switch (node.Name)
      {
        case "Left":
          area2d.InputEvent += ClickedLeft;
          break;
        case "Feet":
          area2d.InputEvent += ClickedFeet;
          break;
        case "Right":
          area2d.InputEvent += ClickedRight;
          break;
      }
    }
  }

  private Vector2 CalcSpriteScale(float sw, float sh, float dw, float dh)
  {
    return new(dw / sw, dh / sh);
  }

  public void UpdateStats()
  {
    _levelDisplay.Text = $"Level: {_dungeon.CurrentLevel.Depth}";
    _arrowsDisplay.Text = $"Arrows: {_player.Arrows}";
    _foodDisplay.Text = $"Food: {_player.Food}";
    _goldDisplay.Text = $"{_player.Gold}";
    _hpDisplay.Text = $"{_player.Health}/{_player.HealthMax}";
    _mindDisplay.Text = $"{_player.Mind}/{_player.MindMax}";
    UpdateDamage();
  }

  public void UpdateDamage()
  {
    _armorDisplay.Text = $"{_player.WarArmor}/{_player.MindArmor}";
    _damageDisplay.Text = $"{_player.WarDamage}/{_player.MindDamage}";
  }

  public void UpdatePack()
  {
    // set_slot_item("hand", _player.right_hand)
    // var at_feet = _player.item_at_feet()
    // set_slot_item("feet", at_feet)
    // set_slot_item("shield", _player.shield)
    // UpdateDamage();
    // var index = 1;
    // for i_name in pack_slots:
    // var p = pack.find_child( i_name )
    // if _player.inventory.has( index ):
    // p.set_item( _player.inventory[index] )
    // else:
    // p.set_item(null);
    // index += 1;
  }


  public void UpdateAll()
  {
    UpdatePack();
    UpdateStats();
  }

  private Sprite2D FindSlot(string slot)
  {
    return slot switch
    {
      "hand" => _rightHandSprite,
      "feet" => _atFeetSprite,
      "shield" => _shieldSprite,
      _ => throw new Exception("Invalid Inventory slot:" + slot)
    };
  }

  public void SetSlotItem(string which, ItemInfo item)
  {
    Sprite2D target = FindSlot(which);
    if (item == null)
    {
      target.Visible = false;
    }
    else
    {
      target.Scale = CalcSpriteScale(32, 32, 50, 50);
      target.Modulate = item.Color;
      target.RegionRect = item.Image;
      target.Visible = true;
      target.RegionEnabled = true;
    }
  }

  // Event handlers

  public void PackSlotClicked(int slot, MouseButton button)
  {
    // ItemType cur = _player.GetInventory(slot);
    // if (button == MouseButton.Left)
    // {
    // 	_player.Inventory[slot] = _player.ItemAtFeet;
    // 	_player.ItemAtFeet = cur;
    // }
    // else if (button == MouseButton.Right)
    // {
    // 	_player.inventory[slot] = _player.RightHand;
    // 	_player.RightHand = cur;
    // }
    UpdateStats();
    UpdatePack();
  }


  public void ClickedFeet(Node _viewport, InputEvent @event, long _shape_index)
  {
    if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
    {
      _player.UseOrTakeItem();
    }

    UpdateStats();
  }

  // Alternate attack method, press F otherwise
  public void ClickedRight(Node _viewport, InputEvent @event, long _shape_idx)
  {
    if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
    {
      _player.AttackAhead();
    }

    UpdateAll();
  }


  // Alternate attack method?
  public void ClickedLeft(Node _viewport, InputEvent @event, long _shape_idx)
  {
    if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Right })
    {
      _player.SwapItems();
    }

    UpdateAll();
  }
}