using Godot;

namespace TicTacToe;

public partial class MainMenu : CanvasLayer
{
	[Signal] public delegate void OnePlayerGameEventHandler();

	[Signal] public delegate void TwoPlayerGameEventHandler();
	[Signal] public delegate void QuitEventHandler();

	public override void _Ready()
	{
		//var test = GetNode<Panel>("MainMenuPanel").GetNode<Button>("1PlayerButton");
		//test.Disabled = true;
	}
	
	private void _on_player_button_pressed()
	{
		EmitSignal(SignalName.OnePlayerGame);
	}
	private void _on_2player_button_pressed()
	{
		EmitSignal(SignalName.TwoPlayerGame);
	}
	private void _on_quit_button_pressed()
	{
		EmitSignal(SignalName.Quit);
	}

}
