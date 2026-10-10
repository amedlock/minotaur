namespace minotaur.Source.model;

// what the player is doing inside the dungeon, GameMode tracks which screen is up
public enum PlayerState
{
  Idle, // default state
  Turning,
  Moving,
  Glance,
  Combat
}