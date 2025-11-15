#region

using System.Collections.Generic;
using minotaur.Source.items;

#endregion

namespace minotaur.Source.player;

// named PlayerData to avoid confusion with Player Node
public class PlayerData
{
  public int Arrows;
  public int Food;

  public  int Gold;
  public  int Health;
  public  int HealthMax;
  
  public  int Mind;
  public int MindMax;

  private ItemInfo helmet;
  private ItemInfo breastplate;
  private ItemInfo hauberk;
  
  
  private ItemInfo leftHand;
  private ItemInfo rightHand;
  

  private bool needsRest;
  private bool resurrected;

  private ItemInfo ring;

  private List<ItemInfo> slots = [null, null, null, null, null, null, null, null, null];
}