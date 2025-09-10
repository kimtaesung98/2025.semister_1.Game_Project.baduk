using System;
using System.Collections.Generic;

public abstract class Piece
{
    public enum Color { White, Black }
    public Color color { get; }
    public int Row { get; private set; }
    public int Col { get; private set; }

    protected Piece(Color color, int row, int col)
    {
        this.color = color;
        this.Row = row;
        this.Col = col;
    }

    public abstract List<Tuple<int, int>> GetPossibleMoves(Board board);
    public abstract string GetPieceType();
    public abstract string GetSymbol();

    public void SetPosition(int row, int col)
    {
        Row = row;
        Col = col;
    }

    public virtual void MovePiece(int newRow, int newCol)
    {
        SetPosition(newRow, newCol);
    }
}