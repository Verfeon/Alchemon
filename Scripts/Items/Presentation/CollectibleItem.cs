using Godot;
using System;

using GameManager = Game.Core.Autoload.GameManager;
using MergeableItemDatabase = Game.Core.Autoload.MergeableItemDatabase;
using BaseItemData = Game.Items.Data.BaseItemData;
using MergeableItemData = Game.Items.Data.MergeableItemData;

namespace Game.Items.Presentation;

public partial class CollectibleItem : Area2D
{
	[Export] private BaseItemData _itemData;
	[Export] private float _bounceHeight = 10.0f;
	[Export] private float _bounceSpeed = 2.0f;
	[Export] private Sprite2D _sprite;
	
	private Vector2 _initialPosition;
	private float _time = 0.0f;
	private bool _isCollected = false;
	
	public override void _Ready()
	{
		_initialPosition = Position;
		
		if (_itemData is MergeableItemData)
		{
			_sprite.Texture = _itemData.Texture2d;
		}
		
		BodyEntered += OnBodyEntered;
		
		_time = (float)GD.RandRange(0, Mathf.Pi * 2);
		
		GD.Print($"Collectible object created : {_itemData.Name}");
	}
	
	public override void _Process(double delta)
	{
		if (_isCollected || _sprite == null) return;
		
		_time += (float)delta * _bounceSpeed;
		float offset = Mathf.Sin(_time) * _bounceHeight;
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
		
		GameManager gameManager = GetNode<GameManager>("/root/GameManager");
		if (gameManager.Inventory != null)
		{
			gameManager.Inventory.AddItem(_itemData);
			GD.Print($"Object collected : {_itemData.Id}");
		}
		
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
