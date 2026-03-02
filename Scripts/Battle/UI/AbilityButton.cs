using Godot;
using System;

public partial class AbilityButton : Node2D
{
	[Export] private int _id;
	
	public void OnButtonPressed() 
	{
		GD.Print($"button {_id} pressed");
	}
}
