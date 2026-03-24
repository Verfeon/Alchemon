using Godot;
using Game.Utils.Random;

using CreatureData = Game.Creatures.Data.CreatureData;
using CreatureNode = Game.Creatures.Presentation.CreatureNode;

namespace Game.Creatures.Domain;

/// <summary>
/// This class is responsible for spawning creatures in the game world. 
/// It uses a PackedScene to instantiate CreatureNodes, which represent the visual and interactive aspect of the creature in the game. 
/// The spawner can be configured with a list of possible CreatureData, from which it will randomly select one to create a new creature instance. 
/// This allows for dynamic and varied encounters with creatures in the game world, as each spawn can result in a different creature being created based on the provided data.
/// </summary>
public partial class CreatureSpawner : Node2D
{
	[Export] private PackedScene _creatureScenePath;
	[Export] private Godot.Collections.Array<CreatureData> _possibleCreatures;
	
	private IRandom _random = new GodotRandomAdapter();
	
	public override void _Ready()
	{
		SpawnRandomCreature();
	} 
	
	/// <summary>
	/// Spawns a random creature from the list of possible creatures defined in the spawner's configuration.
	/// </summary>
	public void SpawnRandomCreature()
	{
		if (_possibleCreatures.Count == 0)
		{
			GD.PushError("There is no creature data available for spawning.");
			return;
		}
		Creature creature = new Creature(_possibleCreatures[_random.NextInt(0, _possibleCreatures.Count - 1)], 10, new GodotRandomAdapter());
		
		CreatureNode newCreature = _creatureScenePath.Instantiate<CreatureNode>();
		newCreature.Setup(creature, true);
		
		AddChild(newCreature);
	}
}
