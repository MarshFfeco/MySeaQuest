using Godot;

public partial class SoundManager : Node
{
	public static SoundManager Instance { get; private set; }
	public AudioStreamPlayer audioStreamPlay { get; private set; }

	private AudioStream[] _musics { get; set; }

	public override void _Ready()
	{
		Instance = this;
	}

	public void ConfigSound(AudioStreamWav sound)
	{
		InstanceAudio();

		audioStreamPlay.Stream = sound;

		PlayAudio();
	}

	public void ConfigSound(AudioStreamOggVorbis sound)
	{
		InstanceAudio();
		audioStreamPlay.ProcessMode = ProcessModeEnum.Always;
		audioStreamPlay.Stream = sound;

		PlayAudio();
	}

	private void InstanceAudio()
	{
		audioStreamPlay = new AudioStreamPlayer();
		audioStreamPlay.Bus = AudioServer.GetBusName(1);
	}

	private void PlayAudio()
	{
		AddChild(audioStreamPlay);

		audioStreamPlay.Play();

		audioStreamPlay.Finished += audioStreamPlay.QueueFree;
	}
}
