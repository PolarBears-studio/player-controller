using System;
using System.Collections.Generic;
using Godot;

public partial class GameManager : Node3D
{
	
	private static GameManager _instance;
	public static GameManager Instance => _instance;
	
	private RandomNumberGenerator _randomNumberGenerator;
	
	public override void _Ready()
	{
		#if DEBUG
			PackedScene devScene = GD.Load<PackedScene>("res://Scenes/DevUtils.tscn");
			
			Node devUtilsInstance =  devScene.Instantiate();
			AddChild(devUtilsInstance);
		#endif
		
		_instance = this;
		
		// We use GameManager globally only in the cases where instances of classes should be Initialized when the game
		// starts
		
		_randomNumberGenerator = new RandomNumberGenerator();
		_randomNumberGenerator.Randomize();
	}
	
	// This method is located in GameManager because we should initialize random number generator
	
	// Move random functions to Global.cs
	public float GetRandomFloatBetween0And1()
	{
		// Return float in the following range [0, 1]
		return _randomNumberGenerator.Randf();
	}
	
	
}
