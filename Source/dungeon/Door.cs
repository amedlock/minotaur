using Godot;

namespace minotaur.Source.dungeon;

public partial class Door : Node3D
{
  private AnimationPlayer _anim;
  private bool _raised;

  public bool Blocked => !_raised;
  public bool Moving => _anim.IsPlaying();

  
  public override void _Ready()
  {
    _anim = GetNode<AnimationPlayer>("anim");
    AddToGroup("doors");
    _anim.AnimationFinished += DoorFinish;
  }

  public void Activate()
  {
    if (!_anim.IsPlaying())
    {
      if (_raised)
      {
        _anim.Play("Lower");
      }
      else
      {
        _anim.Play("Raise");
      }
    }
  }

  public void DoorFinish(StringName which)
  {
    _raised = which=="Raise";
  }
  
}