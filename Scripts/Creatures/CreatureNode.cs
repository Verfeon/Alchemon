using Godot;
using System;

namespace Game.Creatures;

public partial class CreatureNode : Node
{
	[Export] private Sprite2D _sprite;
	private Creature _creature;
	
	public void Init(Creature creature) 
	{
		_creature = creature;
		_sprite.Texture = creature.Data.Texture2d;
	}
}
