using PlayerControllerGlobal;

// NOTE: Recommend removing this script to simplify codebase.
//       The one function it defines can easily be defined in Driver.cs

public partial class AnimationPlayer : Godot.AnimationPlayer
{
	public void PlayCameraRotationOnDeath()
	{
		Play(Constants.PLAYERS_HEAD_ANIMATION_ON_DYING);
	}
}
