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


  public static readonly List<string> WarColors = ["Tan", "Orange", "Blue", "Grey", "Yellow", "White"];
  public static readonly List<string> MagicColors = ["Blue", "Grey", "White", "Pink", "Red", "Purple"];
  public static readonly List<string> MoneyColors = ["Orange", "Grey", "Yellow", "White"];
  public static readonly List<string> ContainerColors = ["Tan", "Orange", "Blue"];


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
    if (enemy.MinLevel > levelInfo.Depth)
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
    LoadEnemies(enemies, "war");
    LoadEnemies(enemies, "magic");
    LoadEnemies(enemies, "both");
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

  private void LoadEnemies(Godot.Collections.Dictionary<string,Variant> data, string section)
  {
    var dict = (Dictionary)data;
    foreach (var pair in (Dictionary)dict[section])
    {
      Array stats = (Array)pair.Value;
      var enemyInfo = new EnemyInfo();
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

  private void LoadItems(Dictionary data)
  {
    LoadSpecials((Dictionary)data["specials"]);
    Dictionary weapons = (Dictionary)data["weapons"];
    LoadWeapons(weapons, "war", WarColors);
    LoadWeapons(weapons, "magic", MagicColors);
    LoadArmor((Dictionary)data["armor"], WarColors);
    LoadKeyItems((Dictionary)data["keys"], _icons["key"], ItemType.Key, "key");
    LoadKeyItems((Dictionary)data["amulets"], _icons["amulet"], ItemType.Armor, "amulet");
    LoadContainers((Dictionary)data["containers"]);
    LoadMoney((Dictionary)data["money"]);
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
    item.NeedsKey = false; // @TODO
    _items.Add(item);
  }


  private void LoadSpecials(Dictionary data)
  {
  }

  private void LoadWeapons(Dictionary data, string type, List<string> colorNames)
  {
    foreach (var pair in (Dictionary)data[type])
    {
      var name = (string)pair.Key;
      var items = (Array)pair.Value;
      var n = 0;
      foreach (var power in items)
      {
        var minLvl = (n * 2) + 1;
        AddItem(name, ItemType.Weapon, _icons[name], _colors[colorNames[n]], minLvl, (int)power, 0);
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


  private void LoadMoney(Dictionary data)
  {
    foreach (var pair in data)
    {
      var name = (string)pair.Key;
      var values = (Array)pair.Value;
      foreach (var colorName in MoneyColors)
      {
        var color =  _colors[colorName];
        AddItem(name, ItemType.Money, _icons[name],  color, (int)values[0], (int)values[1], 0);
      }
    }
  }

  public EnemyInfo FindEnemy(string name)
  {
    return Enemies.FirstOrDefault(e => e.Name == name);
  }
}