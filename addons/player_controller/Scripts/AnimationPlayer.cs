using ExodusGlobal;

public partial class AnimationPlayer : Godot.AnimationPlayer
{
	public override void _Ready() { }

	public void PlayCameraRotationOnDeath()
	{
		Play(Constants.PLAYERS_HEAD_ANIMATION_ON_DYING);
	}
}
