using Godot;
using SeaQuest;
using System;

public partial class Enemy : CharacterBody2D
{
	[ExportGroup("Velocity Enemy")]
	[Export]
	private float _maxSpeed = 200;
	public float MaxSpeed { get => _maxSpeed; }

	[Export]
	private float _speed = 70;
	public float Speed { get => _speed; }

	[Export]
	private Vector2 _direction = Vector2.Zero;
	public Vector2 Direction
	{
		get { return _direction; }
		set { _direction = value; }
	}

	private Vector2 _velocity;

	public AnimatedSprite2D _sprite;
	public AnimatedSprite2D Sprite { get => _sprite; set { _sprite = value; } }

	private int _flipH = 1;
	public int FlipH { get => _flipH; private set => _flipH = value; }

	[Export]
	private float _amplitude = .5f;
	public float Amplitude { get => _amplitude; }

	private Area2D _area;
	public Area2D Area { get => _area; set { _area = value; } }

	private float _offSet;
	public float OffSet { get => _offSet; set { _offSet = value; } }

	private AudioStreamOggVorbis _death;

	private PackedScene ObjectPiece;
	private PackedScene PointPopOver;

	public override void _Ready()
	{
		ObjectPiece = GD.Load<PackedScene>("res://particles/object_piece.tscn");
		PointPopOver = GD.Load<PackedScene>("res://user_interface/PointPopOver/point_pop_over.tscn");

		_death = GD.Load<AudioStreamOggVorbis>("res://enemies/shark/shark_death.ogg");

		OffSet = GD.RandRange(0, 10);

		FlipH = Direction.X > 0 ? 1 : -1;

		Area = GetNode<Area2D>(nameof(Area2D));

		Sprite = GetNode<AnimatedSprite2D>(nameof(AnimatedSprite2D));
		Sprite.FlipV = Direction.X > 0 ? false : true;

		Area.AreaEntered += HitFilter;

		Callable cal2 = new Callable(this, MethodName.PauseEnemy);
		GameEvent.gameEvent.Connect(GameEvent.SignalName.PauseEnemy, cal2);
	}

	public override void _PhysicsProcess(double delta)
	{
		_velocity = Velocity;

		MovementEnemy();
		RotationEnemy();

		_velocity = _velocity.LimitLength(MaxSpeed);

		Velocity = _velocity;

		MoveAndSlide();

		if (GlobalPosition.X < Limits.Left || GlobalPosition.X > Limits.Right)
			QueueFree();
	}

	private void PauseEnemy(bool isPause)
	{
		GetTree().Paused = isPause;
	}

	private void MovementEnemy()
	{
		Direction = new Vector2
		{
			X = Direction.X,
			Y = (float)Math.Sin(Time.GetTicksMsec() * 0.001f + OffSet) * Amplitude
		};

		_velocity = Direction.Normalized() * Speed;
	}

	private void RotationEnemy()
	{
		Rotation = _velocity.Angle();
	}

	private void HitFilter(Area2D area)
	{
		var grupos = area.GetOwner().GetGroups();

		if (grupos.Contains("Player"))
		{
			Player player = area.GetOwner() as Player;
			player.State = SeaQuest.player.PlayerState.DIE;
		}

		else if (grupos.Contains("Enemy"))
		{
			if (area.GetOwner().GetIndex() > GetIndex())
			{
				Tween tween = CreateTween();
				Node2D t = (Node2D)area.GetOwner();
				tween.TweenProperty(this, "scale", new Vector2(.5f, .5f), .25f);
				tween.TweenProperty(this, "scale", new Vector2(1f, 1f), .25f);
			}
		}
		else if (grupos.Contains("BulletPlayer"))
		{
			Global.Instance.CurrentCoins += 10;
			SoundManager.Instance.ConfigSound(_death);
			GameEvent.gameEvent.EmitSignal(GameEvent.SignalName.UpdateMyPoints);

			for (byte i = 0; i < 2; i++)
			{
				Sprite2D instance = ObjectPiece.Instantiate() as Sprite2D;

				ObjectPiece scriptLeft = instance as ObjectPiece;
				scriptLeft._velocity = Direction;

				instance.GlobalPosition = GlobalPosition;

				instance.Hframes = 2;
				instance.Frame = i;

				GetTree().CurrentScene.AddChild(instance);
			}

			PopOverPoint();

			QueueFree();
		}
	}

	private void PopOverPoint()
	{
		Node2D pop = PointPopOver.Instantiate() as Node2D;
		pop.GlobalPosition = GlobalPosition;
		GetTree().CurrentScene.AddChild(pop);

		Tween tween = GetTree().CreateTween().SetParallel(true);
		tween.TweenProperty(pop, "scale", Vector2.Zero, 1.0f);
		tween.TweenProperty(pop, "position", Vector2.Up * 10.0f, 1.0f).AsRelative().SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
		tween.Chain().TweenCallback(Callable.From(pop.QueueFree));
	}
}
