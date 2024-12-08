using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

public class ZobristHashing
{
    // Board size is 20 for your case
    public const int BoardSize = 20;

    // Zobrist hash table for each board position
    private ulong[] zobristKeys = new ulong[BoardSize];

    // Hash representing the current board state
    public ulong currentHash;

    // History of all the board states to detect repetition
    private List<ulong> boardHistory = new List<ulong>();

    void Start()
    {
        // Initialize the Zobrist keys for the 20 positions on the board
        InitializeZobristKeys();
        currentHash = 0;
    }

    // Initialize the Zobrist keys (random values for each board position)
    void InitializeZobristKeys()
    {
        System.Random rand = new System.Random();

        // Generate random keys for each position on the board
        for (int i = 0; i < BoardSize; i++)
        {
            zobristKeys[i] = GenerateRandomUInt64(rand);
        }
    }

    // Helper function to generate a 64-bit random number
    ulong GenerateRandomUInt64(System.Random rand)
    {
        // Combine two 32-bit random integers into one 64-bit integer
        uint high = (uint)rand.Next();
        uint low = (uint)rand.Next();
        ulong result = ((ulong)high << 32) | low;
        return result;
    }

    // Set the current board state and calculate the hash for the given board
    public void SetBoardState(uint[] boardState)
    {
        currentHash = 0;

        // XOR each piece's key with the current board state
        for (int i = 0; i < BoardSize; i++)
        {
            if (boardState[i] != 0) // If there is a piece or state (non-zero)
            {
                currentHash ^= boardState[i] ^ zobristKeys[i]; // XOR the position's Zobrist key
            }
        }

        // Track the current hash for repetition detection
        boardHistory.Add(currentHash);
    }

    // Check if the current board state has been repeated 3 times
    public bool CheckForRepetition()
    {
        int count = 0;

        // Count how many times the current hash has appeared in the board history
        foreach (var hash in boardHistory)
        {
            if (hash == currentHash)
            {
                count++;
            }
        }

        // Return true if the position has been repeated 3 or more times
        return count >= 3;
    }

    public void ClearBoardState()
    {
        // Clear the board history and current hash
        boardHistory.Clear();
        currentHash = 0;
    }

    public ulong GetCurrentHash()
    {
        return currentHash;
    }

    public ulong PopLastHash()
    {
        // Remove the last hash from the history and return it
        int lastIndex = boardHistory.Count - 1;
        ulong lastHash = boardHistory[lastIndex];
        boardHistory.RemoveAt(lastIndex);
        currentHash = boardHistory[lastIndex  - 1];
        return lastHash;
    }

    public ulong GenerateHash(uint[] boardState)
    {
        ulong hash = 0;

        // XOR each piece's key with the current board state
        for (int i = 0; i < BoardSize; i++)
        {
            if (boardState[i] != 0) // If there is a piece or state (non-zero)
            {
                hash ^= zobristKeys[i]; // XOR the position's Zobrist key
            }
        }

        return hash;
    }

    public List<ulong> TryGenerateNextHash(uint[] boardState)
    {
        List<ulong> nextHashes = boardHistory.ToList();

        // XOR each piece's key with the current board state
        for (int i = 0; i < BoardSize; i++)
        {
            if (boardState[i] != 0) // If there is a piece or state (non-zero)
            {
                ulong nextHash = currentHash ^ zobristKeys[i]; // XOR the position's Zobrist key
                nextHashes.Add(nextHash);
            }
        }

        return nextHashes;
    }
    public List<ulong> GetBoardHistory()
    {
        return boardHistory;
    }

    // Debug
    public void dumpBoardHistory(){
        foreach (ulong hash in boardHistory){
            Debug.Log(hash);
        }
    }
}