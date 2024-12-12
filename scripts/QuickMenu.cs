/*
 Tic-Tac-Toe with both two-player game play and single-player game play with different difficulties.
Copyright (C) 2024  Ian Pommer

This program is free software; you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation; either version 2 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License along
with this program; if not, write to the Free Software Foundation, Inc.,
51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

The author can be contacted at pan.i.githubcontact@gmail.com
*/

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



