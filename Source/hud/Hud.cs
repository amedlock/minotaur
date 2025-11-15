#region

using System;
using System.Collections.Generic;
using Godot;
using minotaur.Source.dungeon;
using minotaur.Source.items;
using minotaur.Source.model;
using minotaur.Source.player;

#endregion

namespace minotaur.Source.hud;

public partial class Hud : Node2D
{
  private readonly Dictionary<int, PackSlot> _packSlots = new();
  private Sprite2D _amuletSprite;
  private Label _armorDisplay;
  private Sprite2D _armorSprite;
  private Label _arrowsDisplay;
  private Sprite2D _atFeetSprite;
  private Sprite2D _breastPlateSprite;
  private Label _damageDisplay;

  private Label _foodDisplay;
  private Label _goldDisplay;


  private Sprite2D _helmetSprite;
  private Label _hpDisplay;
  private Sprite2D _leftHandSprite;
  private Label _levelDisplay;
  private Label _mindDisplay;

  [Export] private Control _pack;

  [Export] private Player _player;

  [Export] private Dungeon _dungeon;

  [Export] private GameModel gameModel;
  
  private Sprite2D _rightHandSprite;

  public Sprite2D Compass;


  public override void _Ready()
  {
    Compass = (Sprite2D)FindChild("Compass");
    _hpDisplay = (Label)FindChild("HPDisplay");
    _mindDisplay = (Label)FindChild("MindDisplay");
    _armorDisplay = (Label)FindChild("ArmorDisplay");
    _damageDisplay = (Label)FindChild("DamageDisplay");
    _goldDisplay = (Label)FindChild("GoldDisplay");
    _foodDisplay = (Label)FindChild("FoodDisplay");
    _levelDisplay = (Label)FindChild("LevelDisplay");
    _arrowsDisplay = (Label)FindChild("ArrowsDisplay");
    _leftHandSprite = GetNode<Sprite2D>("Hands/background/Left/Sprite2D");
    _atFeetSprite = GetNode<Sprite2D>("Hands/background/Feet/Sprite2D");
    _rightHandSprite = GetNode<Sprite2D>("Hands/background/Right/Sprite2D");

    _helmetSprite = GetNode<Sprite2D>("ArmorItems/HelmetSprite");
    _breastPlateSprite = GetNode<Sprite2D>("ArmorItems/BreastplateSprite");
    _amuletSprite = GetNode<Sprite2D>("ArmorItems/AmuletSprite");

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
    return new Vector2(dw / sw, dh / sh);
  }

  public void UpdateStats()
  {
    var pdata = gameModel.PlayerData;
    _levelDisplay.Text = $"Level: {gameModel.CurrentLevel.Depth}";
    _arrowsDisplay.Text = $"Arrows: {pdata.Arrows}";
    _foodDisplay.Text = $"Food: {pdata.Food}";
    _goldDisplay.Text = $"{pdata.Gold}";
    _hpDisplay.Text = $"{pdata.Health}/{pdata.HealthMax}";
    _mindDisplay.Text = $"{pdata.Mind}/{pdata.MindMax}";
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
    foreach (var slot in _packSlots.Values) slot.Item = _player.GetSlot(slot.SlotNumber);
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
    if (inputEvent is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
    {
      var prev = _player.SetSlot(slot, _player.LeftHand);
      _player.LeftHand = prev;
    }
    else if (inputEvent is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Right } rightClick)
    {
      var prev = _player.SetSlot(slot, _player.RightHand);
      _player.RightHand = prev;
    }

    UpdateStats();
    UpdatePack();
  }


  public void ClickedFeet(Node _viewport, InputEvent @event, long _shape_index)
  {
    if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left }) _player.UseOrTakeItem();
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
    if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Right }) _player.SwapItems();
    UpdateAll();
  }
}