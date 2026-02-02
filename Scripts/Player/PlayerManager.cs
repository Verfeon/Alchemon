using Godot;
using System;
using System.Collections.Generic;

namespace Game.Player;

public partial class PlayerManager : Node
{
	private Vector2 _playerPosition;
	private string _currentMapId;
	
	[Signal]
	public delegate void PlayerPositionChangedEventHandler(float x, float y);
	
	[Signal]
	public delegate void MapChangedEventHandler(string mapId);
	
	public void SetPlayerPosition(Vector2 position)
	{
		_playerPosition = position;
		EmitSignal(SignalName.PlayerPositionChanged, position.X, position.Y);
	}
	
	public Vector2 GetPlayerPosition()
	{
		return _playerPosition;
	}
	
	public void SetCurrentMap(string mapId)
	{
		_currentMapId = mapId;
		EmitSignal(SignalName.MapChanged, mapId);
	}
	
	public string GetCurrentMap()
	{
		return _currentMapId;
	}
}
