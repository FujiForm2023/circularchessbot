# CircleFish

This is a fanmade circular chess bot of [GreenLemonGames](https://www.youtube.com/@GreenLemonGames).

`Version : Beta.-1`

Bugs :
- All of them

Updates:
- None

To Fix :
- BoardBot.cs, better functions

---
# For Developers

|Scripts|Detail|
|-------|------|
|BoardVisual.cs| As the name suggests, it's a board visual, no model is needed, it's all meshes|
|Tile.cs | Tile in Board Mesh, nothing to do with it (Except when you change something in BoardBot.cs)|
|DetailButton.cs | You can implement it if you want|
|ModeButton.cs | Make the board switch between circle and rectangular forms|
|ZobristHashing.cs | Use for 3 reps draw|
|TestBot.cs | You can use it if you want|
|BoardBot.cs | This is where you write codes about the board (Need huge fix supports)|
|mainBot.cs | This is a bot, where you write simple instructions that can be extended for other bots |

## BoardBot.cs
|Variables/Functions|Detail|
|-------------------|------|
|[Detail]|[C] For class, [S] for struct, [F] for function, [V] for variable, [E] for enum|
|[V] currentPosition|Save currentPosition|
|[E] BoardPiece| This is how the piece represents|
|[E] GameStatus| This is how the game status represents|
|[S] Square| Contains `rank` and `file`|
|[S] Move| Contains `rankFrom`, `fileFrom`, `rankTo`, and `fileTo`, it can also give back `squareFrom` and `squareTo`|
|[F] isPiece and isPieceSafeValid| `isPiece` count `0b0111` and `0b1111` but isPieceSafeValid don't|
|[F] fileOverLoad| Because it's circular chess, `a` and `t` files are connected, so this function exists to handle such numbers. For example: -1, 25|
|[F] isAttackable| Count blank and enemy pieces|
|[C] Position| This is where we save information about the board|
|[F] Position.getMovement and Position.getSoftMovement| `getMovement` will check if the move is valid or not, `getSoftMovement` doesn't|
|[F] Position.pawnPromotion| Use this instead of `Position.movePiece` if it's pawn promotion|
|[F] Position.isCheckAfterMove| Verify if after the move, will the king get checked or not|
|[F] Position.isCheck| The most annoying yet simple function in the game because it is the most time-consuming function|
|[F] CopyPosition| Copy the position, very useful, all you need is a pool to collect the position|

|Debug|Detail|
|-----|------|
|[F] DumpBoard| Print out the `boardArray` into readable values|
|[F] DumpPosition| Print out all variables in the `Position` class|
|[F] DumpSquare| Print out `rank` and `file` of `Square`|
|[F] DumpFile| Print out a specific file of the `Position` (Use `Position.boardArray[file]`)|
|[F] DumpMove| Print out `rankFrom`, `fileFrom`, `rankTo`, and `fileTo` of `Move`|

Other functions' details are written in their names.
