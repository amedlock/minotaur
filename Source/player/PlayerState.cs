namespace minotaur.Source.player;

public enum PlayerState
{
  Idle, // default state
  Turning,
  Moving,
  Glance,
  ViewMap,
  Combat,
  Wait,
  Won,
  Lost
}