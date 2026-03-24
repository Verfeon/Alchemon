using System.Linq;
using System.Collections.Generic;

using Type = Game.Creatures.Data.Type;
using Game.Utils.TypeChart;

namespace Game.Creatures.Domain;

/// <summary>
/// A registry for managing types.
/// </summary>
public partial class TypeRegistry : YARD.Registry<Type>
{
	private TypeChartLoader _typeChartLoader;

	public TypeRegistry() : base("res://Resources/Registries/TypeRegistry.tres")
	{
		_typeChartLoader = new TypeChartLoader(GetAllTypes());
	}

	public List<Type> GetAllTypes()
	{
		return LoadAllBlocking().Values.ToList();
	}

	public List<Type> GetAllTypesSorted()
	{
		return GetAllTypes().OrderBy(t => t.TypeEnum).ToList();
	}

	public float GetTypeEffectiveness(TypeEnum attack, TypeEnum defense)
	{
		return _typeChartLoader.GetEffectiveness(attack, defense);
	}
}
