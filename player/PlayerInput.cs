using System;
using Godot;
using minotaur.dungeon;
using minotaur.hud;

namespace minotaur.player;

// Handle player movement, this is called from Player
public partial class PlayerInput : Node3D
{
	private const float MoveTime = 0.75f;
	private const float TurnTime = 0.5f;
	private const float GlanceTime = 0.25f;

	private int _glanceAmount = 0;

	private int _dir = 270;
	private Vector2I _prevCoord;
	private bool _canRetreat = false;

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
		
		if (_glanceAmount != 0)
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
		_glanceAmount = 0;
	}

	public void MoveBack()
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


	public void MoveForward()
	{
		var wall = _player.WallAhead;
		if (wall != null && wall.Blocked)
		{
			return;
		}

		var cell = _player.CellAhead;
		if (cell!=null && cell.Enemy!=null)
		{
			_player.StartCombat(cell, true);
			return;
		}
		_prevCoord = _player.Coord;
		_canRetreat = true;
		var pos = _player.Position;
		var pvec = _player.Transform.Basis.Z * -3;
		_player.PlayerState = PlayerState.Moving;
		var tween =  CreateTween().TweenProperty(_player, "position", pos + pvec, MoveTime);
		tween.Finished += () =>
		{
			_hud.UpdateAll();
			_player.PlayerState = PlayerState.Idle;
		};
	}


	// ensure amount is only (0, 90, 180, 270)
	private int FixRot(int amount) => ((amount / 90) % 4) * 90;

	public void TurnPlayer(int amount)
	{
		amount = FixRot(amount);
		var crot = _hud.Compass.RotationDegrees;
		var tween = CreateTween();
		var rot = _player.Dir + amount;
		_player.PlayerState = PlayerState.Turning;
		tween.Finished += () =>
		{
			_dir += amount;
			var rotVec = _player.RotationDegrees;
			rotVec.Y = Mathf.PosMod(rotVec.Y, 360f);
			_player.RotationDegrees = rotVec;
			_player.PlayerState = PlayerState.Idle;
		};
		
		tween.TweenProperty(_player, "rotation_degrees:y", rot, TurnTime);
		tween.Parallel().TweenProperty(_hud.Compass, "rotation_degrees", crot - amount, TurnTime);
	}


	public void Glance(int amount)
	{
		_glanceAmount = FixRot(amount);
		var rot = _player.Dir + _glanceAmount;
		_player.PlayerState = PlayerState.Turning;
		var tween = CreateTween().TweenProperty(_player, "rotation_degrees:y", rot, TurnTime);
		tween.Finished += () =>
		{
			var rotVec = _player.RotationDegrees;
			rotVec.Y = Mathf.PosMod(rot, 360);
			_player.RotationDegrees = rotVec;
			_player.PlayerState = PlayerState.Glance;
		};
	}

	public void UnGlance()
	{
		var rot = _player.Dir - _glanceAmount;
		var tween = CreateTween();
		_player.PlayerState = PlayerState.Turning;
		tween.Finished += () =>
		{
			_glanceAmount = 0;
			var rotVec = _player.RotationDegrees;
			rotVec.Y = Mathf.PosMod(rot, 360);
			_player.RotationDegrees = rotVec;
			_player.PlayerState = PlayerState.Idle;
		};
		tween.TweenProperty(_player, "rotation_degrees:y", rot, GlanceTime);
	}

	public void Flee()
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