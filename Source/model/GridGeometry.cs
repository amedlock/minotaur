using Godot;

namespace minotaur.Source.model;

// single home for grid size and grid to world conversions
// grid +X is East, grid +Y is North, and North is world -Z
public static class GridGeometry
{
  public const float CellSize = 3f;
  public const int GridWidth = 12;
  public const int GridHeight = 12;

  // where a wall is placed within its cell, before rotation
  public static readonly Vector3 WallOffset = new(CellSize, 0, -CellSize);

  // position of a cell relative to the grid origin
  public static Vector3 CellOrigin(Vector2I coord)
  {
    return new Vector3(coord.X * CellSize, 0, -coord.Y * CellSize);
  }

  // center of a cell at the given height, relative to the cell
  public static Vector3 CellCenter(float height)
  {
    return new Vector3(CellSize / 2, height, -CellSize / 2);
  }

  // position of a corner post relative to the cell
  public static Vector3 WallPostOffset(WallPost wallPost)
  {
    return wallPost switch
    {
      WallPost.NE => new Vector3(CellSize, 0, 0),
      WallPost.SE => new Vector3(CellSize, 0, CellSize),
      WallPost.SW => new Vector3(0, 0, CellSize),
      _ => Vector3.Zero
    };
  }
}
