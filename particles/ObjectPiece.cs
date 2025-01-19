using Godot;

public partial class ObjectPiece : Sprite2D
{
	public Vector2 _velocity = Vector2.Right;

	[Export]
	private float _speed = 150.0f;
	[Export(PropertyHint.Range, "0,20,0.5")]

	private float _speedRotate = 5.0f;

	public override void _Ready()
	{
		_velocity += _velocity.Rotated((float)GD.RandRange(-1.0f, 1.0f)) * 2.0f;
		_speedRotate = (float)GD.RandRange(-_speedRotate, _speedRotate);

		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(this, "scale", Vector2.Zero, 1.0f);
		tween.TweenCallback(Callable.From(QueueFree));
	}

	public override void _PhysicsProcess(double delta)
	{
		_velocity = _velocity.MoveToward(Vector2.Zero, (float)delta * _speed);

		GlobalPosition += _velocity;
		Rotation += _speedRotate * (float)delta;
	}
}
