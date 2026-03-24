using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

using TypeRegistry = Game.Creatures.Domain.TypeRegistry;
using TypeEnum = Game.Creatures.Domain.TypeEnum;
using Type = Game.Creatures.Data.Type;

namespace Game.Utils.TypeChart;

/// <summary>
/// A control that displays the type effectiveness chart in the Godot editor.
/// It loads type data from resources and builds a visual grid showing the effectiveness of each type against every other type.
/// </summary>
[Tool]
public partial class TypeChartView : Control
{
	[Export] public string TypesFolder = "res://Resources/Types";
	[ExportToolButton("Load Type Chart")]
	public Callable BuildButton => Callable.From(Build);

	private GridContainer grid;
	private readonly Vector2 _cellSize = new(48, 48);
	private const int BorderWidth = 1;

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

	/// <summary>
	/// Builds the type effectiveness chart by loading type data from resources and creating a grid of panels that visually represent the effectiveness of each type against every other type.
	/// Each cell in the grid is colored based on the effectiveness multiplier (e.g., green for super effective, red for not very effective, gray for no effect) and displays the multiplier value.
	/// </summary>
	private void Build()
	{
		GD.Print("Build Type Chart");

		if (grid != null)
		{
			grid.Free();
		}

		TypeRegistry typeRegistry = new TypeRegistry();
		List<Type> types = typeRegistry.GetAllTypesSorted();
		int count = types.Count;

        grid = new GridContainer
        {
            Columns = count + 1
        };

        AddNode(this, grid);

		AddEmptyCorner();

		foreach (var def in types)
		{
			AddHeader(def);
		}

		foreach (var atk in types)
		{
			AddHeader(atk);

			foreach (var def in types)
			{
				float multiplier = typeRegistry.GetTypeEffectiveness(atk.TypeEnum, def.TypeEnum);
				AddCell(multiplier);
			}
		}
	}

	/// <summary>
	/// Adds an empty corner cell to the top-left of the type chart grid to align the headers correctly.
	/// </summary>
	private void AddEmptyCorner()
	{
        var spacer = new Control
        {
            CustomMinimumSize = _cellSize
        };
        AddNode(grid, spacer);
	}

	private void AddHeader(Type type)
	{
        var container = new CenterContainer
        {
            CustomMinimumSize = _cellSize
        };

		AddNode(grid, container);

		var icon = new TextureRect
		{
			Texture = type.Icon,
			ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
			StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
			CustomMinimumSize = _cellSize
		};

		AddNode(container, icon);  
	}
	
	private void AddCell(float value)
	{
        var panel = new PanelContainer
        {
            CustomMinimumSize = _cellSize
        };

        var style = new StyleBoxFlat
        {
            BgColor = GetColor(value),
            BorderWidthBottom = BorderWidth,
            BorderWidthTop = BorderWidth,
            BorderWidthLeft = BorderWidth,
            BorderWidthRight = BorderWidth,
            BorderColor = new Color(0, 0, 0, 0.25f)
        };

        panel.AddThemeStyleboxOverride("panel", style);

        var label = new Label
        {
            Text = value.ToString("0.##"),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        AddNode(grid, panel);
		AddNode(panel, label);
	}

	/// <summary>
	/// Gets the color corresponding to the effectiveness multiplier value for a type matchup.
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	private Color GetColor(float value)
	{
		if (value == 2f)
		{
			return new Color(0.5f, 1f, 0.5f); // Green
		}

		if (value == 0.5f)
		{
			return new Color(1f, 0.5f, 0.5f); // Red
		}

		if (value == 0f)
		{
			return new Color(0.6f, 0.6f, 0.6f); // Gray
		}

		return new Color(1f, 1f, 1f); // White
	}
}
