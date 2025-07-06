using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // 씬 관리를 위해 추가
using TMPro; // TextMeshProUGUI를 위해 추가 (Legacy Text 사용 시 불필요)
using System.Linq; // FindEmptyGroupOwner에서 First() 사용을 위해 추가

public class BadukManager : MonoBehaviour
{
    // --- 인스펙터 설정 변수 ---
    [Header("Game Objects")]
    public GameObject blackStonePrefab;
    public GameObject whiteStonePrefab;
    public Transform boardTransform;

    [Header("Board Settings")]
    [Range(0f, 1f)] // 0과 1 사이의 값으로 설정 가능하도록 슬라이더 추가
    public float boardPadding = 0.05f; // 바둑판 테두리 여백 (0~1 사이의 비율)

    [Header("Audio")]
    public AudioSource sfxSource;
    public AudioClip blackStoneSound;
    public AudioClip whiteStoneSound;

    [Header("UI References")] // 기존 UI 참조
    // Canvas_InGameUI의 자식들 (게임 중 상시 노출)
    public TextMeshProUGUI blackCapturedCountText;
    public TextMeshProUGUI whiteCapturedCountText;
    public TextMeshProUGUI turnIndicatorText;

    public GameObject pausePanel;         // 일시정지 패널
    public GameObject scorePanel;         // 계가 결과 패널

    public TextMeshProUGUI blackScoreText;   // 흑 총 점수 표시
    public TextMeshProUGUI whiteScoreText;   // 백 총 점수 표시
    public TextMeshProUGUI resultText;       // 승패 결과 표시
    public GameObject gameModePanel; // 예: 게임 시작 시 모드 선택 패널 (사용하지 않는 경우 삭제해도 됨)


    // --- 게임 모드 및 상태 변수 ---
    private bool isComputerOpponent = true; // AI 모드인지 2인 모드인지
    private bool isGameStarted = false;     // 게임 시작 여부
    private bool isComputerThinking = false; // 컴퓨터가 생각 중인지
    private bool isBlackTurn = true;        // 흑돌 턴인지 (true: 흑, false: 백)
    private bool isWaitingForPlayerInput = true; // 사용자 입력 대기 중인지 여부 (수정)
    private bool isGameOver = false;        // 게임 종료 상태 추가

    // --- 바둑판 로직 변수 ---
    private const int BOARD_SIZE = 19;
    private int[,] boardState = new int[BOARD_SIZE, BOARD_SIZE]; // 0: 비어있음, 1: 흑돌, 2: 백돌
    private GameObject[,] boardObjects = new GameObject[BOARD_SIZE, BOARD_SIZE]; // 실제 보드 위에 있는 돌 오브젝트 참조
    private int[,] koProhibitedState = null; // 패(ko) 금지 상태 저장

    // --- 바둑판 좌표 계산 변수 ---
    private float boardWorldSize;       // 바둑판 전체 월드 크기 (스프라이트 기준)
    private float playableBoardSize;    // 돌을 놓을 수 있는 실제 바둑판 영역의 월드 크기
    private Vector3 boardOrigin;        // 첫 번째 그리드(0,0)의 월드 좌표
    private float gridSpacing;          // 한 칸의 월드 간격

    // --- 게임 카운터 변수 ---
    private int blackCapturedStones = 0; // 흑이 잡은 돌 개수
    private int whiteCapturedStones = 0; // 백이 잡은 돌 개수
    private int consecutivePasses = 0;   // 연속 패스 횟수

    [Header("Canvas References")] // 캔버스 참조 추가
    public GameObject canvasMainMenu;    // Canvas_MainMenu (Canvas 1)
    public GameObject canvasInGameUI;    // Canvas_InGameUI (Canvas 2)
    public GameObject canvasPauseMenu;   // Canvas_PauseMenu (Canvas 3)
    public GameObject canvasScorePanel;  // Canvas_ScorePanel (Canvas 4)

    // 게임 오브젝트가 활성화될 때 한번만 호출됩니다.
    void Awake()
    {
        InitializeBoardCoordinates();
        // 모든 캔버스의 초기 상태 설정
        // MainMenuManager가 MainMenu 씬에서 캔버스를 관리하므로, 여기서는 GameScene에 필요한 캔버스만 초기화
        if (canvasMainMenu != null) canvasMainMenu.SetActive(true); // GameScene에선 메인메뉴 비활성화
        if (canvasInGameUI != null) canvasInGameUI.SetActive(false);  // 인게임 UI 캔버스 활성화
        if (canvasPauseMenu != null) canvasPauseMenu.SetActive(false); // 일시정지 캔버스 비활성화
        if (canvasScorePanel != null) canvasScorePanel.SetActive(false); // 계가 캔버스 비활성화
    }

    // Start는 Awake 다음에 호출됩니다. 게임 초기화 로직을 여기에 두는 것이 좋습니다.
    void Start()
    {
        // 바둑판 상태 초기화 (모든 칸을 0으로)
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                boardState[i, j] = 0;
                if (boardObjects[i, j] != null)
                {
                    Destroy(boardObjects[i, j]);
                    boardObjects[i, j] = null;
                }
            }
        }
        blackCapturedStones = 0;
        whiteCapturedStones = 0;
        koProhibitedState = null;
        consecutivePasses = 0;

        // 이 부분은 MainMenuManager.cs 의 StartGame(bool isAI) 함수에 의해 호출됩니다.
        // 현재 BadukManager는 GameScene에만 존재하고, StartGame은 MainMenu에서 넘어올 때 호출됩니다.
        // 따라서 여기서는 UI 업데이트만 초기 상태로 설정합니다.
        UpdateCapturedCountUI();
        UpdateTurnIndicatorUI();

        // 씬 로드 시 isGameStarted는 false로 시작하지만, MainMenu에서 StartGame을 통해 true로 바뀝니다.
        // isWaitingForPlayerInput은 항상 초기 플레이어(흑)가 입력 대기 상태이므로 true로 시작
        isWaitingForPlayerInput = true;
        isGameOver = false; // 게임 시작 시 게임 오버 상태 해제
    }


    // 바둑판 좌표 및 크기 초기화 로직
    private void InitializeBoardCoordinates()
    {
        SpriteRenderer boardRenderer = boardTransform.GetComponent<SpriteRenderer>();
        if (boardRenderer != null)
        {
            boardWorldSize = boardRenderer.bounds.size.x;

            // 패딩을 적용한 실제 착수 가능한 바둑판 영역 크기 계산
            playableBoardSize = boardWorldSize * (1f - 2 * boardPadding);

            // 그리드 간격 계산 (19x19의 경우 18개의 간격이 존재)
            gridSpacing = playableBoardSize / (BOARD_SIZE - 1);

            // 바둑판 스프라이트의 좌측 하단 코너
            Vector3 bottomLeftCornerOfSprite = boardTransform.position - boardRenderer.bounds.extents;

            // 첫 번째 그리드(0,0)의 월드 좌표 계산 (패딩만큼 이동)
            boardOrigin = bottomLeftCornerOfSprite + new Vector3(boardWorldSize * boardPadding, boardWorldSize * boardPadding, 0);

            Debug.Log($"[BadukManager] Board World Size: {boardWorldSize}");
            Debug.Log($"[BadukManager] Playable Board Size: {playableBoardSize}");
            Debug.Log($"[BadukManager] Grid Spacing: {gridSpacing}");
            Debug.Log($"[BadukManager] Board Origin (0,0): {boardOrigin}");
        }
        else
        {
            Debug.LogError("[BadukManager] 바둑판 오브젝트에 SpriteRenderer가 없습니다! Board Transform에 SpriteRenderer가 있는지 확인하세요.");
        }
    }

    // MainMenuManager가 호출할 게임 시작 함수
    public void StartGame(bool isAI)
    {
        Debug.Log($"[BadukManager] 게임 시작! AI 모드: {isAI}");
        isGameStarted = true;
        isComputerOpponent = isAI;
        isGameOver = false;
        isBlackTurn = true; // 흑돌부터 시작
        isWaitingForPlayerInput = true; // 흑돌 차례이므로 플레이어 입력 대기
        consecutivePasses = 0;

        // 게임 시작 시 캔버스 활성화/비활성화 (Awake에서 이미 설정했지만 안전장치)
        if (canvasMainMenu != null) canvasMainMenu.SetActive(false); // 메인 메뉴 캔버스 비활성화
        if (canvasInGameUI != null) canvasInGameUI.SetActive(true);  // 인게임 UI 캔버스 활성화
        if (canvasPauseMenu != null) canvasPauseMenu.SetActive(false);
        if (canvasScorePanel != null) canvasScorePanel.SetActive(false);

        // 게임 보드 초기화 (Start에서 이미 했지만, 재시작 시 호출될 경우를 대비)
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                boardState[i, j] = 0;
                if (boardObjects[i, j] != null)
                {
                    Destroy(boardObjects[i, j]);
                    boardObjects[i, j] = null;
                }
            }
        }
        blackCapturedStones = 0;
        whiteCapturedStones = 0;
        koProhibitedState = null;

        UpdateCapturedCountUI();
        UpdateTurnIndicatorUI();
    }

    void Update()
    {
        // 게임 시작 전이거나 종료된 상태면 입력/AI 처리 방지
        // 일시정지 중이거나 계가 화면이 활성화되어 있어도 착수 방지
        if (!isGameStarted || isGameOver || (canvasPauseMenu != null && canvasPauseMenu.activeSelf) || (canvasScorePanel != null && canvasScorePanel.activeSelf))
        {
            return;
        }

        // 플레이어 턴인 경우 (AI 모드이든 2인 모드이든 현재 턴이 플레이어 턴이고 입력 대기 중일 때)
        // 흑돌 턴이거나, 2인 대전 모드이면서 백돌 턴일 때 (즉, AI의 턴이 아닐 때)
        if ((isBlackTurn || !isComputerOpponent) && isWaitingForPlayerInput)
        {
            HandlePlayerInput();
        }
        // 컴퓨터의 턴인 경우 (백돌 턴이고 AI 모드이며, 컴퓨터가 생각 중이 아닐 때)
        else if (!isBlackTurn && isComputerOpponent && !isComputerThinking)
        {
            StartCoroutine(ComputerMoveCoroutine());
        }


        // ESC 키 또는 'P' 키로 일시정지/재개
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }
    }

    // 플레이어의 입력을 처리하는 함수
    private void HandlePlayerInput()
    {
        // isWaitingForPlayerInput이 true일 때만 입력 처리
        if (isWaitingForPlayerInput && Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // 마우스 클릭 위치를 논리적 그리드 좌표로 변환
            int x = Mathf.RoundToInt((mousePos.x - boardOrigin.x) / gridSpacing);
            int y = Mathf.RoundToInt((mousePos.y - boardOrigin.y) / gridSpacing);

            // 유효한 바둑판 범위 내인지 확인
            if (x < 0 || x >= BOARD_SIZE || y < 0 || y >= BOARD_SIZE)
            {
                Debug.Log($"[BadukManager] 유효하지 않은 착수 위치: ({x}, {y}). 바둑판 밖 클릭은 패스로 처리하거나 무시할 수 있습니다.");
                return; // 바둑판 밖을 클릭하면 무시
            }

            // 이미 돌이 있는 곳인지 확인
            if (boardState[x, y] != 0)
            {
                Debug.Log("[BadukManager] 이미 돌이 있는 곳입니다.");
                return;
            }

            // 현재 턴에 맞는 돌 색깔(1:흑, 2:백)을 IsMoveLegal에 전달
            int playerColor = isBlackTurn ? 1 : 2;
            if (!IsMoveLegal(x, y, playerColor))
            {
                Debug.Log("[BadukManager] 둘 수 없는 자리입니다. (자충수 또는 패)");
                return;
            }

            // 유효한 착수가 확인되면, 다음 턴이 오기 전까지 입력을 막음
            isWaitingForPlayerInput = false;
            PlaceStone(x, y);
            consecutivePasses = 0; // 돌을 놓았으니 패스 카운트 초기화
        }
    }

    // 컴퓨터의 AI 로직
    IEnumerator ComputerMoveCoroutine()
    {
        isComputerThinking = true;
        Debug.Log("[BadukManager] 컴퓨터가 생각 중입니다...");
        yield return new WaitForSeconds(0.5f); // 0.5초 대기 (컴퓨터 생각 시간 시뮬레이션)

        List<Vector2Int> legalMoves = new List<Vector2Int>();
        int computerColor = 2; // 컴퓨터는 흰 돌

        // 둘 수 있는 모든 유효한 칸을 찾습니다.
        for (int x = 0; x < BOARD_SIZE; x++)
        {
            for (int y = 0; y < BOARD_SIZE; y++)
            {
                if (boardState[x, y] == 0 && IsMoveLegal(x, y, computerColor))
                {
                    legalMoves.Add(new Vector2Int(x, y));
                }
            }
        }

        if (legalMoves.Count > 0)
        {
            // 랜덤하게 유효한 위치에 돌을 놓습니다. (아주 기본적인 AI)
            int randomIndex = Random.Range(0, legalMoves.Count);
            Vector2Int move = legalMoves[randomIndex];
            Debug.Log($"[BadukManager] 컴퓨터의 선택: ({move.x}, {move.y})");
            PlaceStone(move.x, move.y);
            consecutivePasses = 0; // 돌을 놓았으니 패스 카운트 초기화
        }
        else
        {
            Debug.Log("[BadukManager] 컴퓨터가 둘 곳이 없습니다. (패스)");
            PassTurn(); // 컴퓨터가 패스
        }

        isComputerThinking = false;
        // 컴퓨터가 턴을 마쳤으니, 다음 턴이 플레이어 턴이면 입력 대기 상태로 변경
        // (PlaceStone에서 턴이 전환되므로, isBlackTurn 상태 확인 후 isWaitingForPlayerInput을 설정)
        // 이 부분은 PlaceStone 내부에서 처리되므로 여기서는 특별히 변경할 필요 없음
    }

    // (수정됨) 돌을 놓는 함수 - 중복 코드 제거 및 로직 정리
    void PlaceStone(int x, int y)
    {
        // 1. 패 규칙을 위해 현재 상태 저장 (이동하기 전의 보드 상태)
        int[,] boardStateBeforeMove = (int[,])boardState.Clone();

        // 2. 돌을 놓을 위치 계산 (boardOrigin이 이미 (0,0) 그리드 중심이므로 x*gridSpacing, y*gridSpacing만 더함)
        Vector3 stonePosition = boardOrigin + new Vector3(x * gridSpacing, y * gridSpacing, -1f);

        GameObject stoneToPlace;
        AudioClip soundToPlay;

        // 3. 흑/백돌에 따라 프리팹과 사운드 결정
        if (isBlackTurn)
        {
            stoneToPlace = Instantiate(blackStonePrefab, stonePosition, Quaternion.identity);
            boardState[x, y] = 1; // 흑돌
            soundToPlay = blackStoneSound;
        }
        else
        {
            stoneToPlace = Instantiate(whiteStonePrefab, stonePosition, Quaternion.identity);
            boardState[x, y] = 2; // 백돌
            soundToPlay = whiteStoneSound;
        }

        // 4. 사운드 재생
        if (sfxSource != null && soundToPlay != null)
        {
            sfxSource.PlayOneShot(soundToPlay);
        }

        // 5. 생성된 돌 정보 업데이트
        boardObjects[x, y] = stoneToPlace;
        stoneToPlace.transform.SetParent(boardTransform);

        // 6. 상대 돌 포획 검사
        int capturedCount = CheckForCaptures(x, y);

        // 잡은 돌 개수 업데이트
        if (capturedCount > 0)
        {
            if (isBlackTurn) // 흑이 놓아서 백 돌을 잡았다면
            {
                blackCapturedStones += capturedCount;
            }
            else // 백이 놓아서 흑 돌을 잡았다면
            {
                whiteCapturedStones += capturedCount;
            }
            UpdateCapturedCountUI(); // UI 업데이트
        }

        // 7. 패 상태 업데이트
        // 만약 잡은 돌이 1개이고, 돌을 잡기 전의 상대방 보드 상태가
        // 현재 보드 상태와 같다면 (즉, 되돌리기 패) 해당 위치를 패 금지 상태로 저장
        // 이 로직은 패 검사 함수(IsMoveLegal)에서 이미 수행되었고,
        // 여기서는 다음 턴에 패가 되는지 여부를 판단하기 위한 koProhibitedState를 설정합니다.
        // 현재 로직은 '단 하나의 돌만 잡았을 경우' koProhibitedState를 설정하는데,
        // 패의 정확한 정의는 '직전의 보드 상태와 동일한 보드 상태를 만드는 착수'입니다.
        // 따라서, PlaceStone 함수 내에서 koProhibitedState를 설정하는 방식은 다음 로직을 따라야 합니다.
        // IsMoveLegal에서 패 검사를 통과했다는 것은 현재 놓는 돌이 패가 아니라는 뜻.
        // 따라서, PlaceStone에서 koProhibitedState를 업데이트하는 로직은 다음과 같이 변경해야 합니다.
        // "현재 놓는 돌로 인해 상대방 돌이 포획되어 보드 상태가 바뀌었다면, 다음 턴의 패 검사를 위해 현재 보드 상태를 저장한다."

        // PlaceStone 함수에서 koProhibitedState 설정 로직 개선
        // 돌을 놓은 후 잡은 돌이 있다면, 현재 보드 상태를 koProhibitedState에 저장합니다.
        // 다음 턴에 IsMoveLegal에서 이 koProhibitedState와 일치하는 착수가 패로 간주됩니다.
        if (capturedCount > 0) // 돌을 잡았다면
        {
            koProhibitedState = (int[,])boardState.Clone();
        }
        else // 돌을 잡지 않았다면 패는 없음
        {
            koProhibitedState = null;
        }


        // 8. 턴 전환
        isBlackTurn = !isBlackTurn;
        UpdateTurnIndicatorUI(); // 턴 표시 UI 업데이트

        // 턴이 전환된 후, 다음 플레이어의 턴이면 입력 대기 상태로 변경 (수정)
        // 현재 턴이 흑돌 턴이거나 (플레이어 턴), 2인 대전 모드일 경우 (상대방도 플레이어)
        isWaitingForPlayerInput = (isBlackTurn || !isComputerOpponent);
    }

    // --- 패스 처리 함수 ---
    public void PassTurn()
    {
        if (isGameOver) return; // 게임 종료 상태에서는 패스할 수 없음

        Debug.Log($"[BadukManager] {(isBlackTurn ? "흑" : "백")}이 패스했습니다.");
        consecutivePasses++;

        if (consecutivePasses >= 2)
        {
            Debug.Log("[BadukManager] 양쪽 모두 패스했습니다. 게임 종료 및 계가를 시작합니다.");
            RequestScoreCalculation(); // 연속 2회 패스 시 계가 시작
        }
        else
        {
            isBlackTurn = !isBlackTurn; // 턴 넘기기
            UpdateTurnIndicatorUI(); // UI 업데이트

            // 패스 후 다음 턴이 플레이어 턴이면 입력 대기
            // 컴퓨터 턴이 아니거나, 흑돌 턴일 때 (플레이어 턴)
            isWaitingForPlayerInput = (isBlackTurn || !isComputerOpponent);

            // 다음 턴이 컴퓨터 턴이고 AI 모드면 AI 로직 시작
            if (!isBlackTurn && isComputerOpponent)
            {
                StartCoroutine(ComputerMoveCoroutine());
            }
        }
    }


    // --- 규칙 관련 함수들 (기존 코드 유지) ---

    private bool IsMoveLegal(int x, int y, int playerColor)
    {
        // 1. 이미 돌이 있는 곳은 둘 수 없음 (PlaceStone에서 이미 체크하지만 안전장치)
        if (boardState[x, y] != 0) return false;

        // 2. 가상의 보드에 돌을 놓아본다.
        int[,] tempBoard = (int[,])boardState.Clone();
        tempBoard[x, y] = playerColor;

        int opponentColor = (playerColor == 1) ? 2 : 1;
        bool capturesOpponent = false; // 상대 돌을 잡았는지 여부

        // 캡처될 돌들을 임시로 저장하여 패 검사에 활용 (새로 추가)
        List<Vector2Int> potentialCapturedStones = new List<Vector2Int>();

        int[] dx = { 0, 0, -1, 1 };
        int[] dy = { 1, -1, 0, 0 };

        // 3. 놓은 돌 주변의 상대 돌이 잡히는지 확인
        for (int i = 0; i < 4; i++)
        {
            int nx = x + dx[i];
            int ny = y + dy[i];

            if (nx >= 0 && nx < BOARD_SIZE && ny >= 0 && ny < BOARD_SIZE && tempBoard[nx, ny] == opponentColor)
            {
                var groupInfo = FindGroupAndCountLibertiesOnBoard(nx, ny, tempBoard);
                if (groupInfo.liberties == 0) // 상대 그룹의 활로가 0이면 잡힘
                {
                    capturesOpponent = true;
                    // 가상 보드에서 잡힌 돌 제거 및 potentialCapturedStones에 추가
                    foreach (var stone in groupInfo.stones)
                    {
                        tempBoard[stone.x, stone.y] = 0;
                        potentialCapturedStones.Add(stone); // 패 검사를 위해 추가
                    }
                }
            }
        }

        // 4. 자충수 검사 (내 돌을 놓아서 내 그룹의 활로가 0이 되는 경우)
        // 단, 상대 돌을 잡는 자충수는 허용 (축 따위)
        var myGroupInfo = FindGroupAndCountLibertiesOnBoard(x, y, tempBoard);
        if (!capturesOpponent && myGroupInfo.liberties == 0)
        {
            Debug.Log($"[BadukManager] 자충수입니다. ({x},{y})");
            return false; // 상대 돌을 잡지 못하면서 내 활로가 0이 되면 자충수
        }

        // 5. 패(Ko) 규칙 검사 (수정)
        // 직전의 보드 상태와 동일한 보드 상태를 만드는 착수는 금지
        if (koProhibitedState != null && AreBoardsEqual(tempBoard, koProhibitedState))
        {
            Debug.Log($"[BadukManager] 패 규칙 위반입니다. ({x},{y})");
            return false; // 이전 패 상태와 현재 보드 상태가 같다면 둘 수 없음
        }

        return true; // 모든 규칙을 통과하면 둘 수 있음
    }

    // 돌 포획 확인 및 제거
    int CheckForCaptures(int x, int y)
    {
        int capturedStonesCount = 0;
        int placedStoneColor = boardState[x, y];
        int opponentColor = (placedStoneColor == 1) ? 2 : 1;
        int[] dx = { 0, 0, -1, 1 };
        int[] dy = { 1, -1, 0, 0 };

        for (int i = 0; i < 4; i++)
        {
            int nx = x + dx[i];
            int ny = y + dy[i];

            if (nx >= 0 && nx < BOARD_SIZE && ny >= 0 && ny < BOARD_SIZE && boardState[nx, ny] == opponentColor)
            {
                var groupInfo = FindGroupAndCountLiberties(nx, ny);
                if (groupInfo.liberties == 0) // 활로가 없으면 잡힘
                {
                    capturedStonesCount += RemoveStones(groupInfo.stones);
                }
            }
        }
        // 돌을 놓은 본인 그룹이 자충수로 잡히는 경우 (IsMoveLegal에서 막지만 만약의 경우 대비)
        // 이 부분은 IsMoveLegal에서 이미 처리하므로 여기서는 제거하지 않는 것이 더 안전함.
        // PlaceStone에서는 오직 상대방 돌이 잡히는 경우만 처리하는 것이 일반적.
        return capturedStonesCount;
    }

    // 돌 제거 (실제 보드에서)
    int RemoveStones(List<Vector2Int> stonesToRemove)
    {
        foreach (var pos in stonesToRemove)
        {
            if (boardObjects[pos.x, pos.y] != null)
            {
                Destroy(boardObjects[pos.x, pos.y]);
            }
            boardObjects[pos.x, pos.y] = null;
            boardState[pos.x, pos.y] = 0; // 보드 상태도 0으로 (빈 칸)
        }
        return stonesToRemove.Count;
    }

    // 특정 돌 그룹의 활로 계산 (현재 보드 상태 기준)
    (List<Vector2Int> stones, int liberties) FindGroupAndCountLiberties(int startX, int startY)
    {
        return FindGroupAndCountLibertiesOnBoard(startX, startY, this.boardState);
    }

    // 특정 보드 상태에서 돌 그룹의 활로 계산 (가상 보드 검사 시 사용)
    (List<Vector2Int> stones, int liberties) FindGroupAndCountLibertiesOnBoard(int startX, int startY, int[,] board)
    {
        if (startX < 0 || startX >= BOARD_SIZE || startY < 0 || startY >= BOARD_SIZE)
            return (new List<Vector2Int>(), 0); // 유효하지 않은 좌표 처리

        if (board[startX, startY] == 0) return (new List<Vector2Int>(), 0); // 빈 칸이면 그룹 없음

        var stoneColor = board[startX, startY];
        var stonesInGroup = new List<Vector2Int>();
        var liberties = new HashSet<Vector2Int>(); // 활로는 중복 없이 세기 위해 HashSet 사용
        var q = new Queue<Vector2Int>();
        var visited = new bool[BOARD_SIZE, BOARD_SIZE]; // BFS 방문 체크

        q.Enqueue(new Vector2Int(startX, startY));
        visited[startX, startY] = true;

        while (q.Count > 0)
        {
            var pos = q.Dequeue();
            stonesInGroup.Add(pos);

            int[] dx = { 0, 0, -1, 1 };
            int[] dy = { 1, -1, 0, 0 };

            for (int i = 0; i < 4; i++)
            {
                int nx = pos.x + dx[i];
                int ny = pos.y + dy[i];

                if (nx >= 0 && nx < BOARD_SIZE && ny >= 0 && ny < BOARD_SIZE)
                {
                    if (board[nx, ny] == 0) // 인접한 빈 칸은 활로
                    {
                        liberties.Add(new Vector2Int(nx, ny));
                    }
                    else if (board[nx, ny] == stoneColor && !visited[nx, ny]) // 같은 색 돌이면 그룹에 추가 탐색
                    {
                        visited[nx, ny] = true;
                        q.Enqueue(new Vector2Int(nx, ny));
                    }
                }
            }
        }
        return (stonesInGroup, liberties.Count);
    }

    // 두 바둑판 상태가 같은지 비교 (패 검사에 사용)
    private bool AreBoardsEqual(int[,] boardA, int[,] boardB)
    {
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                if (boardA[i, j] != boardB[i, j]) return false;
            }
        }
        return true;
    }

    // --- UI 업데이트 함수 ---
    void UpdateCapturedCountUI()
    {
        if (blackCapturedCountText != null)
        {
            blackCapturedCountText.text = $"Black : {blackCapturedStones}";
        }
        if (whiteCapturedCountText != null)
        {
            whiteCapturedCountText.text = $"White : {whiteCapturedStones}";
        }
    }

    void UpdateTurnIndicatorUI()
    {
        if (turnIndicatorText != null)
        {
            turnIndicatorText.text = isBlackTurn ? "Black" : "White";
            // 텍스트 색상도 턴에 맞춰 변경할 수 있습니다.
            // turnIndicatorText.color = isBlackTurn ? Color.black : Color.white;
        }
    }

    // --- 일시정지 관련 함수 ---
    public void TogglePause()
    {
        if (isGameOver) return; // 게임 종료 상태에서는 일시정지 불가

        bool isCurrentlyPaused = false;
        if (canvasPauseMenu != null)
        {
            isCurrentlyPaused = canvasPauseMenu.activeSelf;
            canvasPauseMenu.SetActive(!isCurrentlyPaused); // 일시정지 캔버스 활성화/비활성화 토글
        }

        if (!isCurrentlyPaused) // 일시정지 상태로 전환
        {
            Time.timeScale = 0f; // 게임 시간 정지 (착수 방지)
            Debug.Log("[BadukManager] 게임 일시정지");
        }
        else // 게임 재개 상태로 전환
        {
            Time.timeScale = 1f; // 게임 시간 재개
            Debug.Log("[BadukManager] 게임 재개");
        }
    }

    public void ResumeGame()
    {
        TogglePause(); // 일시정지 해제
    }

    // --- 게임 종료 및 계가 관련 함수 ---
    // 계가를 시작하는 함수 (버튼 연결 또는 연속 패스 시 자동 호출)
    public void RequestScoreCalculation()
    {
        if (isGameOver) return;

        Debug.Log("[BadukManager] 계가 신청!");
        isGameOver = true;
        Time.timeScale = 0f; // 게임 시간 정지 (UI 상호작용은 가능)

        // 일시정지 캔버스가 열려있다면 닫기
        if (canvasPauseMenu != null)
        {
            canvasPauseMenu.SetActive(false);
        }

        // 계가 캔버스 활성화
        if (canvasScorePanel != null)
        {
            canvasScorePanel.SetActive(true);
            CalculateAndDisplayScore(); // 계가 결과를 계산하고 표시
        }
        else
        {
            Debug.LogError("[BadukManager] Score Panel Canvas가 할당되지 않았습니다!");
        }
    }

    // 계가 로직 구현
    void CalculateAndDisplayScore()
    {
        Debug.Log("[BadukManager] 계가 진행 중...");
        int blackTerritory = 0;
        int whiteTerritory = 0;

        // 방문 여부 확인을 위한 배열 (집 계산용)
        bool[,] visited = new bool[BOARD_SIZE, BOARD_SIZE];

        // 모든 칸을 순회하며 집 계산
        for (int x = 0; x < BOARD_SIZE; x++)
        {
            for (int y = 0; y < BOARD_SIZE; y++)
            {
                if (boardState[x, y] == 0 && !visited[x, y]) // 빈 칸이고 아직 방문하지 않았다면
                {
                    // BFS를 통해 해당 빈 칸 그룹의 소유자를 찾음
                    (List<Vector2Int> emptyCells, int ownerColor) groupInfo = FindEmptyGroupOwner(x, y, visited);

                    if (groupInfo.ownerColor == 1) // 흑의 집
                    {
                        blackTerritory += groupInfo.emptyCells.Count;
                        Debug.Log($"[BadukManager] 흑집 발견: {groupInfo.emptyCells.Count}칸");
                    }
                    else if (groupInfo.ownerColor == 2) // 백의 집
                    {
                        whiteTerritory += groupInfo.emptyCells.Count;
                        Debug.Log($"[BadukManager] 백집 발견: {groupInfo.emptyCells.Count}칸");
                    }
                    // ownerColor가 0이면, 빈 그룹이 누구의 집도 아니라는 뜻 (공동 영역 또는 사석)
                }
            }
        }

        // 총점 계산 (집 + 잡은 돌)
        // 일반적으로 한국 바둑은 백에게 덤(6.5집)을 줍니다.
        float komi = 6.5f; // 덤 설정

        float finalBlackScore = blackTerritory + blackCapturedStones;
        float finalWhiteScore = whiteTerritory + whiteCapturedStones + komi;

        Debug.Log($"[BadukManager] 흑 총 점수: {finalBlackScore} (집: {blackTerritory}, 잡은 돌: {blackCapturedStones})");
        Debug.Log($"[BadukManager] 백 총 점수: {finalWhiteScore} (집: {whiteTerritory}, 잡은 돌: {whiteCapturedStones}, 덤: {komi})");

        // 결과 UI 업데이트
        if (blackScoreText != null) blackScoreText.text = $"Black Point : {finalBlackScore}";
        if (whiteScoreText != null) whiteScoreText.text = $"White Point: {finalWhiteScore}";

        if (resultText != null)
        {
            if (finalBlackScore > finalWhiteScore)
            {
                resultText.text = $"Winner Black! ({finalBlackScore} : {finalWhiteScore})";
                resultText.color = Color.black; // 흑 승리 시 글자색 변경 (선택 사항)
            }
            else if (finalWhiteScore > finalBlackScore)
            {
                resultText.text = $"Winner White! ({finalWhiteScore} : {finalBlackScore})";
                resultText.color = Color.white; // 백 승리 시 글자색 변경 (선택 사항)
            }
            else
            {
                resultText.text = $"Null! ({finalBlackScore} : {finalWhiteScore})";
                resultText.color = Color.gray; // 무승부 시 글자색 변경 (선택 사항)
            }
        }
    }

    // 빈 칸 그룹의 소유자를 찾는 헬퍼 함수 (BFS)
    // 반환값: (빈 칸들의 목록, 소유자 색깔: 1=흑, 2=백, 0=공동/중립)
    (List<Vector2Int> emptyCells, int ownerColor) FindEmptyGroupOwner(int startX, int startY, bool[,] visited)
    {
        List<Vector2Int> emptyCellsInGroup = new List<Vector2Int>();
        HashSet<int> surroundingColors = new HashSet<int>(); // 이 빈 그룹을 둘러싸고 있는 돌들의 색깔
        Queue<Vector2Int> q = new Queue<Vector2Int>();

        q.Enqueue(new Vector2Int(startX, startY));
        visited[startX, startY] = true; // 시작점 방문 처리

        int[] dx = { 0, 0, -1, 1 };
        int[] dy = { 1, -1, 0, 0 };

        while (q.Count > 0)
        {
            Vector2Int pos = q.Dequeue();
            emptyCellsInGroup.Add(pos);

            for (int i = 0; i < 4; i++)
            {
                int nx = pos.x + dx[i];
                int ny = pos.y + dy[i];

                if (nx >= 0 && nx < BOARD_SIZE && ny >= 0 && ny < BOARD_SIZE)
                {
                    if (boardState[nx, ny] == 0 && !visited[nx, ny]) // 인접한 빈 칸이면 계속 탐색
                    {
                        visited[nx, ny] = true;
                        q.Enqueue(new Vector2Int(nx, ny));
                    }
                    else if (boardState[nx, ny] != 0) // 인접한 돌이면 그 돌의 색깔 기록
                    {
                        surroundingColors.Add(boardState[nx, ny]);
                    }
                }
            }
        }

        if (surroundingColors.Count == 1) // 한 가지 색깔의 돌로만 둘러싸여 있다면 그 색깔이 주인
        {
            return (emptyCellsInGroup, surroundingColors.First());
        }
        else // 여러 색깔의 돌 또는 아무 돌도 둘러싸고 있지 않다면 중립 (공배 또는 아직 주인 없음)
        {
            return (emptyCellsInGroup, 0);
        }
    }

    // 게임 재시작 함수
    public void RestartGame()
    {
        Time.timeScale = 1f; // 시간 스케일 복구
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // 현재 씬 재로드
        Debug.Log("[BadukManager] 게임 재시작");
    }

    // 메인 메뉴로 이동 함수
    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // 게임 시간을 원래대로 되돌림 (필수!)

        // 현재 씬의 UI 캔버스들은 비활성화 (새 씬 로드 시 자동으로 사라지지만, 명시적으로 제어)
        if (canvasInGameUI != null) canvasInGameUI.SetActive(false);
        if (canvasPauseMenu != null) canvasPauseMenu.SetActive(false);
        if (canvasScorePanel != null) canvasScorePanel.SetActive(false);

        SceneManager.LoadScene("StartMenu"); // "MainMenu" 씬으로 이동
        Debug.Log("[BadukManager] 메인 메뉴로 이동");
    }

    // --- 유니티 에디터 기즈모 그리기 (디버깅용) ---
    private void OnDrawGizmosSelected()
    {
        if (boardTransform == null) return;

        var boardRenderer = boardTransform.GetComponent<SpriteRenderer>();
        if (boardRenderer == null) return;

        // 실제 계산에 사용되는 변수들을 기즈모에서도 사용
        float currentBoardWorldSize = boardRenderer.bounds.size.x;
        float currentPlayableBoardSize = currentBoardWorldSize * (1f - 2 * boardPadding);
        float currentGridSpacing = currentPlayableBoardSize / (BOARD_SIZE - 1);
        Vector3 currentBottomLeftCornerOfSprite = boardTransform.position - boardRenderer.bounds.extents;
        Vector3 currentBoardOrigin = currentBottomLeftCornerOfSprite + new Vector3(currentBoardWorldSize * boardPadding, currentBoardWorldSize * boardPadding, 0);

        Gizmos.color = Color.red;

        for (int x = 0; x < BOARD_SIZE; x++)
        {
            for (int y = 0; y < BOARD_SIZE; y++)
            {
                Vector3 worldPos = currentBoardOrigin + new Vector3(x * currentGridSpacing, y * currentGridSpacing, 0);
                Gizmos.DrawWireSphere(worldPos, 0.05f); // 각 그리드 지점에 작은 원 그리기
            }
        }
    }
    public void ExitGame()
    {
        Time.timeScale = 1f; // 혹시 모르니 시간 스케일 복구
        Debug.Log("[BadukManager] 게임 종료!");

        // 빌드된 게임에서만 애플리케이션 종료
#if UNITY_STANDALONE || UNITY_WEBGL || UNITY_IOS || UNITY_ANDROID // 다른 플랫폼도 추가 가능
        Application.Quit();
#endif

        // Unity 에디터에서 플레이 모드 종료
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}