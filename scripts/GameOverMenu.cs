using Godot;

namespace TicTacToe.scripts;

public partial class GameOverMenu : CanvasLayer
{
	[Signal] public delegate void RestartEventHandler();
	[Signal] public delegate void MainMenuEventHandler(); 
	private void _on_play_again_button_pressed()
	{
		EmitSignal(SignalName.Restart);
	}
	private void _on_main_menu_button_pressed()
	{
		EmitSignal(SignalName.MainMenu);
	}
}
