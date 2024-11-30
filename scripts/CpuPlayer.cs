using System;
using System.Threading.Tasks;
using Godot;

namespace TicTacToe.scripts;

public static class CpuPlayer
{
    	
	static Vector2I _gridPosition;
	
	/// <summary>
	/// Handles CPU logic, depending on difficulty setting (_smartCpu field)
	/// Dumb is looking for a random, open box.
	/// Smart is not implemented yet.
	/// </summary>
	/// <param name="gridData">multidimensional array for storing the games current state</param>
	/// <param name="smartCpu">difficulty setting</param>
	internal static Vector2I CpuMove(int[,] gridData, bool smartCpu = false)
	{
		//TODO: Smart CPU logic; refactoring. No comments because of future plans.
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
		return _gridPosition;
	}

}