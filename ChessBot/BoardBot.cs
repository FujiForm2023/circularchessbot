using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class BoardBot : MonoBehaviour
{
    // Constants, make it easy to access

    // BoardData
    public static ZobristHashing zobristHash = new ZobristHashing();
    [SerializeField]
    public Position currentPosition = new Position();
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
        public Move reverse {
            get
            {
                return new Move{rankFrom = rankTo, fileFrom = fileTo, rankTo = rankFrom, fileTo = fileFrom};
            }
        }
    }
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
    public static bool rankOutOfBounds(int rank)
    {
        return rank < 0 || rank > 7;
    }
    public static bool rankOutOfBounds(ref int rank)
    {
        return rank < 0 || rank > 7;
    }
    public static bool rankOutOfBounds(byte rank)
    {
        return rank < 0 || rank > 7;
    }
    public static bool rankOutOfBounds(ref byte rank)
    {
        return rank < 0 || rank > 7;
    }
    // Method overloading
    public static bool rankInBounds(int rank)
    {
        return rank >= 0 && rank <= 7;
    }
    public static bool rankInBounds(ref int rank)
    {
        return rank >= 0 && rank <= 7;
    }
    public static bool rankInBounds(byte rank)
    {
        return rank >= 0 && rank <= 7;
    }
    public static bool rankInBounds(ref byte rank)
    {
        return rank >= 0 && rank <= 7;
    }
    public static bool isWhite(byte piece)
    {
        return (piece & 0b1000) == 0 && isPiece(ref piece);
    }
    public static bool isWhite(ref byte piece)
    {
        return (piece & 0b1000) == 0 && isPiece(ref piece);
    }
    public static bool isBlack(byte piece)
    {
        return (piece & 0b1000) != 0 && isPiece(ref piece);
    }
    public static bool isBlack(ref byte piece)
    {
        return (piece & 0b1000) != 0 && isPiece(ref piece);
    }
    public static bool isPiece(byte piece)
    {
        return (piece & 0b0111) != 0;
    }
    public static bool isPiece(ref byte piece)
    {
        return (piece & 0b0111) != 0;
    }
    public static bool isPieceSafeValid(byte piece)
    {
        return !(((piece & 0b0111) == 0) || ((piece & 0b0111) == 7));
    }
    public static bool isPieceSafeValid(ref byte piece)
    {
        return !(((piece & 0b0111) == 0) || ((piece & 0b0111) == 7));
    }
    public static bool isBlank(byte piece)
    {
        return piece == 0;
    }
    public static bool isBlank(ref byte piece)
    {
        return piece == 0;
    }
    public static bool isVoid(byte piece)
    {
        return piece == 8;
    }
    public static bool isVoid(ref byte piece)
    {
        return piece == 8;
    }
    public static bool isPawn(byte piece)
    {
        return (piece & 0b0111) == 1;
        // WhitePawn = 0x1, // 0001
        // BlackPawn = 0x9, // 1001
    }
    public static bool isPawn(ref byte piece)
    {
        return (piece & 0b0111) == 1;
        // WhitePawn = 0x1, // 0001
        // BlackPawn = 0x9, // 1001
    }
    public static bool isKnight(byte piece)
    {
        return (piece & 0b0111) == 2;
        // WhiteKnight = 0x2, // 0010
        // BlackKnight = 0xA, // 1010
    }
    public static bool isKnight(ref byte piece)
    {
        return (piece & 0b0111) == 2;
        // WhiteKnight = 0x2, // 0010
        // BlackKnight = 0xA, // 1010
    }
    public static bool isBishop(byte piece)
    {
        return (piece & 0b0111) == 3;
        // WhiteBishop = 0x3, // 0011
        // BlackBishop = 0xB, // 1011
    }
    public static bool isBishop(ref byte piece)
    {
        return (piece & 0b0111) == 3;
        // WhiteBishop = 0x3, // 0011
        // BlackBishop = 0xB, // 1011
    }
    public static bool isRook(byte piece)
    {
        return (piece & 0b0111) == 4;
        // WhiteRook = 0x4, // 0100
        // BlackRook = 0xC, // 1100
    }
    public static bool isRook(ref byte piece)
    {
        return (piece & 0b0111) == 4;
        // WhiteRook = 0x4, // 0100
        // BlackRook = 0xC, // 1100
    }
    public static bool isQueen(byte piece)
    {
        return (piece & 0b0111) == 5;
        // WhiteQueen = 0x5, // 0101
        // BlackQueen = 0xD, // 1101
    }
    public static bool isQueen(ref byte piece)
    {
        return (piece & 0b0111) == 5;
        // WhiteQueen = 0x5, // 0101
        // BlackQueen = 0xD, // 1101
    }
    public static bool isKing(byte piece)
    {
        return (piece & 0b0111) == 6;
        // WhiteKing = 0x6, // 0110
        // BlackKing = 0xE // 1110
    }
    public static bool isKing(ref byte piece)
    {
        return (piece & 0b0111) == 6;
        // WhiteKing = 0x6, // 0110
        // BlackKing = 0xE // 1110
    }
    public static bool isMajorPiece(byte piece){
        return isKnight(ref piece) || isBishop(ref piece) || isRook(ref piece) || isQueen(ref piece) || isKing(ref piece);
    }
    public static bool isMajorPiece(ref byte piece){
        return isKnight(ref piece) || isBishop(ref piece) || isRook(ref piece) || isQueen(ref piece) || isKing(ref piece);
    }
    public static bool isMajorNoKingPiece(byte piece){
        return isKnight(ref piece) || isBishop(ref piece) || isRook(ref piece) || isQueen(ref piece);
    }
    public static bool isMajorNoKingPiece(ref byte piece){
        return isKnight(ref piece) || isBishop(ref piece) || isRook(ref piece) || isQueen(ref piece);
    }
    public static byte getType(byte piece)
    {
        return (byte)(piece & 0b0111);
    }
    public static byte getType(ref byte piece)
    {
        return (byte)(piece & 0b0111);
    }
    // Method overloading
    public static byte fileOverload(byte file)
    {
        return (byte)((file+40) % 20);
    }
    public static byte fileOverload(ref byte file)
    {
        return (byte)((file+40) % 20);
    }
    public static byte fileOverload(int file)
    {
        return (byte)((file+40) % 20);
    }
    public static byte fileOverload(ref int file)
    {
        return (byte)((file+40) % 20);
    }
    public static bool isFriendly(byte piece1, byte piece2)
    {
        return (isWhite(ref piece1) == isWhite(ref piece2)) && isPiece(ref piece2);
    }
    public static bool isFriendly(ref byte piece1, ref byte piece2)
    {
        return (isWhite(ref piece1) == isWhite(ref piece2)) && isPiece(ref piece2);
    }
    public static bool isFriendlyPawn(byte piece1, byte piece2)
    {
        return (isWhite(ref piece1) == isWhite(ref piece2)) && isPawn(ref piece2);
    }
    public static bool isFriendlyPawn(ref byte piece1, ref byte piece2)
    {
        return (isWhite(ref piece1) == isWhite(ref piece2)) && isPawn(ref piece2);
    }
    public static bool isFriendlyKnight(byte piece1, byte piece2)
    {
        return (isWhite(ref piece1) == isWhite(ref piece2)) && isKnight(ref piece2);
    }
    public static bool isFriendlyKnight(ref byte piece1, ref byte piece2)
    {
        return (isWhite(ref piece1) == isWhite(ref piece2)) && isKnight(ref piece2);
    }
    public static bool isFriendlyBishop(byte piece1, byte piece2)
    {
        return (isWhite(ref piece1) == isWhite(ref piece2)) && isBishop(ref piece2);
    }
    public static bool isFriendlyBishop(ref byte piece1, ref byte piece2)
    {
        return (isWhite(ref piece1) == isWhite(ref piece2)) && isBishop(ref piece2);
    }
    public static bool isFriendlyRook(byte piece1, byte piece2)
    {
        return (isWhite(ref piece1) == isWhite(ref piece2)) && isRook(ref piece2);
    }
    public static bool isFriendlyRook(ref byte piece1, ref byte piece2)
    {
        return (isWhite(ref piece1) == isWhite(ref piece2)) && isRook(ref piece2);
    }
    public static bool isFriendlyQueen(byte piece1, byte piece2)
    {
        return (isWhite(ref piece1) == isWhite(ref piece2)) && isQueen(ref piece2);
    }
    public static bool isFriendlyQueen(ref byte piece1, ref byte piece2)
    {
        return (isWhite(ref piece1) == isWhite(ref piece2)) && isQueen(ref piece2);
    }
    public static bool isFriendlyKing(byte piece1, byte piece2)
    {
        return (isWhite(ref piece1) == isWhite(ref piece2)) && isKing(ref piece2);
    }
    public static bool isFriendlyKing(ref byte piece1, ref byte piece2)
    {
        return (isWhite(ref piece1) == isWhite(ref piece2)) && isKing(ref piece2);
    }
    public static bool isEnemy(byte piece1, byte piece2)
    {
        return isWhite(ref piece1) == isBlack(ref piece2);
    }
    public static bool isEnemy(ref byte piece1, ref byte piece2)
    {
        return isWhite(ref piece1) == isBlack(ref piece2);
    }
    public static bool isEnemyPawn(byte piece1, byte piece2)
    {
        return (isWhite(ref piece1) == isBlack(ref piece2)) && isPawn(ref piece2);
    }
    public static bool isEnemyPawn(ref byte piece1, ref byte piece2)
    {
        return (isWhite(ref piece1) == isBlack(ref piece2)) && isPawn(ref piece2);
    }
    public static bool isEnemyKnight(byte piece1, byte piece2)
    {
        return (isWhite(ref piece1) == isBlack(ref piece2)) && isKnight(ref piece2);
    }
    public static bool isEnemyKnight(ref byte piece1, ref byte piece2)
    {
        return (isWhite(ref piece1) == isBlack(ref piece2)) && isKnight(ref piece2);
    }
    public static bool isEnemyBishop(byte piece1, byte piece2)
    {
        return (isWhite(ref piece1) == isBlack(ref piece2)) && isBishop(ref piece2);
    }
    public static bool isEnemyBishop(ref byte piece1, ref byte piece2)
    {
        return (isWhite(ref piece1) == isBlack(ref piece2)) && isBishop(ref piece2);
    }
    public static bool isEnemyRook(byte piece1, byte piece2)
    {
        return (isWhite(ref piece1) == isBlack(ref piece2)) && isRook(ref piece2);
    }
    public static bool isEnemyRook(ref byte piece1, ref byte piece2)
    {
        return (isWhite(ref piece1) == isBlack(ref piece2)) && isRook(ref piece2);
    }
    public static bool isEnemyQueen(byte piece1, byte piece2)
    {
        return (isWhite(ref piece1) == isBlack(ref piece2)) && isQueen(ref piece2);
    }
    public static bool isEnemyQueen(ref byte piece1, ref byte piece2)
    {
        return (isWhite(ref piece1) == isBlack(ref piece2)) && isQueen(ref piece2);
    }
    public static bool isEnemyKing(byte piece1, byte piece2)
    {
        return (isWhite(ref piece1) == isBlack(ref piece2)) && isKing(ref piece2);
    }
    public static bool isEnemyKing(ref byte piece1, ref byte piece2)
    {
        return (isWhite(ref piece1) == isBlack(ref piece2)) && isKing(ref piece2);
    }
    public static bool isAttackable(byte piece1, byte piece2)
    {
        return isBlank(ref piece2) || isEnemy(ref piece1, ref piece2);
    }
    public static bool isAttackable(ref byte piece1, ref byte piece2)
    {
        return isBlank(ref piece2) || isEnemy(ref piece1, ref piece2);
    }
    [System.Serializable]
    public class Position
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
            Array.Copy(this.boardArray, newPosition.boardArray, 20);
            newPosition.whiteToMove = this.whiteToMove;
            newPosition.castleAbility = this.castleAbility;
            newPosition.halfMove = this.halfMove;
            newPosition.fullMove = this.fullMove;
            newPosition.whiteKingPosition = this.whiteKingPosition;
            newPosition.blackKingPosition = this.blackKingPosition;
            newPosition.uniqueHash = this.uniqueHash;
            newPosition.gameStatus = this.gameStatus;
            newPosition.isCastle = this.isCastle;
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

        // Position methods
        public byte getPiece(Square square)
        {
            int shift = square.rank << 2;
            return (byte)((boardArray[square.file] >> shift) & 0xF);
        }
        public byte getPiece(ref Square square)
        {
            int shift = square.rank << 2;
            return (byte)((boardArray[square.file] >> shift) & 0xF);
        }
        public byte getPiece(byte rank, byte file)
        {
            int shift = rank << 2;
            return (byte)((boardArray[fileOverload(file)] >> shift) & 0xF);
        }
        public byte getPiece(ref byte rank, ref byte file)
        {
            int shift = rank << 2;
            return (byte)((boardArray[fileOverload(file)] >> shift) & 0xF);
        }
        public byte getPiece(byte rank, int file)
        {
            int shift = rank << 2;
            return (byte)((boardArray[fileOverload(file)] >> shift) & 0xF);
        }
        public byte getPiece(ref byte rank, ref int file)
        {
            int shift = rank << 2;
            return (byte)((boardArray[fileOverload(file)] >> shift) & 0xF);
        }
        public byte getPiece(int rank, byte file)
        {
            int shift = rank << 2;
            return (byte)((boardArray[fileOverload(file)] >> shift) & 0xF);
        }
        public byte getPiece(ref int rank, ref byte file)
        {
            int shift = rank << 2;
            return (byte)((boardArray[fileOverload(file)] >> shift) & 0xF);
        }
        public byte getPiece(int rank, int file)
        {
            int shift = rank << 2;
            return (byte)((boardArray[fileOverload(file)] >> shift) & 0xF);
        }
        public byte getPiece(ref int rank, ref int file)
        {
            int shift = rank << 2;
            return (byte)((boardArray[fileOverload(file)] >> shift) & 0xF);
        }
        public void setPiece(Square square, byte piece)
        {
            int shift = square.rank << 2;
            boardArray[square.file] &= (uint)~(0xF << shift);
            boardArray[square.file] |= (uint)(piece << shift);
        }
        public void setPiece(byte rank, byte file, byte piece)
        {
            int shift = rank << 2;
            boardArray[file] &= (uint)~(0xF << shift);
            boardArray[file] |= (uint)(piece << shift);
        }
        public void setPiece(byte rank, int file, byte piece)
        {
            int shift = rank << 2;
            boardArray[file] &= (uint)~(0xF << shift);
            boardArray[file] |= (uint)(piece << shift);
        }
        public void setPiece(int rank, byte file, byte piece)
        {
            int shift = rank << 2;
            boardArray[file] &= (uint)~(0xF << shift);
            boardArray[file] |= (uint)(piece << shift);
        }
        public void setPiece(int rank, int file, byte piece)
        {
            int shift = rank << 2;
            boardArray[file] &= (uint)~(0xF << shift);
            boardArray[file] |= (uint)(piece << shift);
        }
        public void softMovePiece(Move move){
            byte piece = getPiece(move.squareFrom);
            setPiece(move.squareTo, piece);
            setPiece(move.squareFrom, 0);
            if (isKing(piece)){
                if (isWhite(piece)){
                    whiteKingPosition = move.squareTo;
                } else {
                    blackKingPosition = move.squareTo;
                }
            }
            whiteToMove = !whiteToMove;
        }
        public void softMovePiece(Square squareFrom, Square squareTo){
            byte piece = getPiece(squareFrom);
            setPiece(squareTo, piece);
            setPiece(squareFrom, 0);
            if (isKing(piece)){
                if (isWhite(piece)){
                    whiteKingPosition = squareTo;
                } else {
                    blackKingPosition = squareTo;
                }
            }
            whiteToMove = !whiteToMove;
        }
        public void softMovePiece(byte rankFrom, byte fileFrom, byte rankTo, byte fileTo){
            byte piece = getPiece(rankFrom, fileFrom);
            setPiece(rankTo, fileTo, piece);
            setPiece(rankFrom, fileFrom, 0);
            if (isKing(piece)){
                if (isWhite(piece)){
                    whiteKingPosition.rank = rankTo;
                    whiteKingPosition.file = fileTo;
                } else {
                    blackKingPosition.rank = rankTo;
                    blackKingPosition.file = fileTo;
                }
            }
            whiteToMove = !whiteToMove;
        }
        public void movePiece(Move move) 
        {
            byte piece = getPiece(move.squareFrom);
            bool captured = isPiece(getPiece(move.squareTo));
            setPiece(move.squareTo, piece);
            setPiece(move.squareFrom, 0);

            if (isCastle)
            {
                isCastle = false;
            }

            // Castling
            if (isKing(piece))
            {
                if (isWhite(piece))
                {
                    if (move.fileTo == 6 && K)
                    {
                        setPiece(7, 7, 0);
                        setPiece(7, 5, 4);
                        halfMove = 0;
                        KQ = false;
                        isCastle = true;
                    }
                    else if (move.fileTo == 2 && Q)
                    {
                        setPiece(7, 0, 0);
                        setPiece(7, 3, 4);
                        halfMove = 0;
                        KQ = false;
                        isCastle = true;
                    }
                    whiteKingPosition = move.squareTo;
                }
                else
                {
                    if (move.fileTo == 11 && k)
                    {
                        setPiece(7, 10, 0);
                        setPiece(7, 12, 12);
                        halfMove = 0;
                        kq = false;
                        isCastle = true;
                    }
                    else if (move.fileTo == 15 && q)
                    {
                        setPiece(7, 17, 0);
                        setPiece(7, 14, 12);
                        halfMove = 0;
                        kq = false;
                        isCastle = true;
                    }
                    blackKingPosition = move.squareTo;
                }
            }

            // Castling Check
            if (!isWhite(getPiece(7, 4)) || !isKing(getPiece(7, 4)))
            {
                KQ = false;
            }
            if (!isWhite(getPiece(7, 0)) || !isRook(getPiece(7, 0)))
            {
                castleAbility &= 7;
            }
            if (!isWhite(getPiece(7, 7)) || !isRook(getPiece(7, 7)))
            {
                castleAbility &= 11;
            }
            
            if (!isBlack(getPiece(7, 13)) || !isKing(getPiece(7, 13)))
            {
                kq = false;
            }
            if (!isBlack(getPiece(7, 10)) || !isRook(getPiece(7, 10)))
            {
                castleAbility &= 13;
            }
            if (!isBlack(getPiece(7, 17)) || !isRook(getPiece(7, 17)))
            {
                castleAbility &= 14;
            }

            if (captured || isPawn(piece))
            {
                halfMove = 0;
            }

            if (gameStatus == 0)
            {
                updateGameStatus();
            }

            nextTurn();
        }
        public void movePiece(Square squareFrom, Square squareTo)
        {
            byte piece = getPiece(squareFrom);
            bool captured = isPiece(getPiece(squareTo));
            setPiece(squareTo, piece);
            setPiece(squareFrom, 0);

            if (isCastle)
            {
                isCastle = false;
            }

            // Castling
            if (isKing(piece))
            {
                if (isWhite(piece))
                {
                    if (squareTo.file == 6 && K)
                    {
                        setPiece(7, 7, 0);
                        setPiece(7, 5, 4);
                        halfMove = 0;
                        KQ = false;
                        isCastle = true;
                    }
                    else if (squareTo.file == 2 && Q)
                    {
                        setPiece(7, 0, 0);
                        setPiece(7, 3, 4);
                        halfMove = 0;
                        KQ = false;
                        isCastle = true;
                    }
                    whiteKingPosition = squareTo;
                }
                else
                {
                    if (squareTo.file == 11 && k)
                    {
                        setPiece(7, 10, 0);
                        setPiece(7, 12, 12);
                        halfMove = 0;
                        kq = false;
                        isCastle = true;
                    }
                    else if (squareTo.file == 15 && q)
                    {
                        setPiece(7, 17, 0);
                        setPiece(7, 14, 12);
                        halfMove = 0;
                        kq = false;
                        isCastle = true;
                    }
                    blackKingPosition = squareTo;
                }
            }

            // Castling Check
            if (!isWhite(getPiece(7, 4)) || !isKing(getPiece(7, 4)))
            {
                KQ = false;
            }
            if (!isWhite(getPiece(7, 0)) || !isRook(getPiece(7, 0)))
            {
                castleAbility &= 7;
            }
            if (!isWhite(getPiece(7, 7)) || !isRook(getPiece(7, 7)))
            {
                castleAbility &= 11;
            }
            
            if (!isBlack(getPiece(7, 13)) || !isKing(getPiece(7, 13)))
            {
                kq = false;
            }
            if (!isBlack(getPiece(7, 10)) || !isRook(getPiece(7, 10)))
            {
                castleAbility &= 13;
            }
            if (!isBlack(getPiece(7, 17)) || !isRook(getPiece(7, 17)))
            {
                castleAbility &= 14;
            }

            if (captured || isPawn(piece))
            {
                halfMove = 0;
            }

            if (gameStatus == 0)
            {
                updateGameStatus();
            }

            nextTurn();
        }
        public void movePiece(byte rankFrom, byte fileFrom, byte rankTo, byte fileTo)
        {
            byte piece = getPiece(rankFrom, fileFrom);
            bool captured = isPiece(getPiece(rankTo, fileTo));
            setPiece(rankTo, fileTo, piece);
            setPiece(rankFrom, fileFrom, 0);

            if (isCastle)
            {
                isCastle = false;
            }

            // Castling
            if (isKing(piece))
            {
                if (isWhite(piece))
                {
                    if (fileTo == 6 && K)
                    {
                        setPiece(7, 7, 0);
                        setPiece(7, 5, 4);
                        halfMove = 0;
                        KQ = false;
                        isCastle = true;
                    }
                    else if (fileTo == 2 && Q)
                    {
                        setPiece(7, 0, 0);
                        setPiece(7, 3, 4);
                        halfMove = 0;
                        KQ = false;
                        isCastle = true;
                    }
                    whiteKingPosition.rank = rankTo;
                    whiteKingPosition.file = fileTo;
                }
                else
                {
                    if (fileTo == 11 && k)
                    {
                        setPiece(7, 10, 0);
                        setPiece(7, 12, 12);
                        halfMove = 0;
                        kq = false;
                        isCastle = true;
                    }
                    else if (fileTo == 15 && q)
                    {
                        setPiece(7, 17, 0);
                        setPiece(7, 14, 12);
                        halfMove = 0;
                        kq = false;
                        isCastle = true;
                    }
                    blackKingPosition.rank = rankTo;
                    blackKingPosition.file = fileTo;
                }
            }

            // Castling Check
            if (!isWhite(getPiece(7, 4)) || !isKing(getPiece(7, 4)))
            {
                KQ = false;
            }
            if (!isWhite(getPiece(7, 0)) || !isRook(getPiece(7, 0)))
            {
                castleAbility &= 7;
            }
            if (!isWhite(getPiece(7, 7)) || !isRook(getPiece(7, 7)))
            {
                castleAbility &= 11;
            }
            
            if (!isBlack(getPiece(7, 13)) || !isKing(getPiece(7, 13)))
            {
                kq = false;
            }
            if (!isBlack(getPiece(7, 10)) || !isRook(getPiece(7, 10)))
            {
                castleAbility &= 13;
            }
            if (!isBlack(getPiece(7, 17)) || !isRook(getPiece(7, 17)))
            {
                castleAbility &= 14;
            }

            if (captured || isPawn(piece))
            {
                halfMove = 0;
            }

            if (gameStatus == 0)
            {
                updateGameStatus();
            }

            nextTurn();
        }
        public Move[] getMovement(byte rank, byte file)
        {
            byte piece = getPiece(rank, file);
            byte pieceType = getType(piece);
            List<Move> moves;
            switch (pieceType) {
                case 1:
                    moves = movementPawn(rank, file, piece);
                    break;
                case 2:
                    moves = movementKnight(rank, file, piece);
                    break;
                case 3:
                    moves = movementBishop(rank, file, piece);
                    break;
                case 4:
                    moves = movementRook(rank, file, piece);
                    break;
                case 5:
                    moves = movementQueen(rank, file, piece);
                    break;
                case 6:
                    moves = movementKing(rank, file, piece);
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
        public Move[] getMovement(Square square)
        {
            byte piece = getPiece(square);
            byte pieceType = getType(piece);
            List<Move> moves;
            switch (pieceType) {
                case 1:
                    moves = movementPawn(square, piece);
                    break;
                case 2:
                    moves = movementKnight(square, piece);
                    break;
                case 3:
                    moves = movementBishop(square, piece);
                    break;
                case 4:
                    moves = movementRook(square, piece);
                    break;
                case 5:
                    moves = movementQueen(square, piece);
                    break;
                case 6:
                    moves = movementKing(square, piece);
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
        public Move[] getSoftMovement(byte rank, byte file){
            byte piece = getPiece(rank, file);
            byte pieceType = getType(piece);
            List<Move> moves;
            switch (pieceType) {
                case 1:
                    moves = movementPawn(rank, file, piece);
                    break;
                case 2:
                    moves = movementKnight(rank, file, piece);
                    break;
                case 3:
                    moves = movementBishop(rank, file, piece);
                    break;
                case 4:
                    moves = movementRook(rank, file, piece);
                    break;
                case 5:
                    moves = movementQueen(rank, file, piece);
                    break;
                case 6:
                    moves = movementKing(rank, file, piece);
                    break;
                default:
                    moves = new List<Move>();
                    break;
            }
            return moves.ToArray();
        }
        public Move[] getSoftMovement(Square square){
            byte piece = getPiece(square);
            byte pieceType = getType(piece);
            List<Move> moves;
            switch (pieceType) {
                case 1:
                    moves = movementPawn(square, piece);
                    break;
                case 2:
                    moves = movementKnight(square, piece);
                    break;
                case 3:
                    moves = movementBishop(square, piece);
                    break;
                case 4:
                    moves = movementRook(square, piece);
                    break;
                case 5:
                    moves = movementQueen(square, piece);
                    break;
                case 6:
                    moves = movementKing(square, piece);
                    break;
                default:
                    moves = new List<Move>();
                    break;
            }
            return moves.ToArray();
        }
        public List<Move> getSoftMovementList(byte rank, byte file){
            byte piece = getPiece(ref rank, ref file);
            byte pieceType = getType(piece);
            List<Move> moves;
            switch (pieceType) {
                case 0x1:
                    moves = movementPawn(rank, file, piece);
                    break;
                case 0x2:
                    moves = movementKnight(rank, file, piece);
                    break;
                case 0x3:
                    moves = movementBishop(rank, file, piece);
                    break;
                case 0x4:
                    moves = movementRook(rank, file, piece);
                    break;
                case 0x5:
                    moves = movementQueen(rank, file, piece);
                    break;
                case 0x6:
                    moves = movementKing(rank, file, piece);
                    break;
                default:
                    moves = new List<Move>();
                    break;
            }
            return moves;
        }
        public List<Move> getSoftMovementList(Square square){
            byte piece = getPiece(square);
            byte pieceType = getType(piece);
            List<Move> moves;
            switch (pieceType) {
                case 1:
                    moves = movementPawn(square, piece);
                    break;
                case 2:
                    moves = movementKnight(square, piece);
                    break;
                case 3:
                    moves = movementBishop(square, piece);
                    break;
                case 4:
                    moves = movementRook(square, piece);
                    break;
                case 5:
                    moves = movementQueen(square, piece);
                    break;
                case 6:
                    moves = movementKing(square, piece);
                    break;
                default:
                    moves = new List<Move>();
                    break;
            }
            return moves;
        }
        public List<Move> movementPawn(byte rank, byte file, byte piece)
        {
            List<Move> moves = new List<Move>();
            if (isBlank(getPiece(rank-1, file)))
            {   
                moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank-1), fileTo = file});
                if (rank == 6 && isBlank(getPiece(rank-2, file)))
                {
                    moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank-2), fileTo = file});
                }
            }
            if (rankInBounds(rank-1) && isEnemy(piece, getPiece(rank-1, file+1)))
            {
                moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank-1), fileTo = fileOverload(file+1)});
            }
            if (rankInBounds(rank-1) && isEnemy(piece, getPiece(rank-1, file-1)))
            {
                moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank-1), fileTo = fileOverload(file-1)});
            }
            return moves;
        }
        public List<Move> movementPawn(Square square, byte piece)
        {
            return movementPawn(square.rank, square.file, piece);
        }
        public List<Move> movementKnight(byte rank, byte file, byte piece)
        {
            List<Move> moves = new List<Move>();
            if (rankInBounds(rank+1) && isAttackable(piece, getPiece(rank+1, file+2)))
            {
                moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank+1), fileTo = fileOverload(file+2)});
            }
            if (rankInBounds(rank+1) && isAttackable(piece, getPiece(rank+1, file-2)))
            {
                moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank+1), fileTo = fileOverload(file-2)});
            }
            if (rankInBounds(rank-1) && isAttackable(piece, getPiece(rank-1, file+2)))
            {
                moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank-1), fileTo = fileOverload(file+2)});
            }
            if (rankInBounds(rank-1) && isAttackable(piece, getPiece(rank-1, file-2)))
            {
                moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank-1), fileTo = fileOverload(file-2)});
            }
            if (rankInBounds(rank+2) && isAttackable(piece, getPiece(rank+2, file+1)))
            {
                moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank+2), fileTo = fileOverload(file+1)});
            }
            if (rankInBounds(rank+2) && isAttackable(piece, getPiece(rank+2, file-1)))
            {
                moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank+2), fileTo = fileOverload(file-1)});
            }
            if (rankInBounds(rank-2) && isAttackable(piece, getPiece(rank-2, file+1)))
            {
                moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank-2), fileTo = fileOverload(file+1)});
            }
            if (rankInBounds(rank-2) && isAttackable(piece, getPiece(rank-2, file-1)))
            {
                moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank-2), fileTo = fileOverload(file-1)});
            }
            return moves;
        }
        public List<Move> movementKnight(Square square, byte piece)
        {
            return movementKnight(square.rank, square.file, piece);
        }
        public List<Move> movementBishop(byte rank, byte file, byte piece)
        {
            List<Move> moves = new List<Move>();

            byte i = 1;
            while (rankInBounds(rank+i) && isAttackable(piece, getPiece(rank+i, file+i)))
            {
                if (isPieceSafeValid(getPiece(rank+i, file+i)))
                {
                    if (isEnemy(piece, getPiece(rank+i, file+i)))
                    {
                        moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank+i), fileTo = fileOverload(file+i)});
                    }
                    break;
                }
                moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank+i), fileTo = fileOverload(file+i)});
                ++i;
            }

            i = 1;
            while (rankInBounds(rank-i) && isAttackable(piece, getPiece(rank-i, file-i)))
            {
                if (isPieceSafeValid(getPiece(rank-i, file-i)))
                {
                    if (isEnemy(piece, getPiece(rank-i, file-i)))
                    {
                        moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank-i), fileTo = fileOverload(file-i)});
                    }
                    break;
                }
                moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank-i), fileTo = fileOverload(file-i)});
                ++i;
            }

            i = 1;
            while (rankInBounds(rank+i) && isAttackable(piece, getPiece(rank+i, file-i)))
            {
                if (isPieceSafeValid(getPiece(rank+i, file-i)))
                {
                    if (isEnemy(piece, getPiece(rank+i, file-i)))
                    {
                        moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank+i), fileTo = fileOverload(file-i)});
                    }
                    break;
                }
                moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank+i), fileTo = fileOverload(file-i)});
                ++i;
            }

            i = 1;
            while (rankInBounds(rank-i) && isAttackable(piece, getPiece(rank-i, file+i)))
            {
                if (isPieceSafeValid(getPiece(rank-i, file+i)))
                {
                    if (isEnemy(piece, getPiece(rank-i, file+i)))
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
        public List<Move> movementBishop(Square square, byte piece)
        {
            return movementBishop(square.rank, square.file, piece);
        }
        public List<Move> movementRook(byte rank, byte file, byte piece)
        {
            List<Move> moves = new List<Move>();

            byte i = 1;
            while (rankInBounds(rank+i) && isAttackable(piece, getPiece(rank+i, file)))
            {
                if (isPieceSafeValid(getPiece(rank+i, file)))
                {
                    if (isEnemy(piece, getPiece(rank+i, file)))
                    {
                        moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank+i), fileTo = file});
                    }
                    break;
                }
                moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank+i), fileTo = file});
                ++i;
            }

            i = 1;
            while (rankInBounds(rank-i) && isAttackable(piece, getPiece(rank-i, file)))
            {
                if (isPieceSafeValid(getPiece(rank-i, file)))
                {
                    if (isEnemy(piece, getPiece(rank-i, file)))
                    {
                        moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank-i), fileTo = file});
                    }
                    break;
                }
                moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank-i), fileTo = file});
                ++i;
            }

            i = 1;
            while (rankInBounds(rank) && isAttackable(piece, getPiece(rank, file+i)))
            {
                if (isPieceSafeValid(getPiece(rank, file+i)))
                {
                    if (isEnemy(piece, getPiece(rank, file+i)))
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
            while (rankInBounds(rank) && isAttackable(piece, getPiece(rank, file-i)))
            {
                if (isPieceSafeValid(getPiece(rank, file-i)))
                {
                    if (isEnemy(piece, getPiece(rank, file-i)))
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
        public List<Move> movementRook(Square square, byte piece)
        {
            return movementRook(square.rank, square.file, piece);
        }
        public List<Move> movementQueen(byte rank, byte file, byte piece)
        {
            List<Move> moves = new List<Move>();
            moves.AddRange(movementBishop(rank, file, piece));
            moves.AddRange(movementRook(rank, file, piece));
            return moves;
        }
        public List<Move> movementQueen(Square square, byte piece)
        {
            return movementQueen(square.rank, square.file, piece);
        }
        public List<Move> movementKing(byte rank, byte file, byte piece)
        {
            List<Move> moves = new List<Move>();
            if (rankInBounds(rank+1) && isAttackable(piece, getPiece(rank+1, file)))
            {
                moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank+1), fileTo = file});
            }
            if (rankInBounds(rank-1) && isAttackable(piece, getPiece(rank-1, file)))
            {
                moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank-1), fileTo = file});
            }
            if (isAttackable(piece, getPiece(rank, file+1)))
            {
                moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = rank, fileTo = fileOverload(file+1)});
            }
            if (isAttackable(piece, getPiece(rank, file-1)))
            {
                moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = rank, fileTo = fileOverload(file-1)});
            }
            if (rankInBounds(rank+1) && isAttackable(piece, getPiece(rank+1, file+1)))
            {
                moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank+1), fileTo = fileOverload(file+1)});
            }
            if (rankInBounds(rank+1) && isAttackable(piece, getPiece(rank+1, file-1)))
            {
                moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank+1), fileTo = fileOverload(file-1)});
            }
            if (rankInBounds(rank-1) && isAttackable(piece, getPiece(rank-1, file+1)))
            {
                moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank-1), fileTo = fileOverload(file+1)});
            }
            if (rankInBounds(rank-1) && isAttackable(piece, getPiece(rank-1, file-1)))
            {
                moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = (byte)(rank-1), fileTo = fileOverload(file-1)});
            }

            // Castling
            if (isWhite(piece))
            {
                if (K)
                {
                    if (isBlank(getPiece(7, 5)) && isBlank(getPiece(7, 6)) && !isCheckAfterMove(rank, file, rank, 5) && !isCheckAfterMove(rank, file, rank, 6))
                    {
                        moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = 7, fileTo = 6});
                    }
                }
                if (Q)
                {
                    if (isBlank(getPiece(7, 1)) && isBlank(getPiece(7, 2)) && isBlank(getPiece(7, 3)) && !isCheckAfterMove(rank, file, rank, 3) && !isCheckAfterMove(rank, file, rank, 2))
                    {
                        moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = 7, fileTo = 2});
                    }
                }
            }
            else
            {
                if (k)
                {
                    if (isBlank(getPiece(7, 11)) && isBlank(getPiece(7, 12)) && !isCheckAfterMove(rank, file, rank, 11) && !isCheckAfterMove(rank, file, rank, 12))
                    {
                        moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = 7, fileTo = 11});
                    }
                }
                if (q)
                {
                    if (isBlank(getPiece(7, 14)) && isBlank(getPiece(7, 15)) && isBlank(getPiece(7, 16)) && !isCheckAfterMove(rank, file, rank, 16) && !isCheckAfterMove(rank, file, rank, 15))
                    {
                        moves.Add(new Move{rankFrom = rank, fileFrom = file, rankTo = 7, fileTo = 15});
                    }
                }
            }

            return moves;
        }
        public List<Move> movementKing(Square square, byte piece)
        {
            return movementKing(square.rank, square.file, piece);
        }
        public void pawnPromotion(Move move, byte promotePiece)
        {
            setPiece(move.rankTo, move.fileTo, promotePiece);
            setPiece(move.rankFrom, move.fileFrom, 0);
            if (gameStatus == 0){
                updateGameStatus();
            }
            nextTurn();
        }
        public void pawnPromotion(byte rankFrom, byte fileFrom, byte rankTo, byte fileTo, byte promotePiece)
        {
            pawnPromotion(new Move{rankFrom = rankFrom, fileFrom = fileFrom, rankTo = rankTo, fileTo = fileTo}, promotePiece);
        }
        public void pawnPromotion(Square squareFrom, Square squareTo, byte promotePiece)
        {
            pawnPromotion(squareFrom.rank, squareFrom.file, squareTo.rank, squareTo.file, promotePiece);
        }
        public bool isCheckAfterMove(Move move)
        {
            byte pieceFrom = getPiece(move.rankFrom, move.fileFrom);
            byte pieceTo = getPiece(move.rankTo, move.fileTo);

            setPiece(move.squareTo, pieceFrom);
            setPiece(move.squareFrom, 0);
            if (isKing(pieceFrom)){
                if (isWhite(pieceFrom)){
                    whiteKingPosition = move.squareTo;
                } else {
                    blackKingPosition = move.squareTo;
                }
            }
            whiteToMove = !whiteToMove;

            // softMovePiece(move);
            bool boolCheck = isCheck();

            setPiece(move.squareFrom, pieceFrom);
            setPiece(move.squareTo, pieceTo);
            if (isKing(pieceFrom)){
                if (isWhite(pieceFrom)){
                    whiteKingPosition = move.squareFrom;
                } else {
                    blackKingPosition = move.squareFrom;
                }
            }
            whiteToMove = !whiteToMove;

            // softMovePiece(move.rankTo, move.fileTo, move.rankFrom, move.fileFrom);
            return boolCheck;
        }
        public bool isCheckAfterMove(Square squareFrom, Square squareTo)
        {
            softMovePiece(squareFrom, squareTo);
            bool boolCheck = isCheck();
            softMovePiece(squareTo, squareFrom);
            return boolCheck;
        }
        public bool isCheckAfterMove(byte rankFrom, byte fileFrom, byte rankTo, byte fileTo)
        {
            softMovePiece(rankFrom, fileFrom, rankTo, fileTo);
            bool boolCheck = isCheck();
            softMovePiece(rankTo, fileTo, rankFrom, fileFrom);
            return boolCheck;
        }
        public bool isCheck()
        {
            Square kingPosition = !whiteToMove ? whiteKingPosition : blackKingPosition;
            byte rank = kingPosition.rank;
            byte file = kingPosition.file;
            byte piece = !whiteToMove ? (byte)BoardPiece.WhiteKing : (byte)BoardPiece.BlackKing;

            byte checkingPiece;
            bool isRankInBounds;
            
            // Pawn
            checkingPiece = getPiece(rank-1, file-1);
            isRankInBounds = rankInBounds(rank-1);
            if (isRankInBounds && isEnemyPawn(ref piece, ref checkingPiece)){
                return true;
            }
            checkingPiece = getPiece(rank-1, file+1);
            if (isRankInBounds && isEnemyPawn(ref piece, ref checkingPiece)){
                return true;
            }

            // Knight
            checkingPiece = getPiece(rank+1, file+2);
            isRankInBounds = rankInBounds(rank+1);
            if (isRankInBounds && isEnemyKnight(ref piece, ref checkingPiece))
            {
                return true;
            }
            checkingPiece = getPiece(rank+1, file-2);
            if (isRankInBounds && isEnemyKnight(ref piece, ref checkingPiece))
            {
                return true;
            }
            checkingPiece = getPiece(rank-1, file+2);
            isRankInBounds = rankInBounds(rank-1);
            if (isRankInBounds && isEnemyKnight(ref piece, ref checkingPiece))
            {
                return true;
            }
            checkingPiece = getPiece(rank-1, file-2);
            if (isRankInBounds && isEnemyKnight(ref piece, ref checkingPiece))
            {
                return true;
            }
            checkingPiece = getPiece(rank+2, file+1);
            isRankInBounds = rankInBounds(rank+2);
            if (isRankInBounds && isEnemyKnight(ref piece, ref checkingPiece))
            {
                return true;
            }
            checkingPiece = getPiece(rank+2, file-1);
            if (isRankInBounds && isEnemyKnight(ref piece, ref checkingPiece))
            {
                return true;
            }
            checkingPiece = getPiece(rank-2, file+1);
            isRankInBounds = rankInBounds(rank-2);
            if (isRankInBounds && isEnemyKnight(ref piece, ref checkingPiece))
            {
                return true;
            }
            checkingPiece = getPiece(rank-2, file-1);
            if (isRankInBounds && isEnemyKnight(ref piece, ref checkingPiece))
            {
                return true;
            }
            
            // Bishop
            byte i = 1;
            checkingPiece = getPiece(rank+i, file+i);
            while (rankInBounds(rank+i) && isAttackable(ref piece, ref checkingPiece))
            {
                if (isEnemy(ref piece, ref checkingPiece))
                {
                    if (isBishop(ref checkingPiece) || isQueen(ref checkingPiece))
                    {
                        return true;
                    } else {
                        break;
                    }
                }
                ++i;
                checkingPiece = getPiece(rank+i, file+i);
            }

            i = 1;
            checkingPiece = getPiece(rank-i, file-i);
            while (rankInBounds(rank-i) && isAttackable(ref piece, ref checkingPiece))
            {
                if (isEnemy(ref piece, ref checkingPiece))
                {
                    if (isBishop(ref checkingPiece) || isQueen(ref checkingPiece))
                    {
                        return true;
                    } else {
                        break;
                    }
                }
                ++i;
                checkingPiece = getPiece(rank-i, file-i);
            }

            i = 1;
            checkingPiece = getPiece(rank+i, file-i);
            while (rankInBounds(rank+i) && isAttackable(ref piece, ref checkingPiece))
            {
                if (isEnemy(ref piece, ref checkingPiece))
                {
                    if (isBishop(ref checkingPiece) || isQueen(ref checkingPiece))
                    {
                        return true;
                    } else {
                        break;
                    }
                }
                ++i;
                checkingPiece = getPiece(rank+i, file-i);
            }

            i = 1;
            checkingPiece = getPiece(rank-i, file+i);
            while (rankInBounds(rank-i) && isAttackable(ref piece, ref checkingPiece))
            {
                if (isEnemy(ref piece, ref checkingPiece))
                {
                    if (isBishop(ref checkingPiece) || isQueen(ref checkingPiece))
                    {
                        return true;
                    } else {
                        break;
                    }
                }
                ++i;
                checkingPiece = getPiece(rank-i, file+i);
            }

            // King
            checkingPiece = getPiece(rank+1, file);
            isRankInBounds = rankInBounds(rank+1);
            if (isRankInBounds && isEnemy(ref piece, ref checkingPiece))
            {
                if (isKing(ref checkingPiece))
                {
                    return true;
                }
            }
            checkingPiece = getPiece(rank+1, file+1);
            if (isRankInBounds && isEnemy(ref piece, ref checkingPiece))
            {
                if (isKing(ref checkingPiece))
                {
                    return true;
                }
            }
            checkingPiece = getPiece(rank+1, file-1);
            if (isRankInBounds && isEnemy(ref piece, ref checkingPiece))
            {
                if (isKing(ref checkingPiece))
                {
                    return true;
                }
            }
            checkingPiece = getPiece(rank-1, file);
            isRankInBounds = rankInBounds(rank-1);
            if (isRankInBounds && isEnemy(ref piece, ref checkingPiece))
            {
                if (isKing(ref checkingPiece))
                {
                    return true;
                }
            }
            checkingPiece = getPiece(rank-1, file+1);
            if (isRankInBounds && isEnemy(ref piece, ref checkingPiece))
            {
                if (isKing(ref checkingPiece))
                {
                    return true;
                }
            }
            checkingPiece = getPiece(rank-1, file-1);
            if (isRankInBounds && isEnemy(ref piece, ref checkingPiece))
            {
                if (isKing(ref checkingPiece))
                {
                    return true;
                }
            }
            checkingPiece = getPiece(rank, file+1);
            if (isEnemyKing(ref piece, ref checkingPiece))
            {
                return true;
            }
            checkingPiece = getPiece(rank, file-1);
            if (isEnemyKing(ref piece, ref checkingPiece))
            {
                return true;
            }

            // Rook
            i = 1;
            checkingPiece = getPiece(rank+i, file);
            while (rankInBounds(rank+i) && isAttackable(ref piece, ref checkingPiece))
            {
                if (isEnemy(ref piece, ref checkingPiece))
                {
                    if (isRook(ref checkingPiece) || isQueen(ref checkingPiece))
                    {
                        return true;
                    } else {
                        break;
                    }
                }
                ++i;
                checkingPiece = getPiece(rank+i, file);
            }

            i = 1;
            checkingPiece = getPiece(rank-i, file);
            while (rankInBounds(rank-i) && isAttackable(ref piece, ref checkingPiece))
            {
                if (isEnemy(ref piece, ref checkingPiece))
                {
                    if (isRook(ref checkingPiece) || isQueen(ref checkingPiece))
                    {
                        return true;
                    } else {
                        break;
                    }
                }
                ++i;
                checkingPiece = getPiece(rank-i, file);
            }

            i = 1;
            checkingPiece = getPiece(rank, file+i);
            while (isAttackable(ref piece, ref checkingPiece))
            {
                if (isEnemy(ref piece, ref checkingPiece))
                {
                    if (isRook(ref checkingPiece) || isQueen(ref checkingPiece))
                    {
                        return true;
                    } else {
                        break;
                    }
                }
                ++i;

                checkingPiece = getPiece(rank, file+i);

                if (i >= 20){
                    return false;
                }
            }

            byte j = (byte)(19-i);
            i = 1;
            checkingPiece = getPiece(rank, file-i);
            while (isAttackable(ref piece, ref checkingPiece))
            {
                if (isEnemy(ref piece, ref checkingPiece))
                {
                    if (isRook(ref checkingPiece) || isQueen(ref checkingPiece))
                    {
                        return true;
                    } else {
                        break;
                    }
                }
                ++i;

                checkingPiece = getPiece(rank, file+i);

                if (i == j){
                    return false;
                } else if (i >= 20){
                    return false;
                }
            }

            return false;
        }
        public bool isCheckMate()
        {
            if (!isCheck())
            {
                return false;
            }
            return isHaveNoMove();
        }
        public bool isStaleMate()
        {
            if (isCheck())
            {
                return false;
            }

            if (zobristHash.CheckForRepetition() || isDrawByFiftyMoveRule())
            {
                return true;
            }

            return isHaveNoMove();
        }
        public bool isHaveNoMove()
        {
            whiteToMove = !whiteToMove;
            bool boolCheck = getAllMoves().Count == 0;
            whiteToMove = !whiteToMove;
            return boolCheck;
        }
        public void updateGameStatus()
        {
            if (isCheckMate())
            {
                gameStatus = whiteToMove ? (byte)GameStatus.WhiteWin : (byte)GameStatus.BlackWin;
            }
            else if (isStaleMate())
            {
                gameStatus = (byte)GameStatus.Draw;
            } else {
                gameStatus = (byte)GameStatus.Ongoing;
            }
        }
        public bool isDrawByFiftyMoveRule()
        {
            return halfMove >= 100;
        }
        public List<Move> getAllMoves()
        {
            byte piece = whiteToMove ? (byte)BoardPiece.WhiteKing : (byte)BoardPiece.BlackKing;
            List<Move> moves = new List<Move>();
            for (byte rank = 0; rank < 8; ++rank)
            {
                for (byte file = 0; file < 20; ++file)
                {
                    if (isFriendly(getPiece(rank, file), piece))
                    {
                        Move[] move = getSoftMovement(rank, file);
                        foreach (Move eachMove in move){
                            if (!isCheckAfterMove(eachMove))
                            {
                                moves.Add(eachMove);
                            }
                        }
                    }
                }
            }
            return moves;
        }
        public List<Move> getAllSoftMoves(){
            byte piece = whiteToMove ? (byte)BoardPiece.WhiteKing : (byte)BoardPiece.BlackKing;
            List<Move> moves = new List<Move>();
            for (byte rank = 0; rank < 8; ++rank)
            {
                for (byte file = 0; file < 20; ++file)
                {
                    if (isFriendly(getPiece(rank, file), piece))
                    {
                        moves.AddRange(getSoftMovement(rank, file));
                    }
                }
            }
            return moves;
        }
        public List<Move> legalizeMoves(List<Move> moves){
            List<Move> legalMoves = new List<Move>();
            foreach (Move move in moves){
                if (!isCheckAfterMove(move)){
                    legalMoves.Add(move);
                }
            }
            return legalMoves;
        }
        // FEN Loader
        public void CircularChessLoader(string str){

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
            string[] FENArray = str.Split(' ');
            string[] strArray = FENArray[0].Split('/');
            byte i = 0;
            foreach (string strFile in strArray)
            {
                boardArray[i] = 0;
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
                            whiteKingPosition = new Square{rank = (byte)(8-j), file = i};
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
                            blackKingPosition = new Square{rank = (byte)(8-j), file = i};
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
                boardArray[i++] = file;
            }

            // Who to move
            whiteToMove = (FENArray[1] == "w") ? true : false;

            // Castling
            foreach (char c in FENArray[2])
            {
                switch (c)
                {
                    case 'K':
                        K = true;
                        break;
                    case 'Q':
                        Q = true;
                        break;
                    case 'k':
                        k = true;
                        break;
                    case 'q':
                        q = true;
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
            if (!isWhite(getPiece(7, 4)) | !isKing(getPiece(7, 4)))
            {
                KQ = false;
            }
            if (!isWhite(getPiece(7, 0)) | !isRook(getPiece(7, 0)))
            {
                K = false;
            }
            if (!isWhite(getPiece(7, 7)) | !isRook(getPiece(7, 7)))
            {
                Q = false;
            }

            if (!isBlack(getPiece(7, 13)) | !isKing(getPiece(7, 13)))
            {
                kq = false;
            }
            if (!isBlack(getPiece(7, 10)) | !isRook(getPiece(7, 10)))
            {
                k = false;
            }
            if (!isBlack(getPiece(7, 17)) | !isRook(getPiece(7, 17)))
            {
                q = false;
            }

            // Half move
            halfMove = byte.Parse(FENArray[3]);

            // Full move
            fullMove = ushort.Parse(FENArray[4]);

            // Update Game Status
            updateGameStatus();

            Debug.Log(isCheck());

            // Update Unique Hash
            uniqueHash = zobristHash.GenerateHash(boardArray);
        }
    }
    public static byte[][] deconstructMoves(Move[] moves){
            byte[][] deconstructedMoves = new byte[moves.Length][];
            for (int i = 0; i < moves.Length; ++i)
            {
                deconstructedMoves[i] = new byte[]{moves[i].rankTo, moves[i].fileTo};
            }
            return deconstructedMoves;
        }
    public static void CopyPosition(Position from, Position to){
        if (from == null || to == null || from == to){
            return;
        }
        Array.Copy(from.boardArray, to.boardArray, 20);
        to.whiteToMove = from.whiteToMove;
        to.castleAbility = from.castleAbility;
        to.halfMove = from.halfMove;
        to.fullMove = from.fullMove;
        to.gameStatus = from.gameStatus;
        to.isCastle = from.isCastle;
        to.whiteKingPosition = from.whiteKingPosition;
        to.blackKingPosition = from.blackKingPosition;
    }

    // Debug
    public static void DumpBoard(Position position){
        for (byte rank = 0; rank < 8; rank++)
        {
            string str = "";
            for (byte file = 0; file < 20; file++)
            {
                byte piece = position.getPiece(rank, file);
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
    public static void DumpPosition(Position position){
        DumpBoard(position);
        Debug.Log("White to move : " + position.whiteToMove);
        Debug.Log("White King Position : " + position.whiteKingPosition.rank + " " + position.whiteKingPosition.file);
        Debug.Log("Black King Position : " + position.blackKingPosition.rank + " " + position.blackKingPosition.file);
        Debug.Log("K : " + position.K + " Q : " + position.Q + " k : " + position.k + " q : " + position.q);
        Debug.Log("Half Move : " + position.halfMove);
        Debug.Log("Full Move : " + position.fullMove);
        Debug.Log("Game Status : " + position.gameStatus);
    }
    public static void DumpSquare(Square square){
        Debug.Log("Rank : " + square.rank + " File : " + square.file);
    }
    public static void DumpFile(uint file){
        string str = "";
        for (byte rank = 0; rank < 8; rank++)
        {
            byte piece = (byte)((file >> (4 * rank)) & 0xF);
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
    public static void DumpMove(Move move){
        Debug.Log("From : " + move.rankFrom + " " + move.fileFrom + " To : " + move.rankTo + " " + move.fileTo);
    }
}