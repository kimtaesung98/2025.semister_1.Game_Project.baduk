using System;

public class GameManager
{
    private Board board;

    public GameManager()
    {
        board = new Board();
    }

    public void StartGame()
    {
        board.PrintBoard();

        while (true)
        {
            Console.WriteLine("\n명령어를 입력하세요 (예: a2 a4): ");
            string input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                continue;
            }

            string[] commands = input.Split(' ');
            if (commands.Length == 2)
            {
                string startPos = commands[0].ToLower();
                string endPos = commands[1].ToLower();

                if (startPos.Length == 2 && endPos.Length == 2 &&
                    startPos[0] >= 'a' && startPos[0] <= 'h' && startPos[1] >= '1' && startPos[1] <= '8' &&
                    endPos[0] >= 'a' && endPos[0] <= 'h' && endPos[1] >= '1' && endPos[1] <= '8')
                {
                    int startCol = startPos[0] - 'a';
                    int startRow = int.Parse(startPos[1].ToString()) - 1;
                    int endCol = endPos[0] - 'a';
                    int endRow = int.Parse(endPos[1].ToString()) - 1;

                    Piece pieceToMove = board.GetPiece(startRow, startCol);
                    if (pieceToMove != null)
                    {
                        if (board.MovePiece(pieceToMove, endRow, endCol))
                        {
                            board.PrintBoard();
                        }
                        else
                        {
                            Console.WriteLine("유효하지 않은 이동입니다.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("해당 위치에 기물이 없습니다.");
                    }
                }
                else
                {
                    Console.WriteLine("잘못된 명령어 형식입니다.");
                }
            }
            else if (input.ToLower() == "exit")
            {
                break;
            }
            else
            {
                Console.WriteLine("잘못된 명령어 형식입니다.");
            }
        }

        Console.WriteLine("게임 종료.");
    }
}