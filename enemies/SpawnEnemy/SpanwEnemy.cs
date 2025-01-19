using Godot;
using System;

public partial class SpanwEnemy : Node2D
{
	[Export] private Marker2D A { get; set; }
	[Export] private Marker2D B { get; set; }

	[Export] private Timer time { get; set; }
	[Export] private Timer peopleTime { get; set; }

	[Export]
	private Vector2 _direction;
	private Vector2 Direction { get { return _direction; } set { _direction = value; } }

	private PackedScene enemy;
	private PackedScene people;

	public override void _Ready()
	{
		enemy = ResourceLoader.Load<PackedScene>("res://enemies/shark/enemy.tscn");
		people = ResourceLoader.Load<PackedScene>("res://person/People.tscn");

		Random random = new Random();

		peopleTime.WaitTime = random.Next(5, 7);
		peopleTime.Start();

		time.Timeout += CreateEnemy;
		peopleTime.Timeout += CreatePeople;
	}

	private void CreateEnemy()
	{
		Enemy newEnemy = enemy.Instantiate() as Enemy;
		newEnemy.GlobalPosition = A.GlobalPosition.Lerp(B.GlobalPosition, GD.Randf());
		newEnemy.Direction = Direction;

		GetParent().AddChild(newEnemy);

		time.WaitTime = GD.Randi() % 6 + 1;
		time.Start();
	}

	private void CreatePeople()
	{
		if (GetTree().GetNodeCountInGroup("People") != 0)
		{
			peopleTime.WaitTime = GD.Randi() % 30 + 10;
			peopleTime.Start();
			return;
		}

		People newPeople = people.Instantiate() as People;
		newPeople.GlobalPosition = A.GlobalPosition.Lerp(B.GlobalPosition, GD.Randf());
		newPeople.Direction = Direction;

		GetParent().AddChild(newPeople);

		peopleTime.WaitTime = GD.Randi() % 30 + 10;
		peopleTime.Start();
	}
}
