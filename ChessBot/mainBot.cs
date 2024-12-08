using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class mainBot : MonoBehaviour
{
    public BoardVisual boardVisual;
    public BoardBot boardBot;

    public struct EvalPosition
    {
        public int evalScore;
        public BoardBot.Position position;
        public BoardBot.Move bestMove;
        public byte promotePiece;
    }

    public BoardBot.Position position = new BoardBot.Position();
    public BoardBot.Move bestMove = new BoardBot.Move();
    public int minDepth = 4;
    public int checkExtend = 1;
    public int maxDepth = 6;
    public int minTime = 1000; // ms
    public int maxTime = 2000; // ms
    public bool playAsWhite = false;
    public bool isPlaying = true; // Should bot work
    public bool isCalculating = false; // Is bot calculating position?
    public float searchStartTime;
    public int nodedOccurs = 0;

    void Start()
    {
        boardVisual = GetComponent<BoardVisual>();
        boardBot = GetComponent<BoardBot>();
    }

    public void getMoves()
    {
        SearchMoves(boardBot.currentPosition, 0, 0);
    }

    public EvalPosition SearchMoves(BoardBot.Position position, int currentDepth, byte promoteToPiece){
        ++nodedOccurs;
        // Best Move
        BoardBot.Move bestMove = new BoardBot.Move();
        // Min value
        int Score = position.whiteToMove ? int.MinValue : int.MaxValue;
        // Promote Piece
        byte bestPromotion = 0;
        if (currentDepth != 0){
            // Get moves
            List<BoardBot.Move> moves = boardBot.getAllMoves(position);
            // Play move
            foreach (BoardBot.Move move in moves){
                // Search Time Out
                if (Time.realtimeSinceStartup - searchStartTime > minTime / 1000f)
                {
                    Debug.LogWarning("Search timed out after " + minTime + " ms.");
                    return new EvalPosition { evalScore = Score, position = position, bestMove = bestMove, promotePiece = bestPromotion };
                }
                BoardBot.Position nextPosition = boardBot.movePiece(position, move);
                // If is pawn
                byte piece = boardBot.getPiece(position, move.squareFrom);
                if ((boardBot.getType(piece) == (byte)0x1) && (move.rankTo == (byte)0x0)){
                    for (byte promotionPiece = 2; promotionPiece < 5; ++promotionPiece){
                        nextPosition = boardBot.pawnPromotion(position, move, position.whiteToMove ? promotionPiece : (byte)(promotionPiece | 0b1000));
                        int SubScore = SearchMoves(nextPosition, currentDepth-1, promotionPiece).evalScore;
                        // Alpha = White
                        // Beta = Black
                        if (position.whiteToMove && (Score < SubScore)){
                            Score = SubScore;
                            bestMove = move;
                            bestPromotion = promotionPiece;
                        } else if (!position.whiteToMove && (Score > SubScore)){
                            Score = SubScore;
                            bestMove = move;
                            bestPromotion = promotionPiece;
                        }
                    }
                } else {
                    int SubScore = SearchMoves(nextPosition, currentDepth-1, bestPromotion).evalScore;
                    // Alpha = White
                    // Beta = Black
                    if (position.whiteToMove && (Score < SubScore)){
                        Score = SubScore;
                        bestMove = move;
                    } else if (!position.whiteToMove && (Score > SubScore)){
                        Score = SubScore;
                        bestMove = move;
                    }
                }
            }
        } else {
            // End of depth
            int evalScore = evalDefinition(position);
            return new EvalPosition{evalScore = evalDefinition(position), position = position, promotePiece = promoteToPiece};
        }
        // Only occur when finish the search
        return new EvalPosition{evalScore = Score, position = position, bestMove = bestMove, promotePiece = bestPromotion};
    }

    public int evalDefinition(BoardBot.Position position){
        int Score = 0;
        for (byte file = 0; file < 20; ++file){
            for (byte rank = 0; rank < 8; ++rank){
                byte piece = boardBot.getPiece(position, rank, file);
                Score += materialValue(piece);
            }
        }
        return Score;
    }

    // evalDefinition sub methods
    public int materialValue(byte piece){
        switch (piece){
            // White Pawn
            case (byte)1:
                return 100;
            // White Knight
            case (byte)2:
                return 200;
            // White Bishop
            case (byte)3:
                return 325;
            // White Rook
            case (byte)4:
                return 600;
            // White Queen
            case (byte)5:
                return 1100;
            // Black Pawn
            case (byte)9:
                return -100;
            // Black Knight
            case (byte)10:
                return -200;
            // Black Bishop
            case (byte)11:
                return -325;
            // Black Rook
            case (byte)12:
                return -600;
            // Black Queen
            case (byte)13:
                return -1100;
            // Blank, Void, Kings Idk what is the piece
            default:
                return 0;
        }
    }

    // Ready for next turn
    void Update(){
        if (isPlaying && !isCalculating && (playAsWhite == boardBot.currentPosition.whiteToMove) && (minDepth != 0))
        {      
            // Begin Search
            isCalculating = true;

            // Set Timer
            searchStartTime = Time.realtimeSinceStartup;

            // Node Occurs
            ++nodedOccurs;

            // Calculate
            EvalPosition bestPosition = SearchMoves(boardBot.currentPosition, minDepth, 0);

            // Get Best Move
            BoardBot.Move bestMove = bestPosition.bestMove;
            if ((boardBot.getType(boardBot.getPiece(boardBot.currentPosition, bestPosition.bestMove.squareFrom)) == (byte)0x1) && (bestPosition.bestMove.rankTo == (byte)0x0)){
                boardVisual.PromotePawnBot(bestPosition.bestMove.rankFrom, bestPosition.bestMove.fileFrom, bestPosition.bestMove.rankTo, bestPosition.bestMove.fileTo, bestPosition.promotePiece);
            } else {
                boardBot.movePiece(bestMove);
                boardVisual.MakeMove(bestMove.rankFrom, bestMove.fileFrom, bestMove.rankTo, bestMove.fileTo);
            }
            isCalculating = false;
            Debug.Log(nodedOccurs);
        }
    }
}
