using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;

using CreatureData = Game.Creatures.Data.CreatureData;

namespace Game.Core.Autoload;

public partial class CreatureDatabase : Node
{
	[Export] private string CreatureDataFolder = "res://Resources/CreatureDatas";
	private Dictionary<string, CreatureData> _byId;

	public override void _Ready()
	{	
		_byId = new Dictionary<string, CreatureData>();
		List<CreatureData> creatureDatas = CreatureDatas();

		foreach (var creature in creatureDatas)
		{
			if (_byId.ContainsKey(creature.Id))
			{
				GD.PrintErr($"creature ID duplicated : {creature.Id}");
				continue;
			}
			
			if (string.IsNullOrEmpty(creature.Id))
			{
				GD.PrintErr("creature without ID detected");
				continue;
			}


			_byId[creature.Id] = creature;
		}
	}
	
	private List<CreatureData> CreatureDatas()
	{
		var resources = new List<CreatureData>();

		var dir = DirAccess.Open(CreatureDataFolder);
		if (dir == null)
		{
			GD.PrintErr($"Unable to open folder : {CreatureDataFolder}");
			return resources;
		}

		dir.ListDirBegin();

		string fileName;
		int count = 0;
		while ((fileName = dir.GetNext()) != "")
		{
			if (dir.CurrentIsDir()) continue;
			if (!fileName.EndsWith(".tres") && !fileName.EndsWith(".res")) continue;

			string fullPath = CreatureDataFolder + "/" + fileName;

			var res = ResourceLoader.Load<CreatureData>(fullPath);
			if (res != null)
			{
				resources.Add(res);
				count++;
			}
		}

		dir.ListDirEnd();
		GD.Print($"Loaded {count} CreatureData");
		
		return resources;
	}

	public CreatureData GetRequired(string id)
	{
		if (_byId == null)
		{
			GD.PrintErr("CreatureDatabase not initialized");
			return null;
		}
		;
		if (!_byId.TryGetValue(id, out var creature))
		{
			throw new KeyNotFoundException(id);
		}
		return  creature;
	}
}
