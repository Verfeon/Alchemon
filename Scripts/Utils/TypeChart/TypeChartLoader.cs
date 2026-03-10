using Godot;
using System;
using System.Collections.Generic;
using Game.Creatures.Data;
using Game.Creatures.Domain;

using TypeEnum = Game.Creatures.Domain.TypeEnum;
using Type = Game.Creatures.Data.Type;

namespace Game.Utils.TypeChart;

public static class TypeChartLoader
{
	public static Dictionary<TypeEnum, Type> Types = new();
	public static float[,] Chart;

	public static void LoadTypes(string folderPath)
	{
		Types.Clear();

		var dir = DirAccess.Open(folderPath);
		if (dir == null)
		{
			GD.PushError($"Type folder not found: {folderPath}");
			return;
		}

		dir.ListDirBegin();
		string file;

		while ((file = dir.GetNext()) != "")
		{
			if (dir.CurrentIsDir())
				continue;

			if (!file.EndsWith(".tres") && !file.EndsWith(".res"))
				continue;

			string path = folderPath + "/" + file;
			var type = ResourceLoader.Load<Type>(path);

			if (type != null)
				Types[type.TypeEnum] = type;
		}

		dir.ListDirEnd();

		BuildChart();
	}

	private static void BuildChart()
	{
		int count = Enum.GetValues(typeof(TypeEnum)).Length;
		Chart = new float[count, count];

		for (int atk = 0; atk < count; atk++)
		for (int def = 0; def < count; def++)
			Chart[atk, def] = 1f;

		foreach (var type in Types.Values)
		{
			int atk = (int)type.TypeEnum;

			foreach (var strong in type.StrongAgainst)
				Chart[atk, (int)strong] = 2f;

			foreach (var weak in type.WeakAgainst)
				Chart[atk, (int)weak] = 0.5f;

			foreach (var useless in type.UselessAgainst)
				Chart[atk, (int)useless] = 0f;
		}
	}

	public static float GetEffectiveness(TypeEnum attack, TypeEnum defense)
	{
		return Chart[(int)attack, (int)defense];
	}
}
