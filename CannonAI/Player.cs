using System;
using System.Drawing;

public class Player
{
	public string Type;
	public bool IsWinner;
	public bool IsCastleSet;
	public bool IsCastleAlive;
	public bool CanPiecesMove;
	public bool IsStoneSelected;
	public bool IsMoveComplete;
	public string Name;
	public Point CurrentStone;
	public Player(string type, string name)
	{
		Type = type;
		Name = name;
		IsWinner = false;
		IsCastleSet = false;
		IsCastleAlive = true;
		CanPiecesMove = true;
		IsStoneSelected = false;
		IsMoveComplete = false;
		CurrentStone = new Point(-1, -1);
	}
}
