using System;
using System.Drawing;

// Similar to a "Point", but with extra int Type that specifies if move is a cannon shot or soldier move
public class Move
{
	public int X;
	public int Y;
	public int Type;
	public Move(int x, int y, int t)
	{
		X = x;
		Y = y;
		Type = t; // 0 == normal move, 2 == cannon shot
	}
}

// Way to define a move as the old point and the new point after a certain move
public class FullMove
{
	public Point Old;
	public Move New;
	public float Value;
	public bool isKiller;
	public FullMove(Point pt, Move m)
    {
		Old = pt;
		New = m;
		Value = 0;
		isKiller = false;
    }
	public FullMove(int i, int j, int x, int y, int t)
    {
		Old = new Point(i, j);
		New = new Move(x, y, t);
		Value = 0;
		isKiller = false;
    }
}
