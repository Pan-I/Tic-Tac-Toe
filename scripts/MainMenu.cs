using Godot;

namespace TicTacToe.scripts;

public partial class MainMenu : CanvasLayer
{
	[Signal] public delegate void OnePlayerGameEventHandler();
	[Signal] public delegate void TwoPlayerGameEventHandler();
	[Signal] public delegate void QuitEventHandler();
	
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
