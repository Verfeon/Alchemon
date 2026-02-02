using Godot;
using System;
using System.Collections.Generic;

using GameManager = Game.Core.Autoload.GameManager;

namespace Game.Managers;

public partial class SaveManager : Node
{
	private const string SavePath = "user://savegame.json";
	
	public void SaveGame()
	{
		var saveDict = new Godot.Collections.Dictionary
		{
			{ "inventory", ConvertInventoryToDict() },
			{ "player_position", GetPlayerPosition() },
			{ "discovered_recipes", ConvertRecipesToArray() }
		};
		
		var json = Json.Stringify(saveDict);
		var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
		file.StoreString(json);
		file.Close();
	}
	
	private Godot.Collections.Dictionary ConvertInventoryToDict()
	{
		var dict = new Godot.Collections.Dictionary();
		GameManager gameManager = GetNode<GameManager>("/root/GameManager");
		foreach (var kvp in gameManager.Inventory.GetAllItems())
		{
			dict[kvp.Key] = kvp.Value;
		}
		return dict;
	}
	
	private Godot.Collections.Dictionary GetPlayerPosition()
	{
		var player = GetTree().GetFirstNodeInGroup("Player") as Node2D;
		if (player != null)
		{
			return new Godot.Collections.Dictionary
			{
				{ "x", player.Position.X },
				{ "y", player.Position.Y }
			};
		}
		return new Godot.Collections.Dictionary();
	}
	
	private Godot.Collections.Array ConvertRecipesToArray()
	{
		// À implémenter avec les recettes découvertes du FusionManager
		return new Godot.Collections.Array();
	}
	
	public void LoadGame()
	{
		if (!FileAccess.FileExists(SavePath))
			return;
			
		var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
		var json = file.GetAsText();
		file.Close();
		
		var saveDict = Json.ParseString(json).AsGodotDictionary();
		
		// Restaurer l'inventaire
		if (saveDict.ContainsKey("inventory"))
		{
			var inventoryDict = saveDict["inventory"].AsGodotDictionary();
			// Restaurer les objets dans l'inventaire
		}
		
		// Restaurer l'équipe
		if (saveDict.ContainsKey("party"))
		{
			var partyArray = saveDict["party"].AsGodotArray();
			// Restaurer les créatures
		}
		
		// Restaurer la position du joueur
		if (saveDict.ContainsKey("player_position"))
		{
			var posDict = saveDict["player_position"].AsGodotDictionary();
			var player = GetTree().GetFirstNodeInGroup("Player") as Node2D;
			if (player != null)
			{
				player.Position = new Vector2(
					(float)posDict["x"],
					(float)posDict["y"]
				);
			}
		}
	}
}
