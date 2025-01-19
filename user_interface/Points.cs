using System;
using Godot;

public partial class Points : Label
{
	private string oldText = "000000";
	private string OldText
	{
		get => oldText;
		set => oldText = value;
	}

	public override void _Ready()
	{
		Callable cal = new Callable(this, MethodName.UpdateCount);
		GameEvent.gameEvent.Connect(GameEvent.SignalName.UpdateMyPoints, cal);
	}

	private void UpdateCount()
	{
		Callable cal = new Callable(this, MethodName.AnimateNumberPoint);

		Tween tween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);

		tween.TweenProperty(this, nameof(Position).ToLower(), Vector2.Up * 3.0f, 0.1f).AsRelative();

		tween.TweenMethod(cal, int.Parse(oldText), Global.Instance.CurrentCoins, 0.5f);
		tween.TweenProperty(this, nameof(Position).ToLower(), Vector2.Up * 3.0f * -1.0f, 0.1f).AsRelative();

		oldText = Text;
	}

	private void AnimateNumberPoint(int newValue)
	{
		string newText = "000000" + newValue.ToString();
		string formatedText = newText.Remove(0, Math.Abs(newText.Length - 6));

		Text = formatedText;
	}
}
