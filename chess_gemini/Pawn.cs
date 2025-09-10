using System;
using System.Collections.Generic;

public class Pawn : Piece
{
    private bool hasMoved = false;

    public Pawn(Color color, int row, int col) : base(color, row, col) { }

    public override string GetPieceType() => "Pawn";
    public override string GetSymbol() => (color == Color.White) ? "P" : "p";

    public override List<Tuple<int, int>> GetPossibleMoves(Board board)
    {
        List<Tuple<int, int>> possibleMoves = new List<Tuple<int, int>>();
        int direction = (color == Color.White) ? 1 : -1;
        int startRow = (color == Color.White) ? 1 : 6;

        int forwardRow = Row + direction;
        if (board.IsWithinBounds(forwardRow, Col) && board.GetPiece(forwardRow, Col) == null)
        {
            possibleMoves.Add(Tuple.Create(forwardRow, Col));
            if (!hasMoved && board.IsWithinBounds(Row + 2 * direction, Col) && board.GetPiece(Row + 2 * direction, Col) == null)
            {
                possibleMoves.Add(Tuple.Create(Row + 2 * direction, Col));
            }
        }

        int attackLeftCol = Col - 1;
        int attackRightCol = Col + 1;
        if (board.IsWithinBounds(forwardRow, attackLeftCol) && board.GetPiece(forwardRow, attackLeftCol) != null 
            && board.GetPiece(forwardRow, attackLeftCol).color != color)
        {
            possibleMoves.Add(Tuple.Create(forwardRow, attackLeftCol));
        }
        if (board.IsWithinBounds(forwardRow, attackRightCol) && board.GetPiece(forwardRow, attackRightCol) != null 
            && board.GetPiece(forwardRow, attackRightCol).color != color)
        {
            possibleMoves.Add(Tuple.Create(forwardRow, attackRightCol));
        }

        return possibleMoves;
    }

    public override void MovePiece(int newRow, int newCol)
    {
        base.MovePiece(newRow, newCol);
        hasMoved = true;
    }
}