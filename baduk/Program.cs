using System;

public class GoBoard
{
    private int size;
    private char[,] board;
    private char currentPlayer;

    public GoBoard(int size)
    {
        this.size = size;
        this.board = new char[size, size];
        InitializeBoard();
        this.currentPlayer = '⚫'; // 흑돌부터 시작
    }

    private void InitializeBoard()
    {
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                board[i, j] = '+'; // 빈 칸은 '+'로 표시
            }
        }
    }

    public void PrintBoard()
    {
        Console.Write("  ");
        for (int i = 0; i < size; i++)
        {
            Console.Write($"{i + 1} ");
        }
        Console.WriteLine();
        for (int i = 0; i < size; i++)
        {
            Console.Write($"{i + 1} ");
            for (int j = 0; j < size; j++)
            {
                Console.Write($"{board[i, j]} ");
            }
            Console.WriteLine();
        }
    }

    public bool PlaceStone(int row, int col)
    {
        // 바둑판 범위 확인
        if (row < 1 || row > size || col < 1 || col > size)
        {
            Console.WriteLine("잘못된 좌표입니다.");
            return false;
        }

        // 이미 돌이 놓여 있는지 확인
        if (board[row - 1, col - 1] != '+')
        {
            Console.WriteLine("이미 돌이 놓여진 자리입니다.");
            return false;
        }

        board[row - 1, col - 1] = currentPlayer;
        SwitchPlayer();
        return true;
    }

    private void SwitchPlayer()
    {
        currentPlayer = (currentPlayer == '⚫') ? '⚪' : '⚫';
    }

    public char GetCurrentPlayer()
    {
        return currentPlayer;
    }
}

public class GoGame
{
    public static void Main(string[] args)
    {
        Console.Write("바둑판 크기를 입력하세요: ");
        int size = int.Parse(Console.ReadLine());
        GoBoard gameBoard = new GoBoard(size);

        while (true)
        {
            gameBoard.PrintBoard();
            Console.WriteLine($"현재 차례: {gameBoard.GetCurrentPlayer()}");
            Console.Write("돌을 놓을 좌표를 입력하세요 (예: 2 3): ");
            string input = Console.ReadLine();
            string[] coords = input.Split(' ');
            if (coords.Length != 2 || !int.TryParse(coords[0], out int row) || !int.TryParse(coords[1], out int col))
            {
                Console.WriteLine("잘못된 입력 형식입니다.");
                continue;
            }

            if (!gameBoard.PlaceStone(row, col))
            {
                continue;
            }

            // 기본적인 돌 놓기만 구현했으므로, 게임 종료 조건은 아직 없습니다.
            // 필요에 따라 종료 조건을 추가해야 합니다.
        }
    }
}