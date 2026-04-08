using Godot;
using System;

using ItemQuality = Game.Items.Domain.ItemQuality;

namespace Game.Items.Data;

[GlobalClass, Tool]
public partial class ItemQualityWeights : Resource
{
	private Godot.Collections.Array<ItemQualityWeight> _weights;
	
	[Export] public Godot.Collections.Array<ItemQualityWeight> Weights 
	{ 
		get => _weights;
		set 
		{
			if (_weights == value) return;
			
			DisconnectWeights();
			_weights = value;
			_UpdateTotalWeight();
			ConnectNewWeights();
		}
	}

	public float TotalWeight { get; private set; }
	
	public ItemQualityWeights()
	{
		if (_weights != null) return;
		ItemQuality[] itemQualities = (ItemQuality[])Enum.GetValues(typeof(ItemQuality));
		int count = itemQualities.Length;
		if (_weights == null || _weights.Count != count)
		{
			_weights = new Godot.Collections.Array<ItemQualityWeight>(new ItemQualityWeight[count]);
			for (int i = 0; i < count; i++)
			{
				_weights[i] = new ItemQualityWeight(itemQualities[i]);
			}
		}
	}
	
	public override Godot.Collections.Array<Godot.Collections.Dictionary> _GetPropertyList()
	{
		Godot.Collections.Array<Godot.Collections.Dictionary> properties = [];
		
		properties.Add(new Godot.Collections.Dictionary
		{
			{ "name", nameof(TotalWeight) },
			{ "type", (int)Variant.Type.Float },
			{ "usage", (int)(PropertyUsageFlags.Editor | PropertyUsageFlags.ReadOnly) }, // Ici on force le ReadOnly
			{ "hint", (int)PropertyHint.None }
		});
		
		return properties;
	}
	
	private void ConnectNewWeights()
	{
		if (_weights == null) return;
		foreach (var weight in _weights)
		{
			if (weight != null && !weight.IsConnected(ItemQualityWeight.SignalName.WeightChanged, new Callable(this, MethodName._UpdateTotalWeight)))
			{
				weight.Connect(ItemQualityWeight.SignalName.WeightChanged, new Callable(this, MethodName._UpdateTotalWeight));
			}
		}
	}
	
	private void DisconnectWeights()
	{
		if (_weights == null) return;
		foreach (var weight in _weights)
		{
			if (weight != null && weight.IsConnected(ItemQualityWeight.SignalName.WeightChanged, new Callable(this, MethodName._UpdateTotalWeight)))
			{
				weight.Disconnect(ItemQualityWeight.SignalName.WeightChanged, new Callable(this, MethodName._UpdateTotalWeight));
			}
		}
	}
	
	private void _UpdateTotalWeight()
	{
		if (_weights == null) return;
		float total = 0;
		foreach (ItemQualityWeight weight in _weights)
		{
			if (weight == null) continue;
			
			total += weight.Weight;
		}
		
		TotalWeight = total;
	}
}
