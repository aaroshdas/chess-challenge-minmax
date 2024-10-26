using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks.Sources;
using ChessChallenge.API;
using Microsoft.CodeAnalysis.Emit;


public class MyBot : IChessBot
{
    public int maxDepth =4;
    public int totalEvals = 0;
    List<Move> pvMoves = new List<Move>();

    public Move Think(Board board, Timer timer)
    {
        Move[] allMoves = OrderMoves(board);
        bool isWhite = board.IsWhiteToMove;
         
        PriorityQueue<Move, int> pvBestMoves = new PriorityQueue<Move, int>();
    
        pvBestMoves.Clear();

        int sizeOfPVQueue = 0;

        float bestScore =0;
        Move bestMove = allMoves[0];

        int maxPV = 50;
        
        foreach(Move move in allMoves){
            board.MakeMove(move);
            if(isWhite){
                float eval = Minimize(maxDepth, int.MinValue, int.MaxValue, board);
                board.UndoMove(move);
                if(bestScore < eval){
                    bestScore = eval;
                    bestMove = move;
                }
                if(pvBestMoves.TryPeek(out Move result, out int priority)){
                    if(sizeOfPVQueue < maxPV){
                        pvBestMoves.Enqueue(move, (int)eval);
                        sizeOfPVQueue += 1;
                    }
                    else if(priority <= eval){
                        if(pvBestMoves.TryDequeue(out Move result2, out int priority2)){
                            sizeOfPVQueue -=1;
                        }
                        pvBestMoves.Enqueue(move, (int)eval);
                        sizeOfPVQueue += 1;
                    }
                }
                else{
                    pvBestMoves.Enqueue(move, (int)eval);
                }
            }
            else{
                float eval = Maximize(maxDepth, int.MinValue, int.MaxValue, board);
                board.UndoMove(move);
                if(bestScore > eval){
                    bestScore = eval;
                    bestMove = move;
                }
                if(pvBestMoves.TryPeek(out Move result, out int priority)){
                     if(sizeOfPVQueue < maxPV){
                        pvBestMoves.Enqueue(move, (int)eval);
                        sizeOfPVQueue += 1;
                    }
                    else if(priority >= eval){
                        if(pvBestMoves.TryDequeue(out Move result2, out int priority2)){
                            sizeOfPVQueue -=1;
                        }
                        pvBestMoves.Enqueue(move, (int)eval);
                        sizeOfPVQueue += 1;
                    }
                }
                else{
                    pvBestMoves.Enqueue(move, (int)eval);
                }
            }
            
        }
        pvMoves.Clear();
        while(pvBestMoves.TryDequeue(out Move result, out int priority)){
            pvMoves.Add(result);
            Console.WriteLine(result);
        }
        Console.WriteLine(EvaluateBoard(board));
        Console.WriteLine(bestMove);
        Console.WriteLine(totalEvals);
        totalEvals = 0;
        Console.WriteLine("");
        return bestMove;
    } 
    public Move[] OrderMoves(Board board){
        int[] pieceValues = [0, 100, 300,300,500, 900, 0];
        Move[] allMoves = board.GetLegalMoves();
        float[] weights = new float[allMoves.Length];
        

        for(int i =0; i <allMoves.Length; i++){
            float moveGuessScore = 0;
            Move m = allMoves[i];
            if(allMoves.Contains(m)){moveGuessScore+=10000;}
            if(m.IsPromotion){moveGuessScore+= pieceValues[(int)m.PromotionPieceType];}
            if(m.IsCapture){
                moveGuessScore+= 10*pieceValues[(int)m.CapturePieceType]-pieceValues[(int)m.MovePieceType];
            }
            
            weights[i] = moveGuessScore;
        }
        for (int i = 0; i < allMoves.Length-1; i++) {
            for (int j = i + 1; j > 0; j--) {
                int swapIndex = j - 1;
                if (weights[swapIndex] < weights[j]) {
                    (allMoves[j], allMoves[swapIndex]) = (allMoves[swapIndex], allMoves[j]);
                    (weights[j], weights[swapIndex]) = (weights[swapIndex], weights[j]);
                }
            }
        }
        return allMoves;
    }
    public float EvaluateBoard(Board board){
        totalEvals +=1;
        int[] pieceValues = [0, 100, 300,300,500, 900, 0];
    
        int[] pawnMap= [0,  0,  0,  0,  0,  0,  0,  0,
                50, 50, 50, 50, 50, 50, 50, 50,
                10, 10, 20, 30, 30, 20, 10, 10,
                5,  5, 10, 25, 25, 10,  5,  5,
                0,  0,  0, 20, 20,  0,  0,  0,
                5, -5,-10,  0,  0,-10, -5,  5,
                5, 10, 10,-20,-20, 10, 10,  5,
                0,  0,  0,  0,  0,  0,  0,  0];

        int[] knightMap = [-50,-40,-30,-30,-30,-30,-40,-50,
                            -40,-20,  0,  0,  0,  0,-20,-40,
                            -30,  0, 10, 15, 15, 10,  0,-30,
                            -30,  5, 15, 20, 20, 15,  5,-30,
                            -30,  0, 15, 20, 20, 15,  0,-30,
                            -30,  5, 10, 15, 15, 10,  5,-30,
                            -40,-20,  0,  5,  5,  0,-20,-40,
                            -50,-40,-30,-30,-30,-30,-40,-50];

        int[] bishopMap=[-20,-10,-10,-10,-10,-10,-10,-20,
-10,  0,  0,  0,  0,  0,  0,-10,
-10,  0,  5, 10, 10,  5,  0,-10,
-10,  5,  5, 10, 10,  5,  5,-10,
-10,  0, 10, 10, 10, 10,  0,-10,
-10, 10, 10, 10, 10, 10, 10,-10,
-10,  5,  0,  0,  0,  0,  5,-10,
-20,-10,-10,-10,-10,-10,-10,-20];

        int[] rookMap=[0,  0,  0,  0,  0,  0,  0,  0,
                        5, 10, 10, 10, 10, 10, 10,  5,
                        -5,  0,  0,  0,  0,  0,  0, -5,
                        -5,  0,  0,  0,  0,  0,  0, -5,
                        -5,  0,  0,  0,  0,  0,  0, -5,
                        -5,  0,  0,  0,  0,  0,  0, -5,
                        -5,  0,  0,  0,  0,  0,  0, -5,
                        0,  0,  0,  5,  5,  0,  0,  0];

        int[] queenMap=[-20,-10,-10, -5, -5,-10,-10,-20,
-10,  0,  0,  0,  0,  0,  0,-10,
-10,  0,  5,  5,  5,  5,  0,-10,
 -5,  0,  5,  5,  5,  5,  0, -5,
  0,  0,  5,  5,  5,  5,  0, -5,
-10,  5,  5,  5,  5,  5,  0,-10,
-10,  0,  5,  0,  0,  0,  0,-10,
-20,-10,-10, -5, -5,-10,-10,-20];

        int[] midGameKingMap=[-30,-40,-40,-50,-50,-40,-40,-30,
-30,-40,-40,-50,-50,-40,-40,-30,
-30,-40,-40,-50,-50,-40,-40,-30,
-30,-40,-40,-50,-50,-40,-40,-30,
-20,-30,-30,-40,-40,-30,-30,-20,
-10,-20,-20,-20,-20,-20,-20,-10,
 20, 20,  0,  0,  0,  0, 20, 20,
 20, 30, 10,  0,  0, 10, 30, 20];
        int[][] maps = [[],pawnMap, knightMap, bishopMap, rookMap, queenMap, midGameKingMap];
        float eval = 0;
        PieceList[] pieceLists = board.GetAllPieceLists();

        foreach(PieceList pL in pieceLists){
            for(int i =0; i < pL.Count; i++){
                if(pL.IsWhitePieceList){
                    eval += maps[(int)pL.TypeOfPieceInList][63-pL.GetPiece(i).Square.Index];
                }
                else{
                    eval -= maps[(int)pL.TypeOfPieceInList][pL.GetPiece(i).Square.Index];
                }
            }

            if(pL.IsWhitePieceList){
                eval+= pL.Count * pieceValues[(int)pL.TypeOfPieceInList];
            }
            else{    
                eval-= pL.Count * pieceValues[(int)pL.TypeOfPieceInList];
            }
        }
        return eval;
    }
    public float Maximize(int depth, float alpha, float beta, Board board){
        float maxEval = int.MinValue;
        if(board.IsInCheckmate()){return int.MinValue;}
        if(depth == 0){return EvaluateBoard(board);}   
        Move[] allMoves = OrderMoves(board);
        if(allMoves.Length == 0){return int.MinValue;}
        
        foreach (Move move in allMoves){
            board.MakeMove(move);
            float eval = Minimize(depth-1, alpha, beta, board);
            board.UndoMove(move);
            maxEval = Math.Max(eval, maxEval);
            alpha = Math.Max(alpha, eval);
            if(beta <= alpha){break;}
        }
        return maxEval;
    }
    public float Minimize(int depth, float alpha, float beta, Board board){
        float minEval = int.MaxValue;
        
        if(board.IsInCheckmate()){return int.MaxValue;}
        if(depth == 0){return EvaluateBoard(board);}   
        Move[] allMoves = OrderMoves(board);
        if(allMoves.Length == 0){return int.MaxValue;}

        foreach (Move move in allMoves){
            board.MakeMove(move);
            float eval = Maximize(depth-1, alpha, beta, board);
            board.UndoMove(move);
            minEval = Math.Min(minEval, eval);
            beta = Math.Min(beta, eval);
            if(beta <= alpha){break;}
        }
        return minEval;
    }
}