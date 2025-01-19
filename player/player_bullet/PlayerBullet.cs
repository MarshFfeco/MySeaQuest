using System;
using Godot;

public partial class PlayerBullet : CharacterBody2D
{
	[ExportGroup("Velocity Bullet")]
	[Export]
	private float _maxSpeed = 500;
	public float MaxSpeed { get => _maxSpeed; }

	[Export]
	private float _speed = 400;
	public float Speed { get => _speed; }

	private Vector2 _direction = Vector2.Zero;
	public Vector2 Direction { private get { return _direction; } set { _direction = value; } }

	private VisibleOnScreenNotifier2D _visible2D;
	private VisibleOnScreenNotifier2D Visible2D { get { return _visible2D; } set { _visible2D = value; } }

	private Player _player;
	public Player Player { private get => _player; set { _player = value; } }

	public AnimatedSprite2D _sprite;
	public AnimatedSprite2D Sprite { get => _sprite; set { _sprite = value; } }

	private Area2D _areaBullet;
	public Area2D AreaBullet { get => _areaBullet; set { _areaBullet = value; } }

	private Vector2 _velocity;

	public override void _Ready()
	{
		AreaBullet = GetNode<Area2D>(nameof(Area2D));
		AreaBullet.AreaEntered += HitFilter;

		Sprite = GetNode<AnimatedSprite2D>(nameof(AnimatedSprite2D));
		Visible2D = GetNode<VisibleOnScreenNotifier2D>(nameof(VisibleOnScreenNotifier2D));
		Visible2D.ScreenExited += ScreenExited;

		RotatedSprite();
	}

	private void HitFilter(Area2D area)
	{
		var grupos = area.GetOwner().GetGroups();

		if (grupos.Contains("Enemy"))
			QueueFree();
	}

	private void RotatedSprite()
	{
		Sprite.FlipH = Player.FlipH == 1 ? false : true;
		Rotation = Player.Rotation;
		Direction = new Vector2((float)Math.Cos(Rotation), (float)Math.Sin(Rotation)) * Player.FlipH;
	}

	private void ScreenExited()
	{
		QueueFree();
	}

	public override void _PhysicsProcess(double delta)
	{
		_velocity = Velocity;

		_velocity = Direction * Speed;

		_velocity.LimitLength(MaxSpeed);

		Velocity = _velocity;

		MoveAndSlide();
	}
}
