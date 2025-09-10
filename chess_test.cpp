#include <iostream>
#include <string>
#include <vector>

using namespace std;

class ChessPiece {
public:
    string color;  // 말의 색
    string type;   // 말의 종류 (King, Queen, Rook, Bishop, Knight, Pawn)

    ChessPiece(string c, string t) : color(c), type(t) {}

    virtual bool validMove(int startX, int startY, int endX, int endY) = 0;  // 각 말마다 유효한 이동인지 확인하는 함수
};

class King : public ChessPiece {
public:
    King(string color) : ChessPiece(color, "King") {}

    bool validMove(int startX, int startY, int endX, int endY) override {
        // 왕은 한 번에 한 칸만 이동 가능
        return abs(endX - startX) <= 1 && abs(endY - startY) <= 1;
    }
};

class Queen : public ChessPiece {
public:
    Queen(string color) : ChessPiece(color, "Queen") {}

    bool validMove(int startX, int startY, int endX, int endY) override {
        // 여왕은 직선 또는 대각선으로 이동 가능
        return (startX == endX || startY == endY || abs(endX - startX) == abs(endY - startY));
    }
};

class ChessBoard {
public:
    vector<vector<ChessPiece*>> board;

    ChessBoard() {
        // 체스판 초기화 (8x8)
        board.resize(8, vector<ChessPiece*>(8, nullptr));

        // 체스말 배치 (기본 위치)
        for (int i = 0; i < 8; ++i) {
            board[1][i] = new ChessPiece("White", "Pawn");
            board[6][i] = new ChessPiece("Black", "Pawn");
        }

        board[0][0] = new King("White");
        board[0][7] = new King("Black");
        // 나머지 말을 배치하는 부분 추가
    }

    void printBoard() {
        for (int i = 0; i < 8; ++i) {
            for (int j = 0; j < 8; ++j) {
                if (board[i][j] != nullptr)
                    cout << board[i][j]->type[0] << " ";
                else
                    cout << ". ";
            }
            cout << endl;
        }
    }

    bool movePiece(int startX, int startY, int endX, int endY) {
        if (board[startX][startY] == nullptr) {
            cout << "No piece at starting position!" << endl;
            return false;
        }

        ChessPiece* piece = board[startX][startY];
        if (piece->validMove(startX, startY, endX, endY)) {
            board[endX][endY] = piece;
            board[startX][startY] = nullptr;
            return true;
        } else {
            cout << "Invalid move!" << endl;
            return false;
        }
    }
};

int main() {
    ChessBoard chessBoard;

    chessBoard.printBoard();

    // 예시로 말을 이동시켜보기
    cout << "Move piece (0, 0) to (1, 0): ";
    if (chessBoard.movePiece(0, 0, 1, 0)) {
        cout << "Move successful!" << endl;
    } else {
        cout << "Move failed!" << endl;
    }

    chessBoard.printBoard();

    return 0;
}
