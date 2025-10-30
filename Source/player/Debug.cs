using Godot;
using minotaur.Source.dungeon;

namespace minotaur.Source.player;

//  debug output window
public partial class Debug :Label
{
  private Player _player;
  private MainGame _game;
  private Dungeon _dungeon;
  private DungeonGrid _grid;
  
  public override void _Ready()
  {
    _game = (MainGame)FindParent("Game") as MainGame;
    _player = _game.FindChild("Player") as Player;
    _dungeon = _game.FindChild("Dungeon") as Dungeon;
    _grid = _dungeon.FindChild("Grid") as DungeonGrid;
    SetProcess(false);
    Visible = false;
  }
 
  
  public override void _Process(double delta)
  {
    if (Input.IsActionJustPressed("debug"))
    {
      Visible = !Visible;
      SetProcess(Visible);
    }

    if (!Visible)
    {
      return;
    }

    UpdateLabel();
  }


  private void UpdateLabel()
  {
    // var coord = player.get_coord()
    // var cell_ahead = player.cell_ahead()
    // var cell = grid.get_cell(coord.x, coord.y)
    // var info = [
    //   "Cell: %s" % str(cell.debug_info()),
    //   "World pos: %s" % [str(player.get_coord())],
    //   "Facing: %s %s" % [str(player.get_facing()), str(player.dir_name())],
    //   "Cell Ahead: %s" % str(cell_ahead.debug_info() if cell_ahead else "")
    // ]
    // var item = player.item_at_feet()
    // if item:
    // info.append("Item: %s/%s pwr:%s" % [item.kind, item.name, item.power])
		  //
    // var wall = player.wall_ahead()
    // if wall:
    // info.append("Wall: %s" % wall.name )
    // self.text = join_str(info)
  }
}