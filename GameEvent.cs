using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class GameEvent : Node
{
	public static GameEvent gameEvent { get; private set; }

	#region  Signals
	[Signal]
	public delegate void IMSaveThePeopleEventHandler();

	[Signal]
	public delegate void IMChangePeopleForOxygenEventHandler();

	[Signal]
	public delegate void FollowPlayerEventHandler(float yPosition);

	[Signal]
	public delegate void UpdateMyPointsEventHandler();

	[Signal]
	public delegate void GameOverEventHandler();

	[Signal]
	public delegate void PauseEnemyEventHandler(bool isPause);
	#endregion

	public override void _Ready()
	{
		gameEvent = this;
	}
}
