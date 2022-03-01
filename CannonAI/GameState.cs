using System;
using System.Collections.Generic;

public class GameState : ICloneable
{
	public int[,] Board;
	public int RedCount;
	public int GreyCount;
	public int RedCannons;
	public int GreyCannons;
	public List<FullMove> MoveList;
	public List<FullMove> KillerMoves;
	public GameState(int[,] board)
	{
		Board = board;
		RedCount = 0;
		GreyCount = 0;
		RedCannons = 0;
		GreyCannons = 0;
		MoveList = new List<FullMove>();
	}

	public void UpdateCount()
    {
		RedCount = 0;
		GreyCount = 0;
		for (int i = 0; i <= 9; i++)
			for (int j = 0; j <= 9; j++)
            {
				if (Board[i, j] == 1) RedCount++;
				if (Board[i, j] == 2) GreyCount++;
            }
    }

	public object Clone()
    {
		int[,] newBoard = (int[,])Board.Clone();
		return new GameState(newBoard);
    }
}
