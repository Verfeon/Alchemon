using Godot;
using System.Collections.Generic;

using ItemQualityWeights = Game.Items.Data.ItemQualityWeights;
using GameManager = Game.Core.Autoload.GameManager;
using BattleManager = Game.Battle.Domain.BattleManager;
using BaseItemData = Game.Items.Data.BaseItemData;
using MergeableItemData = Game.Items.Data.MergeableItemData;

namespace Game.Items.Domain;

/// <summary>
/// Manages the player's inventory, including collecting and using items.
/// </summary>
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
	
	/// <summary>
	/// Adds an item to the inventory.
	/// If the item is mergeable, it will be added to the mergeable items array and assigned a quality based on the defined weights.
	/// </summary>
	/// <param name="item"></param>
	/// <param name="quantity"></param>
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
	
	/// <summary>
	/// Checks if the inventory contains a specific item at a certain quantity.
	/// </summary>
	/// <param name="itemId"></param>
	/// <param name="quantity"></param>
	/// <returns></returns>
	public bool HasItem(string itemId, int quantity = 1)
	{
		return _itemIds.ContainsKey(itemId) && _itemIds[itemId] >= quantity;
	}
	
	/// <summary>
	/// Removes a specified quantity of an item from the inventory if it exists.
	/// Returns true if the item was successfully removed, false otherwise.
	/// </summary>
	/// <param name="itemId"></param>
	/// <param name="quantity"></param>
	/// <returns></returns>
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

	/// <summary>
	/// Marks a mergeable item as used based on its item ID.
	/// This is used to track which mergeable items have been used in the current battle.
	/// </summary>
	/// <param name="itemId"></param>
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
	
	/// <summary>
	/// Gets a dictionary of all item IDs in the inventory along with their quantity.
	/// </summary>
	/// <returns></returns>
	public Dictionary<string, int> GetAllItems()
	{
		return new Dictionary<string, int>(_itemIds);
	}
	
	/// <summary>
	/// Gets all mergeable items in the inventory, regardless of whether they have been used or not.
	/// </summary>
	/// <returns></returns>
	public MergeableItem[] GetAllMergeableItems()
	{
		return _mergeableItems;
	}

	/// <summary>
	/// Gets all unused mergeable items in the inventory.
	/// </summary>
	/// <returns></returns>
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

	/// <summary>
	/// Gets all used mergeable items in the inventory.
	/// </summary>
	/// <returns></returns>
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
	
	/// <summary>
	/// Resets the used status of all mergeable items in the inventory.
	/// </summary>
	private void ResetMergeableItemsUsed()
	{
		for (int i = 0; i < MaxMergeableItems; i++)
		{
			_isMergeableItemUsed[i] = false;
		}
	}
}
