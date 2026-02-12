using Game.Merge.Domain;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

using GameManager = Game.Core.Autoload.GameManager;
using MergeableItem = Game.Items.Domain.MergeableItem;

namespace Game.UI;

public partial class MergeUI : Control
{
	[Export] NodePath MergeButton;
	[Export] NodePath CloseButton;
	[Export] NodePath SelectPanel;
	[Export] NodePath MergeableItemsList;
	
	private Button _mergeButton;
	private Button _closeButton;
	private Panel _selectPanel;
	private ItemList _mergeableItemsList;
	
	
	public override void _Ready()
	{
		Visible = false;
		
		_mergeButton = GetNodeOrNull<Button>(MergeButton);
		_closeButton = GetNodeOrNull<Button>(CloseButton);
		_selectPanel = GetNodeOrNull<Panel>(SelectPanel);
		_mergeableItemsList = GetNodeOrNull<ItemList>(MergeableItemsList);
		
		GameManager gameManager = GetNode<GameManager>("/root/GameManager");
		if (gameManager.Inventory != null)
		{
			gameManager.Inventory.itemCollected += OnItemCollected;
		}
		_mergeButton.Pressed += OnMergePressed;
		_closeButton.Pressed += () => Visible = false;
		_mergeableItemsList.ItemClicked += OnMergeableItemListItemClicked;
	}
	
	private void OnItemCollected(string itemId)
	{
		UpdateMergeableItemsListDisplay();
	}
	
	private void UpdateMergeableItemsListDisplay()
	{
		_mergeableItemsList.Clear();
		GameManager gameManager = GetNode<GameManager>("/root/GameManager");
		if (gameManager.Inventory != null)
		{
			MergeableItem[] items = gameManager.Inventory.GetAllMergeableItems();
			foreach (MergeableItem item in items)
			{
				if (item != null)
				{
					_mergeableItemsList.AddItem($"{item.Data.Id}", item.Data.Texture2d);
				}
			}
		}
	}
	
	private void OnMergePressed()
	{
		GameManager gameManager = GetNode<GameManager>("/root/GameManager");
		if (gameManager.Merge == null) return;
		
		
		int[] selectedIndices = _mergeableItemsList.GetSelectedItems();
		List<string> itemIds = new List<string>();
		foreach (int idx in selectedIndices)
		{
			itemIds.Add(_mergeableItemsList.GetItemText(idx));
		}

		var result = gameManager.Merge.AttemptMerge(itemIds);
		
		if (result.Success)
		{
			_mergeButton.Disabled = true;
			_mergeableItemsList.DeselectAll();
			for (int i = 0; i < selectedIndices.Length; i++)
			{
				gameManager.Inventory.MarkMergeableItemAsUsed(itemIds[i]);
			}

			List<string> usedItems = gameManager.Inventory.GetUsedMergeableItems().Select(item => item.Data.Id).ToList();
			foreach (var usedItem in usedItems)
			{
				GD.Print($"Used item: {usedItem}");
			}

			for (int i = 0; i < _mergeableItemsList.GetItemCount(); i++)
			{
				string currentItemId = _mergeableItemsList.GetItemText(i);
				if (usedItems.Contains(currentItemId))
				{
					_mergeableItemsList.SetItemDisabled(i, true);
					usedItems.Remove(currentItemId);
				}
				else
				{
					_mergeableItemsList.SetItemDisabled(i, false);
				}
			}
		}
		else
		{
			GD.Print($"Fail : {result.Message}");
		}
	}
	
	private void OnMergeableItemListItemClicked(long index, Vector2 position, long mouseButtonIndex)
	{
		GameManager gameManager = GetNode<GameManager>("/root/GameManager");
		if (gameManager.Merge == null) return;
		List<int> selectedIndices = _mergeableItemsList.GetSelectedItems().ToList();
		List<string> itemIds = new List<string>();
		foreach (int idx in selectedIndices)
		{
			itemIds.Add(_mergeableItemsList.GetItemText(idx));
		}

		List<string> compatibleItems = gameManager.Merge.GetCompatibleItems(itemIds);
		List<string> usedItems = gameManager.Inventory.GetUsedMergeableItems().Select(item => item.Data.Id).ToList();
		
		for (int i = 0; i < _mergeableItemsList.GetItemCount(); i++)
		{
			string currentItemId = _mergeableItemsList.GetItemText(i);
			if ((compatibleItems.Contains(currentItemId) || selectedIndices.Contains(i) || selectedIndices.Count == 0) && !usedItems.Contains(currentItemId))
			{
				_mergeableItemsList.SetItemDisabled(i, false);
			}
			else
			{
				_mergeableItemsList.SetItemDisabled(i, true);
			}
		}

		if (gameManager.Merge.CanMerge(itemIds))
		{
			_mergeButton.Disabled = false;
		}
	}
	
	public override void _UnhandledInput(InputEvent @event)
	{
		// Touche pour ouvrir la merge (F)
		if (@event.IsActionPressed("ui_merge"))
		{
			GD.Print("Toggle merge");
			Visible = !Visible;
		}
	}
}
