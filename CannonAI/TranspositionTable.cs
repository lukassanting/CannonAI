using System;
using System.Collections;
using System.Collections.Generic;

public class TranspositionTable
{
	public Dictionary<long, object[]> TransTable;
	public TranspositionTable()
	{
		TransTable = new Dictionary<long, object[]>();
	}

	public void Add(long key, int depth, string flag, float value, List<FullMove> moves)
    {
		object[] values = new object[] {depth, flag, value, moves};
		TransTable.Add(key, values);
    }

	public object[] GetValues(long key)
    {
		return TransTable[key];
    }

	public bool ContainsKey(long key)
    {
		return TransTable.ContainsKey(key);
    }

}
