using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class BoardBot : MonoBehaviour
{
    public ZobristHashing zobristHash;
    public enum BoardPiece : byte
    {
        Blank = 0x0, // 0000
        WhitePawn = 0x1, // 0001
        WhiteKnight = 0x2, // 0010
        WhiteBishop = 0x3, // 0011
        WhiteRook = 0x4, // 0100
        WhiteQueen = 0x5, // 0101
        WhiteKing = 0x6, // 0110
        Void = 0x8, // 1000
        BlackPawn = 0x9, // 1001
        BlackKnight = 0xA, // 1010
        BlackBishop = 0xB, // 1011
        BlackRook = 0xC, // 1100
        BlackQueen = 0xD, // 1101
        BlackKing = 0xE // 1110

        // // For better sight

        // Blank = 0x0, // 0000, can also be white
        // Void = 0x8,  // 1000, can also be black
        // WhitePawn = 0x1, // 0001
        // BlackPawn = 0x9, // 1001
        // WhiteKnight = 0x2,  // 0010
        // BlackKnight = 0xA,  // 1010
        // WhiteBishop = 0x3, // 0011
        // BlackBishop = 0xB, // 1011
        // WhiteRook = 0x4, // 0100
        // BlackRook = 0xC, // 1100
        // WhiteQueen = 0x5, // 0101
        // BlackQueen = 0xD, // 1101
        // WhiteKing = 0x6, // 0110
        // BlackKing = 0xE // 1110
    }
    public enum GameStatus : byte
    {
        Ongoing = 0,
        WhiteWin = 1,
        BlackWin = 2,
        Draw = 3,
        Notstart = 4
    }
    public struct Square
    {
        public byte rank;
        private byte _file;
        public byte file
        {
            get
            {
                return _file;
            }
            set
            {
                _file = this.fileOverload(value);
            }
        }
        public byte fileOverload(byte file)
        {
            return (byte)((file + 40) % 20);
        }
    }
    public struct Position
    {
        public uint[] boardArray;
        public bool whiteToMove;
        public byte castleAbility;
        public bool K
        {
            get
            {
                return (castleAbility & 0b1000) == 8;
            }
            set
            {
                if (value)
                {
                    castleAbility |= 0b1000;
                }
                else
                {
                    castleAbility &= 0b0111;
                }
            }
        }
        public bool Q
        {
            get
            {
                return (castleAbility & 0b0100) == 4;
            }
            set
            {
                if (value)
                {
                    castleAbility |= 0b0100;
                }
                else
                {
                    castleAbility &= 0b1011;
                }
            }
        }
        public bool k
        {
            get
            {
                return (castleAbility & 0b0010) == 2;
            }
            set
            {
                if (value)
                {
                    castleAbility |= 0b0010;
                }
                else
                {
                    castleAbility &= 0b1101;
                }
            }
        }
        public bool q
        {
            get
            {
                return (castleAbility & 0b0001) == 1;
            }
            set
            {
                if (value)
                {
                    castleAbility |= 0b0001;
                }
                else
                {
                    castleAbility &= 0b1110;
                }
            }
        }
        public bool KQ
        {
            get
            {
                return (castleAbility & 0b1100) == 12;
            }
            set
            {
                if (value)
                {
                    castleAbility |= 0b1100;
                }
                else
                {
                    castleAbility &= 0b0011;
                }
            }
        }
        public bool kq
        {
            get
            {
                return (castleAbility & 0b0011) == 3;
            }
            set
            {
                if (value)
                {
                    castleAbility |= 0b0011;
                }
                else
                {
                    castleAbility &= 0b1100;
                }
            }
        }
        public byte halfMove;
        public ushort fullMove;
        public void nextTurn()
        {
            ++this.halfMove;
            this.whiteToMove = !this.whiteToMove;
            if (this.whiteToMove)
            {
                this.fullMove++;
            }
        }
        public Square whiteKingPosition;
        public bool whiteKingExists
        {
            get
            {
                return whiteKingPosition.rank == 255 || whiteKingPosition.file == 255;
            }
        }
        public Square blackKingPosition;
        public bool blackKingExists
        {
            get
            {
                return blackKingPosition.rank == 255 || blackKingPosition.file == 255;
            }
        }
        public ulong uniqueHash;
        public byte gameStatus;
        public bool isCastle;
        public Position deepClone(){
            Position newPosition = new Position();
            newPosition.boardArray = new uint[20];
            Array.Copy(boardArray, newPosition.boardArray, 20);
            newPosition.whiteToMove = whiteToMove;
            newPosition.castleAbility = castleAbility;
            newPosition.halfMove = halfMove;
            newPosition.fullMove = fullMove;
            newPosition.whiteKingPosition = whiteKingPosition;
            newPosition.blackKingPosition = blackKingPosition;
            newPosition.uniqueHash = uniqueHash;
            newPosition.gameStatus = gameStatus;
            newPosition.isCastle = isCastle;
            return newPosition;
        }
    }
    public struct Move
    {
        public byte rankFrom;
        public byte fileFrom;
        public byte rankTo;
        public byte fileTo;
        public Square squareFrom {
            get
            {
                return new Square{rank = rankFrom, file = fileFrom};
            }
        }
        public Square squareTo {
            get
            {
                return new Square{rank = rankTo, file = fileTo};
            }
        }
    }
    public Position currentPosition = new Position();
    public void Awake()
    {
        currentPosition = new Position();
        zobristHash = new ZobristHashing();
        currentPosition.boardArray = new uint[20]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
        currentPosition.whiteToMove = true;
        currentPosition.castleAbility = (byte)0b1111;
        currentPosition.halfMove = 0;
        currentPosition.fullMove = 1;
        currentPosition.whiteKingPosition = new Square{rank = 255, file = 255};
        currentPosition.blackKingPosition = new Square{rank = 255, file = 255};
        currentPosition.gameStatus = 0;
        currentPosition.uniqueHash = zobristHash.GenerateHash(currentPosition.boardArray);
        currentPosition.isCastle = false;
    }
    // BoardData Extends
    public bool rankOutOfBounds(int rank)
    {
        return rank < 0 || rank > 7;
    }
    public bool rankOutOfBounds(byte rank)
    {
        return rank < 0 || rank > 7;
    }
    // Method overloading
    public bool rankInBounds(int rank)
    {
        return rank >= 0 && rank <= 7;
    }
    public bool rankInBounds(byte rank)
    {
        return rank >= 0 && rank <= 7;
    }
    public bool isWhite(byte piece)
    {
        return (piece & 0b1000) == 0 && isPiece(piece);
    }
    public bool isBlack(byte piece)
    {
        return (piece & 0b1000) != 0 && isPiece(piece);
    }
    public bool isPiece(byte piece)
    {
        return (piece & 0b0111) != 0;
    }
    public bool isPieceSafeValid(byte piece)
    {
        return !(((piece & 0b0111) == 0) || ((piece & 0b0111) == 7));
    }
    public bool isBlank(byte piece)
    {
        return piece == 0;
    }
    public bool isVoid(byte piece)
    {
        return piece == 8;
    }
    public bool isPawn(byte piece)
    {
        return (piece & 0b0111) == 1;
        // WhitePawn = 0x1, // 0001
        // BlackPawn = 0x9, // 1001
    }
    public bool isKnight(byte piece)
    {
        return (piece & 0b0111) == 2;
        // WhiteKnight = 0x2, // 0010
        // BlackKnight = 0xA, // 1010
    }
    public bool isBishop(byte piece)
    {
        return (piece & 0b0111) == 3;
        // WhiteBishop = 0x3, // 0011
        // BlackBishop = 0xB, // 1011
    }
    public bool isRook(byte piece)
    {
        return (piece & 0b0111) == 4;
        // WhiteRook = 0x4, // 0100
        // BlackRook = 0xC, // 1100
    }
    public bool isQueen(byte piece)
    {
        return (piece & 0b0111) == 5;
        // WhiteQueen = 0x5, // 0101
        // BlackQueen = 0xD, // 1101
    }
    public bool isKing(byte piece)
    {
        return (piece & 0b0111) == 6;
        // WhiteKing = 0x6, // 0110
        // BlackKing = 0xE // 1110
    }
    public bool isMajorPiece(byte piece){
        return isKnight(piece) || isBishop(piece) || isRook(piece) || isQueen(piece) || isKing(piece);
    }
    public bool isMajorNoKingPiece(byte piece){
        return isKnight(piece) || isBishop(piece) || isRook(piece) || isQueen(piece);
    }
    public byte getType(byte piece)
    {
        return (byte)(piece & 0b0111);
    }
    // Method overloading
    public byte fileOverload(byte file)
    {
        return (byte)((file+40) % 20);
    }
    public byte fileOverload(int file)
    {
        return (byte)((file+40) % 20);
    }
    public bool isFriendly(byte piece1, byte piece2)
    {
        return (isWhite(piece1) == isWhite(piece2)) && isPiece(piece2);
    }
    public bool isEnemy(byte piece1, byte piece2)
    {
        return (isWhite(piece1) == isBlack(piece2)) && isPiece(piece2);
    }
    public bool isAttackable(byte piece1, byte piece2)
    {
        return isBlank(piece2) || isEnemy(piece1, piece2);
    }
    public bool isSameWhoToMove(Position position, byte piece){
        return !(position.whiteToMove ^ isWhite(piece));
    }
    public bool isNotSameWhoToMove(Position position, byte piece){
        return position.whiteToMove ^ isWhite(piece);
    }
    public byte getPiece(Position position, Square square)
    {
        return (byte)(position.boardArray[square.file] >> (square.rank * 4) & 0xf);
    }
    public byte getPiece(Position position, byte rank, byte file)
    {
        return (byte)(position.boardArray[fileOverload(file)] >> (rank * 4) & 0xf);
    }
    public byte getPiece(Position position, byte rank, int file)
    {
        return (byte)(position.boardArray[fileOverload(file)] >> (rank * 4) & 0xf);
    }
    public byte getPiece(Position position, int rank, byte file)
    {
        return (byte)(position.boardArray[fileOverload(file)] >> (rank * 4) & 0xf);
    }
    public byte getPiece(Position position, int rank, int file)
    {
        return (byte)(position.boardArray[fileOverload(file)] >> (rank * 4) & 0xf);
    }
    public byte getPiece(Square square)
    {
        return getPiece(currentPosition, square);
    }
    public byte getPiece(byte rank, byte file)
    {
        return getPiece(currentPosition, rank, file);
    }
    public byte getPiece(byte rank, int file)
    {
        return getPiece(currentPosition, rank, file);
    }
    public byte getPiece(int rank, byte file)
    {
        return getPiece(currentPosition, rank, file);
    }
    public byte getPiece(int rank, int file)
    {
        return getPiece(currentPosition, rank, file);
    }
    public Position setPiece(Position position, Square square, byte piece)
    {
        position.boardArray[square.file] &= (uint)~(0xf << (square.rank * 4));
        position.boardArray[square.file] |= (uint)(piece << (square.rank * 4));
        return position;
    }
    public Position setPiece(Position position, byte rank, byte file, byte piece)
    {
        Position newPosition = position;
        newPosition.boardArray[file] &= (uint)~(0xf << (rank * 4));
        newPosition.boardArray[file] |= (uint)(piece << (rank * 4));
        return newPosition;
    }
    public Position setPiece(Position position, byte rank, int file, byte piece)
    {
        Position newPosition = position.deepClone();
        newPosition.boardArray[file] &= (uint)~(0xf << (rank * 4));
        newPosition.boardArray[file] |= (uint)(piece << (rank * 4));
        return newPosition;
    }
    public Position setPiece(Position position, int rank, byte file, byte piece)
    {
        Position newPosition = position;
        newPosition.boardArray[file] &= (uint)~(0xf << (rank * 4));
        newPosition.boardArray[file] |= (uint)(piece << (rank * 4));
        return newPosition;
    }
    public Position setPiece(Position position, int rank, int file, byte piece)
    {
        Position newPosition = position.deepClone();
        newPosition.boardArray[file] &= (uint)~(0xf << (rank * 4));
        newPosition.boardArray[file] |= (uint)(piece << (rank * 4));
        return newPosition;
    }
    public Position softMovePiece(Position position, Move move){
        Position newPosition = position.deepClone();
        byte piece = getPiece(position, move.squareFrom);
        newPosition = setPiece(newPosition, move.squareTo, piece);
        newPosition = setPiece(newPosition, move.squareFrom, 0);
        if (isKing(piece)){
            if (isWhite(piece)){
                newPosition.whiteKingPosition = move.squareTo;
            } else {
                newPosition.blackKingPosition = move.squareTo;
            }
        }
        newPosition.nextTurn();
        return newPosition;
    }
    public void softMovePiece(Move move){
        currentPosition = softMovePiece(currentPosition, move);
    }
    public Position softMovePiece(Position position, Square squareFrom, Square squareTo){
        return softMovePiece(position, new Move{rankFrom = squareFrom.rank, fileFrom = squareFrom.file, rankTo = squareTo.rank, fileTo = squareTo.file});
    }
    public void softMovePiece(Square squareFrom, Square squareTo){
        currentPosition = softMovePiece(currentPosition, squareFrom, squareTo);
    }
    public Position softMovePiece(Position position, byte rankFrom, byte fileFrom, byte rankTo, byte fileTo){
        return softMovePiece(position, new Move{rankFrom = rankFrom, fileFrom = fileFrom, rankTo = rankTo, fileTo = fileTo});
    }
    public void softMovePiece(byte rankFrom, byte fileFrom, byte rankTo, byte fileTo){
        currentPosition = softMovePiece(currentPosition, rankFrom, fileFrom, rankTo, fileTo);
    }
    public Position movePiece(Position position, Move move) 
    {
        Position newPosition = position.deepClone();
        byte piece = getPiece(position, move.squareFrom);
        byte capturedPiece = getPiece(position, move.squareTo);
        bool captured = isPiece(capturedPiece);
        newPosition = setPiece(newPosition, move.squareTo, piece);
        newPosition = setPiece(newPosition, move.squareFrom, 0);

        if (newPosition.isCastle)
        {
            newPosition.isCastle = false;
        }

        // Castling
        if (isKing(piece))
        {
            if (isWhite(piece))
            {
                if (move.fileTo == 6 && newPosition.K)
                {
                    newPosition = setPiece(newPosition, move.rankFrom, 7, 0);
                    newPosition = setPiece(newPosition, move.rankFrom, 5, (byte)BoardPiece.WhiteRook);
                    newPosition.halfMove = 0;
                    newPosition.KQ = false;
                    newPosition.isCastle = true;
                }
                else if (move.fileTo == 2 && newPosition.Q)
                {
                    newPosition = setPiece(newPosition, move.rankFrom, 0, 0);
                    newPosition = setPiece(newPosition, move.rankFrom, 3, (byte)BoardPiece.WhiteRook);
                    newPosition.halfMove = 0;
                    newPosition.KQ = false;
                    newPosition.isCastle = true;
                }
                newPosition.whiteKingPosition = move.squareTo;
            }
            else
            {
                if (move.fileTo == 11 && newPosition.k)
                {
                    newPosition = setPiece(newPosition, move.rankFrom, 10, 0);
                    newPosition = setPiece(newPosition, move.rankFrom, 12, (byte)BoardPiece.BlackRook);
                    newPosition.halfMove = 0;
                    newPosition.kq = false;
                    newPosition.isCastle = true;
                }
                else if (move.fileTo == 15 && newPosition.q)
                {
                    newPosition = setPiece(newPosition, move.rankFrom, 17, 0);
                    newPosition = setPiece(newPosition, move.rankFrom, 14, (byte)BoardPiece.BlackRook);
                    newPosition.halfMove = 0;
                    newPosition.kq = false;
                    newPosition.isCastle = true;
                }
                newPosition.blackKingPosition = move.squareTo;
            }
        }

        // Castling Check
        if (!isWhite(getPiece(newPosition, 7, 4)) || !isKing(getPiece(newPosition, 7, 4)))
        {
            newPosition.KQ = false;
        }
        if (!isWhite(getPiece(newPosition, 7, 0)) || !isRook(getPiece(newPosition, 7, 0)))
        {
            newPosition.castleAbility &= 0b0111;
        }
        if (!isWhite(getPiece(newPosition, 7, 7)) || !isRook(getPiece(newPosition, 7, 7)))
        {
            newPosition.castleAbility &= 0b1011;
        }
        
        if (!isBlack(getPiece(newPosition, 7, 13)) || !isKing(getPiece(newPosition, 7, 13)))
        {
            newPosition.kq = false;
        }
        if (!isBlack(getPiece(newPosition, 7, 10)) || !isRook(getPiece(newPosition, 7, 10)))
        {
            newPosition.castleAbility &= 0b1101;
        }
        if (!isBlack(getPiece(newPosition, 7, 17)) || !isRook(getPiece(newPosition, 7, 17)))
        {
            newPosition.castleAbility &= 0b1110;
        }

        if (captured || isPawn(piece))
        {
            newPosition.halfMove = 0;
        }

        if (newPosition.gameStatus == 0)
        {
            newPosition = updateGameStatus(newPosition);
        }

        newPosition.nextTurn();

        return newPosition;
    }
    public void movePiece(Move move)
    {
        currentPosition = movePiece(currentPosition, move);
    }
    public Position movePiece(Position position, Square squareFrom, Square squareTo)
    {
        return movePiece(position, new Move{rankFrom = squareFrom.rank, fileFrom = squareFrom.file, rankTo = squareTo.rank, fileTo = squareTo.file});
    }
    public void movePiece(Square squareFrom, Square squareTo)
    {
        currentPosition = movePiece(currentPosition, squareFrom, squareTo);
    }
    public Position movePiece(Position position, byte rankFrom, byte fileFrom, byte rankTo, byte fileTo)
    {
        return movePiece(position, new Move{rankFrom = rankFrom, fileFrom = fileFrom, rankTo = rankTo, fileTo = fileTo});
    }
    public void movePiece(byte rankFrom, byte fileFrom, byte rankTo, byte fileTo)
    {
        currentPosition = movePiece(currentPosition, rankFrom, fileFrom, rankTo, fileTo);
    }
    public Move[] getMovement(Position position, byte rank, byte file)
    {
        byte piece = getPiece(position, rank, file);
        byte pieceType = getType(piece);
        List<Move> moves;
        switch (pieceType) {
            case 0x1:
                moves = movementPawn(position, rank, file, piece);
                break;
            case 0x2:
                moves = movementKnight(position, rank, file, piece);
                break;
            case 0x3:
                moves = movementBishop(position, rank, file, piece);
                break;
            case 0x4:
                moves = movementRook(position, rank, file, piece);
                break;
            case 0x5:
                moves = movementQueen(position, rank, file, piece);
                break;
            case 0x6:
                moves = movementKing(position, rank, file, piece);
                break;
            default:
                moves = new List<Move>();
                break;
        }

        // If draw?
        if (moves.Count == 0)
        {
            return moves.ToArray();
        }
        // If illegal move?
        moves.RemoveAll(eachMove => isCheckAfterMove(eachMove));

        return moves.ToArray();
    }
    public Move[] getMovement(Position position, Square square)
    {
        byte piece = getPiece(position, square);
        byte pieceType = getType(piece);
        List<Move> moves;
        switch (pieceType) {
            case 0x1:
                moves = movementPawn(position, square, piece);
                break;
            case 0x2:
                moves = movementKnight(position, square, piece);
                break;
            case 0x3:
                moves = movementBishop(position, square, piece);
                break;
            case 0x4:
                moves = movementRook(position, square, piece);
                break;
            case 0x5:
                moves = movementQueen(position, square, piece);
                break;
            case 0x6:
                moves = movementKing(position, square, piece);
                break;
            default:
                moves = new List<Move>();
                break;
        }

        // If draw?
        if (moves.Count == 0)
        {
            return moves.ToArray();
        }

        // If illegal move?
        moves.RemoveAll(eachMove => isCheckAfterMove(eachMove));

        return moves.ToArray();
    }
    public Move[] getSoftMovement(Position position, byte rank, byte file){
        byte piece = getPiece(position, rank, file);
        byte pieceType = getType(piece);
        List<Move> moves;
        switch (pieceType) {
            case 0x1:
                moves = movementPawn(position, rank, file, piece);
                break;
            case 0x2:
                moves = movementKnight(position, rank, file, piece);
                break;
            case 0x3:
                moves = movementBishop(position, rank, file, piece);
                break;
            case 0x4:
                moves = movementRook(position, rank, file, piece);
                break;
            case 0x5:
                moves = movementQueen(position, rank, file, piece);
                break;
            case 0x6:
                moves = movementKing(position, rank, file, piece);
                break;
            default:
                moves = new List<Move>();
                break;
        }
        return moves.ToArray();
    }
    public Move[] getSoftMovement(Position position, Square square){
        byte piece = getPiece(position, square);
        byte pieceType = getType(piece);
        List<Move> moves;
        switch (pieceType) {
            case 0x1:
                moves = movementPawn(position, square, piece);
                break;
            case 0x2:
                moves = movementKnight(position, square, piece);
                break;
            case 0x3:
                moves = movementBishop(position, square, piece);
                break;
            case 0x4:
                moves = movementRook(position, square, piece);
                break;
            case 0x5:
                moves = movementQueen(position, square, piece);
                break;
            case 0x6:
                moves = movementKing(position, square, piece);
                break;
            default:
                moves = new List<Move>();
                break;
        }
        return moves.ToArray();
    }
    public List<Move> movementPawn(Position position, byte rank, byte file, byte piece)
    {
        List<Move> moves = new List<Move>();
        if (isBlank(getPiece(position, rank-1, file)))
        {   
            moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank-1), fileTo = file});
            if (rank == 6 && isBlank(getPiece(position, rank-2, file)))
            {
                moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank-2), fileTo = file});
            }
        }
        if (rankInBounds(rank-1) && isEnemy(piece, getPiece(position, rank-1, file+1)))
        {
            moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank-1), fileTo = fileOverload(file+1)});
        }
        if (rankInBounds(rank-1) && isEnemy(piece, getPiece(position, rank-1, file-1)))
        {
            moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank-1), fileTo = fileOverload(file-1)});
        }
        return moves;
    }
    public List<Move> movementPawn(Position position, Square square, byte piece)
    {
        return movementPawn(position, square.rank, square.file, piece);
    }
    public List<Move> movementKnight(Position position, byte rank, byte file, byte piece)
    {
        List<Move> moves = new List<Move>();
        if (rankInBounds(rank+1) && isAttackable(piece, getPiece(position, rank+1, file+2)))
        {
            moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank+1), fileTo = fileOverload(file+2)});
        }
        if (rankInBounds(rank+1) && isAttackable(piece, getPiece(position, rank+1, file-2)))
        {
            moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank+1), fileTo = fileOverload(file-2)});
        }
        if (rankInBounds(rank-1) && isAttackable(piece, getPiece(position, rank-1, file+2)))
        {
            moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank-1), fileTo = fileOverload(file+2)});
        }
        if (rankInBounds(rank-1) && isAttackable(piece, getPiece(position, rank-1, file-2)))
        {
            moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank-1), fileTo = fileOverload(file-2)});
        }
        if (rankInBounds(rank+2) && isAttackable(piece, getPiece(position, rank+2, file+1)))
        {
            moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank+2), fileTo = fileOverload(file+1)});
        }
        if (rankInBounds(rank+2) && isAttackable(piece, getPiece(position, rank+2, file-1)))
        {
            moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank+2), fileTo = fileOverload(file-1)});
        }
        if (rankInBounds(rank-2) && isAttackable(piece, getPiece(position, rank-2, file+1)))
        {
            moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank-2), fileTo = fileOverload(file+1)});
        }
        if (rankInBounds(rank-2) && isAttackable(piece, getPiece(position, rank-2, file-1)))
        {
            moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank-2), fileTo = fileOverload(file-1)});
        }
        return moves;
    }
    public List<Move> movementKnight(Position position, Square square, byte piece)
    {
        return movementKnight(position, square.rank, square.file, piece);
    }
    public List<Move> movementBishop(Position position, byte rank, byte file, byte piece)
    {
        List<Move> moves = new List<Move>();

        byte i = 1;
        while (rankInBounds(rank+i) && isAttackable(piece, getPiece(position, rank+i, file+i)))
        {
            if (isPieceSafeValid(getPiece(position, rank+i, file+i)))
            {
                if (isEnemy(piece, getPiece(position, rank+i, file+i)))
                {
                    moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank+i), fileTo = fileOverload(file+i)});
                }
                break;
            }
            moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank+i), fileTo = fileOverload(file+i)});
            ++i;
        }

        i = 1;
        while (rankInBounds(rank-i) && isAttackable(piece, getPiece(position, rank-i, file-i)))
        {
            if (isPieceSafeValid(getPiece(position, rank-i, file-i)))
            {
                if (isEnemy(piece, getPiece(position, rank-i, file-i)))
                {
                    moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank-i), fileTo = fileOverload(file-i)});
                }
                break;
            }
            moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank-i), fileTo = fileOverload(file-i)});
            ++i;
        }

        i = 1;
        while (rankInBounds(rank+i) && isAttackable(piece, getPiece(position, rank+i, file-i)))
        {
            if (isPieceSafeValid(getPiece(position, rank+i, file-i)))
            {
                if (isEnemy(piece, getPiece(position, rank+i, file-i)))
                {
                    moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank+i), fileTo = fileOverload(file-i)});
                }
                break;
            }
            moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank+i), fileTo = fileOverload(file-i)});
            ++i;
        }

        i = 1;
        while (rankInBounds(rank-i) && isAttackable(piece, getPiece(position, rank-i, file+i)))
        {
            if (isPieceSafeValid(getPiece(position, rank-i, file+i)))
            {
                if (isEnemy(piece, getPiece(position, rank-i, file+i)))
                {
                    moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank-i), fileTo = fileOverload(file+i)});
                }
                break;
            }
            moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank-i), fileTo = fileOverload(file+i)});
            ++i;
        }
        
        return moves;
    }
    public List<Move> movementBishop(Position position, Square square, byte piece)
    {
        return movementBishop(position, square.rank, square.file, piece);
    }
    public List<Move> movementRook(Position position, byte rank, byte file, byte piece)
    {
        List<Move> moves = new List<Move>();

        byte i = 1;
        while (rankInBounds(rank+i) && isAttackable(piece, getPiece(position, rank+i, file)))
        {
            if (isPieceSafeValid(getPiece(position, rank+i, file)))
            {
                if (isEnemy(piece, getPiece(position, rank+i, file)))
                {
                    moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank+i), fileTo = file});
                }
                break;
            }
            moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank+i), fileTo = file});
            ++i;
        }

        i = 1;
        while (rankInBounds(rank-i) && isAttackable(piece, getPiece(position, rank-i, file)))
        {
            if (isPieceSafeValid(getPiece(position, rank-i, file)))
            {
                if (isEnemy(piece, getPiece(position, rank-i, file)))
                {
                    moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank-i), fileTo = file});
                }
                break;
            }
            moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank-i), fileTo = file});
            ++i;
        }

        i = 1;
        while (rankInBounds(rank) && isAttackable(piece, getPiece(position, rank, file+i)))
        {
            if (isPieceSafeValid(getPiece(position, rank, file+i)))
            {
                if (isEnemy(piece, getPiece(position, rank, file+i)))
                {
                    moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = rank, fileTo = fileOverload(file+i)});
                }
                break;
            }
            moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = rank, fileTo = fileOverload(file+i)});
            ++i;
            if (i >= 20){
                return moves;
            }
        }

        byte j = (byte)(19 - i);
        i = 1;
        while (rankInBounds(rank) && isAttackable(piece, getPiece(position, rank, file-i)))
        {
            if (isPieceSafeValid(getPiece(position, rank, file-i)))
            {
                if (isEnemy(piece, getPiece(position, rank, file-i)))
                {
                    moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = rank, fileTo = fileOverload(file-i)});
                }
                break;
            }
            moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = rank, fileTo = fileOverload(file-i)});
            ++i;
            if (i == j){
                return moves;
            } else if (i >= 20){
                return moves;
            }
        }

        return moves;
    }
    public List<Move> movementRook(Position position, Square square, byte piece)
    {
        return movementRook(position, square.rank, square.file, piece);
    }
    public List<Move> movementQueen(Position position, byte rank, byte file, byte piece)
    {
        List<Move> moves = new List<Move>();
        moves.AddRange(movementBishop(position, rank, file, piece));
        moves.AddRange(movementRook(position, rank, file, piece));
        return moves;
    }
    public List<Move> movementQueen(Position position, Square square, byte piece)
    {
        return movementQueen(position, square.rank, square.file, piece);
    }
    public List<Move> movementKing(Position position, byte rank, byte file, byte piece)
    {
        List<Move> moves = new List<Move>();
        if (rankInBounds(rank+1) && isAttackable(piece, getPiece(position, rank+1, file)))
        {
            moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank+1), fileTo = file});
        }
        if (rankInBounds(rank-1) && isAttackable(piece, getPiece(position, rank-1, file)))
        {
            moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank-1), fileTo = file});
        }
        if (isAttackable(piece, getPiece(position, rank, file+1)))
        {
            moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = rank, fileTo = fileOverload(file+1)});
        }
        if (isAttackable(piece, getPiece(position, rank, file-1)))
        {
            moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = rank, fileTo = fileOverload(file-1)});
        }
        if (rankInBounds(rank+1) && isAttackable(piece, getPiece(position, rank+1, file+1)))
        {
            moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank+1), fileTo = fileOverload(file+1)});
        }
        if (rankInBounds(rank+1) && isAttackable(piece, getPiece(position, rank+1, file-1)))
        {
            moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank+1), fileTo = fileOverload(file-1)});
        }
        if (rankInBounds(rank-1) && isAttackable(piece, getPiece(position, rank-1, file+1)))
        {
            moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank-1), fileTo = fileOverload(file+1)});
        }
        if (rankInBounds(rank-1) && isAttackable(piece, getPiece(position, rank-1, file-1)))
        {
            moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank-1), fileTo = fileOverload(file-1)});
        }

        // Castling
        if (isWhite(piece))
        {
            if (position.K)
            {
                if (isBlank(getPiece(position, 7, 5)) && isBlank(getPiece(position, 7, 6)) && !isCheckAfterMove(position, rank, file, rank, 5) && !isCheckAfterMove(position, rank, file, rank, 6))
                {
                    moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = 7, fileTo = 6});
                }
            }
            if (position.Q)
            {
                if (isBlank(getPiece(position, 7, 1)) && isBlank(getPiece(position, 7, 2)) && isBlank(getPiece(position, 7, 3)) && !isCheckAfterMove(position, rank, file, rank, 3) && !isCheckAfterMove(position, rank, file, rank, 2))
                {
                    moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = 7, fileTo = 2});
                }
            }
        }
        else
        {
            if (position.k)
            {
                if (isBlank(getPiece(position, 7, 11)) && isBlank(getPiece(position, 7, 12)) && !isCheckAfterMove(position, rank, file, rank, 11) && !isCheckAfterMove(position, rank, file, rank, 12))
                {
                    moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = 7, fileTo = 11});
                }
            }
            if (position.q)
            {
                if (isBlank(getPiece(position, 7, 14)) && isBlank(getPiece(position, 7, 15)) && isBlank(getPiece(position, 7, 16)) && !isCheckAfterMove(position, rank, file, rank, 16) && !isCheckAfterMove(position, rank, file, rank, 15))
                {
                    moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = 7, fileTo = 15});
                }
            }
        }

        return moves;
    }
    public List<Move> movementKing(Position position, Square square, byte piece)
    {
        return movementKing(position, square.rank, square.file, piece);
    }
    public Position pawnPromotion(Position position, Move move, byte promotePiece)
    {
        Position newPosition = position.deepClone();
        newPosition = setPiece(newPosition, move.rankTo, move.fileTo, promotePiece);
        newPosition = setPiece(newPosition, move.rankFrom, move.fileFrom, 0);
        if (newPosition.gameStatus == 0){
            newPosition = updateGameStatus(newPosition);
        }
        newPosition.nextTurn();
        return newPosition;
    }
    public void pawnPromotion(Move move, byte promotePiece)
    {
        currentPosition = pawnPromotion(currentPosition, move, promotePiece);
    }
    public Position pawnPromotion(Position position, byte rankFrom, byte fileFrom, byte rankTo, byte fileTo, byte promotePiece)
    {
        return pawnPromotion(position, new Move{rankFrom = rankFrom, fileFrom = fileFrom, rankTo = rankTo, fileTo = fileTo}, promotePiece);
    }
    public void pawnPromotion(byte rankFrom, byte fileFrom, byte rankTo, byte fileTo, byte promotePiece)
    {
        currentPosition = pawnPromotion(currentPosition, rankFrom, fileFrom, rankTo, fileTo, promotePiece);
    }
    public Position pawnPromotion(Position position, Square squareFrom, Square squareTo, byte promotePiece)
    {
        return pawnPromotion(position, squareFrom.rank, squareFrom.file, squareTo.rank, squareTo.file, promotePiece);
    }
    public void pawnPromotion(Square squareFrom, Square squareTo, byte promotePiece)
    {
        currentPosition = pawnPromotion(currentPosition, squareFrom, squareTo, promotePiece);
    }
    public bool isCheckAfterMove(Position position, Move move)
    {
        Position newPosition = softMovePiece(position, move);
        return isCheck(newPosition);
    }
    public bool isCheckAfterMove(Move move)
    {
        return isCheckAfterMove(currentPosition, move);
    }
    public bool isCheckAfterMove(Position position, Square sqaureFrom, Square squareTo)
    {
        Move move = new Move{rankFrom = sqaureFrom.rank, fileFrom = sqaureFrom.file, rankTo = squareTo.rank, fileTo = squareTo.file};
        return isCheckAfterMove(position, move);
    }
    public bool isCheckAfterMove(Square sqaureFrom, Square squareTo)
    {
        Move move = new Move{rankFrom = sqaureFrom.rank, fileFrom = sqaureFrom.file, rankTo = squareTo.rank, fileTo = squareTo.file};
        return isCheckAfterMove(currentPosition, move);
    }
    public bool isCheckAfterMove(Position position, byte rankFrom, byte fileFrom, byte rankTo, byte fileTo)
    {
        Move move = new Move{rankFrom = rankFrom, fileFrom = fileFrom, rankTo = rankTo, fileTo = fileTo};
        return isCheckAfterMove(position, move);
    }
    public bool isCheckAfterMove(byte rankFrom, byte fileFrom, byte rankTo, byte fileTo)
    {
        Move move = new Move{rankFrom = rankFrom, fileFrom = fileFrom, rankTo = rankTo, fileTo = fileTo};
        return isCheckAfterMove(currentPosition, move);
    }
    public bool isCheck(Position position)
    {
        Square kingPosition = position.whiteToMove ? position.blackKingPosition : position.whiteKingPosition;
        byte rank = kingPosition.rank;
        byte file = kingPosition.file;
        byte piece = position.whiteToMove ? (byte)BoardPiece.BlackKing : (byte)BoardPiece.WhiteKing;

        // Knight
        if (rankInBounds(rank+1) && isEnemy(piece, getPiece(position, rank+1, file+2)) && isKnight(getPiece(position, rank+1, file+2)))
        {
            return true;
        }
        if (rankInBounds(rank+1) && isEnemy(piece, getPiece(position, rank+1, file-2)) && isKnight(getPiece(position, rank+1, file-2)))
        {
            return true;
        }
        if (rankInBounds(rank-1) && isEnemy(piece, getPiece(position, rank-1, file+2)) && isKnight(getPiece(position, rank-1, file+2)))
        {
            return true;
        }
        if (rankInBounds(rank-1) && isEnemy(piece, getPiece(position, rank-1, file-2)) && isKnight(getPiece(position, rank-1, file-2)))
        {
            return true;
        }
        if (rankInBounds(rank+2) && isEnemy(piece, getPiece(position, rank+2, file+1)) && isKnight(getPiece(position, rank+2, file+1)))
        {
            return true;
        }
        if (rankInBounds(rank+2) && isEnemy(piece, getPiece(position, rank+2, file-1)) && isKnight(getPiece(position, rank+2, file-1)))
        {
            return true;
        }
        if (rankInBounds(rank-2) && isEnemy(piece, getPiece(position, rank-2, file+1)) && isKnight(getPiece(position, rank-2, file+1)))
        {
            return true;
        }
        if (rankInBounds(rank-2) && isEnemy(piece, getPiece(position, rank-2, file-1)) && isKnight(getPiece(position, rank-2, file-1)))
        {
            return true;
        }
        
        // Bishop
        byte i = 1;
        while (rankInBounds(rank+i) && isAttackable(piece, getPiece(position, rank+i, file+i)))
        {
            if (isEnemy(piece, getPiece(position, rank+i, file+i)))
            {
                if (isBishop(getPiece(position, rank+i, file+i)) || isQueen(getPiece(position, rank+i, file+i)))
                {
                    return true;
                } else {
                    break;
                }
            }
            ++i;
        }

        i = 1;
        while (rankInBounds(rank-i) && isAttackable(piece, getPiece(position, rank-i, file-i)))
        {
            if (isEnemy(piece, getPiece(position, rank-i, file-i)))
            {
                if (isBishop(getPiece(position, rank-i, file-i)) || isQueen(getPiece(position, rank-i, file-i)) || isPawn(getPiece(position, rank-1, file-1)))
                {
                    return true;
                } else {
                    break;
                }
            }
            ++i;
        }

        i = 1;
        while (rankInBounds(rank+i) && isAttackable(piece, getPiece(position, rank+i, file-i)))
        {
            if (isEnemy(piece, getPiece(position, rank+i, file-i)))
            {
                if (isBishop(getPiece(position, rank+i, file-i)) || isQueen(getPiece(position, rank+i, file-i)) || isPawn(getPiece(position, rank+i, file-i)))
                {
                    return true;
                } else {
                    break;
                }
            }
            ++i;
        }

        i = 1;
        while (rankInBounds(rank-i) && isAttackable(piece, getPiece(position, rank-i, file+i)))
        {
            if (isEnemy(piece, getPiece(position, rank-i, file+i)))
            {
                if (isBishop(getPiece(position, rank-i, file+i)) || isQueen(getPiece(position, rank-i, file+i)) || isPawn(getPiece(position, rank-1, file+1)))
                {
                    return true;
                } else {
                    break;
                }
            }
            ++i;
        }

        // Rook
        i = 1;
        while (rankInBounds(rank+i) && isAttackable(piece, getPiece(position, rank+i,file)))
        {
            if (isEnemy(piece, getPiece(position, rank+i, file)))
            {
                if (isRook(getPiece(position, rank+i, file)) || isQueen(getPiece(position, rank+i, file)))
                {
                    return true;
                } else {
                    break;
                }
            }
            ++i;
        }

        i = 1;
        while (rankInBounds(rank-i) && isAttackable(piece, getPiece(position, rank-i,file)))
        {
            if (isEnemy(piece, getPiece(position, rank-i, file)))
            {
                if (isRook(getPiece(position, rank-i, file)) || isQueen(getPiece(position, rank-i, file)))
                {
                    return true;
                } else {
                    break;
                }
            }
            ++i;
        }

        i = 1;
        while (isAttackable(piece, getPiece(position, rank,file+i)))
        {
            if (isEnemy(piece, getPiece(position, rank, file+i)))
            {
                if (isRook(getPiece(position, rank, file+i)) || isQueen(getPiece(position, rank, file+i)))
                {
                    return true;
                } else {
                    break;
                }
            }
            ++i;

            if (i >= 20){
                return false;
            }
        }

        byte j = (byte)(19-i);
        i = 1;
        while (isAttackable(piece, getPiece(position, rank,file-i)))
        {
            if (isEnemy(piece, getPiece(position, rank, file-i)))
            {
                if (isRook(getPiece(position, rank, file-i)) || isQueen(getPiece(position, rank, file-i)))
                {
                    return true;
                } else {
                    break;
                }
            }
            ++i;

            if (i == j){
                return false;
            } else if (i >= 20){
                return false;
            }
        }

        // King
        if (rankInBounds(rank+1) && isEnemy(piece, getPiece(position, rank+1, file)))
        {
            if (isKing(getPiece(position, rank+1, file)))
            {
                return true;
            }
        }
        if (rankInBounds(rank+1) && isEnemy(piece, getPiece(position, rank+1, file+1)))
        {
            if (isKing(getPiece(position, rank+1, file+1)))
            {
                return true;
            }
        }
        if (rankInBounds(rank+1) && isEnemy(piece, getPiece(position, rank+1, file-1)))
        {
            if (isKing(getPiece(position, rank+1, file-1)))
            {
                return true;
            }
        }
        if (rankInBounds(rank-1) && isEnemy(piece, getPiece(position, rank-1, file)))
        {
            if (isKing(getPiece(position, rank-1, file)))
            {
                return true;
            }
        }
        if (rankInBounds(rank-1) && isEnemy(piece, getPiece(position, rank-1, file+1)))
        {
            if (isKing(getPiece(position, rank-1, file+1)))
            {
                return true;
            }
        }
        if (rankInBounds(rank-1) && isEnemy(piece, getPiece(position, rank-1, file-1)))
        {
            if (isKing(getPiece(position, rank-1, file-1)))
            {
                return true;
            }
        }
        if (rankInBounds(rank) && isEnemy(piece, getPiece(position, rank, file+1)))
        {
            if (isKing(getPiece(position, rank, file+1)))
            {
                return true;
            }
        }
        if (rankInBounds(rank) && isEnemy(piece, getPiece(position, rank, file-1)))
        {
            if (isKing(getPiece(position, rank, file-1)))
            {
                return true;
            }
        }

        return false;
    }
    public bool isCheck()
    {
        return isCheck(currentPosition);
    }
    public bool isCheckMate(Position position)
    {
        if (!isCheck(position))
        {
            return false;
        }
        return isHaveNoMove(position);
    }
    public bool isCheckMate()
    {
        return isCheckMate(currentPosition);
    }
    public bool isStaleMate(Position position)
    {
        if (isCheck(position))
        {
            return false;
        }

        if (zobristHash.CheckForRepetition() || (position.halfMove == 100))
        {
            return true;
        }

        return isHaveNoMove(position);
    }
    public bool isStaleMate()
    {
        return isStaleMate(currentPosition);
    }
    public bool isHaveNoMove(Position position)
    {
        Position newPosition = position.deepClone();
        newPosition.whiteToMove = !newPosition.whiteToMove;
        return getAllMoves(newPosition).Count == 0;
    }
    public bool isHaveNoMove()
    {
        return isHaveNoMove(currentPosition);
    }
    public Position updateGameStatus(Position position)
    {
        if (isCheckMate(position))
        {
            position.gameStatus = position.whiteToMove ? (byte)GameStatus.WhiteWin : (byte)GameStatus.BlackWin;
        }
        else if (isStaleMate(position))
        {
            position.gameStatus = (byte)GameStatus.Draw;
        } else {
            position.gameStatus = (byte)GameStatus.Ongoing;
        }
        return position;
    }
    public Position updateGameStatus()
    {
        return updateGameStatus(currentPosition);
    }
    public bool isDrawByFiftyMoveRule(Position position)
    {
        return position.halfMove >= 100;
    }
    public bool isDrawByFiftyMoveRule()
    {
        return isDrawByFiftyMoveRule(currentPosition);
    }
    public List<Move> getAllMoves(Position position)
    {
        byte piece = position.whiteToMove ? (byte)BoardPiece.WhiteKing : (byte)BoardPiece.BlackKing;
        List<Move> moves = new List<Move>();
        for (byte rank = 0; rank < 8; ++rank)
        {
            for (byte file = 0; file < 20; ++file)
            {
                if (isFriendly(getPiece(position, rank, file), piece))
                {
                    Move[] move = getSoftMovement(position, rank, file);
                    foreach (Move eachMove in move)
                    {
                        if (!isCheckAfterMove(position, eachMove))
                        {
                            moves.Add(eachMove);
                        }
                    }
                }
            }
        }
        return moves;
    }
    public List<Move> getAllMoves()
    {
        return getAllMoves(currentPosition);
    }
    public List<Move> getAllSoftMoves(Position position){
        byte piece = position.whiteToMove ? (byte)BoardPiece.WhiteKing : (byte)BoardPiece.BlackKing;
        List<Move> moves = new List<Move>();
        for (byte rank = 0; rank < 8; ++rank)
        {
            for (byte file = 0; file < 20; ++file)
            {
                if (isFriendly(getPiece(position, rank, file), piece))
                {
                    moves.AddRange(getSoftMovement(position, rank, file));
                }
            }
        }
        return moves;
    }
    public List<Move> getAllSoftMoves(){
        return getAllSoftMoves(currentPosition);
    }
    public List<Move> legalizeMoves(Position position, List<Move> moves){
        List<Move> legalMoves = new List<Move>();
        foreach (Move move in moves){
            if (!isCheckAfterMove(position, move)){
                legalMoves.Add(move);
            }
        }
        return legalMoves;
    }
     // FEN Loader
    public Position CircularChessLoader(Position position, string str){

        // Warning : This is NOT FEN notation, this is a custom notation
        // This is a custom notation for Circular Chess

        // Notation :
        // PNBRQK is white
        // pnbrqk is black
        // / is for the next rank
        // 12345678 (number) is for blank
        // V is for void

        // Example :
        // Start position : RP6/NP6/BP5n/QP6/KP6/BP5n/NP6/RP6/VV5n/VV5N/rp6/np6/bp5N/kp6/qp6/bp5N/np6/rp6/VV5N/VV5n w KQkq 0 1

        // Load
        Position newPosition = position.deepClone();
        string[] FENArray = str.Split(' ');
        string[] strArray = FENArray[0].Split('/');
        byte i = 0;
        foreach (string strFile in strArray)
        {
            newPosition.boardArray[i] = 0;
            uint file = 0;
            byte j = 0;
            foreach (char strPiece in strFile) {
                ++j;
                file = file << 4;
                switch (strPiece) {
                    case 'R':
                        file = file | (uint)BoardPiece.WhiteRook;
                        break;
                    case 'N':
                        file = file | (uint)BoardPiece.WhiteKnight;
                        break;
                    case 'B':
                        file = file | (uint)BoardPiece.WhiteBishop;
                        break;
                    case 'Q':
                        file = file | (uint)BoardPiece.WhiteQueen;
                        break;
                    case 'K':
                        file = file | (uint)BoardPiece.WhiteKing;
                        newPosition.whiteKingPosition = new Square{rank = (byte)(8-j), file = i};
                        break;
                    case 'P':   
                        file = file | (uint)BoardPiece.WhitePawn;
                        break;
                    case 'r':
                        file = file | (uint)BoardPiece.BlackRook;
                        break;
                    case 'n':
                        file = file | (uint)BoardPiece.BlackKnight;
                        break;
                    case 'b':
                        file = file | (uint)BoardPiece.BlackBishop;
                        break;
                    case 'q':
                        file = file | (uint)BoardPiece.BlackQueen;
                        break;
                    case 'k':
                        file = file | (uint)BoardPiece.BlackKing;
                        newPosition.blackKingPosition = new Square{rank = (byte)(8-j), file = i};
                        break;
                    case 'p':
                        file = file | (uint)BoardPiece.BlackPawn;
                        break;
                    case 'V':
                        file = file | (uint)BoardPiece.Void;
                        break;
                    case '1':
                        break;
                    case '2':
                        file = file << 4;
                        ++j; 
                        break;
                    case '3':
                        file = file << 8;
                        j += 2;
                        break;
                    case '4':
                        file = file << 12;
                        j += 3;
                        break;
                    case '5':
                        file = file << 16;
                        j += 4;
                        break;
                    case '6':
                        file = file << 20;
                        j += 5;
                        break;
                    case '7':
                        file = file << 24;
                        j += 6;
                        break;
                    case '8':
                        // file = file << 28; // it's already 0
                        break;
                    default:
                        Debug.Log("Error : FEN Load " + strPiece);
                        break;
                }
            }
            newPosition.boardArray[i++] = file;
        }

        // Who to move
        newPosition.whiteToMove = (FENArray[1] == "w") ? true : false;

        // Castling
        foreach (char c in FENArray[2])
        {
            switch (c)
            {
                case 'K':
                    newPosition.K = true;
                    break;
                case 'Q':
                    newPosition.Q = true;
                    break;
                case 'k':
                    newPosition.k = true;
                    break;
                case 'q':
                    newPosition.q = true;
                    break;
                case '-':
                    continue;
                case '~':
                    break;
                default:
                    Debug.Log("Error : FEN Load Castle " + c);
                    break;
            }
        }
        // Castling Check
        if (!isWhite(getPiece(newPosition, 7, 4)) | !isKing(getPiece(newPosition, 7, 4)))
        {
            newPosition.KQ = false;
        }
        if (!isWhite(getPiece(newPosition, 7, 0)) | !isRook(getPiece(newPosition, 7, 0)))
        {
            newPosition.K = false;
        }
        if (!isWhite(getPiece(newPosition, 7, 7)) | !isRook(getPiece(newPosition, 7, 7)))
        {
            newPosition.Q = false;
        }

        if (!isBlack(getPiece(newPosition, 7, 13)) | !isKing(getPiece(newPosition, 7, 13)))
        {
            newPosition.kq = false;
        }
        if (!isBlack(getPiece(newPosition, 7, 10)) | !isRook(getPiece(newPosition, 7, 10)))
        {
            newPosition.k = false;
        }
        if (!isBlack(getPiece(newPosition, 7, 17)) | !isRook(getPiece(newPosition, 7, 17)))
        {
            newPosition.q = false;
        }

        // Half move
        newPosition.halfMove = byte.Parse(FENArray[3]);

        // Full move
        newPosition.fullMove = ushort.Parse(FENArray[4]);

        // Update Game Status
        newPosition = updateGameStatus(newPosition);

        // Update Unique Hash
        newPosition.uniqueHash = zobristHash.GenerateHash(newPosition.boardArray);

        return newPosition;
    }
    public void CircularChessLoader(string str){
        currentPosition = CircularChessLoader(currentPosition, str);
    }
    public byte[][] deconstructMoves(Move[] moves){
        byte[][] deconstructedMoves = new byte[moves.Length][];
        for (int i = 0; i < moves.Length; ++i)
        {
            deconstructedMoves[i] = new byte[]{moves[i].rankTo, moves[i].fileTo};
        }
        return deconstructedMoves;
    }
    // Debug
    public void DumpBoard(Position position){
        for (byte rank = 0; rank < 8; rank++)
        {
            string str = "";
            for (byte file = 0; file < 20; file++)
            {
                byte piece = getPiece(position, rank, file);
                switch (piece){
                    case (byte)BoardPiece.WhitePawn:
                        str += "P";
                        break;
                    case (byte)BoardPiece.WhiteKnight:
                        str += "N";
                        break;
                    case (byte)BoardPiece.WhiteBishop:
                        str += "B";
                        break;
                    case (byte)BoardPiece.WhiteRook:
                        str += "R";
                        break;
                    case (byte)BoardPiece.WhiteQueen:
                        str += "Q";
                        break;
                    case (byte)BoardPiece.WhiteKing:
                        str += "K";
                        break;
                    case (byte)BoardPiece.BlackPawn:
                        str += "p";
                        break;
                    case (byte)BoardPiece.BlackKnight:
                        str += "n";
                        break;
                    case (byte)BoardPiece.BlackBishop:
                        str += "b";
                        break;
                    case (byte)BoardPiece.BlackRook:
                        str += "r";
                        break;
                    case (byte)BoardPiece.BlackQueen:
                        str += "q";
                        break;
                    case (byte)BoardPiece.BlackKing:
                        str += "k";
                        break;
                    case (byte)BoardPiece.Void:
                        str += "V";
                        break;
                    default:
                        str += "-";
                        break;
                }
            }
            Debug.Log(str);
        }
    }

    public void DumpSquare(Square square){
        Debug.Log("Rank : " + square.rank + " File : " + square.file);
    }

    public void DumpPosition(Position position){
        DumpBoard(position);
        Debug.Log("White to move : " + position.whiteToMove);
        Debug.Log("White King Position : " + position.whiteKingPosition.rank + " " + position.whiteKingPosition.file);
        Debug.Log("Black King Position : " + position.blackKingPosition.rank + " " + position.blackKingPosition.file);
        Debug.Log("K : " + position.K + " Q : " + position.Q + " k : " + position.k + " q : " + position.q);
        Debug.Log("Half Move : " + position.halfMove);
        Debug.Log("Full Move : " + position.fullMove);
        Debug.Log("Game Status : " + position.gameStatus);
    }
}