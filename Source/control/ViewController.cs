#region

using Godot;
using minotaur.Source.model;
using minotaur.Source.views;

#endregion

namespace minotaur.Source.control;

// listens for changes to the model and refreshes the views that show them
public partial class ViewController : Node
{
  [Export] private GameModel _gameModel;

  [Export] private Dungeon _dungeon;

  [Export] private Player _player;

  [Export] private Hud _hud;

  [Export] private MapView _mapView;

  public override void _Ready()
  {
    var playerData = _gameModel.PlayerData;
    playerData.StatsChanged += OnStatsChanged;
    playerData.InventoryChanged += OnInventoryChanged;

    _gameModel.PlayerMoved += OnPlayerMoved;
    _gameModel.LevelChanged += OnLevelChanged;
    _gameModel.ItemChanged += OnItemChanged;
    _gameModel.EnemyKilled += OnEnemyKilled;
  }

  private void OnStatsChanged()
  {
    _hud.UpdateStats();
  }

  private void OnInventoryChanged()
  {
    _hud.UpdatePack();
  }

  private void OnPlayerMoved()
  {
    _player.Update();
    // the item at the player's feet is shown on the hud
    _hud.UpdatePack();
  }

  private void OnLevelChanged()
  {
    _dungeon.BuildLevel();
    _mapView.UpdateMap(_gameModel.CurrentLevel);
    _player.Update();
    _hud.UpdateAll();
  }

  private void OnItemChanged(Vector2I coord)
  {
    _dungeon.UpdateItem(coord);
    if (coord == _gameModel.PlayerCoord)
    {
      _hud.UpdatePack();
    }
  }

  private void OnEnemyKilled(Vector2I coord)
  {
    _dungeon.KillEnemy(coord);
  }
}
