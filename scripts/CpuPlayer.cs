using Godot;
using Random = System.Random;

namespace TicTacToe.scripts;

public static class CpuPlayer
{
    	
	static Vector2I _gridPosition;

	/// <summary>
	/// Handles CPU logic, depending on difficulty setting (_smartCpu field)
	/// Dumb is looking for a random, open box.
	/// Smart minimizes human players moves, while maximizing its own possible outcomes.
	/// </summary>
	/// <param name="gridData">multidimensional array for storing the games current state of the board</param>
	/// <param name="smartCpu">difficulty setting</param>
	/// <param name="moves">how many moves have already been made on the board, used to count next potential moves</param>
	/// <returns> Returns a move the Cpu player will make, as a Vector2I </returns>
	internal static Vector2I CpuMove(int[,] gridData, bool smartCpu = false, int moves = 0)
	{
		//random CPU logic
		if (!smartCpu)
		{
			//create a true false grid to mark which spots are open.
			bool[,] available = new bool[3, 3];
			bool open; //a bool to make sure ranGen doesn't select a cell with a move already in it
			int xIndex; //used to test against the available grid, and pass to new Vector2I;
			int yIndex; //used to test against the available grid, and pass to new Vector2I;
			Random ranGen = new Random();
			
			//loop through the columns of the param gridData
			for (int i = 0; i < gridData.GetLength(0); i++)
			{
				//loop through the rows of the param gridData
				for (int j = 0; j < gridData.GetLength(1); j++)
				{
					//if the cell is empty
					if (gridData[i, j] == 0)
					{
						available[i, j] = true; //mark the cell as open, TRUE;
					}
				}
			}
			//use the ranGen object to randomly generate the next move, as long as the cell is open.
			do
			{
				open = available[yIndex = ranGen.Next(available.GetLength(0)), xIndex = ranGen.Next(available.GetLength(1))];
			} while (!open);

			_gridPosition = new Vector2I(xIndex, yIndex);
		}
		//MiniMax Logic ('Smart CPU')
		else
		{
			_gridPosition = BestMove(gridData, moves);
		}
		return _gridPosition;
	}
	/// <summary>
	///  The method finds an open cell to initiate the method MiniMax() off of.
	///  Will convert MiniMax()'s results into a Vector2I move.
	/// </summary>
	/// <param name="gridData"> The state of the board </param>
	/// <param name="moves"> How many moves have been tracked </param>
	/// <returns> Returns a Vector2I move to make</returns>
	private static Vector2I BestMove(int[,] gridData, int moves)
	{
		int bestScore = -2;
		//every row
		for (int i = 0; i < gridData.GetLength(0); i++)
		{
			//every column
			for (int j = 0; j < gridData.GetLength(1); j++)
			{
				//if the cell is open
				if (gridData[i, j] == 0)
				{
					gridData[i, j] = -1; //mark the cell as taken
					moves++; //iterate local move tracker
					int score = MiniMaxMethod(gridData, 0, false, moves); //start MiniMax while minimizing, which will first check for win from X, then move for 1-player (O)
					gridData[i, j] = 0; //unmarks the cell
					moves--; //undo iteration on moves
					
					if (score > bestScore)
					{
						bestScore = score;
						//Vector2I has horizontal position first, which would be column position.
						//A bit counterintuitive but makes sense when mapped out visually.	
						_gridPosition = new Vector2I(j, i);
					}
				}
			}

		}
		
		return _gridPosition;
	}
	
	/// <summary>
	/// A recursive method that will consider all the possible ways the game can go and returns the best value for that move.
	/// </summary>
	/// <param name="gridData"> The state of the board</param>
	/// <param name="depth"> The recursive depth</param>
	/// <param name="isMaximizing"> Whether the method is minimizing or maximizing a turn</param>
	/// <param name="moves"> How many moves have been made.</param>
	/// <returns></returns>
	private static int MiniMaxMethod(int[,] gridData, int depth, bool isMaximizing, int moves)
	{
		//int debug = depth; 
		int score = Main.CheckWinner(); //Need to check if a move will result in a win (or loss).
		Main.ResetWinnerCheckFields(); //Need to reset some of the fields used to mark the real winner on the board, that were set in the method above.
		//-1 is X, and we want to maximize on X's return.
		if (score == -1) { return 1; } //+1 for X
		if (score == 1) { return -1; } //-1 fo O
		if (moves == 9) { return 0; } //0 for tie, which Main.CheckWinner() does not do.
		
		//If the move doesn't result in an endgame scenario,
		//start maximizing if isMaximizing == true
		if (isMaximizing)
		{
			int bestScore = -2;
			//every row
			for (int k = 0; k < gridData.GetLength(0); k++)
			{
				//every column
				for (int l = 0; l < gridData.GetLength(1); l++)
				{
					//if the cell is open
					if (gridData[k, l] == 0)
					{
						gridData[k, l] = -1; //mark the cell as taken
						moves++; //iterate local move tracker
						score = MiniMaxMethod(gridData, depth + 1, false, moves); //call the method from the top while minimizing, which will first check for win for X, then move for 1-player (O)
						gridData[k, l] = 0; //unmarks the cell
						moves--; //undo iteration on moves
						bestScore = score > bestScore ? score : bestScore; //assign the bigger score to bestScore, since the method is being called from a maximizing state.
					}
				}
			}
			return bestScore; //returns bestScore, which is then compared against some previously saved score.
		}
		else //is minimizing 
		{
			int bestScore = 2;
			//every row
			for (int k = 0; k < gridData.GetLength(0); k++)
			{
				//every column
				for (int l = 0; l < gridData.GetLength(1); l++)
				{
					//if the cell is open
					if (gridData[k, l] == 0)
					{
						gridData[k, l] = 1; //mark the cell as taken
						moves++; //iterate local move tracker
						score = MiniMaxMethod(gridData, depth + 1, true, moves); //call the method from the top while maximizing, which will first check for win for O, then move for 2-player (X)
						gridData[k, l] = 0; //unmarks the cell
						moves--; //undo iteration on moves
						bestScore = score < bestScore ? score : bestScore; //assign the smaller score to bestScore, since the method is being called from a minimizing state.
					}
				}
			}
			return bestScore; //returns bestScore, which is then compared against some previously saved score.
		}
	}
}