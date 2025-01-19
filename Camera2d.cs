using Godot;

public partial class Camera2d : Camera2D
{
	private const float xPos = 213;

	private Vector2 target;
	private Vector2 Target { get => target; set => target = value; }

	public override void _Ready()
	{
		GameEvent.gameEvent.FollowPlayer += FollowPlayer;
	}

	public override void _PhysicsProcess(double delta)
	{
		GlobalPosition = target;
	}

	private void FollowPlayer(float yPosition)
	{
		Target = new Vector2(xPos, yPosition);
	}
}
