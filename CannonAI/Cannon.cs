using System;
using System.Drawing;

// Definition of a Cannon on the board as a Front soldier, a Back soldier and facing one of four direction
public class Cannon
{
	public Point Front;
	public Point Back;
	public int Direction;
	public Cannon(int direction, Point front, Point back)
	{
		Front = front;
		Back = back;
		Direction = direction;
	}

	public string DirectionName()
    {
		if (Direction == 1) return "North-South";
		if (Direction == 2) return "NE-SW";
		if (Direction == 3) return "East-West";
		else return "NE-SW";
    }
}
