
using Godot;
using SeaQuest;

public partial class People : CharacterBody2D
{
	[Export]
	public float Speed = 28.0f;
	[Export]
	public float Amplitude = .2f;

	private Vector2 _velocity;

	private Vector2 _direction = Vector2.Right;
	public Vector2 Direction { get => _direction; set => _direction = value; }

	private Area2D area;

	[Export] private AnimatedSprite2D sprite;

	private AudioStreamOggVorbis _save;
	private AudioStreamOggVorbis _death;

	public override void _Ready()
	{
		_save = GD.Load<AudioStreamOggVorbis>("res://person/saving_person.ogg");
		_death = GD.Load<AudioStreamOggVorbis>("res://person/person_death.ogg");

		sprite = GetNode<AnimatedSprite2D>(nameof(AnimatedSprite2D));
		sprite.FlipH = Direction.X > 0 ? false : true;

		area = GetNode<Area2D>(nameof(Area2D));
		area.AreaEntered += HitFilter;
	}

	private void ScreenExited()
	{
		SoundManager.Instance.ConfigSound(_death);
		QueueFree();
	}

	private void HitFilter(Area2D area)
	{
		var grupos = area.GetOwner().GetGroups();

		if (grupos.Contains("Player") && Global.Instance.SavePeopleCount < Global.Instance.TotalPeople)
		{
			SoundManager.Instance.ConfigSound(_save);
			QueueFree();
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		_velocity = Direction.Normalized() * Speed;

		Velocity = _velocity;
		MoveAndSlide();

		if (GlobalPosition.X < Limits.Left || GlobalPosition.X > Limits.Right)
			ScreenExited();
	}
}
