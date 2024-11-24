using System;
using System.Threading.Tasks;
using Godot;

namespace TicTacToe;

public partial class Main : Node
{
	[Export] public PackedScene CircleScene { get; set; }
	[Export] public PackedScene CrossScene { get; set; }
	[Export] public PackedScene WinnerBarScene {get; set;}
	
	#region Fields Region
	private int _player;
	private int _winner;
	private int _moves;
	private int[,] _gridData;
	private Vector2I _gridPosition;
	private int _boardSize;
	private int _cellSize;
	private int _rowSum;
	private int _rowWin;
	private int _colSum;
	private int _colWin;
	private int _diagSum1;
	private int _diagSum2;
	private bool _1PlayerGame;
	private bool _smartCpu;
	private bool _menuSwitch;
	private bool _cpuPause;

	private Node2D _tempMarker;
	private Vector2I _playerPanelPosition;
	private Vector2I _winnerPanelPosition;
	
	#endregion Fields Region	

	#region Entry and Events Region
	//Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		BoardPrep();	//get size and cell size of board and other panels for gameplay
		GetNode<AudioStreamPlayer>("LobbyMusic").Play(); //play lobby music upon start
	}
	
	//We will watch for user input to control all aspects of the gameplay.
	//Under the right conditions we will mark the cell clicked with the players piece
	//Then we will also check for win conditions, and either change to next player or end the game.
	/// <summary>
	/// This is an override (async) method gets user input (click), and filters if the click is on the board.
	/// The method is async for cpu pause after player moves.
	/// </summary>
	/// <param name="event"></param>
	public override async void _Input(InputEvent @event)
	{
		//if (@event is InputEventMouseMotion eventMouseMotion
		//	{GD.Print("Mouse Motion at: ", eventMouseMotion.Position);}// Mouse in viewport coordinates.
		//GD.Print("Viewport Resolution is: ", GetViewport().GetVisibleRect().Size); // Print the size of the viewport.
		//Check if the board can accept value inputs yet, and event is a mouse click.
		if (_gridData != null && !_cpuPause && @event is InputEventMouseButton eventMouseButton)
		{
			//Check if left mouse button pressed.
			if (eventMouseButton.ButtonIndex == MouseButton.Left && eventMouseButton.Pressed)
			{
				//Check if mouse is on game board.
				if (eventMouseButton.Position.X < _boardSize)
				{
					_gridPosition = new Vector2I((int)eventMouseButton.Position.X / _cellSize,
						(int)eventMouseButton.Position.Y / _cellSize); //convert mouse position into grid location
					//Check if cell does not already hold a player's marker
					if (_gridData[_gridPosition.Y, _gridPosition.X] == 0)
					{
						GameHandling();
					}
				}
			}
		}
	}
	#endregion Entry and Events Region

	#region Game Handling Region
	/// <summary>
	/// Sets some of the fundamental fields like the board size and panel positions.
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
	
	//We will modify the board data, then we will create a marker on the cell modified.
	//After that, we want to check some game-over conditions
	//If the game is not over, switch players
	/// <summary>
	/// Uses the input and place markers, depending on game-mode and player turn.
	/// Then the method will call methods to check game over conditions.
	/// If the game is not over method will call switch player methods.
	/// </summary>
	private async Task GameHandling()
	{
		ModifyGridData(); //change board data
		CreateMarker(_player, _gridPosition); //create marker
		_moves++; //increment move field for stalemate check
		if (CheckWin() != 0) //someone won
		{
			GameOver(_winner);
		}
		else if (_moves == 9) //tie, _winner should be 0 so no winner will display
		{
			GameOver(_winner);
		}
		else
		{
			if (_player == 1)
			{GetNode<AudioStreamPlayer>("CircleDrawSFX").Play();}
			else
			{GetNode<AudioStreamPlayer>("CrossDrawSFX").Play();}
			SwitchPlayer();

			if (_1PlayerGame && _player == -1)
			{
				await DelayMethod();
				CpuMove(_gridData, _smartCpu);
			}
		}
	}

	/// <summary>
	/// Resets all board stats and sprites for a new game. Will hide and show relevant menu panels.
	/// Will randomize starting player field. Will unpause tree, and change music.
	/// If 1-player will call cpu move if cpu is starting.
	/// </summary>
	private void NewGame()
	{
		ClearBoard();
		GetNode<CanvasLayer>("MainMenu").Hide();
		GetNode<CanvasLayer>("QuickMenu").Show();
		GetNode<Label>("PlayerLabel").Show();
		Random rnd = new Random();
		int newPlayer;
		 do { _player = newPlayer = rnd.Next(-1, 2); } while (newPlayer == 0 || newPlayer == 2);//starting move randomly switches. TODO implement a tracker that will switch back and forth until back to the main menu. Random should only be on Ready() and MainMenu Calls
		//create a marker to show starting players turn
		CreateMarker(_player, _playerPanelPosition + new Vector2I(_cellSize /2, _cellSize/2), true);
		_gridData = new [,] 
		{
			{0, 0, 0},
			{0, 0, 0},
			{0, 0, 0}
		};
		//PrintGridData(_gridData);
		GetTree().Paused = false;
		GetNode<AudioStreamPlayer>("LobbyMusic").Stop();
		GetNode<AudioStreamPlayer>("BeginGameSFX").Play();
		GetNode<AudioStreamPlayer>("BeginGameMusic").Play();
		if(_1PlayerGame && _player == -1) CpuMove(_gridData, _smartCpu);
	}

	/// <summary>
	/// Clears the board for main menu. Sets _gridData to null to prevent placing while menus are displayed.
	/// </summary>
	private void ClearBoard()
	{
		_gridData = null;
		GetNode<CanvasLayer>("GameOverMenu").Hide();
		GetTree().CallGroup("circles", "queue_free");
		GetTree().CallGroup("crosses", "queue_free");
		GetTree().CallGroup("winnerBar", "queue_free");
		_winner =_moves = _rowSum = _rowWin = _colSum = _colWin = _diagSum1 = _diagSum2 = 0; //set all the trackers to 0;
	}
	
	/// <summary>
	/// Uses the converted cell coordinates of the mouse click to mark that spot with the player's value (-1 or 1).
	/// </summary>
	private void ModifyGridData()
	{
		_gridData[_gridPosition.Y, _gridPosition.X] = _player;
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
		var marker = player == 1 ? CircleScene.Instantiate<Node2D>() : CrossScene.Instantiate<Node2D>();;
		if (_winner == 0)
		{
			AddChild(marker);
		}
		
		else
		{
			GetNode<CanvasLayer>("GameOverMenu").AddChild(marker);
			GetNode<AudioStreamPlayer>("BeginGameMusic").Stop();
			GetNode<AudioStreamPlayer>("LobbyMusic").Play();
		}
		marker.Scale = GetNode<Sprite2D>("Board").Scale * new Vector2((float).9, (float).9);
		marker.Position = new Vector2(position.X, position.Y);
		
		if (switchTemp)
		{
			_tempMarker = marker;
		}
	}
	
	/// <summary>
	/// Adds up the values in the cells on the board to check for a winner.
	/// </summary>
	/// <returns>The int value (-1 / 1) of the player that won.</returns>
	private int CheckWin()
	{
		//Add up markers in each diagonal
		_diagSum1 = _gridData[0, 0] + _gridData[1, 1] + _gridData[2, 2];
		_diagSum2 = _gridData[0, 2] + _gridData[1, 1] + _gridData[2, 0];
		if (_diagSum1 == 3 || _diagSum2 == 3)
		{
			return _winner = 1;
		}
		if (_diagSum1 == -3 || _diagSum2 == -3)
		{
			return _winner = -1;
		}
		//Add up markers in each row, column
		for (int i = 0; i < 3; i++)
		{
			_rowSum = _gridData[i, 0] + _gridData[i, 1] + _gridData[i, 2];
			_colSum = _gridData[0, i] + _gridData[1, i] + _gridData[2, i];
			if (_rowSum == 3 || _colSum == 3)
			{
				var k = _rowSum == 3 ? _rowWin = (i + 1) : _colWin = (i + 1);
				return _winner = 1;
			}
			if (_rowSum == -3 || _colSum == -3)
			{
				var k = _rowSum == -3 ? _rowWin = (i + 1) : _colWin = (i + 1);
				return _winner = -1;
			}
		}
		return _winner;
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
	
	/// <summary>
	/// Handles CPU logic, depending on difficulty setting (_smartCpu field)
	/// Dumb is looking for a random, open box.
	/// Smart is not implemented yet.
	/// </summary>
	/// <param name="gridData"></param>
	/// <param name="smartCpu"></param>
	private void CpuMove(int[,] gridData, bool smartCpu = false)
	{
		
		bool[,] available = new bool[3, 3];
		for (int i = 0; i < gridData.GetLength(0); i++)
		{
			for (int j = 0; j < gridData.GetLength(1); j++)
			{
				if (gridData[i, j] == 0)
				{
					available[i, j] = true;
				}
			}
		}
		if (!smartCpu)
		{
			bool open;
			int xIndex;
			int yIndex;
			Random r = new Random();
			do
			{
				open = available[yIndex = r.Next(available.GetLength(0)), xIndex = r.Next(available.GetLength(1))];
			} while (!open);

			_gridPosition = new Vector2I(xIndex, yIndex);
		}
		else
		{
			for (int i = 0; i < gridData.GetLength(0); i++)
			{
				for (int j = 0; j < gridData.GetLength(1); j++)
				{
					if (gridData[i, j] == 0)
					{
						available[i, j] = true;
					}
				}
			}
		}
		GameHandling();
	}
	
	/// <summary>
	/// Clears the board and calls the method to mark a winner, if any.
	/// </summary>
	/// <param name="winner">the int value of the player that one. If 0 is passed it is a tie and no marker will be made.</param>
	private void GameOver(int winner)
	{
			//GD.Print("Game Over");
		_tempMarker.QueueFree(); //clear next player sprite
		GetNode<Label>("PlayerLabel").Hide(); 
		GetNode<CanvasLayer>("QuickMenu").Hide();
		GetNode<CanvasLayer>("GameOverMenu").Show();

		if (winner != 0)
		{
			MarkWinnerOnBoard();
			var winnerMessage = winner == 1 ? "Winner: Player 1" : _1PlayerGame ?  "Winner: CPU" : "Winner: Player 2";
			GetNode<CanvasLayer>("GameOverMenu").GetNode<Panel>("GameOverPanel").GetNode<Label>("WinnerLabel").Text = winnerMessage;
			GetNode<CanvasLayer>("GameOverMenu").GetNode<Label>("LabelShadow").Text = winnerMessage;
			CreateMarker(winner, _winnerPanelPosition + new Vector2I(_cellSize / 2, _cellSize / 2));
		}
		if (winner == 0)
		{
			GetNode<AudioStreamPlayer>("GameOver0SFX").Play();
			GetNode<CanvasLayer>("GameOverMenu").GetNode<Panel>("GameOverPanel").GetNode<Label>("WinnerLabel").Text =
				"Stalemate!";
			GetNode<CanvasLayer>("GameOverMenu").GetNode<Label>("LabelShadow").Text = "Stalemate!"; 
		}
		if (winner == 1)
		{
			GetNode<AudioStreamPlayer>("Victory0SFX").Play();
		}
		else
		{
			GetNode<AudioStreamPlayer>("GameOver0SFX").Play();
		}
		GetTree().Paused = true; // pause the game 
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
		winnerMarker.Scale = GetNode<Sprite2D>("Board").Scale;
		winnerMarker.Position = new Vector2(300, 300);
		if (_colWin != 0)
		{ winnerMarker.RotationDegrees = 90;
 			switch (_colWin)
			{
				case 1: winnerMarker.Position = new Vector2(100, 300);
					break;
				case 3: winnerMarker.Position = new Vector2(500, 300);
					break;
			}
		}
		else if (_rowWin != 0)
		{
			winnerMarker.RotationDegrees = 0;
			switch (_rowWin)
			{
				case 1: winnerMarker.Position = new Vector2(300, 100);
					break;
				case 3: winnerMarker.Position = new Vector2(300, 500);
					break;
			}
		}
		else if (_diagSum1 == 3 || _diagSum1 == -3)
		{
			winnerMarker.RotationDegrees = 45;
		}
		else
		{
			winnerMarker.RotationDegrees = -45;
		}
	}
	#endregion Game Handling Region
	
	#region Menu Navigation
	private void GoToMainMenu()
	{
		ClearBoard();
		MenuSwitch();
		GetNode<Label>("PlayerLabel").Hide();
		GetNode<CanvasLayer>("QuickMenu").Hide();
		GetNode<CanvasLayer>("MainMenu").Show();
		if (GetNode<AudioStreamPlayer>("BeginGameMusic").Playing)
		{
			GetNode<AudioStreamPlayer>("BeginGameMusic").Stop();
			GetNode<AudioStreamPlayer>("LobbyMusic").Play();
		}
		GetTree().Paused = false;
	}

	private void MenuSwitch(bool flip = false)
	{
		_menuSwitch = flip;
		if (_menuSwitch)
		{
			GetNode<CanvasLayer>("MainMenu").GetNode<Panel>("MenuPanel").GetNode<Button>("1PlayerButton").Text = "Dumb CPU";
			GetNode<CanvasLayer>("MainMenu").GetNode<Panel>("MenuPanel").GetNode<Button>("2PlayerButton").Text = "Smart CPU";
			GetNode<CanvasLayer>("MainMenu").GetNode<Panel>("MenuPanel").GetNode<Button>("QuitButton").Text = "Back";
		}
		else
		{
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
			_smartCpu = false;
			_1PlayerGame = true;
			NewGame();
		}
		else
		{
			MenuSwitch(true);
		}
	}

	private void _on_main_menu_two_player_game()
	{
		if (_menuSwitch)
		{
			_smartCpu = false; //TODO: change to TRUE;
			_1PlayerGame = true;
			NewGame();
		}
		else
		{
			_1PlayerGame = false;
			NewGame();
		}
	}
	private void _on_main_menu_quit()
	{
		if (_menuSwitch)
		{
			MenuSwitch();
		}
		else
		{
			GetTree().Quit();
		}
	}
	private void _on_game_over_menu_restart()
	{
		NewGame();
	}
	private void _on_game_over_menu_main_menu()
	{
		MenuSwitch();
		//GetNode<AudioStreamPlayer>("LobbyMusic").Stop();
		GoToMainMenu();
	}
	private void _on_quick_menu_main_menu()
	{
		MenuSwitch();
		GoToMainMenu();
	}
	private void _on_quick_menu_quit()
	{
		GetTree().Quit();
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
