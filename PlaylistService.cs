using Godot;

public partial class PlaylistService : AudioStreamPlayer
{
	public override void _Ready()
	{
		Callable setVolumeCal = new Callable(this, MethodName.SetVolumePlaylist);

		GameEvent.gameEvent.Connect(GameEvent.SignalName.GameOver, setVolumeCal);
	}

	private void SetVolumePlaylist()
	{
		VolumeDb = -20;
	}

}
