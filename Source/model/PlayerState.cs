namespace minotaur.Source.model;

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