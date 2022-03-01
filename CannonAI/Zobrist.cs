using System;
using System.Security.Cryptography;

public class Zobrist
{
	static long[][][] zArray;
	public Zobrist()
	{
		ZobristFillArray();
	}

	public static long Random64()
	{
		// create random set of bytes
		RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
		byte[] byteArray = new byte[64];
		rng.GetBytes(byteArray);

		// convert 64 bytes to a long
		var randomLong = BitConverter.ToInt64(byteArray, 0);
		return randomLong;
    }

	public void ZobristFillArray()
    {
		// initialise zArray for 2 colours (red,grey), with 3 piece types (town, soldier, cannon) for each of 100 board squares
		zArray = new long[2][][];
		for (int i = 0; i < zArray.Length; i++)
		{
			zArray[i] = new long[2][];
			for (int j = 0; j < zArray[i].Length; j++)
			{
				zArray[i][j] = new long[100];
			}
		}
		// fill each entry combination of colour, piece type and square with a different random number
		for (int colour = 0; colour < 2; colour++)
			for (int type = 0; type < 2; type++)
				for (int square = 0; square < 100; square++)
				{
					zArray[colour][type][square] = Random64();
				}
	}

	public long GetZobristHash(int[,] array)
    {
		long returnKey = 0;
		for (int i = 0; i <= 9; i++)
			for (int j = 0; j <= 9; j++)
            {
				int square = (10 * j) + i;
				if (array[i, j] == 1) // red soldier
					returnKey ^= zArray[0][0][square];
				else if (array[i, j] == 2) // grey soldier
					returnKey ^= zArray[1][0][square];
				else if (array[i, j] == 3) // red town
					returnKey ^= zArray[0][1][square];
				else if (array[i, j] == 4) //grey town
					returnKey ^= zArray[1][1][square];
			}

		return returnKey;
    }
}
