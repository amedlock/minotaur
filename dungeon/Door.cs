namespace minotaur.dungeon;

public partial class Door : Wall
{
  public override void _EnterTree()
  {
    AddToGroup("doors");
  }
}