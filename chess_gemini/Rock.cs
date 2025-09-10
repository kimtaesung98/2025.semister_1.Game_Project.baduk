using System;
using System.Collections.Generic;

public class Rook : Piece
{
    public Rook(Color color, int row, int col) : base(color, row, col) { }

    public override string GetPieceType() => "Rook";
    public override string GetSymbol() => (color == Color.White) ? "R" : "r";

    public override List<Tuple<int, int>> GetPossibleMoves(Board board)
    {
        List<Tuple<int, int>> possibleMoves = new List<Tuple<int, int>>();
        int[] dx = { 0, 0, 1, -1 };
        int[] dy = { 1, -1, 0, 0 };

        for (int i = 0; i < 4; i++)
        {
            for (int j = 1; ; j++)
            {
                int newRow = Row + dx[i] * j;
                int newCol = Col + dy[i] * j;

                if (!board.IsWithinBounds(newRow, newCol))
                {
                    break;
                }

                Piece targetPiece = board.GetPiece(newRow, newCol);
                if (targetPiece == null)
                {
                    possibleMoves.Add(Tuple.Create(newRow, newCol));
                }
                else
                {
                    if (targetPiece.color != color)
                    {
                        possibleMoves.Add(Tuple.Create(newRow, newCol));
                    }
                    break; // 다른 기물이 있으면 그 방향으로는 더 이상 이동 불가
                }
            }
        }
        return possibleMoves;
    }
}