#region

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using minotaur.Source.dungeon;
using minotaur.Source.items;

#endregion

namespace minotaur.Source.model;

public class LevelRandomizer
{
  RandomNumberGenerator rng;
  GameDb gameDb;
  DungeonGrid grid;

  public LevelRandomizer(uint seed, DungeonGrid grid, GameDb gameDb)
  {
    rng = new RandomNumberGenerator();
    this.grid = grid;
    this.gameDb = gameDb;
  }

  public void BuildLevel(LevelInfo levelInfo)
  {
    rng.Seed = levelInfo.SeedNumber;
    grid.GateType = levelInfo.GateType;
    BuildMazePrim(levelInfo);
    AddMoreDoors();
    AddExit(levelInfo);
    AddGates(levelInfo);
    var emptyCells = grid.EmptyCells.Where(cell => !IsOuterMaze(cell)).ToList();

    AddEnemies(levelInfo, emptyCells);
    AddItems(levelInfo, emptyCells);
    AddMinotaur(levelInfo, emptyCells);
  }


  private MazeCell ChooseRandom(List<MazeCell> items)
  {
    return items.Count == 0 ? null : items[rng.RandiRange(0, items.Count - 1)];
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
      grid.Cell(cell.X, cell.Y + 1),
      grid.Cell(cell.X, cell.Y - 1),
      grid.Cell(cell.X + 1, cell.Y),
      grid.Cell(cell.X - 1, cell.Y)
    ];
    return items.Where(x => x != null);
  }

  private bool AdjacentToAny(MazeCell cell, List<MazeCell> other)
  {
    return other.Any(c => c.AdjacentTo(cell));
  }

  private void BuildMazePrim(LevelInfo levelInfo)
  {
    grid.Reset();
    CreateCorridor();
    AddOuterDoors();

    var notMaze = grid.Cells.Where(cell => !IsOuterMaze(cell)).ToList();
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
    return mc.X == 0 || mc.Y == 0 || mc.X == grid.Width - 1 || mc.Y == grid.Height - 1;
  }

  // clear walls to create outer corridor
  private void CreateCorridor()
  {
    foreach (var cell in grid.Cells)
    {
      var x = cell.X;
      var y = cell.Y;
      cell.Used = IsOuterMaze(cell);
      if (x == 0)
      {
        cell.North = WallType.Empty;
        if (y == 0 || y == grid.Height - 1) cell.East = WallType.Empty;
      }
      else if (y == 0)
      {
        cell.East = WallType.Empty;
        if (x == grid.Width - 1) cell.North = WallType.Empty;
      }
      else if (y == grid.Height - 1)
      {
        cell.North = WallType.Empty;
        cell.East = WallType.Empty;
      }
      else if (x == grid.Width - 1)
      {
        cell.East = WallType.Empty;
        cell.North = WallType.Empty;
      }
    }
  }

  private void AddOuterDoors()
  {
    grid.Cell(3, 0).North = WallType.Door;
    grid.Cell(8, 0).North = WallType.Door;
    grid.Cell(3, grid.Height - 2).North = WallType.Door;
    grid.Cell(8, grid.Height - 2).North = WallType.Door;
    grid.Cell(0, 3).East = WallType.Door;
    grid.Cell(0, 8).East = WallType.Door;
    grid.Cell(grid.Width - 2, 3).East = WallType.Door;
    grid.Cell(grid.Width - 2, 8).East = WallType.Door;
  }

  private bool CheckWall(int x, int y, Direction dir)
  {
    var cell = grid.Cell(x, y);
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
      grid.Cell(3, 3).East = WallType.Door;
    }

    if (CheckWall(8, 4, Direction.North))
    {
      grid.Cell(8, 4).North = WallType.Door;
    }

    if (CheckWall(3, 8, Direction.North))
    {
      grid.Cell(3, 8).North = WallType.Door;
    }

    if (CheckWall(8, 9, Direction.North))
    {
      grid.Cell(8, 9).North = WallType.Door;
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
    var cell = grid.Cell(exit);
    cell.ItemInfo = gameDb.FindItem("ladder");
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
      grid.Cell(grid.Width - 1, 0).Gate = gates[0];
      grid.Cell(0, grid.Height - 1).Gate = gates[1];
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
    var enemies = gameDb.FindEnemies(info);
    var sorted = enemies.OrderBy(_ => rng.Randi()).ToList();
    if (sorted.Count == 0) throw new Exception("No enemies allowed for placement");

    foreach (var n in GD.Range(num))
    {
      if (cells.Count == 0) return;

      var target = TakeRandom(cells);
      var monster = sorted[n % sorted.Count];
      grid.Cell(target.X, target.Y).EnemyInfo = monster;
    }
  }

  private void AddMinotaur(LevelInfo currentLevel, List<MazeCell> cells)
  {
    if (currentLevel.HasMinotaur)
    {
      var cell = TakeRandom(cells);
      cell.EnemyInfo = gameDb.FindEnemy("Minotaur");
    }
  }


  private void AddKey(LevelInfo info, List<MazeCell> cells)
  {
    if (cells.Count == 0) return;

    var keys = gameDb.Items.Where(i => i.ItemType == ItemType.Key && !info.HasItem(i)).ToList();
    if (keys.Count == 0)
    {
      GD.Print("Warning no 'key' items found for level ", info);
      return;
    }

    var c = ChooseRandom(cells);
    grid.Cell(c.X, c.Y).ItemInfo = ChooseRandom(keys);
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

    var items = gameDb.Items.Where(i => i.ItemType == ItemType.Container && names.Contains(i.Name)).ToList();

    foreach (var unused in GD.Range(num))
    {
      if (cells.Count == 0 || items.Count == 0) break;

      var target = TakeRandom(cells);
      grid.Cell(target.X, target.Y).ItemInfo = ChooseRandom(items);
    }
  }

  private void AddMoney(int num, LevelInfo info, List<MazeCell> cells)
  {
    var allowed = gameDb.Items.Where(i => i.ItemType == ItemType.Money).ToList();
    if (allowed.Count > 0)
      foreach (var n in GD.Range(num))
      {
        if (cells.Count == 0) return;

        var c = TakeRandom(cells);
        grid.Cell(c.X, c.Y).ItemInfo = ChooseRandom(allowed);
      }
  }

  private void AddOther(List<MazeCell> cells)
  {
    var food = gameDb.FindItem("food");
    var foodCount = rng.RandiRange(1, 3);
    foreach (var unused in GD.Range(foodCount))
      if (cells.Count > 0)
      {
        var c = TakeRandom(cells);
        grid.Cell(c.X, c.Y).ItemInfo = food;
      }
      else
      {
        return;
      }

    var quiver = gameDb.FindItem("quiver");
    foreach (var unused in GD.Range(rng.RandiRange(1, 3)))
    {
      if (cells.Count == 0) return;

      var c = TakeRandom(cells);
      grid.Cell(c.X, c.Y).ItemInfo = quiver;
    }
  }

  private void AddWeapons(int weaponCount, int armorCount, LevelInfo info, List<MazeCell> cells)
  {
    var armor = gameDb.FindArmor(info);
    foreach (var n in GD.Range(armorCount))
    {
      if (cells.Count == 0 || armor.Count == 0) break;

      var c = TakeRandom(cells);
      c.ItemInfo = TakeRandom(armor);
    }

    var weapons = gameDb.FindWeapons(info);
    foreach (var n in GD.Range(weaponCount))
    {
      if (cells.Count == 0 || armor.Count == 0) break;

      TakeRandom(cells).ItemInfo = TakeRandom(weapons);
    }
  }

  private void AddAmulets(List<MazeCell> cells)
  {
    var amulets = gameDb.Items.Where(i => i.ItemType == ItemType.Armor).ToList();
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