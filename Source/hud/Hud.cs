using System;
using System.Collections.Generic;
using Godot;
using minotaur.Source.dungeon;
using minotaur.Source.items;
using minotaur.Source.player;

namespace minotaur.Source.hud;

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
  private Sprite2D _leftHandSprite;
  private Sprite2D _atFeetSprite;
  private Sprite2D _rightHandSprite;

  private Node2D _pack;
  private Player _player;
  private Dungeon _dungeon;


  private Sprite2D _helmetSprite;
  private Sprite2D _armorSprite;
  private Sprite2D _breastPlateSprite;
  private Sprite2D _amuletSprite;

  private Dictionary<int, PackSlot> _packSlots = new();

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
    _leftHandSprite = GetNode("Hands/background/Left/Sprite2D") as Sprite2D;
    _atFeetSprite = GetNode("Hands/background/Feet/Sprite2D") as Sprite2D;
    _rightHandSprite = GetNode("Hands/background/Right/Sprite2D") as Sprite2D;

    _helmetSprite = GetNode("ArmorItems/HelmetSprite") as Sprite2D;
    _breastPlateSprite = GetNode("ArmorItems/BreastplateSprite") as Sprite2D;
    _amuletSprite = GetNode("ArmorItems/AmuletSprite") as Sprite2D;

    MainGame game = (MainGame)FindParent("Game");
    _dungeon = (Dungeon)game.GetNode("Dungeon");
    _player = (Player)_dungeon.GetNode("Player");

    foreach (var node in GetNode("Pack").GetChildren())
    {
      if (node is PackSlot packSlot)
      {
        _packSlots[packSlot.SlotNumber] = packSlot;
      }
    }
    
    foreach (var node in GetNode("Hands/background").GetChildren())
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

  private void Assign(Sprite2D sprite, ItemInfo item)
  {
    if (item == null)
    {
      sprite.Visible = false;
    }
    else
    {
      sprite.RegionEnabled = true;
      sprite.Visible = true;
      sprite.RegionRect = item.Image;
      sprite.Modulate = item.Color;
    }
  }
  
  public void UpdatePack()
  {
    Assign(_leftHandSprite, _player.LeftHand);
    Assign(_rightHandSprite, _player.RightHand);
    Assign(_atFeetSprite, _player.ItemAtFeet);
    foreach (var slot in _packSlots.Values)
    {
      slot.Item = _player.GetSlot(slot.SlotNumber);
    }
    // UpdateDamage();
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
      "shield" => _leftHandSprite,
      _ => throw new Exception("Invalid Inventory slot:" + slot)
    };
  }

  // Event handlers

  public void PackSlotClicked(int slot, InputEvent inputEvent)
  {
    if (inputEvent is InputEventMouseButton {Pressed:true, ButtonIndex: MouseButton.Left })
    {
      var prev = _player.SetSlot(slot, _player.LeftHand);
      _player.LeftHand = prev;
    }
    else if (inputEvent is InputEventMouseButton {Pressed:true, ButtonIndex: MouseButton.Right } rightClick)
    {
      var prev = _player.SetSlot(slot, _player.RightHand);
      _player.RightHand = prev;
    }
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
      _player.AttackOrUseItem();
      UpdateAll();
    }
    else if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Right })
    {
      _player.SwapHands();
      UpdateAll();
    }
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