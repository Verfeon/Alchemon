using Godot;
using System;

using Game.Managers;

namespace Game.Autoload;

public partial class GameManager : Node
{
	public static GameManager Instance { get; private set; }
	
	[Export] public InventoryManager Inventory { get; private set; }
	[Export] public MergeManager Merge { get; private set; }
	[Export] public CreatureManager Creatures { get; private set; }
	[Export] public PlayerManager Player { get; private set; }
	[Export] public SaveManager SaveData { get; private set; }
	
	public override void _Ready()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		
		// Initialisation des managers
		// Vérifier si les enfants existent, sinon les créer
		Inventory = GetNodeOrNull<InventoryManager>("InventoryManager");
		if (Inventory == null)
		{
			Inventory = new InventoryManager { Name = "InventoryManager" };
			AddChild(Inventory);
		}
		
		Merge = GetNodeOrNull<MergeManager>("MergeManager");
		if (Merge == null)
		{
			Merge = new MergeManager { Name = "MergeManager" };
			AddChild(Merge);
		}
		
		Creatures = GetNodeOrNull<CreatureManager>("CreatureManager");
		if (Creatures == null)
		{
			Creatures = new CreatureManager { Name = "CreatureManager" };
			AddChild(Creatures);
		}
		
		Player = GetNodeOrNull<PlayerManager>("PlayerManager");
		if (Player == null)
		{
			Player = new PlayerManager { Name = "PlayerManager" };
			AddChild(Player);
		}
		
		SaveData = GetNodeOrNull<SaveManager>("SaveManager");
		if (SaveData == null)
		{
			SaveData = new SaveManager { Name = "SaveManager" };
			AddChild(SaveData);
		}
		
		Merge.LoadRecipes("res://Data/merge_recipes.json");
	}
}
