using System;
using System.Net.NetworkInformation;

public class Board
{
    public const int BOARD_SIZE = 8;
    private Piece[,] pieces = new Piece[BOARD_SIZE, BOARD_SIZE];

    public Board()
    {
        InitializeBoard();
    }

    public void InitializeBoard()
    {
        // 흰색 기물 배치
        PlacePiece(new Rook(Piece.Color.White, 0, 0), 0, 0);
        PlacePiece(new Knight(Piece.Color.White, 0, 1), 0, 1);
        PlacePiece(new Bishop(Piece.Color.White, 0, 2), 0, 2);
        PlacePiece(new Queen(Piece.Color.White, 0, 3), 0, 3);
        PlacePiece(new King(Piece.Color.White, 0, 4), 0, 4);
        PlacePiece(new Bishop(Piece.Color.White, 0, 5), 0, 5);
        PlacePiece(new Knight(Piece.Color.White, 0, 6), 0, 6);
        PlacePiece(new Rook(Piece.Color.White, 0, 7), 0, 7);
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            PlacePiece(new Pawn(Piece.Color.White, 1, i), 1, i);
        }

        // 검은색 기물 배치
        PlacePiece(new Rook(Piece.Color.Black, 7, 0), 7, 0);
        PlacePiece(new Knight(Piece.Color.Black, 7, 1), 7, 1);
        PlacePiece(new Bishop(Piece.Color.Black, 7, 2), 7, 2);
        PlacePiece(new Queen(Piece.Color.Black, 7, 3), 7, 3);
        PlacePiece(new King(Piece.Color.Black, 7, 4), 7, 4);
        PlacePiece(new Bishop(Piece.Color.Black, 7, 5), 7, 5);
        PlacePiece(new Knight(Piece.Color.Black, 7, 6), 7, 6);
        PlacePiece(new Rook(Piece.Color.Black, 7, 7), 7, 7);
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            PlacePiece(new Pawn(Piece.Color.Black, 6, i), 6, i);
        }
    }

    public Piece GetPiece(int row, int col)
    {
        if (IsWithinBounds(row, col))
        {
            return pieces[row, col];
        }
        return null;
    }

    public bool IsWithinBounds(int row, int col)
    {
        return row >= 0 && row < BOARD_SIZE && col >= 0 && col < BOARD_SIZE;
    }

    public bool MovePiece(Piece piece, int endRow, int endCol)
    {
        if (piece == null || !IsWithinBounds(endRow, endCol))
        {
            return false;
        }

        List<Tuple<int, int>> possibleMoves = piece.GetPossibleMoves(this);
        foreach (var move in possibleMoves)
        {
            if (move.Item1 == endRow && move.Item2 == endCol)
            {
                pieces[piece.Row, piece.Col] = null;
                pieces[endRow, endCol] = piece;
                piece.MovePiece(endRow, endCol);
                return true;
            }
        }
        return false;
    }

    private void PlacePiece(Piece piece, int row, int col)
    {
        if (IsWithinBounds(row, col) && piece != null)
        {
            pieces[row, col] = piece;
            piece.SetPosition(row, col);
        }
    }

    public void PrintBoard()
    {
        Console.WriteLine("  a b c d e f g h");
        Console.WriteLine("  ---------------");
        for (int i = BOARD_SIZE - 1; i >= 0; i--)
        {
            Console.Write(i + 1 + "|");
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                Piece piece = pieces[i, j];
                if (piece != null)
                {
                    Console.Write(piece.GetSymbol() + " ");
                }
                else
                {
                    Console.Write(". ");
                }
            }
            Console.WriteLine("|" + (i + 1));
        }
        Console.WriteLine("  ---------------");
        Console.WriteLine("  a b c d e f g h");
    }
}