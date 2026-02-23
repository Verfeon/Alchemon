using Godot;
using System;

using Creature = Game.Creatures.Domain.Creature;
using GameManager = Game.Core.Autoload.GameManager;
using BattleManager = Game.Battle.Domain.BattleManager;

namespace Game.Creatures.Presentation;

public partial class CreatureNode : Node2D
{
	[Export] private Sprite2D _sprite;
	[Export] private ProgressBar _hpBar;
	private Creature _creature;
	
	
	[Export] public float Speed = 100f;
	[Export] public float ChangeDirectionTime = 2f;

	private Vector2 _direction = Vector2.Zero;
	private float _timer = 0f;
	private Random _random = new Random();
	
	public override void _Ready()
	{
		PickNewDirection();
	}

	public override void _PhysicsProcess(double delta)
	{
		_timer -= (float)delta;

		if (_timer <= 0f)
		{
			PickNewDirection();
		}

		Position += _direction * Speed * (float)delta;
	}

	private void PickNewDirection()
	{
		_timer = ChangeDirectionTime;

		float angle = (float)(_random.NextDouble() * Math.PI * 2);
		_direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).Normalized();
	}
	
	public void Bind(Creature creature)
	{
		if (creature == null)
		{
			throw new ArgumentNullException(nameof(creature));
		}

		if (_creature == creature)
		{
			return;
		}
		
		Unbind();

		_creature = creature;

		_sprite.Texture = creature.Data.Texture2d;

		_hpBar.MaxValue = creature.RealStats.MaxHP;
		_hpBar.Value = creature.CurrentHP;

		_creature.HpChanged += OnHpChanged;
		_creature.Fainted += OnFainted;
	}

	private void Unbind()
	{
		if (_creature == null) return;

		_creature.HpChanged -= OnHpChanged;
		_creature.Fainted -= OnFainted;
		_creature = null;
	}

	private void OnHpChanged(int current)
	{
		_hpBar.Value = current;
	}

	private void OnFainted(Creature creature)
	{
		// animation, disparition, etc.
	}

	public override void _ExitTree()
	{
		Unbind();
	}
}
