using Godot;
using System;

using Creature = Game.Creatures.Domain.Creature;
using Game.Utils;
using Godot.NativeInterop;
using Game.Creatures.Data;

namespace Game.Creatures.Presentation;

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
			_hpBar.Visible = false;
			if (possibleCreatures.Count == 0)
			{
				GD.PushError("CreatureNode is marked as wild but has no possible creatures assigned.");
				return;
			}
			Creature creature = new Creature(possibleCreatures[_random.NextInt(0, possibleCreatures.Count - 1)], 1, new GodotRandomAdapter());
			Bind(creature);
			Move();
		}
	}

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

	private void PickNewDirection()
	{
		float angle = _random.NextFloat(0f, 2 * (float)Math.PI);
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
	
	public Creature GetCreature()
	{
		return _creature;
	}
}
