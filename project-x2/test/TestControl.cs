using Godot;
using System;
using XWorldLibrary;
public partial class TestControl : Control
{
	public string info;
	private Button _button;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print("_ready() TestControl.cs");
		var mechaNode = new MechaNode();
		GD.Print(mechaNode.Info());

		info = mechaNode.Info();

		_button = GetNode<Button>("Button");
		_button.Text  = info;
	}

	public string GetInfo() {
		var mechaNode = new MechaNode();
		GD.Print(mechaNode.Info());
		
		return mechaNode.Info();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
