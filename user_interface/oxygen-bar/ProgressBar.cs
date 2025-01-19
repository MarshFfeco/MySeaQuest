using System;
using Godot;

public partial class ProgressBar : TextureProgressBar
{
	public Tween tween;

	public ShaderMaterial label;
	public ShaderMaterial mySelf;

	public AudioStreamOggVorbis _oxygenFull;
	public AudioStreamOggVorbis _oxygenAlert;

	Player player;

	public override void _Ready()
	{
		label = GetNode<Label>("Label").Material as ShaderMaterial;
		mySelf = Material as ShaderMaterial;

		_oxygenFull = GD.Load<AudioStreamOggVorbis>("res://user_interface/oxygen-bar/full_oxygen_alert.ogg");
		_oxygenAlert = GD.Load<AudioStreamOggVorbis>("res://user_interface/oxygen-bar/oxygen_alert.ogg");

		Callable oxFull = new Callable(this, MethodName.FullOxygen);
		Connect(SignalName.ValueChanged, oxFull);


		label.SetShaderParameter("strength", 1.0f);
		mySelf.SetShaderParameter("strength", 1.0f);

		player = GetTree().GetFirstNodeInGroup("Player") as Player;
		Global.Instance.progressBar = this;
		Global.Instance.ResetOxygen();
	}

	public void DecreaseOxygen()
	{
		Value = Global.Instance.Oxygen;

		SetParamShader();

		if (Global.Instance.Oxygen <= 15)
			player.ChangeParamShader();
	}

	private void SetParamShader()
	{
		float strength = Mathf.Lerp(1, 5, (100 - Global.Instance.Oxygen) * 0.01f);

		if (strength < 3.5f)
		{
			label.SetShaderParameter("strength", 1);
			mySelf.SetShaderParameter("strength", 1);
			return;
		}

		if (strength == 3.5f)
		{
			label.SetShaderParameter("strength", 20);
			mySelf.SetShaderParameter("strength", 20);
			SoundManager.Instance.ConfigSound(_oxygenAlert);
			return;
		}

		label.SetShaderParameter("strength", 5);
		mySelf.SetShaderParameter("strength", 5);
	}

	public void CreasyOxygen()
	{
		player.OxygenTime.Paused = true;
		SetParamShader();
		tween = CreateTween();

		tween.TweenProperty(this, "value", Global.Instance.Oxygen, 1f).SetTrans(Tween.TransitionType.Linear);
		tween.TweenCallback(Callable.From(() =>
		{
			Value = Global.Instance.Oxygen;

			player.OxygenTime.Paused = false;
		}));
	}

	private void FullOxygen(float value)
	{
		if (value > 99)
		{
			SoundManager.Instance.ConfigSound(_oxygenFull);
		}
	}
}
