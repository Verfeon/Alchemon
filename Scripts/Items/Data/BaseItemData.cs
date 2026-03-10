using Godot;
using System;

namespace Game.Items.Data;

[GlobalClass, Tool]
public partial class BaseItemData : Resource
{
	[Export] public string Id;
	[Export] public string Name;
	[Export] public Texture2D Texture2d;
}
