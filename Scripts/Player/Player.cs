using Godot;
using System;

using GameManager = Game.Core.Autoload.GameManager;
using BattleManager = Game.Battle.Domain.BattleManager;
using CreatureNode = Game.Creatures.Presentation.CreatureNode;
using Creature = Game.Creatures.Domain.Creature;

namespace Game.Player;

public partial class Player : CharacterBody2D
{
	[Export] public float Speed = 120.0f;
	[Export] public string PlayerGroup = "Player";
	
	[Export] private NodePath AnimatedSpritePath;
	[Export] private NodePath CameraPath;
	
	private AnimatedSprite2D _animatedSprite;
	private Camera2D _camera;
	private string _currentDirection = "down";
	
	public override void _Ready()
	{
		_animatedSprite = GetNodeOrNull<AnimatedSprite2D>(AnimatedSpritePath);
		_camera = GetNodeOrNull<Camera2D>(CameraPath);
		
		if (_camera != null)
		{
			_camera.Enabled = true;
		}
		
		if (_animatedSprite != null)
		{
			_animatedSprite.Play("idle_down");
		}
	}
	
	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;
		Vector2 direction = Input.GetVector("left", "right", "up", "down");
		
		if (direction != Vector2.Zero)
		{
			velocity = direction * Speed;
			UpdateAnimation(direction, true);
		}
		else
		{
			velocity = Vector2.Zero;
			UpdateAnimation(direction, false);
		}
		
		Velocity = velocity;
		MoveAndSlide();
	}
	
	private void UpdateAnimation(Vector2 direction, bool isMoving)
	{
		if (_animatedSprite == null) return;
		
		if (direction != Vector2.Zero)
		{
			if (Math.Abs(direction.X) > Math.Abs(direction.Y))
			{
				_currentDirection = direction.X > 0 ? "right" : "left";
			}
			else
			{
				_currentDirection = direction.Y > 0 ? "down" : "up";
			}
		}
		
		string animName = isMoving ? $"walk_{_currentDirection}" : $"idle_{_currentDirection}";
		
		if (_animatedSprite.SpriteFrames.HasAnimation(animName))
		{
			_animatedSprite.Play(animName);
		}
	}
	
	public void _OnBodyEntered(Node2D body)
	{
		if (!body.IsInGroup("Enemy")) return;
		BattleManager battleManager = GetNode<GameManager>("/root/GameManager").Battle;
		CreatureNode enemyNode = body as CreatureNode;
		if (!enemyNode.IsWild) return;
		Creature enemy = enemyNode.GetCreature();
		battleManager.StartBattle(enemy, enemy);
	}
}
