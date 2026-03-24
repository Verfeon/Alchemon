using Godot;

using TypeEnum =  Game.Creatures.Domain.TypeEnum;

namespace Game.Creatures.Data;

[GlobalClass, Tool]
public partial class Type : Resource
{
	[Export] public TypeEnum TypeEnum { get; private set; }
	[Export] public string Name { get; private set; }
	[Export] public Color Color { get; private set; }
	[Export] public Texture2D Icon { get; private set; }
	[Export] public Godot.Collections.Array<TypeEnum> StrongAgainst { get; private set; }
	[Export] public Godot.Collections.Array<TypeEnum> WeakAgainst { get; private set; }
	[Export] public Godot.Collections.Array<TypeEnum> UselessAgainst { get; private set; }

	/// <summary>
	/// Determines if this type is strong against another type.
	/// </summary>
	/// <param name="other"></param>
	/// <returns></returns>
	public bool IsStrongAgainst(Type other)
	{
		return StrongAgainst.Contains(other.TypeEnum);
	}
	
	/// <summary>
	/// Determines if this type is weak against another type.
	/// </summary>
	/// <param name="other"></param>
	/// <returns></returns>
	public bool IsWeakAgainst(Type other)
	{
		return WeakAgainst.Contains(other.TypeEnum);
	}

	/// <summary>
	/// Determines if this type is useless against another type.
	/// </summary>
	/// <param name="other"></param>
	/// <returns></returns>
	public bool IsUselessAgainst(Type other)
	{
		return UselessAgainst.Contains(other.TypeEnum);
	}
}
