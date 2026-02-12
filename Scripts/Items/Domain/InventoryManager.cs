using Godot;
using System;
using System.Collections.Generic;

using MergeableItem = Game.Items.Domain.MergeableItem;
using MergeableItemDatabase = Game.Core.Autoload.MergeableItemDatabase;
using ItemQuality = Game.Items.Domain.ItemQuality;
using ItemQualityWeights = Game.Items.Data.ItemQualityWeights;
using ItemQualityRoller = Game.Items.Domain.ItemQualityRoller;

namespace Game.Items.Domain;

public partial class InventoryManager : Node
{
	private Dictionary<string, int> _items = new(); 
	private const int MaxMergeableItems = 12;
	private MergeableItem[] _mergeableItems = new MergeableItem[MaxMergeableItems]; 
	private bool[] _isMergeableItemUsed = new bool[MaxMergeableItems];
	
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
					_mergeableItems[i] = new MergeableItem(GetNode<MergeableItemDatabase>("/root/MergeableItemDatabase").Get(itemId), quality);
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

	public void MarkMergeableItemAsUsed(string itemId)
	{
		for (int i = 0; i < _mergeableItems.Length; i++)
		{
			if (_mergeableItems[i] != null && _mergeableItems[i].Data.Id == itemId && !_isMergeableItemUsed[i])
			{
				_isMergeableItemUsed[i] = true;
				break;
			}
		}
	}

	public void ResetMergeableItemsUsage()
	{
		for (int i = 0; i < _isMergeableItemUsed.Length; i++)
		{
			_isMergeableItemUsed[i] = false;
		}
	}
	
	public Dictionary<string, int> GetAllItems()
	{
		return new Dictionary<string, int>(_items);
	}
	
	public MergeableItem[] GetAllMergeableItems()
	{
		return _mergeableItems;
	}

	public MergeableItem[] GetUnusedMergeableItems()
	{
		List<MergeableItem> unusedItems = new List<MergeableItem>();
		for (int i = 0; i < _mergeableItems.Length; i++)
		{
			if (_mergeableItems[i] != null && !_isMergeableItemUsed[i])
			{
				unusedItems.Add(_mergeableItems[i]);
			}
		}
		return unusedItems.ToArray();
	}

	public MergeableItem[] GetUsedMergeableItems()
	{
		List<MergeableItem> usedItems = new List<MergeableItem>();
		for (int i = 0; i < _mergeableItems.Length; i++)
		{
			if (_mergeableItems[i] != null && _isMergeableItemUsed[i])
			{
				usedItems.Add(_mergeableItems[i]);
			}
		}
		return usedItems.ToArray();
	}
}
