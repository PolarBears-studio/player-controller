using PolarBears.PlayerControllerAddon;

public partial class AnimationPlayer : Godot.AnimationPlayer
{
	public void PlayCameraRotationOnDeath()
	{
		Play(Constants.PLAYERS_HEAD_ANIMATION_ON_DYING);
	}
}
