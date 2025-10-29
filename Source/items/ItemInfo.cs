using Godot;

namespace minotaur.Source.items;

public class ItemInfo
{
  public ItemType ItemType;
  public string Name;
  public int MinDepth = 1;
  public Color Color;
  public Rect2I Image;
  public Color color = Colors.White;
  private int _minLevel = 1;
  public int Uses = 10 ; // min uses before item could break
  public bool NeedsKey = false;
  public int Stat1;
  public int Stat2;
  public Vector3 Offset = Vector3.Zero;//   # Vector3 offset for items in maze
}