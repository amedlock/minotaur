#region

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using minotaur.Source.dungeon;
using minotaur.Source.enemies;
using minotaur.Source.items;

#endregion

namespace minotaur.Source.model;

public class LevelRandomizer(DungeonGrid dungeonGrid, GameDb db)
{
  RandomNumberGenerator rng = new();

  public void BuildLevel(LevelInfo levelInfo)
  {
    rng.Seed = levelInfo.SeedNumber;
    dungeonGrid.GateType = levelInfo.GateType;
    BuildMazePrim();
    AddMoreDoors();
    AddExit(levelInfo);
    AddGates(levelInfo);
    var emptyCells = dungeonGrid.EmptyCells.Where(cell => !IsOuterMaze(cell)).ToList();

    AddEnemies(levelInfo, emptyCells);
    AddItems(levelInfo, emptyCells);
    AddMinotaur(levelInfo, emptyCells);
  }

  private T ChooseRandom<T>(List<T> items)
  {
    return items.Count == 0 ? default : items[rng.RandiRange(0, items.Count - 1)];
  }

  private T TakeRandom<T>(List<T> items)
  {
    if (items.Count == 0)
    {
      return default;
    }

    var index = rng.RandiRange(0, items.Count - 1);
    var result = items[index];
    items.RemoveAt(index);
    return result;
  }

  private IEnumerable<MazeCell> AdjacentCells(MazeCell cell)
  {
    List<MazeCell> items =
    [
      dungeonGrid.Cell(cell.X, cell.Y + 1),
      dungeonGrid.Cell(cell.X, cell.Y - 1),
      dungeonGrid.Cell(cell.X + 1, cell.Y),
      dungeonGrid.Cell(cell.X - 1, cell.Y)
    ];
    return items.Where(x => x != null);
  }

  private bool AdjacentToAny(MazeCell cell, List<MazeCell> other)
  {
    return other.Any(c => c.AdjacentTo(cell));
  }
  
  private void BuildMazePrim()
  {
    dungeonGrid.Reset();
    CreateCorridor();
    AddOuterDoors();

    var notMaze = dungeonGrid.Cells.Where(cell => !IsOuterMaze(cell)).ToList();
    List<MazeCell> inMaze = [];
    var start = TakeRandom(notMaze);
    start.Used = true;
    inMaze.Add(start);
    while (notMaze.Count > 0)
    {
      var frontier = notMaze.Where(c => AdjacentToAny(c, inMaze)).ToList();
      if (frontier.Count == 0)
      {
        GD.Print("No frontier, aborting");
        break;
      }

      var pick = TakeRandom(frontier);
      pick.Used = true;
      notMaze.Remove(pick);
      inMaze.Add(pick);
      var next = inMaze.First(c => c.AdjacentTo(pick));
      AddPath(next, pick);
    }
  }

  private bool IsOuterMaze(MazeCell mc)
  {
    return mc.X == 0 || mc.Y == 0 || mc.X == dungeonGrid.Width - 1 || mc.Y == dungeonGrid.Height - 1;
  }

  // clear walls to create outer corridor
  private void CreateCorridor()
  {
    foreach (var cell in dungeonGrid.Cells)
    {
      var x = cell.X;
      var y = cell.Y;
      cell.Used = IsOuterMaze(cell);
      if (x == 0)
      {
        cell.North = WallType.Empty;
        if (y == 0 || y == dungeonGrid.Height - 1) cell.East = WallType.Empty;
      }
      else if (y == 0)
      {
        cell.East = WallType.Empty;
        if (x == dungeonGrid.Width - 1) cell.North = WallType.Empty;
      }
      else if (y == dungeonGrid.Height - 1)
      {
        cell.North = WallType.Empty;
        cell.East = WallType.Empty;
      }
      else if (x == dungeonGrid.Width - 1)
      {
        cell.East = WallType.Empty;
        cell.North = WallType.Empty;
      }
    }
  }

  private void AddOuterDoors()
  {
    dungeonGrid.Cell(3, 0).North = WallType.Door;
    dungeonGrid.Cell(8, 0).North = WallType.Door;
    dungeonGrid.Cell(3, dungeonGrid.Height - 2).North = WallType.Door;
    dungeonGrid.Cell(8, dungeonGrid.Height - 2).North = WallType.Door;
    dungeonGrid.Cell(0, 3).East = WallType.Door;
    dungeonGrid.Cell(0, 8).East = WallType.Door;
    dungeonGrid.Cell(dungeonGrid.Width - 2, 3).East = WallType.Door;
    dungeonGrid.Cell(dungeonGrid.Width - 2, 8).East = WallType.Door;
  }

  private bool CheckWall(int x, int y, Direction dir)
  {
    var cell = dungeonGrid.Cell(x, y);
    if (cell == null) return false;

    return dir switch
    {
      Direction.West => CheckWall(x - 1, y, Direction.East),
      Direction.South => CheckWall(x, y - 1, Direction.North),
      Direction.North => cell.North == WallType.Wall,
      Direction.East => cell.East == WallType.Wall,
      _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null)
    };
  }

  // prims algo makes maze too twisty, add some strategic doorways
  private void AddMoreDoors()
  {
    if (CheckWall(3, 3, Direction.East))
    {
      dungeonGrid.Cell(3, 3).East = WallType.Door;
    }

    if (CheckWall(8, 4, Direction.North))
    {
      dungeonGrid.Cell(8, 4).North = WallType.Door;
    }

    if (CheckWall(3, 8, Direction.North))
    {
      dungeonGrid.Cell(3, 8).North = WallType.Door;
    }

    if (CheckWall(8, 9, Direction.North))
    {
      dungeonGrid.Cell(8, 9).North = WallType.Door;
    }
  }

  private void AddPath(MazeCell from, MazeCell to)
  {
    from.Used = true;
    to.Used = true;
    if (from.X == to.X - 1)
      from.East = WallType.Empty;
    else if (from.X == to.X + 1)
      to.East = WallType.Empty;
    else if (from.Y == to.Y - 1)
      from.North = WallType.Empty;
    else if (from.Y == to.Y + 1) to.North = WallType.Empty;
  }


  private void AddExit(LevelInfo info)
  {
    // last level is 100
    if (info.Depth > 99) return;

    List<Vector2I> exitLoc = [new(3, 4), new(7, 4), new(4, 3), new(4, 7)];
    var exit = ChooseRandom(exitLoc);
    var cell = dungeonGrid.Cell(exit);
    cell.ItemInfo = db.FindItem("ladder");
    cell.Used = true;
  }


  private void AddGates(LevelInfo info)
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
      dungeonGrid.Cell(dungeonGrid.Width - 1, 0).Gate = gates[0];
      dungeonGrid.Cell(0, dungeonGrid.Height - 1).Gate = gates[1];
    }
  }


  private void AddEnemies(LevelInfo info, List<MazeCell> cells)
  {
    if (cells.Count == 0)
    {
      GD.PrintErr("No empty cells available for enemies");
      return;
    }

    var num = rng.RandiRange(0, 6) + 12;
    var enemies = db.FindEnemies(info);
    var sorted = enemies.OrderBy(_ => rng.Randi()).ToList();
    if (sorted.Count == 0) throw new Exception("No enemies allowed for placement");

    foreach (var n in GD.Range(num))
    {
      if (cells.Count == 0) return;

      var target = TakeRandom(cells);
      var monster = sorted[n % sorted.Count];
      dungeonGrid.Cell(target.X, target.Y).EnemyInfo = monster;
    }
  }

  private void AddMinotaur(LevelInfo currentLevel, List<MazeCell> cells)
  {
    if (currentLevel.HasMinotaur)
    {
      var cell = TakeRandom(cells);
      cell.EnemyInfo = db.FindEnemy("Minotaur");
    }
  }


  private void AddKey(LevelInfo info, List<MazeCell> cells)
  {
    if (cells.Count == 0) return;

    var keys = db.Items.Where(i => i.ItemType == ItemType.Key && !info.HasItem(i)).ToList();
    if (keys.Count == 0)
    {
      GD.Print("Warning no 'key' items found for level ", info);
      return;
    }

    var c = ChooseRandom(cells);
    dungeonGrid.Cell(c.X, c.Y).ItemInfo = ChooseRandom(keys);
  }

  private void AddLoot(int num, LevelInfo info, List<MazeCell> cells)
  {
    HashSet<string> names = ["small_bag"];
    switch (info.Depth)
    {
      case 2 or 3 or 4: names.Add("bag"); break;
      case 5: names.Add("box"); break;
      case 6: names.Add("pack"); break;
      default: names.Add("chest"); break;
    }

    var items = db.Items.Where(i => i.ItemType == ItemType.Container && names.Contains(i.Name)).ToList();

    foreach (var unused in GD.Range(num))
    {
      if (cells.Count == 0 || items.Count == 0) break;

      var target = TakeRandom(cells);
      dungeonGrid.Cell(target.X, target.Y).ItemInfo = ChooseRandom(items);
    }
  }

  private void AddMoney(int num, LevelInfo info, List<MazeCell> cells)
  {
    var allowed = db.Items.Where(i => i.ItemType == ItemType.Money).ToList();
    if (allowed.Count > 0)
      foreach (var n in GD.Range(num))
      {
        if (cells.Count == 0) return;

        var c = TakeRandom(cells);
        dungeonGrid.Cell(c.X, c.Y).ItemInfo = ChooseRandom(allowed);
      }
  }

  private void AddOther(List<MazeCell> cells)
  {
    var food = db.FindItem("food");
    var foodCount = rng.RandiRange(1, 3);
    foreach (var unused in GD.Range(foodCount))
      if (cells.Count > 0)
      {
        var c = TakeRandom(cells);
        dungeonGrid.Cell(c.X, c.Y).ItemInfo = food;
      }
      else
      {
        return;
      }

    var quiver = db.FindItem("quiver");
    foreach (var unused in GD.Range(rng.RandiRange(1, 3)))
    {
      if (cells.Count == 0) return;

      var c = TakeRandom(cells);
      dungeonGrid.Cell(c.X, c.Y).ItemInfo = quiver;
    }
  }

  private void AddWeapons(int weaponCount, int armorCount, LevelInfo info, List<MazeCell> cells)
  {
    var armor = db.FindArmor(info);
    foreach (var n in GD.Range(armorCount))
    {
      if (cells.Count == 0 || armor.Count == 0) break;

      var c = TakeRandom(cells);
      c.ItemInfo = TakeRandom(armor);
    }

    var weapons = db.FindWeapons(info);
    foreach (var n in GD.Range(weaponCount))
    {
      if (cells.Count == 0 || armor.Count == 0) break;

      TakeRandom(cells).ItemInfo = TakeRandom(weapons);
    }
  }

  private void AddAmulets(List<MazeCell> cells)
  {
    var amulets = db.Items.Where(i => i.ItemType == ItemType.Armor).ToList();
    foreach (var n in GD.Range(rng.RandiRange(0, 3)))
    {
      if (cells.Count == 0 || amulets.Count == 0) break;

      var c = TakeRandom(cells);
      c.ItemInfo = TakeRandom(amulets);
    }
  }

  private void AddItems(LevelInfo info, List<MazeCell> cells)
  {
    var bags = 7 + rng.RandiRange(0, 3);
    var weapons = 7 + rng.RandiRange(1, 5);
    var armor = rng.RandiRange(0, 2);
    AddLoot(bags, info, cells);
    AddKey(info, cells);
    AddOther(cells);
    AddWeapons(weapons, armor, info, cells);
    AddAmulets(cells);
  }
  
  
}