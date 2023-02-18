using Godot;
using System;
using XWorldLibrary;
public partial class InventoryTest : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var mechaNode = new MechaNode();
		GD.Print(mechaNode.Info());
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
