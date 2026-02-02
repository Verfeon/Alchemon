using Godot;
using System;
using System.Collections.Generic;

using GameManager = Game.Core.Autoload.GameManager;

namespace Game.UI;

public partial class MergeUI : Control
{
	[Export] NodePath Slot1Button;
	[Export] NodePath Slot2Button;
	[Export] NodePath MergeButton;
	[Export] NodePath CloseMergeButton;
	[Export] NodePath CloseSelectButton;
	[Export] NodePath ResultLabel;
	[Export] NodePath SelectPanel;
	[Export] NodePath MergePanel;
	[Export] NodePath MergeableItemsList;
	
	private Button _slot1Button;
	private Button _slot2Button;
	private Button _mergeButton;
	private Button _closeMergeButton;
	private Button _closeSelectButton;
	private Label _resultLabel;
	private Panel _selectPanel;
	private Panel _mergePanel;
	private ItemList _mergeableItemsList;
	
	private string _selectedObject1 = null;
	private string _selectedObject2 = null;
	private int _currentSlot = 1;
	
	public override void _Ready()
	{
		Visible = false;
		
		_slot1Button = GetNodeOrNull<Button>(Slot1Button);
		_slot2Button = GetNodeOrNull<Button>(Slot2Button);
		_mergeButton = GetNodeOrNull<Button>(MergeButton);
		_closeMergeButton = GetNodeOrNull<Button>(CloseMergeButton);
		_closeSelectButton = GetNodeOrNull<Button>(CloseSelectButton);
		_resultLabel = GetNodeOrNull<Label>(ResultLabel);
		_selectPanel = GetNodeOrNull<Panel>(SelectPanel);
		_mergePanel = GetNodeOrNull<Panel>(MergePanel);
		_mergeableItemsList = GetNodeOrNull<ItemList>(MergeableItemsList);
		
		_selectPanel.Visible = false;
		
		GameManager gameManager = GetNode<GameManager>("/root/GameManager");
		if (gameManager.Inventory != null)
		{
			gameManager.Inventory.itemCollected += OnItemCollected;
		}
		_slot1Button.Pressed += () => SelectItem(1);
		_slot2Button.Pressed += () => SelectItem(2);
		_mergeButton.Pressed += OnMergePressed;
		_closeMergeButton.Pressed += () => Visible = false;
		_closeSelectButton.Pressed += () => {_selectPanel.Visible = false; _mergePanel.Visible = true;};
		_mergeableItemsList.ItemSelected += OnMergeableItemListItemSelected;
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
			var items = gameManager.Inventory.GetAllMergeableItems();
			foreach (var item in items)
			{
				if (item != null)
				{
					_mergeableItemsList.AddItem($"{item.Data.Id}", item.Data.Texture2d);
				}
			}
		}
	}
	
	private void SelectItem(int slot)
	{
		GameManager gameManager = GetNode<GameManager>("/root/GameManager");
		if (gameManager.Inventory == null) return;
		
		var items = gameManager.Inventory.GetAllMergeableItems();
		if (items.Length == 0)
		{
			_resultLabel.Text = "Aucun objet dans l'inventaire !";
			return;
		}

		_currentSlot = slot;
		_mergeableItemsList.DeselectAll();
		_selectPanel.Visible = true;
		_mergePanel.Visible = false;
		
		_mergeButton.Disabled = string.IsNullOrEmpty(_selectedObject1) || 
							   string.IsNullOrEmpty(_selectedObject2);
	}
	
	private void OnMergePressed()
	{
		GameManager gameManager = GetNode<GameManager>("/root/GameManager");
		if (gameManager.Merge == null) return;
		
		var result = gameManager.Merge.AttemptMerge(new List<string>() { _selectedObject1, _selectedObject2 });
		
		if (result.Success)
		{
			_resultLabel.Text = $"Creature created : {result.Creature.Data.Name} !";
			_resultLabel.AddThemeColorOverride("font_color", Colors.Green);
			
			_selectedObject1 = null;
			_selectedObject2 = null;
			_slot1Button.Text = "Item 1";
			_slot2Button.Text = "Item 2";
			_mergeButton.Disabled = true;
		}
		else
		{
			_resultLabel.Text = $"Fail : {result.Message}";
			_resultLabel.AddThemeColorOverride("font_color", Colors.Red);
		}
	}
	
	private void OnMergeableItemListItemSelected(long index)
	{
		string itemId = _mergeableItemsList.GetItemText((int)index);
		_mergeableItemsList.SetItemDisabled((int)index, true);
		if (_currentSlot == 1)
		{
			_selectedObject1 = itemId;
			_slot1Button.Text = $"{itemId}";
		}
		else
		{
			_selectedObject2 = itemId;
			_slot2Button.Text = $"{itemId}";
		}
		_selectPanel.Visible = false;
		_mergePanel.Visible = true;
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
