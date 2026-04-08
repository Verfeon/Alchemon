using Godot;
using System;

using ItemQuality = Game.Items.Domain.ItemQuality;

namespace Game.Items.Data;

[GlobalClass, Tool]
public partial class ItemQualityWeight : Resource
{
	[Signal] public delegate void WeightChangedEventHandler();
	private float _weight = 1f;
	
	[Export]public ItemQuality Quality;
	
	[Export]
	public float Weight
	{
	get => _weight;
		set
		{
			_weight = value;
			EmitSignal(SignalName.WeightChanged);
		}
	}
	
	public ItemQualityWeight()
	{
		Quality = ((ItemQuality[])Enum.GetValues(typeof(ItemQuality)))[0];
		_weight = 1f;
	}
	
	public ItemQualityWeight(ItemQuality quality)
	{
		Quality = quality;
		_weight = 1f;
	}
}
