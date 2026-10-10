using Godot;

namespace minotaur.Source.model;

public enum Direction
{
  North, // 0,1
  South, // 0,-1
  East, // 1,0
  West // -1,0
}

public static class DirectionExtensions
{
  // grid step of one cell in this direction
  public static Vector2I Offset(this Direction dir)
  {
    return dir switch
    {
      Direction.North => new Vector2I(0, 1),
      Direction.South => new Vector2I(0, -1),
      Direction.East => new Vector2I(1, 0),
      _ => new Vector2I(-1, 0)
    };
  }

  public static Direction Opposite(this Direction dir)
  {
    return dir switch
    {
      Direction.North => Direction.South,
      Direction.South => Direction.North,
      Direction.East => Direction.West,
      _ => Direction.East
    };
  }

  // facing angle in degrees, as a Godot Y rotation
  public static int Degrees(this Direction dir)
  {
    return dir switch
    {
      Direction.North => 0,
      Direction.West => 90,
      Direction.South => 180,
      _ => 270
    };
  }

  public static Direction FromDegrees(int degrees)
  {
    return Mathf.PosMod(degrees, 360) switch
    {
      0 => Direction.North,
      90 => Direction.West,
      180 => Direction.South,
      _ => Direction.East
    };
  }
}
