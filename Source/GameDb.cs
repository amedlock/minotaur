using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using minotaur.Source.dungeon;
using minotaur.Source.enemies;
using minotaur.Source.items;

namespace minotaur;

// Loads items, icons, enemies from JSON file
public partial class GameDb : Node
{
  private Vector2I _imageSize = new(32, 32);

  private System.Collections.Generic.Dictionary<string, Color> _colors = new();
  private System.Collections.Generic.Dictionary<string, Rect2I> _icons = new();
  private List<EnemyInfo> _enemies = [];
  private List<ItemInfo> _items = [];

  private ItemInfo _finalTreasure;


  private static readonly List<string> WarColors = ["Tan", "Orange", "Blue", "Grey", "Yellow", "White"];
  private static readonly List<string> MagicColors = ["Blue", "Grey", "White", "Pink", "Red", "Purple"];
  private static readonly List<string> MoneyColors = ["Orange", "Grey", "Yellow", "White"];
  private static readonly List<string> ContainerColors = ["Tan", "Orange", "Blue"];


  public ItemInfo FindItem(string name, int depth = 1)
  {
    return _items.Find(i => i.Name == name && i.MinDepth <= depth);
  }

  public List<EnemyInfo> FindEnemies(LevelInfo info)
  {
    return Enemies.Where(e => IsAllowed(e, info)).ToList();
  }


  private bool IsAllowed(EnemyInfo enemy, LevelInfo levelInfo)
  {
    if (levelInfo.Depth < enemy.MinLevel)
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
  
  public List<ItemInfo> Items => _items;

  public List<EnemyInfo> Enemies => _enemies;

  public override void _Ready()
  {
    LoadGameInfo();
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
    foreach (var pair in dict)
    {
      _colors[(string)pair.Key] = new Color((string)pair.Value);
    }
  }

  private void LoadIcons(Variant data)
  {
    var dict = (Dictionary)data;
    foreach (var pair in dict)
    {
      Array array = (Array)pair.Value;
      string name = (string)pair.Key;
      Vector2I coord = new((int)array[0] * 32, (int)array[1] * 32);
      _icons[name] = new Rect2I(coord, _imageSize);
    }
  }

  private void LoadEnemies(Variant data, string section, EnemyType enemyType)
  {
    var dict = (Dictionary)data;
    foreach (var pair in (Dictionary)dict[section])
    {
      Array stats = (Array)pair.Value;
      var enemyInfo = new EnemyInfo();
      enemyInfo.Type = enemyType;
      enemyInfo.Name = (string)pair.Key;
      enemyInfo.ImageRect = this._icons[(string)stats[0]];
      enemyInfo.MinLevel = (int)stats[1];
      enemyInfo.MinHp = (int)stats[2];
      enemyInfo.MaxHp = (int)stats[3];
      enemyInfo.MinMind = (int)stats[4];
      enemyInfo.MaxMind = (int)stats[5];
      _enemies.Add(enemyInfo);
    }
  }

  private void LoadItems(Dictionary items)
  {
    LoadSpecials((Dictionary)items["specials"]);
    Dictionary weapons = (Dictionary)items["weapons"];
    LoadWeapons(weapons, "war", WarColors);
    LoadWeapons(weapons, "magic", MagicColors);
    LoadArmor((Dictionary)items["armor"], WarColors);
    LoadKeyItems((Dictionary)items["keys"], _icons["key"], ItemType.Key, "key");
    LoadKeyItems((Dictionary)items["amulets"], _icons["amulet"], ItemType.Armor, "amulet");
    LoadContainers((Dictionary)items["containers"]);
    LoadMoney((Dictionary)items["money"]);
  }


  private void AddItem(string name, ItemType kind, Rect2I icon, Color color, int minDepth, int stat1, int stat2)
  {
    var item = new ItemInfo();
    item.Name = name;
    item.ItemType = kind;
    item.Image = icon;
    item.Color = color;
    item.MinDepth = minDepth;
    item.Stat1 = stat1;
    item.Stat2 = stat2;
    item.NeedsKey = kind == ItemType.Container && name.Match("pack|container|chest");
    _items.Add(item);
  }


  private void LoadSpecials(Dictionary specials)
  {
    foreach (var pair in specials)
    {
      var item = new ItemInfo();
      item.Name = (string)pair.Key;
      Array items = (Array)pair.Value;
      item.Image = _icons[(string)items[0]];
      item.MinDepth = (int)items[1];
      item.Color = _colors[(string)items[2]];
      item.Stat1 = (int)items[3];
      item.Stat2 = (int)items[4];
      item.ItemType = ItemType.Special;
      _items.Add(item);
    }
    
  }

  private void LoadWeapons(Dictionary data, string type, List<string> colorNames)
  {
    var itemType = type=="war" ? ItemType.WarWeapon : ItemType.MagicWeapon;
    foreach (var pair in (Dictionary)data[type])
    {
      var name = (string)pair.Key;
      var items = (Array)pair.Value;
      var n = 0;
      foreach (var power in items)
      {
        var minLvl = (n * 2) + 1;
        AddItem(name, itemType, _icons[name], _colors[colorNames[n]], minLvl, (int)power, 0);
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
        var minLvl = (n * 2) + 1;
        AddItem(name, ItemType.Armor, _icons[name], _colors[colorNames[n]], minLvl, value, 0);
        n += 1;
      }
    }
  }

  private void LoadKeyItems(Dictionary data, Rect2I icon, ItemType itemType, string name)
  {
    foreach (var pair in data)
    {
      var colorName = (string)pair.Key;
      AddItem(name, itemType, icon, _colors[colorName], (int)pair.Value, 0, 0);
    }
  }

  private void LoadContainers(Dictionary data)
  {
    var n = 0;
    foreach (var pair in data)
    {
      var name = (string)pair.Key;
      var value = (int)pair.Value;
      foreach( var color in ContainerColors)
      {
        var minLvl = value * 2;
        AddItem(name, ItemType.Container, _icons[name], _colors[color], minLvl, value, 0 );
      }
    }
  }


  private void LoadMoney(Dictionary money)
  {
    
    foreach (var pair in money)
    {
      var name = (string)pair.Key;
      var items = (Array)pair.Value;
      var minDepth = (int)items[0];
      var value = (int)items[1];

      // only one crown
      if (name == "crown")
      {
        AddItem("crown", ItemType.Money, _icons["crown"], _colors["Yellow"], 6, 200, 0);
        continue;
      }
      
      // bronze, silver, gold, platinum
      foreach (var colorName in MoneyColors)
      {
        var color =  _colors[colorName];
        AddItem(name, ItemType.Money, _icons[name],  color, minDepth, value, 0);
        value *= 2;
      }
    }
  }

  public EnemyInfo FindEnemy(string name)
  {
    return Enemies.FirstOrDefault(e => e.Name == name);
  }

  private readonly List<string> _warWeapons = ["axe", "spear", "dagger"];
  private readonly List<string> _magicWeapons = ["small_fireball", "fireball"];
  
  public List<ItemInfo> FindWeapons(EnemyType enemyType, LevelInfo current)
  {
    var result = _items.Where(it => it.IsWeapon);
    if (enemyType != EnemyType.War)
    {
      result = result.Where(it => _magicWeapons.Contains(it.Name));
    }
    if (enemyType != EnemyType.Magic)
    {
      result = result.Where(it => _warWeapons.Contains(it.Name));
    }
    return result.ToList();
  }

  public ItemInfo FindMissile(ItemInfo item)
  {
    return item.Name switch
    {
      "bow" or "crossbow" => FindItem("arrow"),
      "staff" or "book" => FindItem("small_fireball"),
      "wand" or "scroll" => FindItem("small_lightning"),
      _ => item
    };
  }
}