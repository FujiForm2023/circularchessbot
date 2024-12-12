using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class mainBot : MonoBehaviour
{
    public BoardVisual boardVisual;
    public BoardBot boardBot;
    public BotController botController;
    public ZobristHashing zobristHashing;
    public class EvalPosition
    {
        public BoardBot.Position position;
        public BoardBot.Move bestMove;
        public int evalScore;
        public byte promotePiece;
    }

    public BoardBot.Position position = new BoardBot.Position();
    public BoardBot.Move bestMove = new BoardBot.Move();
    public int minDepth = 4;
    public int maxIncrease = 2;
    public int maxDepth = 6;
    public int checkExtendInc = 2;
    public int checkExtendMax = 4;
    public bool allowCheckExtend = false;
    public bool allowCheckExtendToMax = false;
    public bool allowCheckExtendOverMax = false;
    public int minTime = 1000; // ms
    public int maxTime = 2000; // ms
    public bool playAsWhite = false;
    public bool isPlaying = true; // Should bot work
    public bool isCalculating = false; // Is bot calculating position?
    public float searchStartTime;
    public int nodedOccurs = 0;
    public BoardBot.Position[] positionPool;
    public BoardBot.Move[] movePool;
    public int[] scorePool;
    public EvalPosition[] evalPositionPool;
    public List<BoardBot.Move>[] movesPerDepthPool;
    public byte[] promotePiecePool;
    public int reverseCurrentDepth = 0;
    public BoardBot.Move lastBestMove = new BoardBot.Move{rankFrom = 0, fileFrom = 0, rankTo = 0, fileTo = 0};
    public bool botControllerAllowed = false;
    public bool noBotController = false;
    public List<ulong> tranpositionTable = new List<ulong>();

    void Start()
    {
        boardVisual = GetComponent<BoardVisual>();
        boardBot = GetComponent<BoardBot>();
        zobristHashing = new ZobristHashing();
        positionPool = new BoardBot.Position[maxDepth+1];
        movePool = new BoardBot.Move[maxDepth+1];
        scorePool = new int[maxDepth+1];
        evalPositionPool = new EvalPosition[maxDepth+1];
        movesPerDepthPool = new List<BoardBot.Move>[maxDepth+1];
        promotePiecePool = new byte[maxDepth+1];
        ResetPool();
    }

    public void SearchMoves(BoardBot.Position position, int currentDepth, byte promoteToPiece){
        // Node Occurs
        ++nodedOccurs;
        // Best Move
        // // movePool[currentDepth] = new BoardBot.Move(); // Already created
        // Min value
        scorePool[currentDepth] = positionPool[currentDepth].whiteToMove ? int.MinValue : int.MaxValue; // Only call once per depth per node(move)
        // Promote Piece
        // promotePiecePool[currentDepth] = 0; // Already created

        ulong boardHash = zobristHashing.GenerateHash(positionPool[currentDepth].boardArray);
        if (currentDepth != 0)
        {
            if (tranpositionTable.Contains(boardHash)){
            return;
            } else {
                tranpositionTable.Add(boardHash);
            }
        }

        if (currentDepth != 0){
            // Get moves
            movesPerDepthPool[currentDepth].Clear();
            movesPerDepthPool[currentDepth].AddRange(positionPool[currentDepth].getAllSoftMoves());
            // Play move
            foreach (BoardBot.Move move in movesPerDepthPool[currentDepth]){
                // Search Time Out
                if (Time.realtimeSinceStartup - searchStartTime > minTime / 1000f)
                {
                    Debug.LogWarning("Search timed out after " + minTime + " ms.");
                    evalPositionPool[currentDepth].evalScore = scorePool[currentDepth];
                    evalPositionPool[currentDepth].position = positionPool[currentDepth];
                    evalPositionPool[currentDepth].bestMove = movePool[currentDepth];
                    evalPositionPool[currentDepth].promotePiece = promotePiecePool[currentDepth];
                    return;
                }
                if (BoardBot.isEnemyKing(positionPool[currentDepth].getPiece(move.squareFrom), positionPool[currentDepth].getPiece(move.squareTo))){
                    scorePool[currentDepth] = position.whiteToMove ? int.MaxValue : int.MinValue;
                    break;
                }
                // If is pawn
                byte piece = positionPool[currentDepth].getPiece(move.squareFrom);
                if ((BoardBot.getType(piece) == (byte)0x1) && (move.rankTo == (byte)0x0)){
                    for (byte promotionPiece = (byte)(2 + (positionPool[currentDepth].whiteToMove ? 0 : 8)); promotionPiece < 5 + (positionPool[currentDepth].whiteToMove ? 0 : 8); ++promotionPiece){
                        BoardBot.CopyPosition(positionPool[currentDepth], positionPool[currentDepth-1]);
                        positionPool[currentDepth-1].pawnPromotion(move, promotionPiece);
                        SearchMoves(positionPool[currentDepth-1], currentDepth-1, promotionPiece);
                        int SubScore = evalPositionPool[currentDepth-1].evalScore;
                        // Alpha = White
                        // Beta = Black
                        if (position.whiteToMove && (scorePool[currentDepth] < SubScore)){
                            scorePool[currentDepth] = SubScore;
                            movePool[currentDepth] = move;
                            promotePiecePool[currentDepth] = promotionPiece;
                        } else if (!position.whiteToMove && (scorePool[currentDepth] > SubScore)){
                            scorePool[currentDepth] = SubScore;
                            movePool[currentDepth] = move;
                            promotePiecePool[currentDepth] = promotionPiece;
                        }
                    }
                } else {
                    BoardBot.CopyPosition(positionPool[currentDepth], positionPool[currentDepth-1]);
                    positionPool[currentDepth-1].softMovePiece(move);
                    SearchMoves(positionPool[currentDepth-1], currentDepth-1, promotePiecePool[currentDepth]);
                    int SubScore = evalPositionPool[currentDepth-1].evalScore;
                    // Alpha = White
                    // Beta = Black
                    if (position.whiteToMove && (scorePool[currentDepth] < SubScore)){
                        scorePool[currentDepth] = SubScore;
                        movePool[currentDepth] = move;
                    } else if (!position.whiteToMove && (scorePool[currentDepth] > SubScore)){
                        scorePool[currentDepth] = SubScore;
                        movePool[currentDepth] = move;
                    }
                }
            }
        } else { // If depth == 0
            // End of depth
            evalPositionPool[currentDepth].evalScore = evalDefinition(position);
            evalPositionPool[currentDepth].position = position;
            evalPositionPool[currentDepth].promotePiece = promotePiecePool[currentDepth+1];
            return;
        }
        // Only occur when finish the search
        evalPositionPool[currentDepth].evalScore = scorePool[currentDepth];
        evalPositionPool[currentDepth].position = position;
        evalPositionPool[currentDepth].bestMove = movePool[currentDepth];
        evalPositionPool[currentDepth].promotePiece = promotePiecePool[currentDepth];
        return;
    }

    public int evalDefinition(BoardBot.Position position){
        int Score = 0;
        for (byte file = 0; file < 20; ++file){
            for (byte rank = 0; rank < 8; ++rank){
                byte piece = position.getPiece(rank, file);
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
            // White King
            case (byte)6:
                return 65536;
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
            // Black King
            case (byte)14:
                return -65536;
            // Blank, Void Idk what is the piece
            default:
                return 0;
        }
    }

    // Ready for next turn
    void Update(){
        if ((noBotController || botControllerAllowed) && isPlaying && !isCalculating && (playAsWhite == boardBot.currentPosition.whiteToMove) && (minDepth != 0))
        {    
            nodedOccurs = 0;

            ResetPool();

            // Begin Search
            isCalculating = true;

            // Set Timer
            searchStartTime = Time.realtimeSinceStartup;

            BoardBot.CopyPosition(boardBot.currentPosition, positionPool[minDepth]);

            // Calculate
            SearchMoves(boardBot.currentPosition, minDepth, 0);

            // Get Best Move
            BoardBot.Move bestMove = evalPositionPool[minDepth].bestMove;

            if ((bestMove.rankFrom == bestMove.rankTo && bestMove.fileFrom == bestMove.fileTo)){
                Debug.Log("Checkmate!");
                isPlaying = false;
            } else if ((BoardBot.getType(boardBot.currentPosition.getPiece(evalPositionPool[minDepth].bestMove.squareFrom)) == (byte)0x1) && (evalPositionPool[minDepth].bestMove.rankTo == (byte)0x0)){
                boardVisual.PromotePawnBot(evalPositionPool[minDepth].bestMove.rankFrom, evalPositionPool[minDepth].bestMove.fileFrom, evalPositionPool[minDepth].bestMove.rankTo, evalPositionPool[minDepth].bestMove.fileTo, evalPositionPool[minDepth].promotePiece);
            } else {
                boardBot.currentPosition.movePiece(bestMove);
                boardVisual.MakeMove(bestMove.rankFrom, bestMove.fileFrom, bestMove.rankTo, bestMove.fileTo);
            }
            lastBestMove.rankFrom = bestMove.rankFrom;
            lastBestMove.rankTo = bestMove.rankTo;
            lastBestMove.fileFrom = bestMove.fileFrom;
            lastBestMove.fileTo = bestMove.fileTo;
            isCalculating = false;
            Debug.Log(nodedOccurs + " nodes");
            Debug.Log((Time.realtimeSinceStartup - searchStartTime)*1000 + " ms");
            botControllerAllowed = !botControllerAllowed;
        }
    }

    bool SameMove(BoardBot.Move bestMove){
        if (bestMove.rankFrom == lastBestMove.rankFrom && bestMove.rankTo == lastBestMove.rankTo && bestMove.fileFrom == lastBestMove.fileFrom && bestMove.fileTo == lastBestMove.fileTo){
            return true;
        }
        return false;
    }

    public void ResetPool(){
        for (int i = 0; i < maxDepth+1; ++i)
        {
            positionPool[i] = new BoardBot.Position();
        }
        for (int i = 0; i < maxDepth+1; ++i)
        {
            movePool[i] = new BoardBot.Move();
        }
        for (int i = 0; i < maxDepth+1; ++i)
        {
            evalPositionPool[i] = new EvalPosition();
        }
        for (int i = 0; i < maxDepth+1; ++i)
        {
            movesPerDepthPool[i] = new List<BoardBot.Move>();
        }
        for (int i = 0; i < maxDepth+1; ++i)
        {
            promotePiecePool[i] = 0;
        }
        tranpositionTable.Clear();
    }
}
