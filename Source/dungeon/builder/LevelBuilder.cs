using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using minotaur.Source.enemies;
using minotaur.Source.items;
using minotaur.Source.player;

namespace minotaur.Source.dungeon.builder;

// sole purpose is to generate content and store in DungeonGrid
// then using that content, creates Godot Nodes for visuals
public partial class LevelBuilder : Node
{
  private Dungeon _dungeon;
  private GameDb _gameDb;
  private Node3D _outerWall;
  private RandomNumberGenerator _rng = new();
  
  // prefabs  
  private PackedScene _wallPrefab;
  private PackedScene _cornerPrefab;
  private PackedScene _doorPrefab;
  private PackedScene _itemPrefab;
  private PackedScene _ladderPrefab;
  private PackedScene _enemyPrefab;
  private PackedScene _greenGate;
  private PackedScene _blueGate;
  private PackedScene _tanGate;

  DungeonGrid Grid => _dungeon.Grid;

  public override void _Ready()
  {
    _dungeon = (Dungeon)GetParent();
    _outerWall = _dungeon.FindChild("outer_wall") as Node3D;
    _gameDb = FindParent("Game").GetNode("GameDB") as GameDb;
    _wallPrefab = ResourceLoader.Load<PackedScene>("res://data/dungeon/dungeon_wall.tscn");
    _doorPrefab = ResourceLoader.Load<PackedScene>("res://data/door/door_prefab.tscn");
    _cornerPrefab = ResourceLoader.Load<PackedScene>("res://data/dungeon/wall_corner.tscn");
    _itemPrefab = ResourceLoader.Load<PackedScene>("res://data/items/item_prefab.tscn");
    _ladderPrefab = ResourceLoader.Load<PackedScene>("res://data/trapdoor/trapdoor.tscn");
    _enemyPrefab = ResourceLoader.Load<PackedScene>("res://data/enemies/enemy_prefab.tscn");
    _greenGate = ResourceLoader.Load<PackedScene>("res://data/gate/green_gate.tscn");
    _blueGate = ResourceLoader.Load<PackedScene>("res://data/gate/blue_gate.tscn");
    _tanGate = ResourceLoader.Load<PackedScene>("res://data/gate/tan_gate.tscn");
  }

  private bool IsOuterMaze(MazeCell mc)
  {
    return mc.X == 0 || mc.Y == 0 || mc.X == _dungeon.Width - 1 || mc.Y == _dungeon.Height - 1;
  }

  private void ClearOuterWall()
  {
    foreach (var cell in Grid.Cells)
    {
      if (cell.X == 0 || cell.X == _dungeon.Width - 1)
      {
        cell.North = WallType.Empty;
      }

      if (cell.Y == 0 || cell.Y == _dungeon.Height - 1)
      {
        cell.East = WallType.Empty;
      }
    }
  }

  private void AddOuterDoors()
  {
    Grid.Cell(3, 0).North = WallType.Door;
    Grid.Cell(8, 0).North = WallType.Door;
    Grid.Cell(3, _dungeon.Height - 2).North = WallType.Door;
    Grid.Cell(8, _dungeon.Height - 2).North = WallType.Door;
    Grid.Cell(0, 3).East = WallType.Door;
    Grid.Cell(0, 8).East = WallType.Door;
    Grid.Cell(_dungeon.Width - 2, 3).East = WallType.Door;
    Grid.Cell(_dungeon.Width - 2, 8).East = WallType.Door;
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

  private MazeCell ChooseRandom(List<MazeCell> items)
  {
    if (items.Count == 0)
    {
      return null;
    }

    return items[_rng.RandiRange(0, items.Count - 1)];
  }

  private T ChooseRandom<T>(List<T> items)
  {
    if (items.Count == 0)
    {
      return default(T);
    }

    return items[_rng.RandiRange(0, items.Count - 1)];
  }


  private T TakeRandom<T>(List<T> items)
  {
    if (items.Count == 0)
    {
      return default(T);
    }

    var index = _rng.RandiRange(0, items.Count - 1);
    var result = items[index];
    items.RemoveAt(index);
    return result;
  }

  private IEnumerable<MazeCell> AdjacentCells(MazeCell cell)
  {
    List<MazeCell> items =
    [
      Grid.Cell(cell.X, cell.Y + 1),
      Grid.Cell(cell.X, cell.Y - 1),
      Grid.Cell(cell.X + 1, cell.Y),
      Grid.Cell(cell.X - 1, cell.Y)
    ];
    return items.Where(x => x != null);
  }

  List<MazeCell> AllInactiveNear(MazeCell mc)
  {
    return AdjacentCells(mc).Where(x => !(x.Active || IsOuterMaze(x))).ToList();
  }

  MazeCell FindActiveNear(MazeCell mc)
  {
    var work = AdjacentCells(mc).Where(x => x.Active).ToList();
    return ChooseRandom(work);
  }

  // add walls adjacent to outer corridor

  void AddOuterWalls()
  {
    foreach (var cell in Grid.Cells.Where(IsOuterMaze))
    {
      cell.Active = true;
      if (cell.Y == 0)
      {
        if (cell.X > 0 && cell.X < _dungeon.Width - 1)
        {
          cell.North = WallType.Wall;
        }
      }
      else if (cell.Y > 0 && cell.Y < _dungeon.Height - 1)
      {
        if (cell.X == 0)
        {
          cell.East = WallType.Wall;
        }
      }
    }
  }

  bool CheckWall(int x, int y, Direction dir)
  {
    var cell = Grid.Cell(x, y);
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
    AddOuterWalls();
    AddOuterDoors();

    var size = 0;
    var sx = _rng.RandiRange(1, 5);
    var sy = _rng.RandiRange(1, 5);
    var start = Grid.Cell(sx, sy);
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
    AddAllCorners();
  }


  private void AddAllCorners()
  {
    foreach (var m in Grid.Cells)
    {
      if (IsOuterMaze(m))
      {
        continue;
      }

      // if (m.North != WallType.Empty)
      // {
      //   m.Corners.NE = m.Corners.NW = true;
      // }
      //
      // if (m.East != WallType.Empty)
      // {
      //   m.Corners.SE = m.Corners.NE = true;
      // }
      // var southCell = GetCell(m.X, m.Y -1);
      // if (southCell != null)
      // {
      //   if (southCell.North!=WallType.Empty)
      //   {
      //     m.Corners.SE = m.Corners.SW = true;
      //   }
      // }
      //
      // var westCell = GetCell(m.X - 1, m.Y);
      // if (westCell != null)
      // {
      //   if (westCell.East != WallType.Empty)
      //   {
      //     m.Corners.NE = m.Corners.SE = true;
      //   }
      // }
    }
    
  }
  

  // prims algo makes maze too twisty, add some strategic doorways
  void AddMoreDoors()
  {
    if (CheckWall(3, 3, Direction.East))
    {
      Grid.Cell(3, 3).East = WallType.Door;
    }

    if (CheckWall(8, 4, Direction.North))
    {
      Grid.Cell(8, 4).North = WallType.Door;
    }

    if (CheckWall(3, 8, Direction.North))
    {
      Grid.Cell(3, 8).North = WallType.Door;
    }

    if (CheckWall(8, 9, Direction.North))
    {
      Grid.Cell(8, 9).North = WallType.Door;
    }
  }

  void AddGates(LevelInfo info)
  {
    if (info.Depth > 2 || info.UsedGate)
    {
      return;
    }

    List<GateType> gates = info.LevelType switch
    {
      LevelType.War => [GateType.Magic, GateType.Both],
      LevelType.Magic => [GateType.War, GateType.Both],
      LevelType.Both => [GateType.Magic, GateType.War],
      _ => []
    };
    if (gates.Count == 2)
    {
      Grid.Cell(_dungeon.Width - 1, 0).Gate = gates[0];
      Grid.Cell(0, _dungeon.Height - 1).Gate = gates[1];
    }
  }

  void AddExit(LevelInfo info)
  {
    // last level is 100
    if (info.Depth > 99)
    {
      return;
    }

    List<Vector2I> exitLoc = [new(3, 4), new(7, 4), new(4, 3), new(4, 7)];
    Vector2I exit = ChooseRandom(exitLoc);
    var cell = Grid.Cell(exit);
    cell.ItemInfo = _gameDb.FindItem("ladder");
    cell.Active = true;
  }

  void AddEnemies(LevelInfo info, List<MazeCell> cells)
  {
    if (cells.Count == 0)
    {
      GD.PrintErr("No empty cells available for enemies");
      return;
    }

    var num = _rng.RandiRange(0, 6) + 12;
    var enemies = _gameDb.FindEnemies(info);
    var sorted = enemies.OrderBy(_ => _rng.Randi()).ToList();
    if (sorted.Count == 0)
    {
      throw new Exception("No enemies allowed for placement");
    }
    foreach (var n in GD.Range(num))
    {
      if (cells.Count == 0)
      {
        return;
      }

      var target = TakeRandom(cells);
      EnemyInfo monster = sorted[n % sorted.Count];
      Grid.Cell(target.X, target.Y).EnemyInfo = monster;
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
    Grid.Cell(c.X, c.Y).ItemInfo = ChooseRandom(keys);
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

    foreach (var unused in GD.Range(num))
    {
      if (cells.Count == 0 || items.Count == 0)
      {
        break;
      }
      else
      {
        var target = TakeRandom(cells);
        Grid.Cell(target.X, target.Y).ItemInfo = ChooseRandom(items);
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
        Grid.Cell(c.X, c.Y).ItemInfo = ChooseRandom(allowed);
      }
    }
  }

  void AddOther(List<MazeCell> cells)
  {
    var food = _gameDb.FindItem("food");
    var foodCount = _rng.RandiRange(1, 3);
    foreach (var unused in GD.Range(foodCount))
    {
      if (cells.Count > 0)
      {
        var c = TakeRandom(cells);
        Grid.Cell(c.X, c.Y).ItemInfo = food;
      }
      else
      {
        return;
      }
    }

    var quiver = _gameDb.FindItem("quiver");
    foreach (var unused in GD.Range(_rng.RandiRange(1, 3)))
    {
      if (cells.Count == 0)
      {
        return;
      }

      var c = TakeRandom(cells);
      Grid.Cell(c.X, c.Y).ItemInfo = quiver;
    }
  }

  void AddWeapons(int weaponCount, int armorCount, LevelInfo info, List<MazeCell> cells)
  {
    var armor = _gameDb.FindArmor(info);
    foreach (var n in GD.Range(armorCount))
    {
      if (cells.Count == 0 || armor.Count == 0)
      {
        break;
      }

      var c = TakeRandom(cells);
      c.ItemInfo = TakeRandom(armor);
    }

    var weapons = _gameDb.FindWeapons(info);
    foreach (var n in GD.Range(weaponCount))
    {
      if (cells.Count == 0 || armor.Count == 0)
      {
        break;
      }
      TakeRandom(cells).ItemInfo = TakeRandom(weapons);
    }
  }

  void AddAmulets(List<MazeCell> cells)
  {
    var amulets = _gameDb.Items.Where(i => i.ItemType == ItemType.Armor).ToList();
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
    AddLoot(bags, info, cells);
    AddKey(info, cells);
    AddOther(cells);
    AddWeapons(weapons, armor, info, cells);
    AddAmulets(cells);
  }



  public void BuildMaze(LevelInfo currentLevel)
  {
    foreach (var cell in Grid.Cells)
    {
      cell.Reset();
    }

    _rng.Seed = currentLevel.SeedNumber;
    BuildMazePrim();
    AddMoreDoors();
    AddExit(currentLevel);
    AddGates(currentLevel);
    var emptyCells = Grid.EmptyCells.ToList();

    AddEnemies(currentLevel, emptyCells);
    AddItems(currentLevel, emptyCells);
    AddMinotaur(currentLevel, emptyCells);
    BuildGrid(currentLevel); // build the actual geometry
    _dungeon.MuralColor = currentLevel._gateType;
  }

  private void AddMinotaur(LevelInfo currentLevel, List<MazeCell> cells)
  {
    if (currentLevel.HasMinotaur)
    { 
      var cell = TakeRandom(cells);
      cell.EnemyInfo = _gameDb.FindEnemy("Minotaur");
    }
  }

  void BuildGrid(LevelInfo levelInfo)
  {
    foreach (var cell in Grid.Cells)
    {
      var dungeonCell = _dungeon.GetCell(cell.X, cell.Y);
      dungeonCell.ClearAll();
      dungeonCell.MazeCell = cell;
      CreateWall(dungeonCell, cell.North, Direction.North);
      CreateWall(dungeonCell, cell.East, Direction.East);
      foreach (var wallPost in Grid.WallPosts(cell.X, cell.Y))
      {
        AddCorner(wallPost, dungeonCell);
      }
      AddItem(dungeonCell, cell.ItemInfo);
      AddEnemy(dungeonCell, cell.EnemyInfo);
      AddGate(dungeonCell, cell, levelInfo.LevelType);
    }
  }
  

  private void AddEnemy(DungeonCell dc, EnemyInfo enemy)
  {
    if (dc.Enemy != null)
    {
      dc.RemoveChild(dc.Enemy);
      dc.Enemy.QueueFree();
      dc.Enemy = null;
    }

    if (enemy == null)
    {
      return;
    }

    var node = (Enemy)_enemyPrefab.Instantiate();
    node.Init(enemy, _rng);
    dc.AddChild(node);
    dc.Enemy = node;
    node.Position = new Vector3(1.5f, .7f, -1.5f);
  }


  public void AddItem(DungeonCell cell, ItemInfo itemInfo)
  {
    if (cell.Item != null)
    {
      cell.Item.QueueFree();
      cell.RemoveChild(cell.Item);
      cell.Item = null;
    }

    if (cell.ItemInfo is { Name: "ladder" })
    {
      var exit = (Node3D)_ladderPrefab.Instantiate();
      exit.Name = "Ladder";
      exit.Position = new Vector3(1.5f, 0, -1.5f);
      cell.AddChild(exit);
    }
    else if (itemInfo != null)
    {
      var result = _itemPrefab.Instantiate() as Item;
      result.Init(itemInfo);
      result.Position = new Vector3(1.5f, .35f, -1.5f);
      cell.Item = result;
      cell.AddChild(result);
    }
  }
  
  public void AddGate(DungeonCell dungeonCell, MazeCell cell, LevelType level)
  {
    if (dungeonCell.Gate != null)
    {
      dungeonCell.RemoveChild(dungeonCell.Gate);
      dungeonCell.Gate.QueueFree();
      dungeonCell.Gate = null;
    }
    
    if (cell.Gate == GateType.None)
    {
      return;
    }
    var result = level switch
    {
      LevelType.War => (DungeonGate)_greenGate.Instantiate(),
      LevelType.Magic => (DungeonGate)_blueGate.Instantiate(),
      LevelType.Both => (DungeonGate)_tanGate.Instantiate(),
      _ or LevelType.None => null
    };
    if (result != null)
    {
      dungeonCell.AddChild(result);
      dungeonCell.Gate = result;
      result.Position = new Vector3(1.5f, 0.25f, -1.5f);
    }
  }
  
  // walls and corners
  private readonly Vector3 _wallOffset = new(3f, 0, -3f);

  private readonly Dictionary<Direction, Vector3I> _wallRotation = new()
  {
    [Direction.North] = new Vector3I(0, 180, 0),
    [Direction.East] = new Vector3I(0, 270, 0)
  };



  private void CreateWall(DungeonCell dest, WallType wallType, Direction direction)
  {
    var wallRot = _wallRotation[direction];
    Node3D node = wallType switch
    {
      WallType.Door => (Node3D)_doorPrefab.Instantiate(),
      WallType.Wall => (Node3D)_wallPrefab.Instantiate(),
      _ => null
    };
    if (node == null)
    {
      return;
    }
    dest.AddChild(node);
    node.Position = _wallOffset;
    node.RotationDegrees = wallRot;    
  }
 
  
  private Dictionary<WallPost, Vector3> _wallPostOffset = new()
  {
    [WallPost.NW] = new Vector3(0, 0f, 0),
    [WallPost.NE] = new Vector3(3, 0f, 0),
    [WallPost.SE] = new Vector3(3, 0f, 3),
    [WallPost.SW] = new Vector3(0, 0f, 3)
  };
  
  public void AddCorner(WallPost wallPost, DungeonCell dest)
  {
    var corner = (Node3D)_cornerPrefab.Instantiate();
    corner.Name = "corner_" + nameof(wallPost);
    dest.AddChild(corner);
    corner.Position = _wallPostOffset[wallPost];
  }
}
