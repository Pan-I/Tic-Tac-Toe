using Godot;
using Random = System.Random;

namespace TicTacToe.scripts;

public static class CpuPlayer
{
    	
	static Vector2I _gridPosition;
	
	private static int MinMax(int[,] gridData, int i, int j)
	{
		//check if moving in this spot will win the game, and reset spot to open.
		int score;
		gridData[i, j] = -1;
		int checkWin = Main.CheckWin();
		gridData[i, j] = 0;
		// if this move will win, return higher than possible score, will force CPU to move in this position.
		if (checkWin == -1) return 10;

		return 1;
	}
	
	/// <summary>
	/// Handles CPU logic, depending on difficulty setting (_smartCpu field)
	/// Dumb is looking for a random, open box.
	/// Smart is not implemented yet.
	/// </summary>
	/// <param name="gridData">multidimensional array for storing the games current state of the board</param>
	/// <param name="smartCpu">difficulty setting</param>
	internal static Vector2I CpuMove(int[,] gridData, bool smartCpu = false)
	{
		//random CPU logic
		if (!smartCpu)
		{
			//create a true false table to mark which spots are open.
			bool[,] available = new bool[3, 3];
			bool open;
			int xIndex;
			int yIndex;
			Random r = new Random();
			
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
			
			do
			{
				open = available[yIndex = r.Next(available.GetLength(0)), xIndex = r.Next(available.GetLength(1))];
			} while (!open);

			_gridPosition = new Vector2I(xIndex, yIndex);
		}
		//MinMax Logic ('Smart CPU')
		else
		{
			int bestScore = -9;
			Vector2I move = new Vector2I(0,0);
			for (int i = 0; i < gridData.GetLength(0); i++)
			{
				for (int j = 0; j < gridData.GetLength(1); j++)
				{
					if (gridData[i, j] == 0)
					{
						gridData[i, j] = 1;
						int score = MinMax(gridData, i, j);
						gridData[i, j] = 0;
						if (score > bestScore)
						{
							bestScore = score;
							//Vector2I has horizontal position first, which would be column position.
							//A bit counterintuitive but makes sense when mapped out visually.	
							move = new Vector2I(j, i); 
						}
					}
				}
			}
			_gridPosition = new Vector2I(move.X, move.Y);
		}
		return _gridPosition;
	}
}