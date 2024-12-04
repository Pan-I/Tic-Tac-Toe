using System;
using System.Threading.Tasks;
using Godot;

namespace TicTacToe.scripts;

public partial class Main : Node
{
	[Export] public PackedScene CircleScene { get; set; }
	[Export] public PackedScene CrossScene { get; set; }
	[Export] public PackedScene WinnerBarScene {get; set;}
	
	#region Fields Region
	private static int _player; //which player is making a move
	private static int[,] _gridData; //stores moves of game on board
	private static bool _1PlayerGame; //true if a single player game
	private static int _moves; //how many moves have been made
	private static int _winner; //which player, if any is the winner
	private static int _rowSum; //adds row values to check for victory conditions
	private static int _rowWin; //which row won the match
	private static int _colSum; //adds column values to check for victory conditions
	private static int _colWin; //which column won the match
	private static int _diagSum1; //adds one diagonal value to check for victory conditions
	private static int _diagSum2; //adds other diagonal value to check for victory conditions
	private static bool _smartCpu; //true if smart cpu option is chosen
	private static bool _menuSwitch; //true if altering menu actions and looks
	private static bool _cpuPause; //true used to pause input by player
	private static int _boardSize; //size of the game board, in pixels
	private static int _cellSize; //size of a single cell of the board, in pixels
	//Godot data-type fields;
	private static Vector2I _gridPosition;  //translates pixels of input to coordinates on the board
	private Node2D _tempMarker; //switches sprite to indicate who is next
	private Vector2I _playerPanelPosition; //the position of the nex player panel
	private Vector2I _winnerPanelPosition; //the position of the winner panel
	
	#endregion Fields Region	

	#region Entry and Events Region
		//Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		BoardPrep();	//get size and cell size of board and other panels for gameplay
		GetNode<AudioStreamPlayer>("LobbyMusic").Play(); //play lobby music upon start
	}
	
		//We will watch for user input to control all aspects of the gameplay.
		//Under the right conditions we will trigger the game logic.
	/// <summary>
	/// This is an override (async) method gets user input (click), and filters if the click is on the board.
	/// The method is async for cpu pause after player moves.
	/// </summary>
	/// <param name="event"></param>
	public override async void _Input(InputEvent @event)
	{
		//	if (@event is InputEventMouseMotion eventMouseMotion)
		//	{GD.Print("Mouse Motion at: ", eventMouseMotion.Position);}// Mouse in viewport coordinates.
		//	GD.Print("Viewport Resolution is: ", GetViewport().GetVisibleRect().Size); // Print the size of the viewport.
		//Check if the board can accept value inputs yet, and event is a mouse click.
		if (_gridData != null && !_cpuPause && @event is InputEventMouseButton eventMouseButton)
		{
			//Check if left mouse button pressed.
			if (eventMouseButton.ButtonIndex == MouseButton.Left && eventMouseButton.Pressed)
			{
				//Check if mouse is on game board.
				if (eventMouseButton.Position.X < _boardSize)
				{
					//convert mouse position into grid location
					_gridPosition = new Vector2I((int)eventMouseButton.Position.X / _cellSize, (int)eventMouseButton.Position.Y / _cellSize); 
					//Check if cell does not already hold a player's marker
					if (_gridData[_gridPosition.Y, _gridPosition.X] == 0)
					{
						//since there is no marker in the cell, start handling all the game logic
						await GameHandling();
					}
				}
			}
		}
		else if(@event is InputEventKey { Keycode: Key.Escape, Pressed: true })
		{
			if (GetNode<CanvasLayer>("MainMenu").Visible 
				 || GetNode<CanvasLayer>("GameOverMenu").Visible)
			{
				GetTree().Quit();
			}
		}
	}
	#endregion Entry and Events Region
	
	#region Game Handling Region
	/// <summary>
	/// Sets some of the fundamental fields for the area of gameplay like the board size and panel positions.
	/// </summary>
	private void BoardPrep()
	{
		//If image has been scaled in editor you want to take that in to account.
		//This can also be used to scale the markers placed as well.
		_boardSize = (int)(GetNode<Sprite2D>("Board").Scale.X * GetNode<Sprite2D>("Board").Texture.GetWidth());
		_cellSize = _boardSize / 3;		//divide board size by 3 to get size of individual cell.
			//GD.Print("Board size: ", _boardSize);
			//GD.Print("Board size: ", _cellSize);
		_playerPanelPosition = (Vector2I)GetNode<Panel>("NextPanel").Position;//get coordinates of panel on right side for placing "next player" markers
		_winnerPanelPosition = (Vector2I)GetNode<CanvasLayer>("GameOverMenu").GetNode<Panel>("WinnerPanel").Position; //get coordinates of panel displayed for winner.
	}
	
	/// <summary>
	/// Clears the board for main menu.
	/// Sets _gridData to null to prevent placing while menus are displayed.
	/// Sets all game and victory condition fields to 0
	/// </summary>
	private void ClearBoard()
	{
		_gridData = null; //makes sure no clicks can accidentally change board
		GetNode<CanvasLayer>("GameOverMenu").Hide(); //hide the game over menu
		//clear the area of sprites
		GetTree().CallGroup("circles", "queue_free");
		GetTree().CallGroup("crosses", "queue_free");
		GetTree().CallGroup("winnerBar", "queue_free");
		_winner = _moves = _rowSum = _rowWin = _colSum = _colWin = _diagSum1 = _diagSum2 = 0; //set all the trackers to 0;
	}
	
	/// <summary>
	/// Resets all board stats and sprites for a new game. Will hide and show relevant menu panels.
	/// Will randomize starting player field. Will unpause tree, and change music.
	/// If 1-player will call cpu move if cpu is starting.
	/// </summary>
	private void NewGame()
	{ 
		//hide and show relevant panels and labels
		GetNode<CanvasLayer>("MainMenu").Hide();
		GetNode<CanvasLayer>("QuickMenu").Show();
		GetNode<Label>("PlayerLabel").Show();
		if (_winner == 0)
		{
			//create a Random object for randomizing starting player
			Random rnd = new Random();
			int newPlayer;
			do
			{
				_player = newPlayer = rnd.Next(-1, 2);
			} while (newPlayer == 0 || newPlayer == 2); //starting move randomly switches.
		}
		else { _player = (_winner *-1); }
		ClearBoard(); //make sure board is clean, sets _gridData to null
		//create a marker to show starting players turn
		CreateMarker(_player, _playerPanelPosition + new Vector2I(_cellSize /2, _cellSize/2), true);
		//create new, 'empty' board data
		_gridData = new [,] 
		{
			{0, 0, 0},
			{0, 0, 0},
			{0, 0, 0}
		};
		//PrintGridData(_gridData);
		_cpuPause = false; //unpause game
		//stop and start relevant audio
		GetNode<AudioStreamPlayer>("LobbyMusic").Stop();
		GetNode<AudioStreamPlayer>("BeginGameSFX").Play();
		GetNode<AudioStreamPlayer>("GameMusic").Play();
		//check for and start cpu logic if singly player and cpu is starting
		if (_1PlayerGame && _player == -1)
		{
			_gridPosition = CpuPlayer.CpuMove(_gridData, _smartCpu);
			GameHandling();
		}
	}
	
	// We will modify the board data, then we will create a marker on the cell modified.
	// After that, we want to check some game-over conditions
	// If the game is not over, switch players
	/// <summary>
	/// Uses the input and place markers, depending on game-mode and player turn.
	/// Then the method will call methods to check game over conditions.
	/// If the game is not over method will call switch player methods.
	/// </summary>
	internal async Task GameHandling()
	{
		ModifyGridData(); //change board data
		CreateMarker(_player, _gridPosition); //create marker
		_moves++; //increment move field for stalemate check
		if (CheckWinner() != 0) //someone won
		{
			GameOver(_winner); //call the game over menu and display the winner
		}
		else if (_moves == 9) //tie, _winner should be 0 so no winner will display
		{
			GameOver(_winner); //call the game over menu and display the stalemate
		}
		else //the game is still going
		{
			//play the drawing sfx for the move
			if (_player == 1) {GetNode<AudioStreamPlayer>("CircleDrawSFX").Play();}
			else {GetNode<AudioStreamPlayer>("CrossDrawSFX").Play();}
			SwitchPlayer(); //call the switch player method

			//if single player, and player 1 just went, start cpu logic
			if (_1PlayerGame && _player == -1)
			{
				await DelayMethod(); //small delay to give SFX time to play, also feels more natural
				_gridPosition = CpuPlayer.CpuMove(_gridData, _smartCpu); //call the CPU logic with the current grid data and difficulty setting
				GameHandling();
			}
		}
	}
	
	/// <summary>
	/// Uses the converted cell coordinates of the mouse click to mark that spot with the player's value (-1 or 1).
	/// Modifies _gridPosition for placing marker
	/// </summary>
	static void ModifyGridData()
	{
		//mark the player value into the grid data, at the relevant cell
		_gridData[_gridPosition.Y, _gridPosition.X] = _player;
		//multiple grid coord by cell size to for placing marker
		_gridPosition *= _cellSize;
		_gridPosition[0] += _cellSize /2;
		_gridPosition[1] += _cellSize /2;
			//PrintGridData(_gridData);
	}
	
	/// <summary>
	/// Creates a marker sprite on the board for the player on the board using the params given.
	/// will also switch the temporary sprite for the next player indicator.
	/// </summary>
	/// <param name="player">the int value for the player</param>
	/// <param name="position">coordinates for the center of the cell to place the marker</param>
	/// <param name="switchTemp">bool to switch the image of the next player sprite.</param>
	private void CreateMarker(int player, Vector2I position, bool switchTemp=false)
	{
		//Create marker node and add as a child
		var marker = player == 1 ? CircleScene.Instantiate<Node2D>() : CrossScene.Instantiate<Node2D>();
		// If there is no winner yet, create the child node
		if (_winner == 0)
		{
			AddChild(marker);
		}
		else
		{
			GetNode<CanvasLayer>("GameOverMenu").AddChild(marker); //add marker to winner panel
			//stop and start relevant audio
			GetNode<AudioStreamPlayer>("GameMusic").Stop();
			GetNode<AudioStreamPlayer>("LobbyMusic").Play();
		}
		//put marker at parameter coordinates
		marker.Scale = GetNode<Sprite2D>("Board").Scale * new Vector2((float).9, (float).9);
		marker.Position = new Vector2(position.X, position.Y);
		//if true, switch the next player marker
		if (switchTemp)
		{
			_tempMarker = marker;
		}
	}
	
	/// <summary>
	/// Switches the player field and sets the next player marker.
	/// </summary>
	private void SwitchPlayer()
	{
		_player *= -1; //switch player;
		_tempMarker.QueueFree(); //clear next player sprite
		CreateMarker(_player, _playerPanelPosition + new Vector2I(_cellSize / 2, _cellSize / 2), true); //display next player
	}
	
	/// <summary>
	/// Adds up the values in the cells on the board to check for a winner.
	/// Also alters the *Sum fields so that the method for marking the winner
	/// can evaluate which cells were used to win.
	/// </summary>
	/// <returns>The int value (-1 / 1) of the player that won.</returns>
	public static int CheckWinner()
	{
		//Add up markers in each diagonal
		//if any sum value is 3 or -3, then all symbols are the same
		_diagSum1 = _gridData[0, 0] + _gridData[1, 1] + _gridData[2, 2];
		_diagSum2 = _gridData[0, 2] + _gridData[1, 1] + _gridData[2, 0];
		if (_diagSum1 == 3 || _diagSum2 == 3)
		{
			return _winner = 1; //player-1 won
		}
		if (_diagSum1 == -3 || _diagSum2 == -3)
		{
			return _winner = -1; //player-2 won
		}
		//Add up markers in each row, column
		for (int i = 0; i < 3; i++)
		{
			_rowSum = _gridData[i, 0] + _gridData[i, 1] + _gridData[i, 2];
			if (_rowSum == 3)
			{
				_rowWin = (i + 1); //add one to assign row number
				return _winner = 1; //player-1 won
			}
			if (_rowSum == -3)
			{
				_rowWin = (i + 1); //add one to assign row number
				return _winner = -1; //player-2 won
			}
			_colSum = _gridData[0, i] + _gridData[1, i] + _gridData[2, i];
			if (_colSum == 3)
			{
				_colWin = (i + 1); //add one to assign column number
				return _winner = 1; //player-1 won
			}
			if (_colSum == -3)
			{
				_colWin = (i + 1); //add one to assign column number
				return _winner = -1; //player-2 won
			}
		}
		return _winner;
	}
	
	internal static void ResetWinnerCheckFields()
	{
		_winner = _rowSum = _rowWin = _colSum = _colWin = _diagSum1 = _diagSum2 = 0; //set all the trackers to 0;
	}
	
	/// <summary>
	/// Evaluates the parameter 'winner' to determine what to display.
	/// Shows user the game over menu, allowing to restart or return to main.
	/// </summary>
	/// <param name="winner">the int value of the player that one. If 0 is passed it is a tie and no marker will be made.</param>
	private void GameOver(int winner)
	{
			//GD.Print("Game Over");
		_tempMarker.QueueFree(); //clear next player sprite
		//hide and show relevant panels and labels
		GetNode<Label>("PlayerLabel").Hide(); 
		GetNode<CanvasLayer>("QuickMenu").Hide();
		GetNode<CanvasLayer>("GameOverMenu").Show();
		//if someone won, mark which cells won the game, and which player won
		if (winner != 0)
		{
			MarkWinnerOnBoard(); //places the winner bar, sets the correct sprite for which player won
			//nested ternary for displaying text for which player won.
			//not ideal for readability, possible TODO?
			var winnerMessage = winner == 1 ? "_winner: Player 1" : _1PlayerGame ?  "_winner: CPU" : "_winner: Player 2";
			//pass winnerMessage into labels' text values
			GetNode<CanvasLayer>("GameOverMenu").GetNode<Panel>("GameOverPanel").GetNode<Label>("WinnerLabel").Text = winnerMessage;
			GetNode<CanvasLayer>("GameOverMenu").GetNode<Label>("LabelShadow").Text = winnerMessage;
			//place a marker of the winning player at the position of the winner panel
			CreateMarker(winner, _winnerPanelPosition + new Vector2I(_cellSize / 2, _cellSize / 2));
		}
		//stalemate, play loss sound and display stalemate game over state
		if (winner == 0) 
		{
			GetNode<AudioStreamPlayer>("GameOver0SFX").Play(); //play the loss sound
			GetNode<CanvasLayer>("GameOverMenu").GetNode<Panel>("GameOverPanel").GetNode<Label>("WinnerLabel").Text =
				"Stalemate!";
			GetNode<CanvasLayer>("GameOverMenu").GetNode<Label>("LabelShadow").Text = "Stalemate!"; 
		}
		//player one (circles) won, play victory sound
		if (winner == 1) 
		{
			GetNode<AudioStreamPlayer>("Victory0SFX").Play(); //play the short victory sound
		}
		//defeat, play loss sound
		else 
		{
			GetNode<AudioStreamPlayer>("GameOver0SFX").Play(); //play the loss sound
		}
		_cpuPause = true; //pause the game
		//GetTree().Paused = true; //this would prevent using InputEvent
	}

	/// <summary>
	/// Marks the winner. Places the winner bar based on what 3 squares won the game.
	/// Calls a method to mark the winner symbol on the game over panel.
	/// </summary>
	private void MarkWinnerOnBoard()
	{
		//Create marker node and add as a child
		var winnerMarker = WinnerBarScene.Instantiate<Node2D>();
		AddChild(winnerMarker);
		winnerMarker.Scale = GetNode<Sprite2D>("Board").Scale; //scale to same size as board
		//start with the bar in the center of the screen.
		winnerMarker.Position = new Vector2(300, 300);
		//check winner fields to see where to place bar
		if (_colWin != 0) // if won with column condition 
		{ 
			winnerMarker.RotationDegrees = 90; //make sure to rotate winner bar vertical
 			switch (_colWin) //int represents which col won
			{
				case 1: winnerMarker.Position = new Vector2(100, 300); //places bar on left column
					break;
				//case 2 not needed since sprite is made at that location by default.
				case 3: winnerMarker.Position = new Vector2(500, 300); //places bar on right column
					break;
			}
		}
		else if (_rowWin != 0) // if won with row condition 
		{
			winnerMarker.RotationDegrees = 0; //make sure to reset winner bar horizontal
			switch (_rowWin) //int represents which row won
			{
				case 1: winnerMarker.Position = new Vector2(300, 100); //places bar on top row
					break;
				//case 2 not needed since sprite is made at that location by default.
				case 3: winnerMarker.Position = new Vector2(300, 500); //places bar on bottom row
					break;
			}
		}
		//otherwise it was a diagonal win.
		else if (_diagSum1 == 3 || _diagSum1 == -3) 
		{
			winnerMarker.RotationDegrees = 45; //bottom-left to top-right
		}
		else
		{
			winnerMarker.RotationDegrees = -45; //bottom-right to top-left
		}
	}
	
	//Short pause so CPU doesn't move right after, gives SFX a chance to play.
	/// <summary>
	/// Delays to give natural pause before CPU moves.
	/// Manipulates _cpuPause field to mimic unsubscribing from event. 
	/// </summary>
	private async Task DelayMethod()
	{
		_cpuPause = true;
		await Task.Delay(TimeSpan.FromMilliseconds(1250));
		_cpuPause = false;
	}

	#endregion Game Handling Region
	
	#region Menu Navigation
	/// <summary>
	/// Shows main menu
	/// Clears the board, and hides any gameplay related panels.
	/// Stops gameplay music, and starts lobby music.
	/// </summary>
	private void GoToMainMenu()
	{
		ClearBoard(); //wipe board of game data
		MenuSwitch(); //reset main menu switch bool
		GetNode<Label>("PlayerLabel").Hide(); //hide the label for next player
		GetNode<CanvasLayer>("QuickMenu").Hide(); //hide the quick menu
		GetNode<CanvasLayer>("MainMenu").Show(); //show the main menu
		//if the game music is playing, stop game music and start lobby music
		if (GetNode<AudioStreamPlayer>("GameMusic").Playing)
		{
			GetNode<AudioStreamPlayer>("GameMusic").Stop();
			GetNode<AudioStreamPlayer>("LobbyMusic").Play();
		}
		_cpuPause = false; //unpause the game
	}

	/// <summary>
	/// Alters the behavior and button-text of the main menu. Default parameter value will reset main menu
	/// </summary>
	/// <param name="flip"> if TRUE method will go to difficulty menu behavior. FALSE will reset to main menu </param>
	private void MenuSwitch(bool flip = false)
	{
		_menuSwitch = flip; //pass parameter value to field to maintain behavior in other methods.
		if (_menuSwitch) //if true, user has selected single player
		{
			//show difficulty texts
			GetNode<CanvasLayer>("MainMenu").GetNode<Panel>("MenuPanel").GetNode<Button>("1PlayerButton").Text = "Dumb CPU";
			GetNode<CanvasLayer>("MainMenu").GetNode<Panel>("MenuPanel").GetNode<Button>("2PlayerButton").Text = "Smart CPU";
			GetNode<CanvasLayer>("MainMenu").GetNode<Panel>("MenuPanel").GetNode<Button>("QuitButton").Text = "Back";
		}
		else //revert to main menu behavior
		{
			//show default texts
			GetNode<CanvasLayer>("MainMenu").GetNode<Panel>("MenuPanel").GetNode<Button>("1PlayerButton").Text = "1-Player";
			GetNode<CanvasLayer>("MainMenu").GetNode<Panel>("MenuPanel").GetNode<Button>("2PlayerButton").Text = "2-Player";
			GetNode<CanvasLayer>("MainMenu").GetNode<Panel>("MenuPanel").GetNode<Button>("QuitButton").Text = "Quit";
		}
		
	}
	#endregion Menu Navigation
	
	#region Signal Link Region
	private void _on_main_menu_one_player_game()
	{
		if (_menuSwitch)
		{
			_smartCpu = false; // setting cpu to dumb, will randomly choose empty cell with no logic.
			_1PlayerGame = true; //setting property to true to make sure single player logic is followed.
			NewGame(); //starting the game
		}
		else
		{
			MenuSwitch(true); //player has chosen single player game.
					 //The menu needs to be altered to display difficulty settings
		}
	}

	private void _on_main_menu_two_player_game()
	{
		// if true, user is one single player menu. Set difficulty to hard
		if (_menuSwitch)
		{
			_smartCpu = true;
			_1PlayerGame = true; //setting property to true to make sure single player logic is followed.
			NewGame(); //starting the game
		}
		else
		{
			_1PlayerGame = false; //setting the property to false so two people can play.
			NewGame(); //starting the game
		}
	}
	private void _on_main_menu_quit()
	{
		//if true, go 'back' to main instead of quitting
		if (_menuSwitch)
		{
			MenuSwitch(); //call method to reset menu switching bool, resetting menu
		}
		else
		{
			GetTree().Quit(); //closes application entirely
		}
	}
	private void _on_game_over_menu_restart()
	{
		NewGame(); //calls method to start a new game based on current game settings
	}
	private void _on_game_over_menu_main_menu()
	{
		MenuSwitch(); //call method to reset menu switching bool
		GoToMainMenu(); //calls main menu method
	}
	private void _on_quick_menu_main_menu()
	{
		MenuSwitch(); //call method to reset menu switching bool
		GoToMainMenu(); //calls main menu method
	}
	private void _on_quick_menu_quit()
	{
		GetTree().Quit(); //closes application entirely
	}
	
	private void _on_sfx_button_toggled(bool toggled_on)
	{
		GetNode<AudioStreamPlayer>("BeginGameSFX").VolumeDb = toggled_on ? 0 : -80;
		GetNode<AudioStreamPlayer>("Victory0SFX").VolumeDb = toggled_on ? 0 : -80;
		GetNode<AudioStreamPlayer>("GameOver0SFX").VolumeDb = toggled_on ? 0 : -80;
		GetNode<AudioStreamPlayer>("CircleDrawSFX").VolumeDb = toggled_on ? 0 : -80;
		GetNode<AudioStreamPlayer>("CrossDrawSFX").VolumeDb = toggled_on ? 0 : -80;
	}

	private void _on_music_button_toggled(bool toggled_on)
	{
		GetNode<AudioStreamPlayer>("LobbyMusic").VolumeDb = toggled_on ? 0 : -80;
		GetNode<AudioStreamPlayer>("GameMusic").VolumeDb = toggled_on ? 0 : -80;
	}
	
	#endregion Signal Link Region
	
	#region Debug Region
	/// <summary>
	/// Loops through the multidimensional array and prints the data in it.
	/// </summary>
	/// <param name="gridData"></param>
	private void PrintGridData(int[,] gridData)
	{
		for (int i = 0; i < gridData.GetLength(0); i++)
		{
			for (int j = 0; j < gridData.GetLength(1); j++)
			{
				GD.PrintRaw(gridData[i,j] + "\t");	//PrintRaw will print inline. adding a tab to space the columns out a bit
			}
			GD.Print();//prints a new row.
		}
	}
	#endregion Debug Region
}
