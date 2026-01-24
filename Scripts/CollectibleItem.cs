using Godot;
using System;

using GameManager = Game.Autoload.GameManager;
using MergeableItemDatabase = Game.Autoload.MergeableItemDatabase;

namespace Game;

public partial class CollectibleItem : Area2D
{
	[Export] public string ObjectId;
	[Export] public float BounceHeight = 10.0f;
	[Export] public float BounceSpeed = 2.0f;
	[Export] public bool IsMergeable;
	
	private Sprite2D _sprite;
	private Vector2 _initialPosition;
	private float _time = 0.0f;
	private bool _isCollected = false;
	
	public override void _Ready()
	{
		_sprite = GetNodeOrNull<Sprite2D>("Sprite2D");
		_initialPosition = Position;
		
		if (IsMergeable)
		{
			_sprite.Texture = MergeableItemDatabase.Instance.Get(ObjectId).Texture2d;
		}
		
		// Connecter le signal de collision
		BodyEntered += OnBodyEntered;
		
		// Variation aléatoire du temps pour désynchroniser les bounces
		_time = (float)GD.RandRange(0, Mathf.Pi * 2);
		
		GD.Print($"Objet collectible créé : {ObjectId}");
	}
	
	public override void _Process(double delta)
	{
		if (_isCollected || _sprite == null) return;
		
		// Animation de bounce
		_time += (float)delta * BounceSpeed;
		float offset = Mathf.Sin(_time) * BounceHeight;
		Position = _initialPosition + new Vector2(0, offset);
	}
	
	private void OnBodyEntered(Node2D body)
	{
		if (_isCollected) return;
		
		if (body.IsInGroup("Player"))
		{
			CollectItem();
		}
	}
	
	private void CollectItem()
	{
		_isCollected = true;
		
		// Ajouter à l'inventaire
		if (GameManager.Instance?.Inventory != null)
		{
			GameManager.Instance.Inventory.AddItem(ObjectId, isMergeable: IsMergeable);
			GD.Print($"Objet collecté : {ObjectId}");
		}
		
		// Animation de collecte
		if (_sprite != null)
		{
			var tween = CreateTween();
			tween.SetParallel(true);
			tween.TweenProperty(_sprite, "scale", Vector2.Zero, 0.3);
			tween.TweenProperty(_sprite, "modulate:a", 0.0f, 0.3);
			tween.Chain();
			tween.TweenCallback(Callable.From(QueueFree));
		}
		else
		{
			QueueFree();
		}
	}
}
