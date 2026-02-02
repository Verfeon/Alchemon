using Godot;
using System;
using System.Diagnostics;

using InventoryManager = Game.Items.Domain.InventoryManager;
using MergeManager = Game.Merge.Domain.MergeManager;
using PlayerManager = Game.Player.PlayerManager;
using SaveManager = Game.Managers.SaveManager;

namespace Game.Core.Autoload;

public partial class GameManager : Node
{
	[Export] public InventoryManager Inventory { get; private set; }
	[Export] public MergeManager Merge { get; private set; }
	[Export] public PlayerManager Player { get; private set; }
	[Export] public SaveManager SaveData { get; private set; }
	
	public override void _Ready()
	{
		Debug.Assert(Inventory != null);
		Debug.Assert(Merge != null);
		Debug.Assert(Player != null);
		Debug.Assert(SaveData != null);
	}
}
