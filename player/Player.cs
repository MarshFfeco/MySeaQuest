using System;
using System.Linq;
using Godot;
using SeaQuest.player;

public partial class Player : CharacterBody2D
{
    private AnimatedSprite2D _sprite;

    #region Velocity
    [Export]
    [ExportGroup("Velocity Player")]
    private float _speed = 100;
    public float Speed { get => _speed; }

    [Export]
    private float _maxSpeed = 150;
    public float MaxSpeed { get => _maxSpeed; }

    [Export]
    private float _friction = 40;
    public float Friction { get => _friction; }

    [Export]
    private float _accel = 300;
    private float Acceleration { get => _accel; }

    [Export]
    private float _forceRotation = 10;
    public float ForceRotation { get => _forceRotation; }
    #endregion

    #region others
    private PlayerState _state = PlayerState.DEFAULT;
    public PlayerState State { get => _state; set => _state = value; }

    [Export]
    [ExportGroup("Ocean force")]
    private Vector2 _streamDirection = Vector2.Zero;
    private Vector2 StreamDirection
    {
        get => _streamDirection.Normalized();
        set => _streamDirection = value.Normalized();
    }

    [Export]
    private float _streamForce = 50f;
    private float StreamForce
    {
        get => Math.Clamp(_streamForce, 0, 200f);
        set => Math.Clamp(_streamForce, 0, 200f);
    }

    private Vector2 _velocity;

    private Vector2 _direction;
    private Vector2 Direction { get => _direction; set => _direction = value; }

    private Resources resources;

    private PackedScene _bulletPlayer;

    private AudioStreamWav _soundBullet;
    public AudioStreamOggVorbis _soundDeath { get; private set; }
    private AudioStreamOggVorbis _soundOxygenZone;

    private Timer _countDownFire;
    private Timer CountDownFire { get => _countDownFire; set => _countDownFire = value; }

    private Timer _oxygenTime;
    public Timer OxygenTime { get => _oxygenTime; set => _oxygenTime = value; }

    private bool _canFire = true;
    private bool CanFire { get => _canFire; set => _canFire = value; }

    private int _flipH = 1;
    public int FlipH { get => _flipH; private set => _flipH = value; }

    private VisibleOnScreenNotifier2D _visiblePlayer;
    private VisibleOnScreenNotifier2D VisiblePlayer { get => _visiblePlayer; set => _visiblePlayer = value; }
    #endregion

    private Area2D _area;

    private ShaderMaterial shader;

    private const byte PIECE_COUNT = 10;
    private PackedScene ObjectPiecePackage;
    private Texture2D PieceTexture;

    public override void _Ready()
    {
        GetTree().Paused = false;

        Global.Instance.OldBestCoins = Global.Instance.BestCoins;
        Global.Instance.CurrentCoins = 0;
        Global.Instance.BestCoins = 0;

        ObjectPiecePackage = GD.Load<PackedScene>("res://particles/object_piece.tscn");
        PieceTexture = GD.Load<Texture2D>("res://player/player_pieces.png");

        VisiblePlayer = GetNode<VisibleOnScreenNotifier2D>(nameof(VisibleOnScreenNotifier2D));
        VisiblePlayer.ScreenExited += PlayerOffScreen;

        CountDownFire = GetNode<Timer>("CountDownFire");
        CountDownFire.Timeout += CountDownFireYes;

        OxygenTime = GetNode<Timer>("OxygenDe");
        OxygenTime.Timeout += () =>
        {
            Global.Instance.DecreaseOxygen(.5f);
        };

        _bulletPlayer = GD.Load<PackedScene>("res://player/player_bullet/player_bullet.tscn");

        _soundBullet = GD.Load<AudioStreamWav>("res://player/laserShoot.wav");
        _soundDeath = GD.Load<AudioStreamOggVorbis>("res://player/player_death.ogg");
        _soundOxygenZone = GD.Load<AudioStreamOggVorbis>("res://player/player_surface.ogg");

        resources = new Resources();
        _sprite = GetNode<AnimatedSprite2D>(nameof(AnimatedSprite2D));

        shader = _sprite.Material as ShaderMaterial;
        shader.SetShaderParameter("oxygen", Global.Instance.Oxygen / 100);

        _area = GetNode<Area2D>(nameof(Area2D));
        _area.AreaEntered += HitFilter;
        _area.AreaExited += HitFilterExited;

        GameEvent.gameEvent.GameOver += () => State = PlayerState.DIE;
    }

    public override void _Process(double delta)
    {
        ICanFire();

        switch (State)
        {
            case PlayerState.DIE:
                SetPhysicsProcess(false);
                SetProcess(false);
                _sprite.Visible = false;

                for (int i = 0; i < PIECE_COUNT; i++)
                {
                    Sprite2D objectPiece = ObjectPiecePackage.Instantiate() as Sprite2D;
                    objectPiece.Texture = PieceTexture;
                    objectPiece.Hframes = PIECE_COUNT;
                    objectPiece.GlobalPosition = GlobalPosition;
                    objectPiece.Frame = i;

                    ObjectPiece objectPieceScript = objectPiece as ObjectPiece;
                    objectPieceScript._velocity = Direction;

                    GetTree().CurrentScene.AddChild(objectPiece);
                }

                Tween tween = CreateTween();
                tween.TweenCallback(Callable.From(() => SoundManager.Instance.ConfigSound(_soundDeath)));
                tween.TweenCallback(Callable.From(() => GameEvent.gameEvent.EmitSignal(GameEvent.SignalName.GameOver))).SetDelay(Math.Round(_soundDeath.GetLength(), 2));
                break;
            case PlayerState.TOP:
                if (Direction == Vector2.Zero)
                    GlobalPosition = GlobalPosition.MoveToward(new Vector2(GlobalPosition.X, 43), (float)delta * 50);
                break;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        _velocity = Velocity;

        ActionMovement((float)delta);

        LimitVelocity();

        Velocity = _velocity;

        MoveAndSlide();

        if (GlobalPosition.Y < 43)
            GlobalPosition = new Vector2(GlobalPosition.X, 43);

        GameEvent.gameEvent.EmitSignal(GameEvent.SignalName.FollowPlayer, GlobalPosition.Y);
    }

    public void ChangeParamShader()
    {
        shader.SetShaderParameter("oxygen", 15 / 100);
    }

    private void PlayerOffScreen()
    {
        Global.Instance.ResetOxygen();
        GameEvent.gameEvent.EmitSignal(GameEvent.SignalName.GameOver);
    }

    private void HitFilter(Area2D area)
    {
        Godot.Collections.Array<StringName> group = area.GetOwner().GetGroups();

        if (group.Any(a => a.Equals("People")) && Global.Instance.SavePeopleCount < Global.Instance.TotalPeople)
        {
            Global.Instance.IMSavePeople(1);
            GameEvent.gameEvent.EmitSignal(GameEvent.SignalName.IMSaveThePeople);
        }

        if (group.Any(a => a.Equals("OxygenZone")))
            OxygenZone();
    }

    private void HitFilterExited(Area2D area)
    {
        if (!area.GetParent().IsInGroup("OxygenZone")) return;

        OxygenTime.Paused = false;
        State = PlayerState.DEFAULT;
    }

    private void OxygenZone()
    {
        State = PlayerState.TOP;

        OxygenTime.Paused = true;

        SoundManager.Instance.ConfigSound(_soundOxygenZone);

        if (Global.Instance.SavePeopleCount != 0)
            Global.Instance.CreaseOxygen((byte)(Global.Instance.SavePeopleCount * 10));
    }

    private void ICanFire()
    {
        if (Input.IsActionPressed(resources.Fire))
            State = PlayerState.FIRE;
        else
            State = PlayerState.DEFAULT;

        if (Input.IsActionPressed(resources.Fire) && CanFire)
        {
            Node2D instance = _bulletPlayer.Instantiate() as Node2D;
            PlayerBullet script = instance as PlayerBullet;

            instance.GlobalPosition = GlobalPosition + new Vector2((float)Math.Cos(Rotation), (float)Math.Sin(Rotation)) * 15 * FlipH;
            script.Player = this;

            GetParent().AddChild(instance);
            CountDownFire.Start();
            SoundManager.Instance.ConfigSound(_soundBullet);
            CanFire = false;
        }
    }

    private void ActionMovement(float delta)
    {
        Vector2 ForceWatter = StreamDirection * StreamForce;

        if (Direction != Vector2.Zero)
        {
            _velocity = _velocity.MoveToward(Direction * Speed + ForceWatter, Acceleration * delta);

            if (State != PlayerState.FIRE)
                FlipSprite();

            RotationSprite((float)delta);
        }
        else
            _velocity = _velocity.MoveToward(ForceWatter, Friction * delta);
    }

    public override void _Input(InputEvent @event)
    {
        Direction = Input.GetVector(resources.MoveLeft, resources.MoveRight, resources.MoveUp, resources.MoveDown);

        if (@event.IsActionPressed(resources.PAUSE))
            GameEvent.gameEvent.EmitSignal(GameEvent.SignalName.PauseEnemy, true);
    }

    private void LimitVelocity()
    {
        _velocity = _velocity.LimitLength(MaxSpeed);
    }

    private void FlipSprite()
    {
        if (Direction.X > 0)
            _sprite.FlipH = false;
        else if (Direction.X < 0)
            _sprite.FlipH = true;
    }

    private void RotationSprite(float delta)
    {
        FlipH = _sprite.FlipH ? -1 : 1;

        if (Direction.Y != 0)
            Rotation = Mathf.Lerp(Rotation, .6f * _velocity.Normalized().Y * FlipH, ForceRotation * delta);
        else
            Rotation = Mathf.Lerp(Rotation, 0, ForceRotation * delta);
    }

    private void CountDownFireYes()
    {
        CanFire = true;
    }
}
