using Godot;

namespace TicTacToe.scripts;

public partial class QuickMenu : CanvasLayer
{
	[Signal] public delegate void MainMenuEventHandler(); 
	[Signal] public delegate void QuitEventHandler(); 
	
	private void _on_quit_button_pressed()
	{
		EmitSignal(SignalName.Quit);
	}

	private void _on_main_menu_button_pressed()
	{
		EmitSignal(SignalName.MainMenu);
	}
}



