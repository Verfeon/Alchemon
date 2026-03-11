using Godot;
using System;
using System.Collections.Generic;

using MergeableItem = Game.Items.Domain.MergeableItem;
using ItemQuality = Game.Items.Domain.ItemQuality;
using ItemQualityWeights = Game.Items.Data.ItemQualityWeights;
using ItemQualityRoller = Game.Items.Domain.ItemQualityRoller;
using GameManager = Game.Core.Autoload.GameManager;
using BattleManager = Game.Battle.Domain.BattleManager;
using BaseItemData = Game.Items.Data.BaseItemData;
using MergeableItemData = Game.Items.Data.MergeableItemData;

namespace Game.Items.Domain;

public partial class InventoryManager : Node
{
	private const int MaxMergeableItems = 12;
	
	[Export] private ItemQualityWeights _itemQualityWeights;
	
	private Dictionary<string, int> _itemIds = new(); 
	private MergeableItem[] _mergeableItems = new MergeableItem[MaxMergeableItems]; 
	private bool[] _isMergeableItemUsed = new bool[MaxMergeableItems];
	
	[Signal]
	public delegate void itemCollectedEventHandler(string itemId);
	
	[Signal]
	public delegate void itemUsedEventHandler(string itemId);
	
	public override void _Ready()
	{
		BattleManager battleManager = GetNode<GameManager>("/root/GameManager").Battle;
		battleManager.BattleEnded += (bool playerHasWon) => ResetMergeableItemsUsed();
	}
	
	public void AddItem(BaseItemData item, int quantity = 1)
	{
		if (_itemIds.ContainsKey(item.Id))
		{
			_itemIds[item.Id] += quantity;
		}
		else
		{
			_itemIds[item.Id] = quantity;
		}
		
		if (item is MergeableItemData) 
		{
			for (int i = 0; i < _mergeableItems.Length; i++)
			{
				if (_mergeableItems[i] == null)
				{
					ItemQuality quality = ItemQualityRoller.Roll(_itemQualityWeights);
					_mergeableItems[i] = new MergeableItem(item as MergeableItemData, quality);
					break;
				}
			}
		}
		
		EmitSignal(SignalName.itemCollected, item.Id);
	}
	
	public bool HasItem(string itemId, int quantity = 1)
	{
		return _itemIds.ContainsKey(itemId) && _itemIds[itemId] >= quantity;
	}
	
	public bool RemoveItem(string itemId, int quantity = 1)
	{
		if (!HasItem(itemId, quantity))
			return false;
			
		_itemIds[itemId] -= quantity;
		if (_itemIds[itemId] <= 0)
			_itemIds.Remove(itemId);
			
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
	
	public Dictionary<string, int> GetAllItems()
	{
		return new Dictionary<string, int>(_itemIds);
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
	
	private void ResetMergeableItemsUsed()
	{
		for (int i = 0; i < MaxMergeableItems; i++)
		{
			_isMergeableItemUsed[i] = false;
		}
	}
}
