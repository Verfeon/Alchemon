using Godot;
using System;

using GameManager = Game.Autoload.GameManager;

namespace Game;

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
		
		// Ajouter au groupe Player
		AddToGroup(PlayerGroup);
		
		// Activer la caméra
		if (_camera != null)
		{
			_camera.Enabled = true;
		}
		
		// Configuration de l'AnimatedSprite2D si elle existe
		if (_animatedSprite != null)
		{
			// Assurez-vous d'avoir ces animations dans votre SpriteFrames :
			// idle_down, idle_up, idle_left, idle_right
			// walk_down, walk_up, walk_left, walk_right
			_animatedSprite.Play("idle_down");
		}
		
		GD.Print("Player initialisé !");
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
		
		// Mettre à jour la position du joueur dans le PlayerManager
		if (GameManager.Instance?.Player != null)
		{
			GameManager.Instance.Player.SetPlayerPosition(GlobalPosition);
		}
	}
	
	private void UpdateAnimation(Vector2 direction, bool isMoving)
	{
		if (_animatedSprite == null) return;
		
		// Déterminer la direction
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
		
		// Jouer l'animation appropriée
		string animName = isMoving ? $"walk_{_currentDirection}" : $"idle_{_currentDirection}";
		
		if (_animatedSprite.SpriteFrames.HasAnimation(animName))
		{
			_animatedSprite.Play(animName);
		}
	}
}
