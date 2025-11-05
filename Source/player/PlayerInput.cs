using Godot;
using minotaur.Source.dungeon;
using minotaur.Source.hud;

namespace minotaur.Source.player;

// Handle player movement, this is called from Player
public partial class PlayerInput : Node3D
{
	private const float MoveTime = 0.75f;
	private const float TurnTime = 0.3f;
	private const float GlanceTime = 0.25f;

	private int _dir = 270;
	private Vector2I _prevCoord;
	private bool _canRetreat ;

	Player _player;
	Dungeon _dungeon;
	Hud _hud;


	public override void _Ready()
	{
		_player = GetParent() as Player;
		_dungeon = _player.GetParent<Dungeon>();
		_hud = _player.GetNode("Camera3D/HUD") as Hud;
	}

	public override void _Input(InputEvent @event)
	{
		if (_player.PlayerState==PlayerState.Glance)
		{
			if (@event.IsActionReleased("look_left") || @event.IsActionReleased("look_right"))
			{
				UnGlance();
			}
		}
	}

	public override void _Process(double delta)
	{
		
		switch(_player.PlayerState)
		{
			case PlayerState.Idle:
				HandleIdle();
				break;
			case PlayerState.Glance:
				HandleGlance();
				break;
			default:
				return;
		}
	}

	private void HandleIdle()
	{
		if (Input.IsActionJustPressed("forward"))
		{
			MoveForward();
			return;
		}

		if (Input.IsActionJustPressed("left"))
		{
			TurnPlayer(90);
			return;
		}
		if (Input.IsActionJustPressed("right"))
		{
			TurnPlayer(-90);
			return;
		}
		if (Input.IsActionJustPressed("back"))
		{
			MoveBack();
			return;
		}
		if (Input.IsActionJustPressed("look_left"))
		{
			Glance(90);
			return;
		}
		if (Input.IsActionJustPressed("look_right"))
		{
			Glance(-90);
			return;
		}
		if (Input.IsActionJustPressed("open"))
		{
			_player.OpenDoor();
			return;
		}

		if (Input.IsActionJustPressed("rest"))
		{
			_player.Rest();
			return;
		}
		if (Input.IsActionJustPressed("use"))
		{
			_player.UseOrTakeItem();
			return;
		}
		if (Input.IsActionJustPressed("descend"))
		{
			_player.UseExit();
		}
	}
	

	private void HandleGlance()
	{
		if (!(Input.IsActionPressed("look_left") || Input.IsActionPressed("look_right")))
		{
			UnGlance();
		}		
	}


	public void Reset()
	{
	}

	protected void MoveBack()
	{
		var wall = _player.WallBehind;
		if (wall!=null && wall.Blocked)
		{
			return;
		}

		var pos = _player.Position;
		var pvec = _player.Transform.Basis.Z * -3f;
		_player.PlayerState = PlayerState.Moving;
		var tweener = CreateTween().TweenProperty(this, "position", pos - pvec, MoveTime);
		tweener.Finished += () =>
		{
			_hud.UpdateAll();
			_player.PlayerState = PlayerState.Idle;
		};
	}


	protected void MoveForward()
	{
		var wall = _player.WallAhead;
		if ( (wall is Wall or Door { Blocked: true }))
		{
			return;
		}
		var nextCell = _player.CellAhead;
		if (nextCell is { Enemy: not null })
		{
			_player.StartCombat(nextCell, true);
			return;
		}
		_prevCoord = _player.Coord;
		_canRetreat = true;
		var pos = _player.Position;
		var delta = _player.Transform.Basis.Z * -3;
		_player.PlayerState = PlayerState.Moving;
		var tween =  CreateTween().TweenProperty(_player, "position", pos + delta, MoveTime);
		tween.Finished += () =>
		{
			_player.PlayerState = PlayerState.Idle;
			_player.Coord = nextCell.Coord;
			_hud.UpdateAll();
		};
	}


	// ensure amount is only (0, 90, 180, 270)
	private int FixRot(int amount) => ((amount / 90) % 4) * 90;

	protected void TurnPlayer(int amount)
	{
		var crot = _hud.Compass.RotationDegrees;
		var rot = _player.Dir + amount;
		_player.PlayerState = PlayerState.Turning;
		var tween = CreateTween();
		tween.Finished += () =>
		{
			_player.PlayerState = PlayerState.Idle;
			_player.Dir = rot;
		};
		tween.TweenProperty(_player, "Dir", rot, TurnTime);
		tween.Parallel().TweenProperty(_hud.Compass, "rotation_degrees", crot - amount, TurnTime);
	}


	protected void Glance(int amount)
	{
		var rot = _player.Dir + amount;
		_player.PlayerState = PlayerState.Turning;
		var tween = CreateTween().TweenProperty(_player, "rotation_degrees:y", rot, TurnTime);
		tween.Finished += () =>
		{
			_player.PlayerState = PlayerState.Glance;
		};
	}

	protected void UnGlance()
	{
		var tween = CreateTween();
		_player.PlayerState = PlayerState.Turning;
		tween.Finished += () =>
		{
			_player.PlayerState = PlayerState.Idle;
		};
		tween.TweenProperty(_player, "rotation_degrees:y", _player.Dir, GlanceTime);
	}

	protected void Flee()
	{
		if (_canRetreat)
		{
			_canRetreat = false;
			_player.Coord = _prevCoord;
		}
		else
		{
			MoveBack();
		}
	}
	

}