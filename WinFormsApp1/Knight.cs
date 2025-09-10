using System;
using System.Collections.Generic;

public class Knight : Piece
{
    public Knight(Color color, int row, int col) : base(color, row, col) { }

    public override string GetPieceType() => "Knight";
    public override string GetSymbol() => (color == Color.White) ? "N" : "n";

    public override List<Tuple<int, int>> GetPossibleMoves(Board board)
    {
        List<Tuple<int, int>> possibleMoves = new List<Tuple<int, int>>();
        int[] dx = { -2, -2, -1, -1, 1, 1, 2, 2 };
        int[] dy = { -1, 1, -2, 2, -2, 2, -1, 1 };

        for (int i = 0; i < 8; i++)
        {
            int newRow = Row + dx[i];
            int newCol = Col + dy[i];

            if (board.IsWithinBounds(newRow, newCol))
            {
                Piece targetPiece = board.GetPiece(newRow, newCol);
                if (targetPiece == null || targetPiece.color != color)
                {
                    possibleMoves.Add(Tuple.Create(newRow, newCol));
                }
            }
        }

        return possibleMoves;
    }
}