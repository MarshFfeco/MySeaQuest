using System;
using Godot;

public partial class GameOverScreen : Control
{
	private Label score;
	private Label bestScore;

	private Node gameplay;

	private AudioStreamOggVorbis _soundGameOver;

	private string oldText = "000000";
	private string OldText
	{
		get => oldText;
		set => oldText = value;
	}

	private Button start;

	public override void _Ready()
	{
		_soundGameOver = GD.Load<AudioStreamOggVorbis>("res://player/game_over.ogg");

		start = GetNode<Button>("Button");
		start.Pressed += () => GetTree().ReloadCurrentScene(); ;

		gameplay = GetOwner().GetNode<Node>("GamePlay");

		score = GetNode<Label>("Scores/ScoreLabel");
		bestScore = GetNode<Label>("Scores/BestScore");

		Visible = false;

		Callable cal = new Callable(this, MethodName.GameOver);
		GameEvent.gameEvent.Connect(GameEvent.SignalName.GameOver, cal);
	}

	private void DisableGame()
	{
		GameEvent.gameEvent.EmitSignal(GameEvent.SignalName.PauseEnemy, true);
	}

	private void GameOver()
	{
		Global.Instance.BestCoins = Global.Instance.CurrentCoins;

		CallDeferred(MethodName.DisableGame);

		Callable cal = new Callable(this, MethodName.AnimateNumberPoint);
		Callable calBest = new Callable(this, MethodName.AnimateBestNumberPoint);

		Tween tween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
		tween.TweenProperty(this, "visible", true, 1).SetDelay(.5f);
		SoundManager.Instance.ConfigSound(_soundGameOver);
		tween.TweenMethod(cal, int.Parse(oldText), Global.Instance.CurrentCoins, 0.5f);
		tween.TweenMethod(calBest, Global.Instance.OldBestCoins, Global.Instance.BestCoins, 0.5f);
	}

	private void AnimateNumberPoint(int newValue)
	{
		string newText = "000000" + newValue.ToString();
		string formatedText = newText.Remove(0, Math.Abs(newText.Length - 6));

		score.Text = "Score: " + formatedText;
	}

	private void AnimateBestNumberPoint(int newValue)
	{
		string newText = "000000" + newValue.ToString();
		string formatedText = newText.Remove(0, Math.Abs(newText.Length - 6));

		bestScore.Text = "Best Score: " + formatedText;
	}
}
