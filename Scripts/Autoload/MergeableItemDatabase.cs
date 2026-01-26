using Godot;
using System.Collections.Generic;

using MergeableItemData = Game.Resources.MergeableItemData;

namespace Game.Autoload;

public partial class MergeableItemDatabase : Node
{
	[Export] private string MergeableItemDataFolder = "res://Resources/MergeableItemDatas";
	private Dictionary<string, MergeableItemData> _byId;

	public override void _EnterTree()
	{
		_byId = new Dictionary<string, MergeableItemData>();
		List<MergeableItemData> mergeableItemDatas = LoadAllMergeableItemDatas();

		foreach (var item in mergeableItemDatas)
		{
			if (_byId.ContainsKey(item.Id))
			{
				GD.PrintErr($"item ID dupliqué : {item.Id}");
				continue;
			}

			_byId[item.Id] = item;
		}
	}
	
	private List<MergeableItemData> LoadAllMergeableItemDatas()
	{
		var resources = new List<MergeableItemData>();

		var dir = DirAccess.Open(MergeableItemDataFolder);
		if (dir == null)
		{
			GD.PrintErr($"Impossible d'ouvrir le dossier : {MergeableItemDataFolder}");
			return resources;
		}

		dir.ListDirBegin();

		string fileName;
		int count = 0;
		while ((fileName = dir.GetNext()) != "")
		{
			if (dir.CurrentIsDir()) continue;
			if (!fileName.EndsWith(".tres") && !fileName.EndsWith(".res")) continue;

			string fullPath = MergeableItemDataFolder + "/" + fileName;

			var res = ResourceLoader.Load<MergeableItemData>(fullPath);
			if (res != null)
			{
				resources.Add(res);
				count++;
			}
		}

		dir.ListDirEnd();
		GD.Print($"Loaded {count} MergeableItemData");
		
		return resources;
	}

	public MergeableItemData Get(string id)
	{
		if (_byId == null)
		{
			GD.PrintErr("MergeableItemDatabase not initialized");
			return null;
		}
		_byId.TryGetValue(id, out var item);
		if (item == null)
		{
			GD.PrintErr($"Couldn't find mergeable item with id : {id}");
		}
		return  item;
	}
}
