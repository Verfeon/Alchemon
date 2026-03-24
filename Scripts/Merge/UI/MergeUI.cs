using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

using GameManager = Game.Core.Autoload.GameManager;
using MergeableItem = Game.Items.Domain.MergeableItem;

namespace Game.Merge.UI;

/// <summary>
/// Represents the UI for the merge system, allowing the player to select items to merge.
/// </summary>
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
	
	/// <summary>
	/// Handles the event when an item is collected by updating the display of mergeable items in the UI, ensuring that the player can see and select newly collected items for merging.
	/// </summary>
	/// <param name="itemId"></param>
	private void OnItemCollected(string itemId)
	{
		UpdateMergeableItemsListDisplay();
	}
	
	/// <summary>
	/// Updates the display of mergeable items in the UI.
	/// This method clears the current list of mergeable items and repopulates it based on the player's inventory, marking any items that have already been used in a merge as disabled to prevent them from being selected again.
	/// It ensures that the player has an up-to-date view of their available items for merging and can easily identify which items are currently eligible for selection in the merge process.
	/// </summary>
	private void UpdateMergeableItemsListDisplay()
	{
		_mergeableItemsList.Clear();
		GameManager gameManager = GetNode<GameManager>("/root/GameManager");
		if (gameManager.Inventory != null)
		{
			MergeableItem[] items = gameManager.Inventory.GetAllMergeableItems();
			MergeableItem[] usedItems = gameManager.Inventory.GetUsedMergeableItems();
			foreach (MergeableItem item in items)
			{
				if (item != null)
				{
					_mergeableItemsList.AddItem($"{item.Data.Id}", item.Data.Texture2d);
					if (usedItems.Contains(item))
					{
						_mergeableItemsList.SetItemDisabled(_mergeableItemsList.GetItemCount() - 1, true);
					}
				}
			}
		}
	}
	
	/// <summary>
	/// Handles the event when the merge button is pressed, attempting to merge the selected items and updating the UI accordingly.
	/// </summary>
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
			Visible = false;
		}
		else
		{
			GD.Print($"Fail : {result.Message}");
		}
	}
	
	/// <summary>
	/// Handles the event when an item in the mergeable items list is clicked, updating the selection and enabling or disabling the merge button based on whether the selected items can be merged together.
	/// </summary>
	/// <param name="index"></param>
	/// <param name="position"></param>
	/// <param name="mouseButtonIndex"></param>
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
	
	/// <summary>
	/// Handles unhandled input events, specifically listening for the "open_merge_ui" action to toggle the visibility of the merge UI and update the display of mergeable items when the UI is opened, ensuring that the player can access the merge interface and see their available items for merging when they press the designated key or button.
	/// </summary>
	/// <param name="event"></param>
	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("open_merge_ui"))
		{
			GD.Print("Toggle merge");
			UpdateMergeableItemsListDisplay();
			Visible = !Visible;
		}
	}
}
