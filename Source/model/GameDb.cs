#region

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

#endregion

namespace minotaur.Source.model;

// Loads items, icons, enemies from JSON file
public class GameDb
{
  private static readonly List<string> WarColors = ["Tan", "Orange", "Blue", "Grey", "Yellow", "White"];
  private static readonly List<string> MagicColors = ["Blue", "Grey", "White", "Pink", "Red", "Purple"];
  private static readonly List<string> ContainerColors = ["Tan", "Orange", "Blue"];

  private readonly System.Collections.Generic.Dictionary<string, Color> _colors = new();
  private readonly System.Collections.Generic.Dictionary<string, Rect2I> _icons = new();
  private readonly Vector2I _imageSize = new(32, 32);


  public GameDb()
  {
    LoadGameInfo();
  }

  public List<ItemInfo> Items { get; } = [];

  public List<EnemyInfo> Enemies { get; } = [];

  // sound name to resource path
  public System.Collections.Generic.Dictionary<string, string> Sounds { get; } = new();


  public ItemInfo FindItem(string name, int depth = 1)
  {
    return Items.Find(i => i.Name == name && i.MinDepth <= depth);
  }

  public List<EnemyInfo> FindEnemies(LevelInfo info)
  {
    return Enemies.Where(e => IsAllowed(e, info)).ToList();
  }

  public List<ItemInfo> FindWeapons(LevelInfo info)
  {
    var result = Items.Where(i => i.IsAllowed(info.LevelType));
    return result.Where(i => i.MinDepth <= info.Depth).ToList();
  }


  public List<ItemInfo> FindArmor(LevelInfo info)
  {
    return Items.Where(i => i.ItemType == ItemType.Armor && i.MinDepth <= info.Depth).ToList();
  }

  private bool IsAllowed(EnemyInfo enemy, LevelInfo levelInfo)
  {
    if (levelInfo.Depth < enemy.MinDepth || levelInfo.Depth > enemy.MaxDepth)
    {
      return false;
    }

    return enemy.Type switch
    {
      EnemyType.War => levelInfo.LevelType != LevelType.Magic,
      EnemyType.Magic => levelInfo.LevelType != LevelType.War,
      _ => true
    };
  }


  private void LoadGameInfo()
  {
    var src = FileAccess.Open("res://data/game_info.json", FileAccess.ModeFlags.Read);
    var error = src.GetError();
    if (error != Error.Ok)
    {
      GD.PrintErr("Error loading game info: " + error);
      return;
    }

    var txt = src.GetAsText();
    src.Close();

    var parser = new Json();
    var json = parser.Parse(txt);
    if (json != Error.Ok)
    {
      GD.PrintErr("JSON Error line:" + parser.GetErrorLine());
      return;
    }

    var data = (Dictionary)parser.Data;
    LoadColors(data["colors"]);
    LoadSounds(data["sounds"]);
    LoadIcons(data["item_icons"]);
    LoadIcons(data["enemy_icons"]);
    var enemies = (Godot.Collections.Dictionary<string, Variant>)data["enemies"];
    LoadEnemies(enemies, "war", EnemyType.War);
    LoadEnemies(enemies, "magic", EnemyType.Magic);
    LoadEnemies(enemies, "both", EnemyType.Both);
    LoadItems((Dictionary)data["items"]);
  }

  private void LoadColors(Variant data)
  {
    var dict = (Dictionary)data;
    foreach (var pair in dict) _colors[(string)pair.Key] = new Color((string)pair.Value);
  }

  private void LoadSounds(Variant data)
  {
    var dict = (Dictionary)data;
    foreach (var pair in dict) Sounds[(string)pair.Key] = (string)pair.Value;
  }

  // optional fields of a json object
  private static string Text(Dictionary data, string key)
  {
    return data.ContainsKey(key) ? (string)data[key] : null;
  }

  private static int Number(Dictionary data, string key)
  {
    return data.ContainsKey(key) ? (int)data[key] : 0;
  }

  private static bool Flag(Dictionary data, string key)
  {
    return data.ContainsKey(key) && (bool)data[key];
  }

  private void LoadIcons(Variant data)
  {
    var dict = (Dictionary)data;
    foreach (var pair in dict)
    {
      var array = (Array)pair.Value;
      var name = (string)pair.Key;
      Vector2I coord = new((int)array[0] * 32, (int)array[1] * 32);
      _icons[name] = new Rect2I(coord, _imageSize);
    }
  }


  // for each enemy load 3 variants of it 
  private void LoadEnemies(Variant data, string section, EnemyType enemyType)
  {
    List<string> colorNames = enemyType switch
    {
      EnemyType.War => ["White", "Grey", "Tan"],
      EnemyType.Magic => ["Blue", "Pink", "Purple"],
      EnemyType.Both => ["White", "Yellow", "Red"],
      _ => throw new ArgumentOutOfRangeException(nameof(enemyType), enemyType, null)
    };

    var dict = (Dictionary)data;
    foreach (var pair in (Dictionary)dict[section])
    {
      var name = (string)pair.Key;
      var stats = (Dictionary)pair.Value;
      int depth = (int)stats["min_level"];

      for (var power = 0; power < 3; power++)
      {
        var minDepth = power * 3 + depth;
        var maxDepth = minDepth + 5;

        var info = new EnemyInfo();
        info.Type = enemyType;
        info.Name = name;
        info.Color = _colors[colorNames[power]];
        info.ImageRect = _icons[name];
        info.Power = power;
        info.MinDepth = minDepth;
        info.MaxDepth = maxDepth;
        info.WarHp = (int)stats["war_hp"];
        info.MindHp = (int)stats["mind_hp"];
        info.Armor = (int)stats["armor"];
        info.Weapon = (string)stats["weapon"];
        info.WeaponLevel = (int)stats["weapon_level"];
        Enemies.Add(info);
      }
    }
  }

  private void LoadItems(Dictionary items)
  {
    LoadSpecials((Dictionary)items["specials"]);
    LoadWeapons((Dictionary)items["weapons"]);
    LoadArmor((Dictionary)items["armor"], WarColors);
    LoadKeyItems((Dictionary)items["keys"], _icons["key"], ItemType.Key, "key");
    LoadKeyItems((Dictionary)items["amulets"], _icons["amulet"], ItemType.Armor, "amulet");
    LoadContainers((Dictionary)items["containers"]);
    LoadMoney((Dictionary)items["money"]);
  }


  private ItemInfo AddItem(string name, ItemType kind, Rect2I icon, Color color, int minDepth, int stat1, int stat2)
  {
    var item = new ItemInfo();
    item.Name = name;
    item.ItemType = kind;
    item.Image = icon;
    item.Projectile = icon;
    item.Color = color;
    item.MinDepth = minDepth;
    item.Stat1 = stat1;
    item.Stat2 = stat2;
    Items.Add(item);
    return item;
  }


  private void LoadSpecials(Dictionary specials)
  {
    foreach (var pair in specials)
    {
      var item = new ItemInfo();
      item.Name = (string)pair.Key;
      var info = (Dictionary)pair.Value;
      item.Image = _icons[(string)info["icon"]];
      item.Projectile = item.Image;
      item.MinDepth = (int)info["min_level"];
      item.Color = _colors[(string)info["color"]];
      item.Use = Text(info, "use");
      item.Stat1 = Number(info, "amount");
      item.ItemType = ItemType.Special;
      Items.Add(item);
    }
  }

  // one item for each colour of a weapon
  private void LoadWeapons(Dictionary data)
  {
    foreach (var pair in data)
    {
      var name = (string)pair.Key;
      var info = (Dictionary)pair.Value;
      var isWar = (string)info["type"] == "war";
      var itemType = isWar ? ItemType.WarWeapon : ItemType.MagicWeapon;
      var colorNames = isWar ? WarColors : MagicColors;
      var n = 0;
      foreach (var power in (Array)info["power"])
      {
        var minLvl = n * 2 + 1;
        var item = AddItem(name, itemType, _icons[name], _colors[colorNames[n]], minLvl, (int)power, 0);
        item.Ammo = Text(info, "ammo");
        item.Sound = Text(info, "sound");
        item.Spin = Flag(info, "spin");
        item.SingleUse = Flag(info, "single_use");
        item.BreakChance = Number(info, "break_chance");
        var projectile = Text(info, "projectile");
        if (projectile != null) item.Projectile = _icons[projectile];
        n += 1;
      }
    }
  }

  private void LoadArmor(Dictionary data, List<string> colorNames)
  {
    // "small_shield": [6,12,18,24,30,36] ,
    foreach (var pair in data)
    {
      var name = (string)pair.Key;
      var values = (Array)pair.Value;
      var n = 0;
      foreach (int value in values)
      {
        var minLvl = n * 2 + 1;
        AddItem(name, ItemType.Armor, _icons[name], _colors[colorNames[n]], minLvl, value, 0);
        n += 1;
      }
    }
  }

  private void LoadKeyItems(Dictionary data, Rect2I icon, ItemType itemType, string name)
  {
    var n = 1;
    foreach (var pair in data)
    {
      var colorName = (string)pair.Key;
      AddItem(name, itemType, icon, _colors[colorName], (int)pair.Value, n, 0);
      n += 1;
    }
  }

  private void LoadContainers(Dictionary data)
  {
    var n = 1;
    foreach (var pair in data)
    {
      var name = (string)pair.Key;
      var info = (Dictionary)pair.Value;
      var baseDepth = (int)info["value"];
      foreach (var color in ContainerColors)
      {
        var keyLevel = ContainerColors.IndexOf(color);
        var minDepth = baseDepth * 2;
        var item = AddItem(name, ItemType.Container, _icons[name], _colors[color], minDepth, keyLevel, n);
        item.NeedsKey = Flag(info, "needs_key");
      }

      n += 1;
    }
  }


  private void LoadMoney(Dictionary money)
  {
    foreach (var pair in money)
    {
      var name = (string)pair.Key;

      // only one crown
      if (name == "crown")
      {
        AddItem("crown", ItemType.Money, _icons["crown"], _colors["Yellow"], 6, 200, 0);
        continue;
      }

      var items = (Array)pair.Value;
      var minDepth = (int)items[0];
      var silverValue = (int)items[1];
      var goldValue = (int)items[2];
      var platinumValue = (int)items[3];

      //  silver, gold, platinum
      AddItem(name, ItemType.Money, _icons[name], _colors["Grey"], minDepth, silverValue, 0);
      AddItem(name, ItemType.Money, _icons[name], _colors["Yellow"], minDepth + 3, goldValue, 0);
      AddItem(name, ItemType.Money, _icons[name], _colors["White"], minDepth + 6, platinumValue, 0);
    }
  }

  public EnemyInfo FindEnemy(string name)
  {
    return Enemies.FirstOrDefault(e => e.Name == name);
  }

  public ItemInfo FindWeapon(string weapon, int power, int depth)
  {
    return Items.FirstOrDefault(i => i.Name == weapon && i.MinDepth <= depth);
  }
}