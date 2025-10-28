using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using minotaur.enemies;
using minotaur.items;
using minotaur.player;

namespace minotaur.dungeon;

public partial class LevelBuilder : Node
{
  public struct Corners
  {
    private bool ne, nw, sw, se;

    public void Reset()
    {
      ne = nw = sw = se = false;
    }
  }

  public class MazeCell(int x, int y)
  {
    public int X => x;
    public int Y => y;
    
    // until this is set to true, nothing set on this maze cell (walls, items, etc)
    public bool Active;
    
    public WallType North = WallType.Empty;
    public WallType East = WallType.Empty;
    public Corners Corners;
    public LevelType LevelType = LevelType.None;
    public ItemInfo ItemInfo;
    public EnemyInfo EnemyInfo;


    public void Reset()
    {
      Active = false;
      Corners.Reset();
      North = East = WallType.Empty;
      LevelType = LevelType.None;
      ItemInfo = null;
      EnemyInfo = null;
    }

    public bool AdjacentTo(MazeCell m2)
    {
      if (m2.X == X)
      {
        return m2.Y == Y + 1 || m2.Y == Y - 1;
      }
      else if (m2.Y == Y)
      {
        return m2.X == X + 1 || m2.X == X - 1;
      }

      return false;
    }
  }

  private Dungeon _dungeon;
  private GameDb _gameDb;
  private RandomNumberGenerator _rng = new RandomNumberGenerator();
  private List<MazeCell> _maze = [];

  // prefabs  
  private PackedScene _wallPrefab;
  private PackedScene _cornerPrefab;
  private PackedScene _doorPrefab;
  private PackedScene _itemPrefab;
  private PackedScene _enemyPrefab;
  private PackedScene _greenGate;
  private PackedScene _blueGate;
  private PackedScene _tanGate;

  
  public IEnumerable<MazeCell>  EmptyCells => _maze.Where(c => !c.Active);
  
  
  public override void _Ready()
  {
    _dungeon = GetParent<Dungeon>();
    _gameDb = FindParent("Game").GetNode("GameDB") as GameDb;
    _maze.Clear();
    _wallPrefab = ResourceLoader.Load<PackedScene>("res://data/dungeon/dungeon_wall.tscn");
    _cornerPrefab = ResourceLoader.Load<PackedScene>("res://data/door/door_prefab.tscn");
    _itemPrefab = ResourceLoader.Load<PackedScene>("res://data/items/item_prefab.tscn");
    _enemyPrefab = ResourceLoader.Load<PackedScene>("res://data/enemies/enemy_prefab.tscn");
    _greenGate = ResourceLoader.Load<PackedScene>("res://data/gate/green_gate.tscn");
    _blueGate = ResourceLoader.Load<PackedScene>("res://data/gate/blue_gate.tscn");
    _tanGate = ResourceLoader.Load<PackedScene>("res://data/gate/tan_gate.tscn");    
    foreach (var yp in GD.Range(_dungeon.Height))
    {
      foreach (var xp in GD.Range(_dungeon.Width))
      {
        var n = xp + (yp * _dungeon.Width);
        _maze.Add(new MazeCell(xp, yp));
      }
    }
  }

  bool IsValid(int x, int y)
  {
    return (x >= 0) && (x < _dungeon.Width) && (y >= 0) && (y < _dungeon.Height);
  }
  
  private MazeCell GetCell(Vector2I v) => GetCell(v.X, v.Y);

  private MazeCell GetCell(int x, int y)
  {
    return IsValid(x, y) ? _maze[x + (y * _dungeon.Width)] : null;
  }

  private bool IsOuterMaze(MazeCell mc)
  {
    return mc.X == 0 || mc.Y == 0 || mc.X == _dungeon.Width - 1 || mc.Y == _dungeon.Height - 1;
  }
  
  private void ClearOuterWall()
  {
    foreach (var yp in GD.Range(_dungeon.Height))
    {
      foreach (var xp in GD.Range(_dungeon.Width))
      {
        var mc = GetCell(xp, yp);
        if (xp == 0 || xp == _dungeon.Width - 1)
        {
          mc.North = WallType.Empty;
        }

        if (yp == 0 || yp == _dungeon.Height - 1)
        {
          mc.East =  WallType.Empty;
        }
      }
    }
  }
  
  private void AddOuterDoors()
  {
    GetCell(3, 0).North = WallType.Door;
    GetCell(8, 0).North = WallType.Door;
    GetCell(3,_dungeon.Height-2).North = WallType.Door;
    GetCell(8,_dungeon.Height-2).North = WallType.Door;
    GetCell(0,3).East = WallType.Door;
    GetCell(0,8).East = WallType.Door;
    GetCell(_dungeon.Width-2,3).East = WallType.Door; 
    GetCell(_dungeon.Width-2,8).East  = WallType.Door;
  }

  private void AddPath(MazeCell from, MazeCell to)
  {
    from.Active = true;
    to.Active = true;
    if (from.X == to.X - 1)
    {
      from.East = WallType.Empty;
    }
    else if (from.X == to.X + 1)
    {
      to.East = WallType.Empty;      
    }
    else if (from.Y == to.Y - 1)
    {
      from.North = WallType.Empty;
    }
    else if (from.Y == to.Y + 1)
    {
      to.North = WallType.Empty;
    }
  }

  private  MazeCell ChooseRandom(List<MazeCell> items)
  {
    if (items.Count == 0)
    {
      return null;
    }
    return items[_rng.RandiRange(0, items.Count-1)];
  }
  
  private T ChooseRandom<T>(List<T> items)
  {
    if (items.Count == 0)
    {
      return default(T);
    }
    return items[_rng.RandiRange(0, items.Count-1)];
  }
  

  private T TakeRandom<T>(List<T> items)
  {
    if (items.Count == 0)
    {
      return default(T);
    }
    var index = _rng.RandiRange(0, items.Count-1);
    var result = items[index];
    items.RemoveAt(index);
    return result;
  }

  private IEnumerable<MazeCell> AdjacentCells(MazeCell cell)
  {
    List<MazeCell> items =
    [
      GetCell(cell.X, cell.Y + 1),
      GetCell(cell.X, cell.Y - 1),
      GetCell(cell.X + 1, cell.Y),
      GetCell(cell.X - 1, cell.Y)
    ];
    return items.Where(x => x != null);  
  }

  List<MazeCell> AllInactiveNear(MazeCell mc)
  {
    return AdjacentCells(mc).Where(x => !x.Active || IsOuterMaze(x)).ToList();
  }

  MazeCell FindActiveNear(MazeCell mc)
  {
    List<MazeCell> work = AdjacentCells(mc).Where(x => x.Active).ToList();
    return ChooseRandom(work);
  }

  // add walls adjacent to outer corridor

  void AddOuterWalls(MazeCell mc)
  {
    if (mc.X == _dungeon.Width - 1 || mc.Y == _dungeon.Height - 1)
    {
      return;
    }

    if (mc.Y == 0)
    {
      if (mc.X > 0)
      {
        mc.North = WallType.Wall;
      }
    }
    else if (mc.X == 0)
    {
      if (mc.Y > 0)
      {
        mc.East = WallType.Wall;
      }
    }
    else
    {
      mc.North = WallType.Wall;
      mc.East = WallType.Wall;
    }
  }

  bool CheckWall(int x, int y, Direction dir)
  {
    var cell = GetCell(x, y);
    if (cell == null)
    {
      return false;
    }
    return dir switch
    {
      Direction.West => CheckWall(x - 1, y, Direction.East),
      Direction.South => CheckWall(x, y - 1, Direction.North),
      Direction.North => cell.North == WallType.Wall,
      Direction.East => cell.East == WallType.Wall,
      _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null)
    };
  }

  private void BuildMazePrim()
  {
    foreach (var mc in _maze)
    {
      mc.Active = false;
      AddOuterWalls(mc);
    }
    AddOuterDoors();

    var size = 0;
    var sx = _rng.RandiRange(1,5);
    var sy = _rng.RandiRange(1,5);
    var start = GetCell(sx, sy);
    start.Active = true; // first active cell
    var frontier = AllInactiveNear(start); // potential paths
    while (frontier.Count > 0)
    {
      if (size > 140)
      {
        break;
      }

      size += 1;
      MazeCell pick = TakeRandom(frontier);
      pick.Active = true;
      var near = FindActiveNear(pick);
      if (near != null)
      {
        AddPath(near, pick);
        foreach (var x in AllInactiveNear(pick))
        {
          if (!frontier.Contains(x))
          {
            frontier.Add(x);
          }
        }
      }
      else
      {
        throw new Exception("Could not create maze");
      }
    }
  }


  // prims algo makes maze too twisty, add some strategic doorways
  void AddMoreDoors()
  {
    if (CheckWall(3, 3, Direction.East))
    {
      GetCell(3,3).East = WallType.Door;
    }

    if (CheckWall(8, 4, Direction.North))
    {
      GetCell(8, 4).North = WallType.Door;  
    }

    if (CheckWall(3, 8, Direction.North))
    {
      GetCell(3, 8).North = WallType.Door;
    }
 
    if (CheckWall(8, 9, Direction.North))
    {
      GetCell(8, 9).North = WallType.Door;
    }
  }

  void AddGates(LevelInfo info)
  {
    if (info.Depth > 2 || info.UsedGate)
    {
      return;
    }

    List<LevelType> gates = info.LevelType switch
    {
      LevelType.War => [LevelType.Magic, LevelType.Both],
      LevelType.Magic => [LevelType.War, LevelType.Both],
      LevelType.Both => [LevelType.Magic, LevelType.War],
      _ => []
    };
    if (gates.Count == 2)
    {
      GetCell(_dungeon.Width - 1, 0).LevelType = gates[0];
      GetCell(0, _dungeon.Height - 1).LevelType = gates[1];  
    }
  }

  void AddExit(LevelInfo info)
  {
    if (info.Depth >= 99)
    {
      return;
    }

    List<Vector2I> exitLoc = [new Vector2I(3, 4), new Vector2I(7, 4), new Vector2I(4, 3), new Vector2I(4, 7)];
    Vector2I exit = ChooseRandom(exitLoc);
    GetCell(exit).ItemInfo = _gameDb.FindItem("ladder");
  }

  void AddEnemies(LevelInfo info, List<MazeCell> cells)
  {
    var num = _rng.RandiRange(0, 6) + 12;
    List<EnemyInfo> allowed = _gameDb.FindEnemies(info);
    foreach (var n in GD.Range(num))
    {
      if (allowed.Count==0 || cells.Count==0){
        return;
      }
      var target = TakeRandom(cells);
      EnemyInfo monster = ChooseRandom(allowed);
      GetCell(target.X, target.Y).EnemyInfo = monster;
    }
  }

  void AddKey(LevelInfo info, List<MazeCell> cells)
  {
    if (cells.Count == 0)
    {
      return;
    }
    List<ItemInfo> keys = _gameDb.Items.Where(i => i.ItemType == ItemType.Key && !info.HasItem(i)).ToList();
    if (keys.Count == 0)
    {
      GD.Print("Warning no 'key' items found for level ", info);
      return;
    }
    var c = ChooseRandom(cells);
    GetCell(c.X, c.Y).ItemInfo = ChooseRandom(keys);
  }

  void AddLoot(int num, LevelInfo info, List<MazeCell> cells)
  {
    HashSet<string> names = ["small_bag"];
    switch (info.Depth)
    {
      case 2 or 3 or 4: names.Add("bag"); break;
      case 5: names.Add("box"); break;
      case 6: names.Add("pack"); break;
      default: names.Add("chest"); break;
    }
    var items = _gameDb.Items.Where(i => i.ItemType == ItemType.Container && names.Contains(i.Name)).ToList();

    foreach (var n in GD.Range(num))
    {
      if (cells.Count == 0 || items.Count == 0)
      {
        break;
      }
      else
      {
        var target = TakeRandom(cells);
        GetCell(target.X, target.Y).ItemInfo = ChooseRandom(items);
      }
    }
  }

  void AddMoney(int num, LevelInfo info, List<MazeCell> cells)
  {
    var allowed = _gameDb.Items.Where(i => i.ItemType == ItemType.Money).ToList();
    if (allowed.Count > 0)
    {
      foreach (var n in GD.Range(num))
      {
        if (cells.Count == 0)
        {
          return;
        }

        var c = TakeRandom(cells);
        GetCell(c.X, c.Y).ItemInfo = ChooseRandom(allowed);
      }
    }
  }

  void AddOther(List<MazeCell> cells)
  {
    var food = _gameDb.FindItem("food");
    var foodCount = _rng.RandiRange(1, 3);
    foreach (var n in GD.Range(foodCount))
    {
      if (cells.Count > 0)
      {
        var c = TakeRandom(cells);
        GetCell(c.X, c.Y).ItemInfo = food;
      }
      else
      {
        return;
      }
    }
    var quiver = _gameDb.FindItem("quiver");
    foreach (var n in GD.Range(_rng.RandiRange(1, 3)))
    {
      if (cells.Count == 0)
      {
        return;
      }
      var c = TakeRandom(cells);
      GetCell(c.X, c.Y).ItemInfo = quiver;
    }
  }

  void AddWeapons(int weaponCount, int armorCount, LevelInfo info, List<MazeCell> cells)
  {
    var armor = _gameDb.Items.Where(i => i.ItemType == ItemType.Armor).ToList();
    foreach(var n in GD.Range(armorCount))
    {
      if (cells.Count == 0 || armor.Count == 0)
      {
        break;
      }
      var c = TakeRandom(cells);
      c.ItemInfo = TakeRandom(armor);
    }
    var weapons = _gameDb.Items.Where(i => i.ItemType == ItemType.Weapon).ToList();
    foreach (var n in GD.Range(weaponCount))
    {
      if (cells.Count == 0 || armor.Count == 0)
      {
        break;
      }
      var c = TakeRandom(cells);
      c.ItemInfo = TakeRandom(weapons);
    }
  }

  void AddAmulets(List<MazeCell> cells)
  {
    var amulets = _gameDb.Items.Where(i => i.ItemType==ItemType.Armor).ToList();
    foreach (var n in GD.Range(_rng.RandiRange(0, 3)))
    {
      if (cells.Count == 0 || amulets.Count == 0)
      {
        break;
      }
      var c = TakeRandom(cells);
      c.ItemInfo = TakeRandom(amulets);
    }
  }

  void AddItems(LevelInfo info, List<MazeCell> cells)
  {
    var bags = 7 + _rng.RandiRange(0, 3);
    var weapons = 7 + _rng.RandiRange(1, 5);
    var armor = _rng.RandiRange(0, 2);
    AddLoot( bags, info, cells );
    AddKey( info, cells );
    AddOther( cells );
    AddWeapons( weapons, armor, info, cells );
    AddAmulets(cells);
  }

 
  
  public void BuildMaze(LevelInfo currentLevel)
  {
    foreach (var cell in _maze)
    {
      cell.Reset();
    }

    _rng.Seed = currentLevel.SeedNumber;
    BuildMazePrim();
    AddMoreDoors();
    AddExit(currentLevel);
    AddGates(currentLevel);
    var empty_cells = EmptyCells.ToList();
	
    AddEnemies(currentLevel, empty_cells);
    AddItems( currentLevel, empty_cells);
    AddMinotar(currentLevel, empty_cells);
    BuildGrid() ; // build the actual geometry
    _dungeon.MuralColor = currentLevel._gateType;	
  }

  private void AddMinotar(LevelInfo currentLevel, List<MazeCell> cells)
  {
    if (currentLevel.HasMinotaur)
    {
      var cell = TakeRandom(cells);
      cell.EnemyInfo = _gameDb.FindEnemy("Minotaur");
    }
  }

  void BuildGrid()
  {
    var grid = _dungeon.Grid;
    foreach (var cell in _maze)
    {
      var dcell = grid.AddCell(cell.X, cell.Y);
      AddWalls(dcell, cell.North, cell.East);
      AddCorners(dcell, cell.Corners);
      AddItem(dcell, cell.ItemInfo);
      AddEnemy(dcell, cell.EnemyInfo);
      AddGate(dcell, cell.LevelType);
    }
  }

  private void AddEnemy(DungeonCell dcell, EnemyInfo enemy)
  {
    if (dcell.Enemy != null)
    {
      dcell.RemoveChild(dcell.Enemy);
      dcell.Enemy.QueueFree();
      dcell.Enemy = null;
    }

    if (enemy == null)
    {
      return;
    }

    var node = _enemyPrefab.Instantiate() as Enemy;
    dcell.AddChild(node);
    dcell.Enemy = node;
    node.Position = new Vector3(1.5f, 0.9f, -1.5f);
  }

  public void AddWalls(DungeonCell dcell, WallType north, WallType east)
  {
    
  }

  public void AddCorners(DungeonCell dungeonCell, Corners corners)
  {
    
  }

  public void AddItem(DungeonCell cell, ItemInfo itemInfo)
  {
    if (cell.Item != null)
    {
      cell.Item.QueueFree();
      cell.RemoveChild(cell.Item);
      cell.Item = null;
    }
    var result = _itemPrefab.Instantiate() as Item;
    result.Init(itemInfo);
    cell.Item = result;
    cell.AddChild(result);
  }
  
  public void AddGate(DungeonCell dungeonCell, LevelType level)
  {
    if (dungeonCell.Gate != null)
    {
      dungeonCell.RemoveChild(dungeonCell.Gate);
      dungeonCell.Gate.QueueFree();
      dungeonCell.Gate = null;
    }

    var result = level switch
    {
      LevelType.War => (DungeonGate)_greenGate.Instantiate(),
      LevelType.Magic => (DungeonGate)_blueGate.Instantiate(),
      LevelType.Both => (DungeonGate)_tanGate.Instantiate(),
      _ or LevelType.None => null
    };
    if (result == null)
    {
      return;
    }

    dungeonCell.AddChild(result);    
    result.Position = new Vector3(1.5f, 0.25f, -1.5f);
  }
  
  // walls and corners
  private Vector3 _wallOffset = new(3f, 0.25f, -3f);

  private Dictionary<Direction, int> _wallAngle = new()
  {
    [Direction.North] = 180,
    [Direction.East] = 270
  };

  public void SetNorth(DungeonCell cell, WallType wallType)
  {
    if (cell.North != null)
    {
      cell.RemoveChild(cell.North);
      cell.North.QueueFree();
      cell.North = null;
    }
    
    var result = wallType switch
    {
      WallType.Door => _doorPrefab.Instantiate() as Wall,
      WallType.Wall => _wallPrefab.Instantiate() as Wall,
      _ => null
    };
    if (result != null)
    {
      cell.AddChild(result);
      cell.North = result;  
    }
  }
  
  public void SetEast(DungeonCell cell, WallType wallType)
  {
    if (cell.East != null)
    {
      cell.RemoveChild(cell.East);
      cell.East.QueueFree();
      cell.East = null;
    }
    
    var result = wallType switch
    {
      WallType.Door => _doorPrefab.Instantiate() as Wall,
      WallType.Wall => _wallPrefab.Instantiate() as Wall,
      _ => null
    };
    if (result != null)
    {
      cell.AddChild(result);
      cell.East = result;  
    }
  }
  

  private Dictionary<WallPost, Vector3> _wallPostOffset = new()
  {
    [WallPost.NW] = new Vector3(0, 0.25f, 0),
    [WallPost.NE] = new Vector3(3, 0.25f, 0),
    [WallPost.SE] = new Vector3(3, 0.25f, 3),
    [WallPost.SW] = new Vector3(0, 0.25f, 3)
  };

  public void ClearCorner(string which)
  {
    var n = FindChild($"corner_{which}", false, false);
    if (n != null)
    {
      RemoveChild(n);
      n.QueueFree();
    }
  }

  public void SetCorner(WallPost post)
  {
    var name = $"corner_{nameof(post)}";
    var n = FindChild(name, false, false);
    if (n == null)
    {
      var it = (Node3D)_cornerPrefab.Instantiate();
      it.Position = _wallPostOffset[post];
      it.Name = name;
      AddChild(it);
    }
  }


  
}
