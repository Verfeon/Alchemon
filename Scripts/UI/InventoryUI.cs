using Godot;
using System;

using GameManager = Game.Core.Autoload.GameManager;

namespace Game.UI;

public partial class InventoryUI : Control
{
	[Export] private NodePath InventoryLabelPath;
	[Export] private NodePath ItemsListPath;
	[Export] private NodePath MergeableItemsListPath;
	[Export] private NodePath ChangeItemsListButtonPath;

	private Label _inventoryLabel;
	private VBoxContainer _itemsList;
	private VBoxContainer _mergeableItemsList;
	private VBoxContainer _currentItemsList;
	private Button _changeItemsListButton;
	
	public override void _Ready()
	{
		Visible = false;
		
		_inventoryLabel = GetNodeOrNull<Label>(InventoryLabelPath);
		_itemsList = GetNodeOrNull<VBoxContainer>(ItemsListPath);
		_mergeableItemsList = GetNodeOrNull<VBoxContainer>(MergeableItemsListPath);
		_changeItemsListButton = GetNodeOrNull<Button>(ChangeItemsListButtonPath);
		_currentItemsList = _itemsList;
		_mergeableItemsList.Visible = false;
		
		_changeItemsListButton.Pressed += ChangeItemsList;
		
		GameManager gameManager = GetNode<GameManager>("/root/GameManager");
		if (gameManager.Inventory != null)
		{
			gameManager.Inventory.itemCollected += OnItemCollected;
		}
		
		UpdateInventoryDisplay();
	}
	
	private void OnItemCollected(string itemId)
	{
		UpdateInventoryDisplay();
	}
	
	private void UpdateInventoryDisplay()
	{
		foreach (Node child in _itemsList.GetChildren())
		{
			child.QueueFree();
		}
		foreach (Node child in _mergeableItemsList.GetChildren())
		{
			child.QueueFree();
		}
		
		GameManager gameManager = GetNode<GameManager>("/root/GameManager");
		if (gameManager.Inventory != null)
		{
			var items = gameManager.Inventory.GetAllItems();
			var mergeableItems = gameManager.Inventory.GetAllMergeableItems();
			
			if (items.Count == 0)
			{
				var emptyLabel = new Label();
				emptyLabel.Text = "No item";
				emptyLabel.Modulate = new Color(0.7f, 0.7f, 0.7f);
				_itemsList.AddChild(emptyLabel);
			}
			else
			{
				foreach (var item in items)
				{
					var itemLabel = new Label();
					itemLabel.Text = $"{item.Key} x{item.Value}";
					_itemsList.AddChild(itemLabel);
				}
			}
			
			if (mergeableItems.Length == 0)
			{
				var emptyLabel = new Label();
				emptyLabel.Text = "No item";
				emptyLabel.Modulate = new Color(0.7f, 0.7f, 0.7f);
				_mergeableItemsList.AddChild(emptyLabel);
			}
			else
			{
				foreach (var item in mergeableItems)
				{
					if (item == null) continue;
					var itemLabel = new Label();
					itemLabel.Text = $"{item.Data.Id} {item.Quality}";
					_mergeableItemsList.AddChild(itemLabel);
				}
			}
		}
	}
	
	private void ChangeItemsList()
	{
		_currentItemsList.Visible = false;
		if (_currentItemsList == _itemsList)
		{
			_currentItemsList = _mergeableItemsList;
		} 
		else 
		{
			_currentItemsList = _itemsList;
		}
		_currentItemsList.Visible = true;
	}
	
	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_inventory"))
		{
			GD.Print("Toggle inventory");
			Visible = !Visible;
		}
	}
}
