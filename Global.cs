using System;
using Godot;

public partial class Global : Node
{
	public static Global Instance { get; private set; }

	public byte TotalPeople = 10;

	private byte _savePeopleCount = 0;
	public byte SavePeopleCount
	{
		get => _savePeopleCount;
		private set
		{
			_savePeopleCount = value;
		}
	}

	private float _oxygen = 100;

	public float Oxygen
	{
		get => _oxygen;
		private set { _oxygen = Math.Clamp(value, 0, 100); }
	}

	private uint _currentCoins;
	public uint CurrentCoins
	{
		get => _currentCoins;
		set { _currentCoins = Math.Clamp(value, 0, 999999); }
	}

	private uint _oldBestCoins;
	public uint OldBestCoins
	{
		get => _oldBestCoins;
		set
		{
			if (value > CurrentCoins)
				_oldBestCoins = value;
		}
	}

	private uint _bestCoins;
	public uint BestCoins
	{
		get => _bestCoins;
		set
		{
			if (value > OldBestCoins)
				_bestCoins = value;
		}
	}

	public ProgressBar progressBar;

	public override void _Ready()
	{
		Instance = this;
	}

	public void CreaseOxygen(byte percent)
	{
		if (Oxygen > 80 && SavePeopleCount < TotalPeople)
		{
			GameEvent.gameEvent.EmitSignal(GameEvent.SignalName.GameOver);
			return;
		}

		Oxygen += percent;
		progressBar.CreasyOxygen();
		GameEvent.gameEvent.EmitSignal(GameEvent.SignalName.IMChangePeopleForOxygen);
		ResetSavePeople();
	}

	public void DecreaseOxygen(float percent)
	{
		Oxygen -= percent;
		progressBar.DecreaseOxygen();

		if (Oxygen <= 0)
			GameEvent.gameEvent.EmitSignal(GameEvent.SignalName.GameOver);
	}

	public void ResetOxygen()
	{
		Oxygen = 100;
	}

	public void IMSavePeople(byte people)
	{
		SavePeopleCount += people;
	}

	public void ResetSavePeople()
	{
		SavePeopleCount = 0;
	}
}
