using MergeableItemData = Game.Items.Data.MergeableItemData;

namespace Game.Items.Domain;

public class MergeableItem
{
	public MergeableItemData Data { get;}
	public ItemQuality Quality { get; }

	public MergeableItem(MergeableItemData data, ItemQuality quality)
	{
		Data = data;
		Quality = quality;
	}
}
