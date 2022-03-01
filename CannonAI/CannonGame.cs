using System;
using System.Linq;
using System.Drawing;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

public class CannonGame
{
	public string GameType;
    public string BotType;
	public GameState gameState;
    public TranspositionTable TT;
	public int[,] HelpArray;
    public int legalCount;
    public bool IsGameOver;
    public bool AllowBotMove;
    public bool updateHistory;
    public Player Grey;
    public Player Red;
    public Player CurrentPlayer;
    public Player CurrentOpponent;
    public Player Winner;
    List<Move>[,] allMoves;
    int current;
    int opposition;
    public string LabelText;
    public string HistoryText;
    public string TurnText;
    Random r;
    Zobrist z;

    public CannonGame(string gameType, string botType)
	{
        // -- Game --
		GameType = gameType;
        BotType = botType;
		gameState = new GameState(new int[10, 10]);
        TT = new TranspositionTable();
		HelpArray = new int[10, 10];
        allMoves = new List<Move>[10, 10];
        legalCount = 0;
        IsGameOver = false;
        r = new Random();
        z = new Zobrist();
        // -- Players -- 
        MakePlayerTypes();
		CurrentPlayer = Grey;
        CurrentOpponent = Red;
        current = 2;
        opposition = 1;
        // -- Labels --
        LabelText = "Waiting...";
        HistoryText = "";
        TurnText = $"Turn =  {CurrentPlayer.Name}";
        updateHistory = false;

        // When using bots, a GameUpdate is always: A Form click, so HumanMove followed by BotMove. If BvP, Bot must still go first
        if (GameType == "Bot vs Player")
            BotMove();
        AllowBotMove = false;

    }
    private void MakePlayerTypes()
    {
        if (GameType == "Player vs Player")
        {
            Grey = new Player("human", "Grey");
            Red = new Player("human", "Red");
        }
        else if (GameType == "Player vs Bot")
        {
            Grey = new Player("human", "Grey");
            Red = new Player("bot", "Red");
        }
        else if (GameType == "Bot vs Player")
        {
            Grey = new Player("bot", "Grey");
            Red = new Player("human", "Red");
        }
    }

    // ----------------------------------------------------------------------
    // ~~~~~~~~~~~~~~~~~~~~~~~~~ General Functions ~~~~~~~~~~~~~~~~~~~~~~~~~~
    // ----------------------------------------------------------------------

    public void ChangePlayer()
	{
        if (CurrentPlayer == Red)
        {
            CurrentPlayer = Grey;
            CurrentOpponent = Red;
            current = 2;
            opposition = 1;
        }
        else
        {
            CurrentPlayer = Red;
            CurrentOpponent = Grey;
            current = 1;
            opposition = 2;
        }
        // Some labels only need to be updated when players change
        UpdateHistory();
        TurnText = $"Turn = {CurrentPlayer.Name}";
    }

    // Checks if any win conditions have been met, and otherwise switches player
    public void GameOverCheck()
    {
        // check if player who just moved captured Opponent's castle
        if (CurrentPlayer.IsWinner || !CurrentOpponent.IsCastleAlive)
        {
            IsGameOver = true;
            Winner = CurrentPlayer;
        }
        else if (CurrentPlayer.IsMoveComplete) // switch players and check if next player has any legal moves
        {
            ChangePlayer();
            AllowBotMove = true;
            if (CurrentPlayer.IsCastleSet)
            {
                // moves are generated for next players turn
                allMoves = GenerateAllMoves(gameState, true, current, opposition);
                if (legalCount == 0)
                {
                    IsGameOver = true;
                    Winner = CurrentOpponent;
                    CurrentOpponent.IsWinner = true;
                }
            }
        }
    }

    public void GameUpdate(int x, int y)
    {
        // Reset game labels
        updateHistory = false;
        LabelText = "Waiting...";
        HistoryText = "";

        // Check for Game Type:
        //     Type: PvP        - A game update is just a human move
        //     Type: PvB or BvP - A game update is a human move, followed by bot move
        if (!IsGameOver)
            HumanMove(x, y);
        if (!IsGameOver)
        {
            if ((GameType == "Player vs Bot" || GameType == "Bot vs Player") && AllowBotMove)
                BotMove();
            AllowBotMove = false;
        }
    }

    // ----------------------------------------------------------------------
    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~ Human Move ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    // ----------------------------------------------------------------------

    public void HumanMove(int x, int y)
    {
        bool isMoveAllowed = true;
        CurrentPlayer.IsMoveComplete = false;
        // check if castle needs to be set (opening move)
        if (!CurrentPlayer.IsCastleSet)
        {
            //castles can only be placed on player's respective back line
            if (CurrentPlayer == Grey && y != 9) isMoveAllowed = false;
            else if (CurrentPlayer == Red && y != 0) isMoveAllowed = false;
            else gameState.Board[x, y] = current + 2; // red castle stored as 3, grey castle stored as 4

            if (isMoveAllowed) // a legal spot for castle has been chosen
            {
                CurrentPlayer.IsCastleSet = true;
                HistoryText = $"{CurrentPlayer.Name} castle at {ChessNotation("X", x)}{ChessNotation("Y", y)}";
                CurrentPlayer.IsMoveComplete = true;
                GameOverCheck();
            }
            else LabelText = "Please choose a valid spot for the castle.";
        }
        // castle doesn't need to be set, so we are playing a normal move
        else
        {
            // move is two steps: select stone, then place it in new spot
            if (!CurrentPlayer.IsStoneSelected)
            {
                if (gameState.Board[x, y] != current) isMoveAllowed = false;

                if (isMoveAllowed)
                {
                    CurrentPlayer.CurrentStone = new Point(x, y);
                    UpdateHelpArray(x, y, current);
                    CurrentPlayer.IsStoneSelected = true;
                }
                else LabelText = "Please select a valid stone to move";
            }
            else //Stone to move has been selected, choosing spot to move to
            {
                Point newPoint = new Point(x, y);
                if (!(newPoint == CurrentPlayer.CurrentStone))
                {
                    if (HelpArray[x, y] == current)
                    {
                        int ox = CurrentPlayer.CurrentStone.X;
                        int oy = CurrentPlayer.CurrentStone.Y;
                        gameState.Board[ox, oy] = 0;
                        if (gameState.Board[x, y] == opposition + 2)
                        {
                            CurrentOpponent.IsCastleAlive = false;
                            CurrentPlayer.IsWinner = true;
                        }
                        HistoryText = $"{CurrentPlayer.Name}: {ChessNotation("X", ox)}{ChessNotation("Y", oy)} to {ChessNotation("X", x)}{ChessNotation("Y", y)}";
                        gameState.Board[x, y] = current;
                    }
                    else if (HelpArray[x, y] == current + 2) // Cannon shot
                    {
                        if (gameState.Board[x, y] == opposition + 2)
                        {
                            CurrentOpponent.IsCastleAlive = false;
                            CurrentPlayer.IsWinner = true;
                        }
                        HistoryText = $"{CurrentPlayer.Name}: shoots {ChessNotation("X", x)}{ChessNotation("Y", y)}";
                        gameState.Board[x, y] = 0;
                    }
                    else
                    {
                        isMoveAllowed = false;
                        CurrentPlayer.IsMoveComplete = false;
                    }

                    if (isMoveAllowed && !IsGameOver) // a legal spot to move to has been chosen
                    {
                        CurrentPlayer.CurrentStone = newPoint;
                        CurrentPlayer.IsMoveComplete = true;
                    }
                }
                // reset bool and HelpArray
                CurrentPlayer.IsStoneSelected = false;
                GameOverCheck();
                System.Array.Clear(HelpArray, 0, 100);
            }
        }
    }

    public void UpdateHelpArray(int x, int y, int current)
    {
        // based on legal moves, update the array that acts as human legal move represenation (for form click & graphics)
        List<Move> xyMoves = allMoves[x, y];
        foreach (Move m in xyMoves)
            HelpArray[m.X, m.Y] = current + m.Type;
    }

    // ----------------------------------------------------------------------
    // ~~~~~~~~~~~~~~~~~~~~~~ BOT: General Functions ~~~~~~~~~~~~~~~~~~~~~~~~
    // ----------------------------------------------------------------------

    public void BotMove()
    {
        CurrentPlayer.IsMoveComplete = false;
        if (!CurrentPlayer.IsCastleSet)
        {
            int y;
            int x = r.Next(0, 5);
            if (CurrentPlayer == Red)
            {
                y = 0;
                x = (x * 2) + 1;
            }
           else 
            { 
                y = 9;
                x *= 2;
            }
            gameState.Board[x, y] = current + 2;
            CurrentPlayer.IsCastleSet = true;
            HistoryText += $"\n{CurrentPlayer.Name} castle at {ChessNotation("X", x)}{ChessNotation("Y", y)}";
            CurrentPlayer.IsMoveComplete = true;
            GameOverCheck();
        }
        else
        {
            // List possible moves
            if (BotType == "Random")
            {
                List<FullMove> fullMoves = GenerateMoveList(allMoves, gameState, current);
                int next = r.Next(0, fullMoves.Count);
                FullMove move = fullMoves[next];
                ApplyBotMove(move);
            }
            if (BotType == "Mark I (MiniMax)")
            {
                FullMove move = PreMiniMax(allMoves, gameState, 2);
                ApplyBotMove(move);
            }
            if (BotType == "Mark II (Alpha-Beta)")
            {
                FullMove move = PreAlphaBeta(allMoves, gameState, 3);
                ApplyBotMove(move);
            }
            if (BotType == "Mark III (AB with TT)")
            {
                FullMove move = PreABwithTT(allMoves, gameState, 5);
                ApplyBotMove(move);
            }
            if (BotType == "Mark IV (Iterative DS)")
            {
                FullMove move = IterativeDS(allMoves, gameState);
                ApplyBotMove(move);
            }
        }
    }

    public GameState GenerateState(FullMove move, GameState state, int curr, int opp)
    {
        if (move.New.Type == 0)
        {
            // normal move
            state.Board[move.Old.X, move.Old.Y] = 0;
            state.Board[move.New.X, move.New.Y] = curr;
        }
        else if (move.New.Type == 2) // cannon shot
            state.Board[move.New.X, move.New.Y] = 0;

        return state;
    }

    public float Evaluation(GameState state, Player player)
    {
        float redCount = 0;
        float greyCount = 0;
        float val = 0;
        bool isGreyCastle = false;
        bool isRedCastle = false;
        Point greyCastle = new Point(0, 0);
        Point redCastle = new Point(0, 0);
        for (int i = 0; i <= 9; i++)
            for (int j = 0; j <= 9; j++)
            {
                if (state.Board[i, j] == 1) redCount++;
                if (state.Board[i, j] == 2) greyCount++;
                if (state.Board[i, j] == 3)
                {
                    redCastle = new Point(i, j);
                    isRedCastle = true;
                }
                if (state.Board[i, j] == 4) 
                { 
                    greyCastle = new Point(i, j);
                    isGreyCastle = true;
                }
            }

        // Taking and losing castles have to be very dominant in the score
        if (!isGreyCastle)
        {
            if (player == Grey) val -= 100;
            else val += 150;
        }
        if (!isRedCastle)
        {
            if (player == Grey) val += 150;
            else val -= 100;
        }
        // Value being close to enemy castle, but enemies close to yours is slightly worse
        for (int i = 0; i <= 9; i++)
            for (int j = 0; j <= 9; j++)
            {
                if (player == Grey)
                {
                    if (isGreyCastle)
                    {
                        if (state.Board[i, j] == 1)
                        {
                            if (Math.Abs(greyCastle.X - i) <= 3 && Math.Abs(greyCastle.Y - j) <= 3) val -= 3;
                            if (Math.Abs(greyCastle.X - i) <= 2 && Math.Abs(greyCastle.Y - j) <= 2) val -= 5;
                            if (Math.Abs(greyCastle.X - i) <= 1 && Math.Abs(greyCastle.Y - j) <= 1) val -= 8;
                        }
                    }
                    if (isRedCastle)
                    {
                        if (state.Board[i, j] == 2)
                        {
                            if (Math.Abs(redCastle.X - i) <= 3 && Math.Abs(redCastle.Y - j) <= 3) val += 1;
                            if (Math.Abs(redCastle.X - i) <= 2 && Math.Abs(redCastle.Y - j) <= 2) val += 3;
                            if (Math.Abs(redCastle.X - i) <= 1 && Math.Abs(redCastle.Y - j) <= 1) val += 5;
                        }
                    }
                }
                if (player == Red)
                {
                    if (isGreyCastle)
                    {
                        if (state.Board[i, j] == 1)
                        {
                            if (Math.Abs(greyCastle.X - i) <= 3 && Math.Abs(greyCastle.Y - j) <= 3) val += 1;
                            if (Math.Abs(greyCastle.X - i) <= 2 && Math.Abs(greyCastle.Y - j) <= 2) val += 3;
                            if (Math.Abs(greyCastle.X - i) <= 1 && Math.Abs(greyCastle.Y - j) <= 1) val += 5;
                        }
                    }
                    if (isRedCastle)
                    {
                        if (state.Board[i, j] == 2)
                        {
                            if (Math.Abs(redCastle.X - i) <= 3 && Math.Abs(redCastle.Y - j) <= 3) val -= 3;
                            if (Math.Abs(redCastle.X - i) <= 2 && Math.Abs(redCastle.Y - j) <= 2) val -= 5;
                            if (Math.Abs(redCastle.X - i) <= 1 && Math.Abs(redCastle.Y - j) <= 1) val -= 8;
                        }
                    }
                }
            }
        // Finally, value captures
        float count;
        if (player == Grey) count = (greyCount / redCount) * (greyCount / redCount);
        else count = (redCount / greyCount) * (redCount / greyCount);
        return val += count;
    }

    public bool InterEndGameCheck(int count, Player player)
    {
        if (!player.IsCastleAlive) return true;
        if (count == 0) return true;
        return false;
    }

    public List<FullMove> MoveOrdering(List<FullMove> killerMoves, List<FullMove> movesList)
    { 
        List<FullMove> sortedList = movesList.OrderByDescending(o => o.Value).ToList();
        if (killerMoves.Count >= 2)
        {
            sortedList.Prepend(killerMoves[0]);
            sortedList.Prepend(killerMoves[1]);
        }
        
        return sortedList;
    }

    public void ApplyBotMove(FullMove move)
    {
        if (move.New.Type == 0)
        {
            // normal move
            gameState.Board[move.Old.X, move.Old.Y] = 0;
            if (gameState.Board[move.New.X, move.New.Y] == opposition + 2)
            {
                CurrentOpponent.IsCastleAlive = false;
                CurrentPlayer.IsWinner = true;
            }
            gameState.Board[move.New.X, move.New.Y] = current;
            HistoryText += $"\n{CurrentPlayer.Name}: {ChessNotation("X", move.Old.X)}{ChessNotation("Y", move.Old.Y)} to {ChessNotation("X", move.New.X)}{ChessNotation("Y", move.New.Y)}";
        }
        else if (move.New.Type == 2)
        {
            // cannon shot
            if (gameState.Board[move.New.X, move.New.Y] == opposition + 2)
            {
                CurrentOpponent.IsCastleAlive = false;
                CurrentPlayer.IsWinner = true;
            }
            gameState.Board[move.New.X, move.New.Y] = 0;
            HistoryText += $"\n{CurrentPlayer.Name}: shoots {ChessNotation("X", move.New.X)}{ChessNotation("Y", move.New.Y)}";
        }
        CurrentPlayer.IsMoveComplete = true;
        GameOverCheck();
    }

    // ----------------------------------------------------------------------
    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~ BOT: MiniMax ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    // ----------------------------------------------------------------------

    public FullMove PreMiniMax(List<Move>[,] allMoves, GameState state, int depth) 
    {
        // for every possible move in fullMoves, call fullMoves and store value. Choose move that returns biggest value
        float bestValue = -9000;
        FullMove bestMove = new FullMove(new Point(0, 0), new Move(0, 0, 1));
        for (int i = 0; i <= 9; i++)
            for(int j = 0; j <= 9; j++)
            {
                if (state.Board[i,j] == current)
                {
                    List<Move> ijMove = allMoves[i, j];
                    foreach (Move m in ijMove)
                    {
                        GameState stateClone = (GameState)state.Clone();
                        FullMove fm = new FullMove(new Point(i, j), m);
                        GameState newState = GenerateState(fm, stateClone, current, opposition);
                        float value = MiniMaxMove(newState, depth - 1, CurrentOpponent, "MIN");
                        if (value > bestValue)
                        {
                            bestValue = value;
                            bestMove = fm;
                        }
                    }
                }
            }
        return bestMove;
    }

    public float MiniMaxMove(GameState state, int depth, Player player, string type)
    {
        // label player and opponent
        Player opponent = Red;
        int curr = 2;
        int opp = 1;
        if (player == Red) 
        {
            opponent = Grey;
            curr = 1;
            opp = 2;
        }

        // check if endgame (with no. of moves) OR depth == 0
        List<Move>[,] allMoves = GenerateAllMoves(state, true, curr, opp);
        if (InterEndGameCheck(legalCount, player) || depth == 0) return Evaluation(state, player);

        float bestScore;
        if (type == "MAX")
        {
            bestScore = -9000;
            for (int i = 0; i <= 9; i++)
                for (int j = 0; j <= 9; j++)
                {
                    if (state.Board[i, j] == curr)
                    {
                        List<Move> ijMove = allMoves[i, j];
                        foreach (Move m in ijMove)
                        {

                            FullMove fm = new FullMove(new Point(i, j), m);
                            GameState stateClone = (GameState)state.Clone();
                            GameState newState = GenerateState(fm, stateClone, curr, opp);
                            float score = MiniMaxMove(newState, depth - 1, opponent, "MIN");
                            if (score > bestScore)
                                bestScore = score;
                        }
                    }
                }
        }
        else // type = "MIN"
        {
            bestScore = 9000;
            for (int i = 0; i <= 9; i++)
                for (int j = 0; j <= 9; j++)
                {
                    if (state.Board[i,j] == curr)
                    {
                        List<Move> ijMove = allMoves[i, j];
                        foreach (Move m in ijMove)
                        {
                            FullMove fm = new FullMove(new Point(i, j), m);
                            GameState stateClone = (GameState)state.Clone();
                            GameState newState = GenerateState(fm, stateClone, curr, opp);
                            float score = MiniMaxMove(newState, depth - 1, opponent, "MAX");
                            if (score < bestScore)
                                bestScore = score;
                        }
                    }
                }
        }
        return bestScore; 
    }

    // ----------------------------------------------------------------------
    // ~~~~~~~~~~~~~~~~~~~~~~~~ BOT: Alpha-Beta ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    // ----------------------------------------------------------------------

    public FullMove PreAlphaBeta(List<Move>[,] allMoves, GameState state, int depth)
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        FullMove bestMove = new FullMove(new Point(0, 0), new Move(0, 0, 1));
        float alpha = -9000, beta = 9000, bestValue = -9000;

        for (int i = 0; i <= 9; i++)
            for (int j = 0; j <= 9; j++)
            {
                if (state.Board[i,j] == current)
                {
                    List<Move> ijMove = allMoves[i, j];
                    foreach (Move m in ijMove)
                    {
                        FullMove fm = new FullMove(new Point(i, j), m);
                        GameState stateClone = (GameState)state.Clone();
                        GameState newState = GenerateState(fm, stateClone, current, opposition);
                        float value = -1 * AlphaBeta(newState, depth - 1, CurrentOpponent, -1 * beta, -1 * alpha);
                        if (value > bestValue)
                        {
                            bestValue = value;
                            bestMove = fm;
                        }
                        if (bestValue > alpha) alpha = bestValue;
                        if (bestValue >= beta) break;
                    }
                }
            }
        return bestMove;
    }

    public float AlphaBeta(GameState state, int depth, Player player, float alpha, float beta)
    {
        // label player and opponent
        Player opponent = Red;
        int curr = 2;
        int opp = 1;
        if (player == Red)
        {
            opponent = Grey;
            curr = 1;
            opp = 2;
        }

        // check if endgame (with no. of moves) OR depth == 0
        List<Move>[,] allMoves = GenerateAllMoves(state, true, curr, opp);
        if (InterEndGameCheck(legalCount, player) || depth == 0) return Evaluation(state, player);
        float bestScore = -9000;
        for (int i = 0; i <= 9; i++)
            for (int j = 0; j <= 9; j++)
            {
                if (state.Board[i, j] == curr)
                {
                    List<Move> ijMove = allMoves[i, j];
                    foreach (Move m in ijMove)
                    {
                        FullMove fm = new FullMove(new Point(i, j), m);
                        GameState stateClone = (GameState)state.Clone();
                        GameState newState = GenerateState(fm, stateClone, curr, opp);
                        float score = -1 * AlphaBeta(newState, depth - 1, opponent, -1 * beta, -1 * alpha);
                        if (score > bestScore) bestScore = score;
                        if (bestScore > alpha) alpha = bestScore;
                        if (bestScore >= beta) break;
                    }
                }
            }

        return bestScore;
    }

    // ----------------------------------------------------------------------
    // ~~~~~~~~~~~~~~~~~~~~~~~~~~ BOT: AB wth TT ~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    // ----------------------------------------------------------------------

    public FullMove PreABwithTT(List<Move>[,] allMoves, GameState state, int depth)
    {

        float alpha = -9000, beta = 9000, bestValue = -9000;
        float olda = alpha;
        FullMove bestMove = new FullMove(new Point(0, 0), new Move(0, 0, 1));

        List<FullMove> KillerMoves = new List<FullMove>();
        List<FullMove> fullMoves = GenerateMoveList(GenerateAllMoves(state, true, current, opposition), state, current);

        foreach (FullMove fm in fullMoves)
        {
            GameState stateClone = (GameState)state.Clone();
            GameState newState = GenerateState(fm, stateClone, current, opposition);
            float value = -1 * ABwithTT(newState, depth - 1, CurrentOpponent, -1 * beta, -1 * alpha);
            fm.Value = value; // Only update score if it isn't move TT
            if (value > bestValue)
            {
                bestValue = value;
                bestMove = fm;
            }
            if (bestValue > alpha) alpha = bestValue;
            if (bestValue >= beta) 
            {
                if (!KillerMoves.Contains(fm)) KillerMoves.Add(fm); 
                break;
            }
        }
        long zobHash = z.GetZobristHash(state.Board);
        if (!TT.ContainsKey(zobHash))
        {
            string flag;
            if (bestValue <= olda) flag = "Upper";
            else if (bestValue >= beta) flag = "Lower";
            else flag = "Exact";
            TT.Add(zobHash, depth, flag, bestValue, MoveOrdering(KillerMoves, fullMoves));
        }

        return bestMove;
    }

    public float ABwithTT(GameState state, int depth, Player player, float alpha, float beta)
    {
        List<FullMove> KillerMoves = new List<FullMove>();
        // label player and opponent
        Player opponent = Red;
        int curr = 2;
        int opp = 1;
        if (player == Red)
        {
            opponent = Grey;
            curr = 1;
            opp = 2;
        }

        float olda = alpha;
        long zobHash= z.GetZobristHash(state.Board);
        List<FullMove> allMoves;
        // Check transposition table
        bool zKeyInTT = TT.ContainsKey(zobHash);
        if (zKeyInTT)
        {
            object[] tValues = TT.GetValues(zobHash);
            var tDepth = (int)tValues[0];
            var tFlag = (string)tValues[1];
            var tValue = (float)tValues[2];
            var tMoves = (List<FullMove>)tValues[3];
            if (tDepth >= depth)
            {
                if (tFlag == "Exact") return tValue;
                else if (tFlag == "Lower") alpha = Math.Max(alpha, tValue);
                else if (tFlag == "Upper") beta = Math.Min(beta, tValue);
                if (alpha >= beta) return tValue;
                allMoves = tMoves;
            }
            else allMoves = GenerateMoveList(GenerateAllMoves(state, true, curr, opp), state, curr);
        }
        else allMoves = GenerateMoveList(GenerateAllMoves(state, true, curr, opp), state, curr);
        // check if endgame (with no. of moves) OR depth == 0
        if (InterEndGameCheck(legalCount, player) || depth == 0) return Evaluation(state, player);
        float bestScore = -9000;
        foreach (FullMove fm in allMoves)
        {
            {
                GameState stateClone = (GameState)state.Clone();
                GameState newState = GenerateState(fm, stateClone, curr, opp);
                float score = -1 * ABwithTT(newState, depth - 1, opponent, -1 * beta, -1 * alpha);
                if (!zKeyInTT) fm.Value = score; // Only update score if it isn't move TT
                if (score > bestScore) bestScore = score;
                if (bestScore > alpha) alpha = bestScore;
                if (bestScore >= beta) 
                {
                    if (!KillerMoves.Contains(fm)) KillerMoves.Add(fm);
                    //prunings++;
                    break; 
                }
            }
        }
        // add state to transposition table if not already in it
        if (!zKeyInTT)
        {
            string flag;
            if (bestScore <= olda) flag = "Upper";
            else if (bestScore >= beta) flag = "Lower";
            else flag = "Exact";
            TT.Add(zobHash, depth, flag, bestScore, MoveOrdering(KillerMoves, allMoves));
        }
        return bestScore;
    }

    // ----------------------------------------------------------------------
    // ~~~~~~~~~~~~~~~~~~~~~ BOT: ITERATIVE DEEPENING ~~~~~~~~~~~~~~~~~~~~~~~
    // ----------------------------------------------------------------------

    public FullMove IterativeDS(List<Move>[,] allMoves, GameState state)
    {
        FullMove bestMove = new FullMove(new Point(0, 0), new Move(0, 0, 1));

        for (int depth = 1; depth <= 5; depth++)
        {
            List<FullMove> KillerMoves = new List<FullMove>();
            List<FullMove> fullMoves;
            float alpha = -9000, beta = 9000, bestValue = -9000;
            float olda = alpha;
            bestMove = new FullMove(new Point(0, 0), new Move(0, 0, 1));

            long zobHash = z.GetZobristHash(state.Board);
            bool zKeyInTT = TT.ContainsKey(zobHash);
            if (zKeyInTT)
            {
                object[] tValues = TT.GetValues(zobHash);
                var tDepth = (int)tValues[0];
                var tFlag = (string)tValues[1];
                var tValue = (float)tValues[2];
                var tMoves = (List<FullMove>)tValues[3]; 
                if (tDepth >= depth)
                {
                    if (tFlag == "Lower") alpha = Math.Max(alpha, tValue);
                    else if (tFlag == "Upper") beta = Math.Min(beta, tValue);
                    fullMoves = tMoves;
                }
                else fullMoves = GenerateMoveList(GenerateAllMoves(state, true, current, opposition), state, current);
            }
            else fullMoves = GenerateMoveList(GenerateAllMoves(state, true, current, opposition), state, current);

            foreach (FullMove fm in fullMoves)
            {
                GameState stateClone = (GameState)state.Clone();
                GameState newState = GenerateState(fm, stateClone, current, opposition);
                float value = -1 * ABwithTT(newState, depth - 1, CurrentOpponent, -1 * beta, -1 * alpha);
                if (!zKeyInTT) fm.Value = value; // Only update score if it isn't move TT
                if (value > bestValue)
                {
                    bestValue = value;
                    bestMove = fm;
                }
                if (bestValue > alpha) alpha = bestValue;
                if (bestValue >= beta) 
                {
                    if (!KillerMoves.Contains(fm)) KillerMoves.Add(fm);
                    //prunings++;
                    break; 
                }
            }
            if (!zKeyInTT)
            {
                string flag;
                if (bestValue <= olda) flag = "Upper";
                else if (bestValue >= beta) flag = "Lower";
                else flag = "Exact";
                TT.Add(zobHash, depth, flag, bestValue, MoveOrdering(KillerMoves, fullMoves));
            }
        }

        return bestMove;
    }

    // ----------------------------------------------------------------------
    // ~~~~~~~~~~~~~~~~~~~~~~~~~~ Move Generation ~~~~~~~~~~~~~~~~~~~~~~~~~~~
    // ----------------------------------------------------------------------

    public List<FullMove> GenerateMoveList(List<Move>[,] movesArray, GameState state, int curr)
    {
        List<FullMove> fullMoves = new List<FullMove>();
        for (int i = 0; i <= 9; i++)
            for (int j = 0; j <= 9; j++)
            {
                if (state.Board[i, j] == curr)
                {
                    foreach (Move m in movesArray[i, j])
                        fullMoves.Add(new FullMove(new Point(i, j), m));
                }
            }

        return fullMoves;
    }

    // for every cell on the board, generate all possible moves for a given player
    public List<Move>[,] GenerateAllMoves(GameState state, bool count, int curr, int opp)
    {
        if (count)
            legalCount = 0;
        List<Move>[,] movesArray = new List<Move>[10,10];
        for (int x = 0; x <= 9; x++)
            for (int y = 0; y <= 9; y++)
            {
                if (state.Board[x,y] == curr)
                {
                    List<Move> xyMoves = GenerateMoves(x, y, state, curr, opp);
                    if (count)
                        legalCount += xyMoves.Count;
                    movesArray[x, y] = xyMoves;
                }
            }
        return movesArray;
    }

    // checks legal moves for a stone in a given position (x,y) and stores them in a list of Moves
    public List<Move> GenerateMoves(int x, int y, GameState state, int curr, int opp)
    {
        List<Move> moveList = new List<Move>(); //any time a move is legal, add it to this list
        
        List<Cannon> cannons = CheckForCannons(x, y, state, curr); // check if current stone is part of 1 or more cannons
        CheckForCannons(x, y, state, opp); // update count for enemy cannons too
        if (cannons.Count > 0) // if current stone is part of 1 or more cannons, check legal moves for each
        {
            foreach (Cannon c in cannons)
            {
                if (c.Direction == 1) //n-s
                {
                    // -- SHOOT --
                    // For N-S cannon: for back.y - 2 (or -3), front.y +2 (or +3) try to shoot
                    if (c.Front.Y + 1 >= 0 && c.Front.Y + 1 <= 9)
                    {
                        if (state.Board[c.Front.X, c.Front.Y + 1] == 0) // Only shoot if spots in between are empty
                        {
                            if (CanShootCannon(c.Front.X, c.Front.Y + 2, state, opp)) moveList.Add(new Move(c.Front.X, c.Front.Y + 2, 2));
                            else if (c.Front.Y + 2 >= 0 && c.Front.Y + 2 <= 9)
                                if (state.Board[c.Front.X, c.Front.Y + 2] == 0)
                                    if (CanShootCannon(c.Front.X, c.Front.Y + 3, state, opp)) moveList.Add(new Move(c.Front.X, c.Front.Y + 3, 2));
                        }
                    }
                    if (c.Back.Y - 1 >= 0 && c.Back.Y - 1 <= 9)
                    {
                        if (state.Board[c.Back.X, c.Back.Y - 1] == 0)
                        {
                            if (CanShootCannon(c.Back.X, c.Back.Y - 2, state, opp)) moveList.Add(new Move(c.Back.X, c.Back.Y - 2, 2));
                            else if (c.Back.Y - 2 >= 0 && c.Back.Y - 2 <= 9)
                                if (state.Board[c.Back.X, c.Back.Y - 2] == 0)
                                    if (CanShootCannon(c.Back.X, c.Back.Y - 3, state, opp)) moveList.Add(new Move(c.Back.X, c.Back.Y - 3, 2));
                        }
                    }

                    // -- SLIDE -- Move back stone of cannon to front or vice versa
                    if (x == c.Back.X && y == c.Back.Y && (c.Front.Y + 1) >= 0 && (c.Front.Y + 1) <= 9)
                        if (state.Board[c.Front.X, c.Front.Y + 1] == 0)
                            moveList.Add(new Move(c.Front.X, c.Front.Y + 1, 0));
                    if (x == c.Front.X && y == c.Front.Y && (c.Back.Y - 1) >= 0 && (c.Back.Y - 1) <= 9)
                        if (state.Board[c.Back.X, c.Back.Y - 1] == 0)
                            moveList.Add(new Move(c.Back.X, c.Back.Y - 1, 0));
                }
                else if (c.Direction == 2) //ne-sw
                {
                    // -- SHOOT --
                    //For NE-SW cannon: for back.x,y +,- 2 (or +,-3), front.x,y -,+2  (or -,+3) try to shoot
                    if (c.Front.X + 1 >= 0 && c.Front.X + 1 <= 9 && c.Front.Y - 1 >= 0 && c.Front.Y - 1 <= 9)
                    {
                        if (state.Board[c.Front.X + 1, c.Front.Y - 1] == 0)
                        {
                            if (CanShootCannon(c.Front.X + 2, c.Front.Y - 2, state, opp)) moveList.Add(new Move(c.Front.X + 2, c.Front.Y - 2, 2));
                            else if (c.Front.X + 2 >= 0 && c.Front.X + 2 <= 9 && c.Front.Y - 2 >= 0 && c.Front.Y - 2 <= 9)
                                if(state.Board[c.Front.X + 2, c.Front.Y - 2] == 0)
                                    if (CanShootCannon(c.Front.X + 3, c.Front.Y - 3, state, opp)) moveList.Add(new Move(c.Front.X + 3, c.Front.Y - 3, 2));
                        }
                    }
                    if (c.Back.X - 1 >= 0 && c.Back.X - 1 <= 9 && c.Back.Y + 1 >= 0 && c.Back.Y + 1 <= 9)
                    {
                        if (state.Board[c.Back.X - 1, c.Back.Y + 1] == 0)
                        {
                            if (CanShootCannon(c.Back.X - 2, c.Back.Y + 2, state, opp)) moveList.Add(new Move(c.Back.X - 2, c.Back.Y + 2, 2));
                            else if (c.Back.X - 2 >= 0 && c.Back.X - 2 <= 9 && c.Back.Y + 2 >= 0 && c.Back.Y + 2 <= 9)
                                if (state.Board[c.Back.X - 2, c.Back.Y + 2] == 0)
                                    if (CanShootCannon(c.Back.X - 3, c.Back.Y + 3, state, opp)) moveList.Add(new Move(c.Back.X - 3, c.Back.Y + 3, 2));
                        }
                    }

                    // -- SLIDE -- Move back stone of cannon to front or vice versa
                    if (x == c.Back.X && y == c.Back.Y)
                        if ((c.Front.X + 1) >= 0 && (c.Front.X + 1) <= 9 && (c.Front.Y - 1) >= 0 && (c.Front.Y - 1) <= 9)
                            if (state.Board[c.Front.X + 1, c.Front.Y - 1] == 0)
                                moveList.Add(new Move(c.Front.X + 1, c.Front.Y - 1, 0));
                    if (x == c.Front.X && y == c.Front.Y)
                        if ((c.Back.X - 1) >= 0 && (c.Back.X - 1) <= 9 && (c.Back.Y + 1) >= 0 && (c.Back.Y + 1) <= 9)
                            if (state.Board[c.Back.X - 1, c.Back.Y + 1] == 0)
                                moveList.Add(new Move(c.Back.X - 1, c.Back.Y + 1, 0));
                }
                else if (c.Direction == 3) //w-e
                {
                    // -- SHOOT --
                    // Fr W-E cannon: for back.x - 2 (or -3), front.x +2 (or + 3) try to shoot
                    if (c.Front.X + 1 >= 0 && c.Front.X + 1 <= 9)
                    {
                        if (state.Board[c.Front.X + 1, c.Front.Y] == 0)
                        {
                            if (CanShootCannon(c.Front.X + 2, c.Front.Y, state, opp)) moveList.Add(new Move(c.Front.X + 2, c.Front.Y, 2));
                            else if (c.Front.X + 2 >= 0 && c.Front.X + 2 <= 9)
                                if (state.Board[c.Front.X + 2, c.Front.Y] == 0)
                                    if (CanShootCannon(c.Front.X + 3, c.Front.Y, state, opp)) moveList.Add(new Move(c.Front.X + 3, c.Front.Y, 2));
                        }
                    }
                    if (c.Back.X - 1 >= 0 && c.Back.X - 1 <= 9)
                    {
                        if (state.Board[c.Back.X - 1, c.Back.Y] == 0)
                        {
                            if (CanShootCannon(c.Back.X - 2, c.Back.Y, state, opp)) moveList.Add(new Move(c.Back.X - 2, c.Back.Y, 2));
                            else if (c.Back.X - 2 >= 0 && c.Back.X - 2 <= 9)
                                if (state.Board[c.Back.X - 2, c.Back.Y] == 0)
                                    if (CanShootCannon(c.Back.X - 3, c.Back.Y, state, opp)) moveList.Add(new Move(c.Back.X - 3, c.Back.Y, 2));
                        }
                    }

                    // -- SLIDE --  Move back stone of cannon to front or vice versa 
                    if (x == c.Back.X && y == c.Back.Y && (c.Front.X + 1) >= 0 && (c.Front.X + 1) <= 9)
                        if (state.Board[c.Front.X + 1, c.Front.Y] == 0)
                            moveList.Add(new Move(c.Front.X + 1, c.Front.Y, 0));
                    if (x == c.Front.X && y == c.Front.Y && (c.Back.X - 1) >= 0 && (c.Back.X - 1) <= 9)
                        if (state.Board[c.Back.X - 1, c.Back.Y] == 0)
                            moveList.Add(new Move(c.Back.X - 1, c.Back.Y, 0));
                }
                else //nw-se
                {
                    // -- SHOOT --
                    // For NW-SE cannon: for back.x,y - 2 (or -3), front.x,y +2 (or +3), try to shoot
                    if (c.Front.X + 1 >= 0 && c.Front.X + 1 <= 9 && c.Front.Y + 1 >= 0 && c.Front.Y + 1 <= 9)
                    {
                        if (state.Board[c.Front.X + 1, c.Front.Y + 1] == 0)
                        {
                            if (CanShootCannon(c.Front.X + 2, c.Front.Y + 2, state, opp)) moveList.Add(new Move(c.Front.X + 2, c.Front.Y + 2, 2));
                            else if (c.Front.X + 2 >= 0 && c.Front.X + 2 <= 9 && c.Front.Y + 2 >= 0 && c.Front.Y + 2 <= 9)
                                if (state.Board[c.Front.X + 2, c.Front.Y + 2] == 0)
                                    if (CanShootCannon(c.Front.X + 3, c.Front.Y + 3, state, opp)) moveList.Add(new Move(c.Front.X + 3, c.Front.Y + 3, 2));
                        }
                    }
                    if (c.Back.X - 1 >= 0 && c.Back.X - 1 <= 9 && c.Back.Y - 1 >= 0 && c.Back.Y - 1 <= 9)
                    {
                        if (state.Board[c.Back.X - 1, c.Back.Y - 1] == 0)
                        {
                            if (CanShootCannon(c.Back.X - 2, c.Back.Y - 2, state, opp)) moveList.Add(new Move(c.Back.X - 2, c.Back.Y - 2, 2));
                            else if (c.Back.X - 2 >= 0 && c.Back.X - 2 <= 9 && c.Back.Y - 2 >= 0 && c.Back.Y - 2 <= 9)
                                if (state.Board[c.Back.X - 2, c.Back.Y - 2] == 0)
                                    if (CanShootCannon(c.Back.X - 3, c.Back.Y - 3, state, opp)) moveList.Add(new Move(c.Back.X - 3, c.Back.Y - 3, 2));
                        }
                    }

                    // -- SLIDE -- Move back stone of cannon to front or vice versa
                    if (x == c.Back.X && y == c.Back.Y)
                        if ((c.Front.X + 1) >= 0 && (c.Front.X + 1) <= 9 && (c.Front.Y + 1) >= 0 && (c.Front.Y + 1) <= 9)
                            if (state.Board[c.Front.X + 1, c.Front.Y + 1] == 0)
                                moveList.Add(new Move(c.Front.X + 1, c.Front.Y + 1, 0));
                    if (x == c.Front.X && y == c.Front.Y)
                        if ((c.Back.X - 1) >= 0 && (c.Back.X - 1) <= 9 && (c.Back.Y - 1) >= 0 && (c.Back.Y - 1) <= 9)
                            if (state.Board[c.Back.X - 1, c.Back.Y - 1] == 0)
                                moveList.Add(new Move(c.Back.X - 1, c.Back.Y - 1, 0));
                }
            }
        }
        // No longer checking cannons, just normal moves
        for (int i = x - 1; i <= x + 1; i++)
        {
            int t = 1;
            if (curr == 2) t = -1;
            int j = y + t;
            if (i >= 0 && i <= 9)
            {
                // empty or enemy soldiers / towns directly one space north/south, north/south-east, north/south-west
                if (j >= 0 && j <= 9)
                    if (state.Board[i, j] == 0 || state.Board[i, j] == opp || state.Board[i, j] == opp + 2)
                        moveList.Add(new Move(i, j, 0));
                // enemy squares directly east or west
                if (i != x && state.Board[i, y] == opp)
                    moveList.Add(new Move(i, y, 0));
            }
            bool retreat = false;
            // check if retreat is possible (i.e. if enemy is adjacent)
            for (int q = y - 1; q <= y + 1; q++)
            {
                if (i >= 0 && i <= 9 && q >= 0 && q <= 9)
                    if (state.Board[i, q] == opp)
                        retreat = true;
            }
            if (retreat) // enemy is adjacent: allow retreat backwards if space is free
            {
                t = t * -1; // reverse move direction
                if (x - 2 >= 0 && x - 2 <= 9 && (y + (t * 2)) >= 0 && (y + (t * 2)) <= 9) // check if retreat spot is on the board
                    if (state.Board[x - 1, y + (t * 1)] == 0 && state.Board[x - 2, y + (t * 2)] == 0)
                        moveList.Add(new Move(x - 2, y + (t * 2), 0));
                if (x >= 0 && x <= 9 && (y + (t * 2)) >= 0 && (y + (t * 2)) <= 9)
                    if (state.Board[x, y + (t * 1)] == 0 && state.Board[x, y + (t * 2)] == 0)
                        moveList.Add(new Move(x, y + (t * 2), 0));
                if (x + 2 >= 0 && x + 2 <= 9 && (y + (t * 2)) >= 0 && (y + (t * 2)) <= 9)
                    if (state.Board[x + 1, y + (t * 1)] == 0 && state.Board[x + 2, y + (t * 2)] == 0)
                        moveList.Add(new Move(x + 2, y + (t * 2), 0));
            }
        }
        return moveList;
    }

    public bool CanShootCannon(int cx, int cy, GameState state, int opp)
    {
        // Check if possible target is (1) on the board and (2) an enemy piece
        if (cy >= 0 && cy <= 9 && cx >= 0 && cx <= 9)
            if (state.Board[cx, cy] == opp || state.Board[cx, cy] == opp + 2) // allow shooting if piece is enemy soldier or town
                return true;
        return false;
    }

    public List<Cannon> CheckForCannons(int x, int y, GameState state, int curr)
    {
        // Basically the same code 4 times for each direction. TODO: GENERALISE CODE HERE
        // For: N-S, NE-SW, W-E, NW-SE, check five stones (+2 steps each direction from current stone)
        // If 3 or more connecting, create new Cannon object and add to list of current Cannons
        List<Cannon> cannons = new List<Cannon>();
        //---- north-south check ----
        int count = 0;
        List<Point> points = new List<Point>();
        for (int j = 0; j <= 9; j++)
        {
            if (state.Board[x, j] == curr)
            {
                count += 1;
                Point pt = new Point(x, j);
                points.Add(pt);
            }
            else // check not reached when j == 9 and [x,j] == current
            {
                if (count >= 3)
                {
                    Point back = points[0];
                    Point front = points[points.Count - 1];
                    Cannon northSouth = new Cannon(1, front, back);
                    cannons.Add(northSouth);
                }
                count = 0;
                points.Clear();
            }
        }
        if (count >= 3) // Case for when j ==0 (final cannon in line) : duplicate for other four checks
        {
            Point back = points[0];
            Point front = points[points.Count - 1];
            Cannon northSouth = new Cannon(1, front, back);
            cannons.Add(northSouth);
        }
        //---- northeast-southwest check ----
        count = 0;
        points.Clear();
        for (int t = 9; t >= -9; t--)
        {
            int i = x + t;
            int j = y + (t * -1);
            if ((i >= 0) && (i <= 9) && (j >= 0) && (j <= 9))
            {
                if (state.Board[i, j] == curr)
                {
                    count += 1;
                    Point pt = new Point(i, j);
                    points.Add(pt);
                }
                else
                {
                    if (count >= 3)
                    {
                        Point front = points[0];
                        Point back = points[points.Count - 1];
                        Cannon northEsouthW = new Cannon(2, front, back);
                        cannons.Add(northEsouthW);
                    }
                    count = 0;
                    points.Clear();
                }
            }
        }
        if (count >= 3)
        {
            Point front = points[0];
            Point back = points[points.Count - 1];
            Cannon northEsouthW = new Cannon(2, front, back);
            cannons.Add(northEsouthW);
        }
        //---- west-east check ----
        count = 0;
        points.Clear();
        for (int i = 0; i <= 9; i++)
        {
            if (state.Board[i, y] == curr)
            {
                count += 1;
                Point pt = new Point(i, y);
                points.Add(pt);
            }
            else
            {
                if (count >= 3)
                {
                    Point back = points[0];
                    Point front = points[points.Count - 1];
                    Cannon westEast = new Cannon(3, front, back);
                    cannons.Add(westEast);
                }
                count = 0;
                points.Clear();
            }
        }
        if (count >= 3)
        {
            Point back = points[0];
            Point front = points[points.Count - 1];
            Cannon westEast = new Cannon(3, front, back);
            cannons.Add(westEast);
        }
        //---- northwest-southeast check ----
        count = 0;
        points.Clear();
        for (int t = 9; t >= -9; t--)
        {
            int i = x + t;
            int j = y + t;
            if ((i >= 0) && (i <= 9) && (j >= 0) && (j <= 9))
            {
                if (state.Board[i, j] == curr)
                {
                    count += 1;
                    Point pt = new Point(i, j);
                    points.Add(pt);
                }
                else
                {
                    if (count >= 3)
                    {
                        Point front = points[0];
                        Point back = points[points.Count - 1];
                        Cannon northWsouthE = new Cannon(4, front, back);
                        cannons.Add(northWsouthE);
                    }
                    count = 0;
                    points.Clear();
                }
            }
        }
        if (count >= 3)
        {
            Point front = points[0];
            Point back = points[points.Count - 1];
            Cannon northWsouthE = new Cannon(4, front, back);
            cannons.Add(northWsouthE);
        }

        if (curr == 2) state.GreyCannons = cannons.Count;
        if (curr == 1) state.RedCannons = cannons.Count;
        return cannons;
    }

    // ----------------------------------------------------------------------
    // ~~~~~~~~~~~~~~~~~~~~~~~ Misscellaneous Extra ~~~~~~~~~~~~~~~~~~~~~~~~~
    // ----------------------------------------------------------------------
    public string ChessNotation(string type, int t)
    {
        if (type == "X")
        {
            if (t == 0) return "A";
            if (t == 1) return "B";
            if (t == 2) return "C";
            if (t == 3) return "D";
            if (t == 4) return "E";
            if (t == 5) return "F";
            if (t == 6) return "G";
            if (t == 7) return "H";
            if (t == 8) return "I";
            else return "J";
        }

        if (t == 0) return "0";
            if (t == 1) return "9";
            if (t == 2) return "8";
            if (t == 3) return "7";
            if (t == 4) return "6";
            if (t == 5) return "5";
            if (t == 6) return "4";
            if (t == 7) return "3";
            if (t == 8) return "2";
            else return "1";
    }
    public void UpdateHistory()
    {
        updateHistory = true;
    }
}
