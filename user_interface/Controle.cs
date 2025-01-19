using Godot;
using System.Linq;

public partial class Controle : HBoxContainer
{
	private Sprite2D[] emptyPeople;

	private Texture2D savePeopleTex;
	private Texture2D noSavePeopleTex;

	private ShaderMaterial shader;

	public override void _Ready()
	{
		savePeopleTex = GD.Load<Texture2D>("res://user_interface/people-count/person_ui.png");
		noSavePeopleTex = GD.Load<Texture2D>("res://user_interface/people-count/person_empty_ui.png");

		emptyPeople = GetChildren(true).Select(n => n.GetChild(0) as Sprite2D).ToArray();

		Reset();
		Global.Instance.ResetSavePeople();

		SetSignal();
	}

	private void SetSignal()
	{
		Callable iMSaveThePeople = new Callable(this, MethodName.IMSaveThePeople);
		Callable iMChangePeopleForOxygen = new Callable(this, MethodName.IMChangePeopleForOxygen);

		GameEvent.gameEvent.Connect(GameEvent.SignalName.IMSaveThePeople, iMSaveThePeople);
		GameEvent.gameEvent.Connect(GameEvent.SignalName.IMChangePeopleForOxygen, iMChangePeopleForOxygen);
	}

	public void IMSaveThePeople()
	{
		uint s = Global.Instance.SavePeopleCount;
		--s;

		Tween tween = CreateTween();
		tween.TweenProperty(emptyPeople[s], "scale", new Vector2(1.3f, 1.3f), .25f).SetTrans(Tween.TransitionType.Linear);
		tween.TweenCallback(Callable.From(() => emptyPeople[s].Texture = savePeopleTex));
		tween.TweenProperty(emptyPeople[s], "scale", new Vector2(1, 1), .25f).SetTrans(Tween.TransitionType.Linear);
		tween.TweenCallback(Callable.From(() => ChangeParamTexture(s)));
	}

	public void IMChangePeopleForOxygen()
	{
		uint s = Global.Instance.SavePeopleCount;

		Tween tween = CreateTween().SetEase(Tween.EaseType.InOut);

		emptyPeople
		.Where(s => s.Texture == savePeopleTex)
		.Reverse()
		.Select(s =>
		{

			tween.TweenProperty(s, "scale", new Vector2(1.3f, 1.3f), .1f);
			tween.TweenCallback(Callable.From(() => s.Texture = noSavePeopleTex));
			tween.TweenProperty(s, "scale", new Vector2(1, 1), .1f);
			tween.TweenCallback(Callable.From(() =>
			{
				shader = s.Material as ShaderMaterial;
				shader.SetShaderParameter("isFull", false);
			}));

			return s;
		})
		.ToArray();
	}

	private void ChangeParamTexture(uint offset)
	{
		shader = emptyPeople[offset].Material as ShaderMaterial;
		shader.SetShaderParameter("isFull", true);
		shader.SetShaderParameter("offset", offset);
	}

	private void Reset()
	{
		uint s = Global.Instance.SavePeopleCount;

		if (s == 0)
			return;

		emptyPeople
		.Select(s =>
		{
			s.Texture = noSavePeopleTex;
			shader = s.Material as ShaderMaterial;
			shader.SetShaderParameter("isFull", false);

			return s;
		})
		.ToArray();
	}

	private void ResetScene()
	{
		GameEvent.gameEvent.IMSaveThePeople -= IMSaveThePeople;
		GetTree().ReloadCurrentScene();
	}
}
