#region

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using minotaur.Source.enemies;
using minotaur.Source.items;
using minotaur.Source.model;

#endregion

namespace minotaur.Source.dungeon.builder;

// sole purpose is to generate content and store in DungeonGrid
// then using that content, creates Godot Nodes for visuals
public partial class LevelBuilder : Node
{
  // walls and corners
  private readonly Vector3 _wallOffset = new(3f, 0, -3f);


  private readonly Dictionary<WallPost, Vector3> _wallPostOffset = new()
  {
    [WallPost.NW] = new Vector3(0, 0f, 0),
    [WallPost.NE] = new Vector3(3, 0f, 0),
    [WallPost.SE] = new Vector3(3, 0f, 3),
    [WallPost.SW] = new Vector3(0, 0f, 3)
  };

  private readonly Dictionary<Direction, Vector3I> _wallRotation = new()
  {
    [Direction.North] = new Vector3I(0, 180, 0),
    [Direction.East] = new Vector3I(0, 270, 0)
  };

  private PackedScene _blueGate;
  private PackedScene _cornerPrefab;
  private PackedScene _doorPrefab;
  private PackedScene _enemyPrefab;

  private PackedScene _greenGate;
  private PackedScene _itemPrefab;
  private PackedScene _ladderPrefab;
  private RandomNumberGenerator _rng = new();
  private PackedScene _tanGate;

  // prefabs  
  private PackedScene _wallPrefab;

  public override void _Ready()
  {
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

  public void Build(Dungeon dungeon, LevelInfo currentLevel, DungeonGrid grid)
  {
    dungeon.ClearAll();
    
    _rng.Seed = currentLevel.SeedNumber;
    foreach (var cell in grid.Cells)
    {
      var dungeonCell = dungeon.GetCell(cell.X, cell.Y);
      dungeonCell.MazeCell = cell;
      CreateWall(dungeonCell, cell.North, Direction.North);
      CreateWall(dungeonCell, cell.East, Direction.East);
      foreach (var wallPost in grid.WallPosts(cell.X, cell.Y))
      {
        AddCorner(wallPost, dungeonCell);
      }

      AddItem(dungeonCell, cell.ItemInfo);
      // AddEnemy(dungeonCell, cell.EnemyInfo);
      AddGate(dungeonCell, cell, currentLevel.LevelType);
    }
    dungeon.MuralColor = currentLevel.GateType;
  }
  

  private void AddEnemy(DungeonCell dc, EnemyInfo enemy)
  {
    if (dc.Enemy != null)
    {
      dc.RemoveChild(dc.Enemy);
      dc.Enemy.QueueFree();
      dc.Enemy = null;
    }

    if (enemy == null) return;

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
      var result = (Item)_itemPrefab.Instantiate();
      result.Info = itemInfo;
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

    if (cell.Gate == GateType.None) return;

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


  private void CreateWall(DungeonCell dest, WallType wallType, Direction direction)
  {
    var wallRot = _wallRotation[direction];
    var node = wallType switch
    {
      WallType.Door => (Node3D)_doorPrefab.Instantiate(),
      WallType.Wall => (Node3D)_wallPrefab.Instantiate(),
      _ => null
    };
    if (node == null) return;

    node.Position = _wallOffset;
    node.RotationDegrees = wallRot;
    node.Name = "Wall_" + wallType;
    dest.AddWall(node, wallType, direction);
  }

  public void AddCorner(WallPost wallPost, DungeonCell dest)
  {
    var corner = (Node3D)_cornerPrefab.Instantiate();
    corner.Name = "corner_" + nameof(wallPost);
    dest.AddChild(corner);
    corner.Position = _wallPostOffset[wallPost];
  }
}