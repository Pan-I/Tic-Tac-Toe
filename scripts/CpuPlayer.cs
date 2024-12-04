using Godot;
using Random = System.Random;

namespace TicTacToe.scripts;

public static class CpuPlayer
{
    	
	static Vector2I _gridPosition;

	/// <summary>
	/// Handles CPU logic, depending on difficulty setting (_smartCpu field)
	/// Dumb is looking for a random, open box.
	/// Smart is not implemented yet.
	/// </summary>
	/// <param name="gridData">multidimensional array for storing the games current state of the board</param>
	/// <param name="smartCpu">difficulty setting</param>
	/// <param name="moves">how many moves have already been made on the board, used to count next potential moves</param>
	internal static Vector2I CpuMove(int[,] gridData, bool smartCpu = false, int moves =0)
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
	
	private static Vector2I BestMove(int[,] gridData, int moves)
	{
		int bestScore = -9;
		for (int i = 0; i < gridData.GetLength(0); i++)
		{
			for (int j = 0; j < gridData.GetLength(1); j++)
			{
				if (gridData[i, j] == 0)
				{
					gridData[i, j] = -1;
					moves++;
					int score = MiniMaxMethod(gridData, 0, false, moves);
					gridData[i, j] = 0;
					moves--;
					
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
	/// 
	/// </summary>
	/// <param name="gridData"></param>
	/// <param name="depth"></param>
	/// <param name="isMaximizing"></param>
	/// <param name="moves"></param>
	/// <returns></returns>
	private static int MiniMaxMethod(int[,] gridData, int depth, bool isMaximizing, int moves)
	{
		//int debug = depth; 
		int score = Main.CheckWinner();
		Main.ResetWinnerCheckFields();
		if (score == -1)
		{
			return 1;
		}
		if (score == 1)
		{
			return -1;
		}
		if (moves == 9)
		{
			return 0;
		}
		//If the move doesn't result in an endgame scenario,
		//start maximizing if isMaximizing == true
		if (isMaximizing)
		{
			int bestScore = -9;
			for (int k = 0; k < gridData.GetLength(0); k++)
			{
				for (int l = 0; l < gridData.GetLength(1); l++)
				{
					if (gridData[k, l] == 0)
					{
						gridData[k, l] = -1;
						moves++;
						score = MiniMaxMethod(gridData, depth + 1, false, moves);
						gridData[k, l] = 0;
						moves--;
						bestScore = score > bestScore ? score : bestScore;
					}
					
				}
			}
			return bestScore;
		}
		else //is minimizing 
		{
			int bestScore = 9;
			for (int k = 0; k < gridData.GetLength(0); k++)
				
			{
				for (int l = 0; l < gridData.GetLength(1); l++)
				{
					if (gridData[k, l] == 0)
					{
						gridData[k, l] = 1;
						moves++;
						score = MiniMaxMethod(gridData, depth + 1, true, moves);
						gridData[k, l] = 0;
						moves--;
						bestScore = score < bestScore ? score : bestScore;
					}
					
				}
				
			}
			return bestScore;
		}
	}
}