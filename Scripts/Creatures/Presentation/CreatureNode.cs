using Godot;
using System;
using Game.Utils.Random;

using Creature = Game.Creatures.Domain.Creature;
using StatEnum = Game.Creatures.Domain.StatEnum;
using CreatureData = Game.Creatures.Data.CreatureData;
using GameManager = Game.Core.Autoload.GameManager;
using BattleManager = Game.Battle.Domain.BattleManager;

namespace Game.Creatures.Presentation;

/// <summary>
/// Represents the visual and interactive representation of a creature in the game world.
/// It binds to a Creature instance to handle interactions.
/// </summary>
public partial class CreatureNode : Node2D
{
	[Export] private Sprite2D _sprite;
	[Export] private ProgressBar _hpBar;
	private Creature _creature;
	
	
	[Export] private float speed = 50f;
	[Export] private float changeDirectionTime = 2f;
	[ExportGroup("Is Wild")]
	[Export(PropertyHint.GroupEnable, "")] public bool IsWild { get; private set; } = false;
	[Export] private Godot.Collections.Array<CreatureData> possibleCreatures;

	private Vector2 _direction = Vector2.Zero;
	private float _timer = 0f;
	private IRandom _random = new GodotRandomAdapter();
	
	public override void _Ready()
	{
		if (IsWild)
		{
			Move();
		}
	}
	
	/// <summary>
	/// Sets up the creature node with the specified creature and wild status.
	/// </summary>
	/// <param name="creature"></param>
	/// <param name="isWild"></param>
	public void Setup(Creature creature, bool isWild)
	{
		Bind(creature);
		IsWild = isWild;
		if (isWild)
		{
			_hpBar.Visible = false;
		}
	}

	/// <summary>
	/// Makes the creature move in a random direction, changing direction at regular intervals defined by <see cref="changeDirectionTime"/>.
	/// </summary>
	/// <returns></returns>
	private async void Move()
	{
		while (true)
		{
			await ToSignal(GetTree(), "physics_frame");
			float delta = (float)GetProcessDeltaTime();
			_timer -= delta;

			if (_timer <= 0f)
			{
				_timer = changeDirectionTime;
				PickNewDirection();
			}
			Position += _direction * speed * delta;
		}
	}

	/// <summary>
	/// Chooses a new random direction for the creature to move in.
	/// </summary>
	private void PickNewDirection()
	{
		float angle = _random.NextFloat(0f, 2 * (float)Math.PI);
		_direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).Normalized();
	}
	
	/// <summary>
	/// Binds the creature node to a specific <see cref="Creature"/> instance, updating the visual and setting up event handlers for HP changes and fainting. 
	/// </summary>
	/// <param name="creature"></param>
	public void Bind(Creature creature)
	{
		if (creature == null)
		{
			throw new ArgumentNullException(nameof(creature));
		}

		if (_creature == creature) return ;
		
		Unbind();

		_creature = creature;

		_sprite.Texture = creature.Data.Texture2d;

		_hpBar.MaxValue = creature.GetBattleStat(StatEnum.MaxHP);
		_hpBar.Value = creature.CurrentHP;

		_creature.HpChanged += OnHpChanged;
		_creature.Fainted += OnFainted;
	}

	/// <summary>
	/// Unbinds the creature node from its current <see cref="Creature"/> instance, removing event handlers and clearing the reference.
	/// </summary>
	private void Unbind()
	{
		if (_creature == null) return;

		_creature.HpChanged -= OnHpChanged;
		_creature.Fainted -= OnFainted;
		_creature = null;
	}

	public override void _ExitTree()
	{
		Unbind();
	}
	
	public Creature GetCreature()
	{
		return _creature;
	}

	private void OnHpChanged(int current)
	{
		_hpBar.Value = current;
	}

	private void OnFainted(Creature creature)
	{
		// animation, disparition, etc.
	}
	
	/// <summary>
	/// Handles the end of battle event. 
	/// </summary>
	/// <param name="playerHasWon"></param>
	private void OnBattleEnded(bool playerHasWon)
	{
		if (playerHasWon)
		{
			BattleManager battleManager = GetNode<GameManager>("/root/GameManager").Battle;
			battleManager.BattleEnded -= OnBattleEnded;
			QueueFree();
		}
	}
	
	/// <summary>
	/// Handles the event when a body enters the creature's area.
	/// If the body is the player and the creature is wild, it starts a battle with the creature and binds the battle end event to handle post-battle logic.
	/// </summary>
	/// <param name="body"></param>
	public void OnBodyEntered(Node2D body)
	{
		if (!body.IsInGroup("Player")) return;
		BattleManager battleManager = GetNode<GameManager>("/root/GameManager").Battle;
		if (!IsWild) return;
		_ = battleManager.StartBattle(_creature);
		battleManager.BattleEnded += OnBattleEnded;
	}
}
