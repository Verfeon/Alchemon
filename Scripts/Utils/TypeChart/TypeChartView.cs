using Godot;
using System;
using System.Collections.Generic;

using TypeEnum = Game.Creatures.Domain.TypeEnum;
using Type = Game.Creatures.Data.Type;

namespace Game.Utils.TypeChart;

[Tool]
public partial class TypeChartView : Control
{
	[Export] public string TypesFolder = "res://Resources/Types";
	[ExportToolButton("Load Type Chart")]
	public Callable BuildButton => Callable.From(Build);

	private GridContainer grid;

	public override void _Ready()
	{
		if (!Engine.IsEditorHint())
			return;

		Build();
	}

	private Node GetEditorOwner()
	{
		return GetTree().EditedSceneRoot ?? this;
	}

	private void AddNode(Node parent, Node child)
	{
		parent.AddChild(child);
		child.Owner = GetEditorOwner();
	}

	public void Rebuild()
	{
		Build();
	}

	private void Build()
	{
		GD.Print("Build Type Chart");

		if (grid != null)
			grid.Free();

		var types = LoadTypes();
		var enums = (TypeEnum[])Enum.GetValues(typeof(TypeEnum));
		int count = enums.Length;

		grid = new GridContainer();
		grid.Columns = count + 1;

		AddNode(this, grid);

		AddEmptyCorner();

		foreach (var def in enums)
			AddHeader(types, def);

		foreach (var atk in enums)
		{
			AddHeader(types, atk);

			foreach (var def in enums)
			{
				float mult = GetMultiplier(types, atk, def);
				AddCell(mult);
			}
		}
	}

	private Dictionary<TypeEnum, Type> LoadTypes()
	{
		var dict = new Dictionary<TypeEnum, Type>();

		var dir = DirAccess.Open(TypesFolder);
		dir.ListDirBegin();

		string file;

		while ((file = dir.GetNext()) != "")
		{
			if (!file.EndsWith(".tres") && !file.EndsWith(".res"))
				continue;

			var type = ResourceLoader.Load<Type>(TypesFolder + "/" + file);

			if (type != null)
				dict[type.TypeEnum] = type;
		}

		dir.ListDirEnd();

		return dict;
	}

	private float GetMultiplier(Dictionary<TypeEnum, Type> types, TypeEnum atk, TypeEnum def)
	{
		if (!types.TryGetValue(atk, out var type))
			return 1f;

		if (type.StrongAgainst.Contains(def))
			return 2f;

		if (type.WeakAgainst.Contains(def))
			return 0.5f;

		if (type.UselessAgainst.Contains(def))
			return 0f;

		return 1f;
	}

	private void AddEmptyCorner()
	{
		var spacer = new Control();
		spacer.CustomMinimumSize = new Vector2(48, 48);
		AddNode(grid, spacer);
	}

	private void AddHeader(Dictionary<TypeEnum, Type> types, TypeEnum typeEnum)
	{
		var container = new CenterContainer();
		container.CustomMinimumSize = new Vector2(48, 48);

		if (types.TryGetValue(typeEnum, out var type) && type.Icon != null)
		{
			var icon = new TextureRect();
			icon.Texture = type.Icon;
			icon.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
			icon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
			icon.CustomMinimumSize = new Vector2(32, 32);

			AddNode(container, icon);
		}
		else
		{
			var label = new Label();
			label.Text = typeEnum.ToString();
			label.HorizontalAlignment = HorizontalAlignment.Center;
			label.VerticalAlignment = VerticalAlignment.Center;

			AddNode(container, label);
		}

		AddNode(grid, container);
	}
	
	private void AddCell(float value)
	{
		var panel = new PanelContainer();
		panel.CustomMinimumSize = new Vector2(48, 32);

		var style = new StyleBoxFlat();
		style.BgColor = GetColor(value);
		style.BorderWidthBottom = 1;
		style.BorderWidthTop = 1;
		style.BorderWidthLeft = 1;
		style.BorderWidthRight = 1;
		style.BorderColor = new Color(0, 0, 0, 0.25f);

		panel.AddThemeStyleboxOverride("panel", style);

		var label = new Label();
		label.Text = value.ToString("0.##");
		label.HorizontalAlignment = HorizontalAlignment.Center;
		label.VerticalAlignment = VerticalAlignment.Center;

		AddNode(grid, panel);
		AddNode(panel, label);
	}

	private Color GetColor(float value)
	{
		if (value == 2f)
			return new Color(0.5f, 1f, 0.5f);

		if (value == 0.5f)
			return new Color(1f, 0.5f, 0.5f);

		if (value == 0f)
			return new Color(0.6f, 0.6f, 0.6f);

		return new Color(1f, 1f, 1f);
	}
}
