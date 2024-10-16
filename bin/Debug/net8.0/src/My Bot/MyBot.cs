using System;
using System.Reflection.Metadata.Ecma335;
using ChessChallenge.API;


public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        Move[] allMoves = OrderMoves(board);
        Console.WriteLine(allMoves.Length);
        bool isWhite = board.IsWhiteToMove;

        float bestScore =0;
        Move bestMove = allMoves[0];
        foreach(Move move in allMoves){
        
            board.MakeMove(move);
            if(isWhite){
                float eval = Minimize(4, int.MinValue, int.MaxValue, board);
                board.UndoMove(move);
                 if(bestScore < eval){
                    bestScore = eval;
                    bestMove = move;
                }
            }
            else{
                float eval = Maximize(4, int.MinValue, int.MaxValue, board);
                board.UndoMove(move);
                if(bestScore > eval){
                    bestScore = eval;
                    bestMove = move;
                }
            }
            
        }
        Console.WriteLine("");
        Console.WriteLine(bestScore);
        Console.WriteLine(bestMove);
        return bestMove;
    } 
    public Move[] OrderMoves(Board board){
        int[] pieceValues = [0, 100, 300,300,500, 900, 0];
        Move[] allMoves = board.GetLegalMoves();
        float[] weights = new float[allMoves.Length];
        for(int i =0; i <allMoves.Length; i++){
            float moveGuessScore = 0;
            Move m = allMoves[i];

            if(m.IsPromotion){moveGuessScore+= pieceValues[(int)m.PromotionPieceType];}
            if(m.IsCapture){
                moveGuessScore+= 10*pieceValues[(int)m.CapturePieceType]-pieceValues[(int)m.MovePieceType];
            }
            weights[i] = moveGuessScore;
        }
        Array.Sort(weights,allMoves);
        return allMoves;
    }
    public float EvaluateBoard(Board board){
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
                    eval += maps[(int)pL.TypeOfPieceInList][pL.GetPiece(i).Square.Index];
                }
                else{
                    eval -= maps[(int)pL.TypeOfPieceInList][63- pL.GetPiece(i).Square.Index];
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