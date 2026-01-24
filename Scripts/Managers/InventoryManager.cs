using Godot;
using System;
using System.Collections.Generic;

using MergeableItem = Game.MergeableItem;
using MergeableItemDatabase = Game.Autoload.MergeableItemDatabase;
using ItemQuality = Game.Enums.ItemQuality;
using ItemQualityWeights = Game.Resources.ItemQualityWeights;
using ItemQualityRoller = Game.ItemQualityRoller;

namespace Game.Managers;

public partial class InventoryManager : Node
{
	private Dictionary<string, int> _items = new(); 
	private MergeableItem[] _mergeableItems = new MergeableItem[12]; 
	
	[Signal]
	public delegate void itemCollectedEventHandler(string itemId);
	
	[Signal]
	public delegate void itemUsedEventHandler(string itemId);
	
	public void AddItem(string itemId, int quantity = 1, bool isMergeable = false)
	{
		if (_items.ContainsKey(itemId))
		{
			_items[itemId] += quantity;
		}
		else
		{
			_items[itemId] = quantity;
		}
		
		if (isMergeable) 
		{
			for (int i = 0; i < _mergeableItems.Length; i++)
			{
				if (_mergeableItems[i] == null)
				{
					ItemQualityWeights weights = ResourceLoader.Load<ItemQualityWeights>("res://Resources/ItemQualityWeights.tres");
					ItemQuality quality = ItemQualityRoller.Roll(weights);
					_mergeableItems[i] = new MergeableItem(MergeableItemDatabase.Instance.Get(itemId), quality);
					break;
				}
			}
		}
		
		EmitSignal(SignalName.itemCollected, itemId);
	}
	
	public bool HasItem(string itemId, int quantity = 1)
	{
		return _items.ContainsKey(itemId) && _items[itemId] >= quantity;
	}
	
	public bool RemoveItem(string itemId, int quantity = 1)
	{
		if (!HasItem(itemId, quantity))
			return false;
			
		_items[itemId] -= quantity;
		if (_items[itemId] <= 0)
			_items.Remove(itemId);
			
		EmitSignal(SignalName.itemUsed, itemId);
		return true;
	}
	
	public Dictionary<string, int> GetAllItems()
	{
		return new Dictionary<string, int>(_items);
	}
	
	public MergeableItem[] GetAllMergeableItems()
	{
		return _mergeableItems;
	}
}
