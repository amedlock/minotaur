using System;

namespace minotaur.Source.dungeon;

public enum Direction
{
  [Vector(0,-1)]
  North,
  [Vector(0,1)]
  South, 
  
  [Vector(1,0)]
  East, 
  [Vector(-1,0)]
  West
}

public class VectorAttribute(int x, int y) : Attribute
{
  public int Y => y;
  public int X => x;
}