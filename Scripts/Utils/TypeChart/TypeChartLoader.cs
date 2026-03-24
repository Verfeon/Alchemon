using System;
using System.Collections.Generic;
using System.Linq;

using TypeEnum = Game.Creatures.Domain.TypeEnum;
using Type = Game.Creatures.Data.Type;
using TypeRegistry = Game.Creatures.Domain.TypeRegistry;

namespace Game.Utils.TypeChart;

/// <summary>
/// Loads and manages the type chart for the game.
/// </summary>
public class TypeChartLoader
{
	private List<Type> _types = new();
	private float[,] _chart;

	public TypeChartLoader(List<Type> types)
	{
		_types = types;
		BuildChart();
	}

	/// <summary>
	/// Builds the type effectiveness chart based on the loaded types and their relationships.
	/// </summary>
	private void BuildChart()
	{
		int count = Enum.GetValues(typeof(TypeEnum)).Length;
		_chart = new float[count, count];

		for (int atk = 0; atk < count; atk++)
		{
			for (int def = 0; def < count; def++)
			{
				_chart[atk, def] = 1f;
			}
		}

		foreach (var type in _types)
		{
			int atk = (int)type.TypeEnum;

			foreach (var strong in type.StrongAgainst)
			{
				_chart[atk, (int)strong] = 2f;
			}

			foreach (var weak in type.WeakAgainst)
			{
				_chart[atk, (int)weak] = 0.5f;
			}

			foreach (var useless in type.UselessAgainst)
			{
				_chart[atk, (int)useless] = 0f;
			}
		}
	}

	/// <summary>
	/// Gets the effectiveness of an attack type against a defense type./// If the type chart has not been built yet, it loads the types from resources and builds the chart before returning the effectiveness multiplier.
	/// </summary>
	/// <param name="attack"></param>
	/// <param name="defense"></param>
	/// <returns></returns>
	public float GetEffectiveness(TypeEnum attack, TypeEnum defense)
	{
		return _chart[(int)attack, (int)defense];
	}
}
