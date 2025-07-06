using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // �� ������ ���� �߰�
using TMPro; // TextMeshProUGUI�� ���� �߰� (Legacy Text ��� �� ���ʿ�)
using System.Linq; // FindEmptyGroupOwner���� First() ����� ���� �߰�

public class BadukManager : MonoBehaviour
{
    // --- �ν����� ���� ���� ---
    [Header("Game Objects")]
    public GameObject blackStonePrefab;
    public GameObject whiteStonePrefab;
    public Transform boardTransform;

    [Header("Board Settings")]
    [Range(0f, 1f)] // 0�� 1 ������ ������ ���� �����ϵ��� �����̴� �߰�
    public float boardPadding = 0.05f; // �ٵ��� �׵θ� ���� (0~1 ������ ����)

    [Header("Audio")]
    public AudioSource sfxSource;
    public AudioClip blackStoneSound;
    public AudioClip whiteStoneSound;

    [Header("UI References")] // ���� UI ����
    // Canvas_InGameUI�� �ڽĵ� (���� �� ��� ����)
    public TextMeshProUGUI blackCapturedCountText;
    public TextMeshProUGUI whiteCapturedCountText;
    public TextMeshProUGUI turnIndicatorText;

    public GameObject pausePanel;         // �Ͻ����� �г�
    public GameObject scorePanel;         // �谡 ��� �г�

    public TextMeshProUGUI blackScoreText;   // �� �� ���� ǥ��
    public TextMeshProUGUI whiteScoreText;   // �� �� ���� ǥ��
    public TextMeshProUGUI resultText;       // ���� ��� ǥ��
    public GameObject gameModePanel; // ��: ���� ���� �� ��� ���� �г� (������� �ʴ� ��� �����ص� ��)


    // --- ���� ��� �� ���� ���� ---
    private bool isComputerOpponent = true; // AI ������� 2�� �������
    private bool isGameStarted = false;     // ���� ���� ����
    private bool isComputerThinking = false; // ��ǻ�Ͱ� ���� ������
    private bool isBlackTurn = true;        // �浹 ������ (true: ��, false: ��)
    private bool isWaitingForPlayerInput = true; // ����� �Է� ��� ������ ���� (����)
    private bool isGameOver = false;        // ���� ���� ���� �߰�

    // --- �ٵ��� ���� ���� ---
    private const int BOARD_SIZE = 19;
    private int[,] boardState = new int[BOARD_SIZE, BOARD_SIZE]; // 0: �������, 1: �浹, 2: �鵹
    private GameObject[,] boardObjects = new GameObject[BOARD_SIZE, BOARD_SIZE]; // ���� ���� ���� �ִ� �� ������Ʈ ����
    private int[,] koProhibitedState = null; // ��(ko) ���� ���� ����

    // --- �ٵ��� ��ǥ ��� ���� ---
    private float boardWorldSize;       // �ٵ��� ��ü ���� ũ�� (��������Ʈ ����)
    private float playableBoardSize;    // ���� ���� �� �ִ� ���� �ٵ��� ������ ���� ũ��
    private Vector3 boardOrigin;        // ù ��° �׸���(0,0)�� ���� ��ǥ
    private float gridSpacing;          // �� ĭ�� ���� ����

    // --- ���� ī���� ���� ---
    private int blackCapturedStones = 0; // ���� ���� �� ����
    private int whiteCapturedStones = 0; // ���� ���� �� ����
    private int consecutivePasses = 0;   // ���� �н� Ƚ��

    [Header("Canvas References")] // ĵ���� ���� �߰�
    public GameObject canvasMainMenu;    // Canvas_MainMenu (Canvas 1)
    public GameObject canvasInGameUI;    // Canvas_InGameUI (Canvas 2)
    public GameObject canvasPauseMenu;   // Canvas_PauseMenu (Canvas 3)
    public GameObject canvasScorePanel;  // Canvas_ScorePanel (Canvas 4)

    // ���� ������Ʈ�� Ȱ��ȭ�� �� �ѹ��� ȣ��˴ϴ�.
    void Awake()
    {
        InitializeBoardCoordinates();
        // ��� ĵ������ �ʱ� ���� ����
        // MainMenuManager�� MainMenu ������ ĵ������ �����ϹǷ�, ���⼭�� GameScene�� �ʿ��� ĵ������ �ʱ�ȭ
        if (canvasMainMenu != null) canvasMainMenu.SetActive(true); // GameScene���� ���θ޴� ��Ȱ��ȭ
        if (canvasInGameUI != null) canvasInGameUI.SetActive(false);  // �ΰ��� UI ĵ���� Ȱ��ȭ
        if (canvasPauseMenu != null) canvasPauseMenu.SetActive(false); // �Ͻ����� ĵ���� ��Ȱ��ȭ
        if (canvasScorePanel != null) canvasScorePanel.SetActive(false); // �谡 ĵ���� ��Ȱ��ȭ
    }

    // Start�� Awake ������ ȣ��˴ϴ�. ���� �ʱ�ȭ ������ ���⿡ �δ� ���� �����ϴ�.
    void Start()
    {
        // �ٵ��� ���� �ʱ�ȭ (��� ĭ�� 0����)
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

        // �� �κ��� MainMenuManager.cs �� StartGame(bool isAI) �Լ��� ���� ȣ��˴ϴ�.
        // ���� BadukManager�� GameScene���� �����ϰ�, StartGame�� MainMenu���� �Ѿ�� �� ȣ��˴ϴ�.
        // ���� ���⼭�� UI ������Ʈ�� �ʱ� ���·� �����մϴ�.
        UpdateCapturedCountUI();
        UpdateTurnIndicatorUI();

        // �� �ε� �� isGameStarted�� false�� ����������, MainMenu���� StartGame�� ���� true�� �ٲ�ϴ�.
        // isWaitingForPlayerInput�� �׻� �ʱ� �÷��̾�(��)�� �Է� ��� �����̹Ƿ� true�� ����
        isWaitingForPlayerInput = true;
        isGameOver = false; // ���� ���� �� ���� ���� ���� ����
    }


    // �ٵ��� ��ǥ �� ũ�� �ʱ�ȭ ����
    private void InitializeBoardCoordinates()
    {
        SpriteRenderer boardRenderer = boardTransform.GetComponent<SpriteRenderer>();
        if (boardRenderer != null)
        {
            boardWorldSize = boardRenderer.bounds.size.x;

            // �е��� ������ ���� ���� ������ �ٵ��� ���� ũ�� ���
            playableBoardSize = boardWorldSize * (1f - 2 * boardPadding);

            // �׸��� ���� ��� (19x19�� ��� 18���� ������ ����)
            gridSpacing = playableBoardSize / (BOARD_SIZE - 1);

            // �ٵ��� ��������Ʈ�� ���� �ϴ� �ڳ�
            Vector3 bottomLeftCornerOfSprite = boardTransform.position - boardRenderer.bounds.extents;

            // ù ��° �׸���(0,0)�� ���� ��ǥ ��� (�е���ŭ �̵�)
            boardOrigin = bottomLeftCornerOfSprite + new Vector3(boardWorldSize * boardPadding, boardWorldSize * boardPadding, 0);

            Debug.Log($"[BadukManager] Board World Size: {boardWorldSize}");
            Debug.Log($"[BadukManager] Playable Board Size: {playableBoardSize}");
            Debug.Log($"[BadukManager] Grid Spacing: {gridSpacing}");
            Debug.Log($"[BadukManager] Board Origin (0,0): {boardOrigin}");
        }
        else
        {
            Debug.LogError("[BadukManager] �ٵ��� ������Ʈ�� SpriteRenderer�� �����ϴ�! Board Transform�� SpriteRenderer�� �ִ��� Ȯ���ϼ���.");
        }
    }

    // MainMenuManager�� ȣ���� ���� ���� �Լ�
    public void StartGame(bool isAI)
    {
        Debug.Log($"[BadukManager] ���� ����! AI ���: {isAI}");
        isGameStarted = true;
        isComputerOpponent = isAI;
        isGameOver = false;
        isBlackTurn = true; // �浹���� ����
        isWaitingForPlayerInput = true; // �浹 �����̹Ƿ� �÷��̾� �Է� ���
        consecutivePasses = 0;

        // ���� ���� �� ĵ���� Ȱ��ȭ/��Ȱ��ȭ (Awake���� �̹� ���������� ������ġ)
        if (canvasMainMenu != null) canvasMainMenu.SetActive(false); // ���� �޴� ĵ���� ��Ȱ��ȭ
        if (canvasInGameUI != null) canvasInGameUI.SetActive(true);  // �ΰ��� UI ĵ���� Ȱ��ȭ
        if (canvasPauseMenu != null) canvasPauseMenu.SetActive(false);
        if (canvasScorePanel != null) canvasScorePanel.SetActive(false);

        // ���� ���� �ʱ�ȭ (Start���� �̹� ������, ����� �� ȣ��� ��츦 ���)
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
        // ���� ���� ���̰ų� ����� ���¸� �Է�/AI ó�� ����
        // �Ͻ����� ���̰ų� �谡 ȭ���� Ȱ��ȭ�Ǿ� �־ ���� ����
        if (!isGameStarted || isGameOver || (canvasPauseMenu != null && canvasPauseMenu.activeSelf) || (canvasScorePanel != null && canvasScorePanel.activeSelf))
        {
            return;
        }

        // �÷��̾� ���� ��� (AI ����̵� 2�� ����̵� ���� ���� �÷��̾� ���̰� �Է� ��� ���� ��)
        // �浹 ���̰ų�, 2�� ���� ����̸鼭 �鵹 ���� �� (��, AI�� ���� �ƴ� ��)
        if ((isBlackTurn || !isComputerOpponent) && isWaitingForPlayerInput)
        {
            HandlePlayerInput();
        }
        // ��ǻ���� ���� ��� (�鵹 ���̰� AI ����̸�, ��ǻ�Ͱ� ���� ���� �ƴ� ��)
        else if (!isBlackTurn && isComputerOpponent && !isComputerThinking)
        {
            StartCoroutine(ComputerMoveCoroutine());
        }


        // ESC Ű �Ǵ� 'P' Ű�� �Ͻ�����/�簳
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }
    }

    // �÷��̾��� �Է��� ó���ϴ� �Լ�
    private void HandlePlayerInput()
    {
        // isWaitingForPlayerInput�� true�� ���� �Է� ó��
        if (isWaitingForPlayerInput && Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // ���콺 Ŭ�� ��ġ�� ���� �׸��� ��ǥ�� ��ȯ
            int x = Mathf.RoundToInt((mousePos.x - boardOrigin.x) / gridSpacing);
            int y = Mathf.RoundToInt((mousePos.y - boardOrigin.y) / gridSpacing);

            // ��ȿ�� �ٵ��� ���� ������ Ȯ��
            if (x < 0 || x >= BOARD_SIZE || y < 0 || y >= BOARD_SIZE)
            {
                Debug.Log($"[BadukManager] ��ȿ���� ���� ���� ��ġ: ({x}, {y}). �ٵ��� �� Ŭ���� �н��� ó���ϰų� ������ �� �ֽ��ϴ�.");
                return; // �ٵ��� ���� Ŭ���ϸ� ����
            }

            // �̹� ���� �ִ� ������ Ȯ��
            if (boardState[x, y] != 0)
            {
                Debug.Log("[BadukManager] �̹� ���� �ִ� ���Դϴ�.");
                return;
            }

            // ���� �Ͽ� �´� �� ����(1:��, 2:��)�� IsMoveLegal�� ����
            int playerColor = isBlackTurn ? 1 : 2;
            if (!IsMoveLegal(x, y, playerColor))
            {
                Debug.Log("[BadukManager] �� �� ���� �ڸ��Դϴ�. (����� �Ǵ� ��)");
                return;
            }

            // ��ȿ�� ������ Ȯ�εǸ�, ���� ���� ���� ������ �Է��� ����
            isWaitingForPlayerInput = false;
            PlaceStone(x, y);
            consecutivePasses = 0; // ���� �������� �н� ī��Ʈ �ʱ�ȭ
        }
    }

    // ��ǻ���� AI ����
    IEnumerator ComputerMoveCoroutine()
    {
        isComputerThinking = true;
        Debug.Log("[BadukManager] ��ǻ�Ͱ� ���� ���Դϴ�...");
        yield return new WaitForSeconds(0.5f); // 0.5�� ��� (��ǻ�� ���� �ð� �ùķ��̼�)

        List<Vector2Int> legalMoves = new List<Vector2Int>();
        int computerColor = 2; // ��ǻ�ʹ� �� ��

        // �� �� �ִ� ��� ��ȿ�� ĭ�� ã���ϴ�.
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
            // �����ϰ� ��ȿ�� ��ġ�� ���� �����ϴ�. (���� �⺻���� AI)
            int randomIndex = Random.Range(0, legalMoves.Count);
            Vector2Int move = legalMoves[randomIndex];
            Debug.Log($"[BadukManager] ��ǻ���� ����: ({move.x}, {move.y})");
            PlaceStone(move.x, move.y);
            consecutivePasses = 0; // ���� �������� �н� ī��Ʈ �ʱ�ȭ
        }
        else
        {
            Debug.Log("[BadukManager] ��ǻ�Ͱ� �� ���� �����ϴ�. (�н�)");
            PassTurn(); // ��ǻ�Ͱ� �н�
        }

        isComputerThinking = false;
        // ��ǻ�Ͱ� ���� ��������, ���� ���� �÷��̾� ���̸� �Է� ��� ���·� ����
        // (PlaceStone���� ���� ��ȯ�ǹǷ�, isBlackTurn ���� Ȯ�� �� isWaitingForPlayerInput�� ����)
        // �� �κ��� PlaceStone ���ο��� ó���ǹǷ� ���⼭�� Ư���� ������ �ʿ� ����
    }

    // (������) ���� ���� �Լ� - �ߺ� �ڵ� ���� �� ���� ����
    void PlaceStone(int x, int y)
    {
        // 1. �� ��Ģ�� ���� ���� ���� ���� (�̵��ϱ� ���� ���� ����)
        int[,] boardStateBeforeMove = (int[,])boardState.Clone();

        // 2. ���� ���� ��ġ ��� (boardOrigin�� �̹� (0,0) �׸��� �߽��̹Ƿ� x*gridSpacing, y*gridSpacing�� ����)
        Vector3 stonePosition = boardOrigin + new Vector3(x * gridSpacing, y * gridSpacing, -1f);

        GameObject stoneToPlace;
        AudioClip soundToPlay;

        // 3. ��/�鵹�� ���� �����հ� ���� ����
        if (isBlackTurn)
        {
            stoneToPlace = Instantiate(blackStonePrefab, stonePosition, Quaternion.identity);
            boardState[x, y] = 1; // �浹
            soundToPlay = blackStoneSound;
        }
        else
        {
            stoneToPlace = Instantiate(whiteStonePrefab, stonePosition, Quaternion.identity);
            boardState[x, y] = 2; // �鵹
            soundToPlay = whiteStoneSound;
        }

        // 4. ���� ���
        if (sfxSource != null && soundToPlay != null)
        {
            sfxSource.PlayOneShot(soundToPlay);
        }

        // 5. ������ �� ���� ������Ʈ
        boardObjects[x, y] = stoneToPlace;
        stoneToPlace.transform.SetParent(boardTransform);

        // 6. ��� �� ��ȹ �˻�
        int capturedCount = CheckForCaptures(x, y);

        // ���� �� ���� ������Ʈ
        if (capturedCount > 0)
        {
            if (isBlackTurn) // ���� ���Ƽ� �� ���� ��Ҵٸ�
            {
                blackCapturedStones += capturedCount;
            }
            else // ���� ���Ƽ� �� ���� ��Ҵٸ�
            {
                whiteCapturedStones += capturedCount;
            }
            UpdateCapturedCountUI(); // UI ������Ʈ
        }

        // 7. �� ���� ������Ʈ
        // ���� ���� ���� 1���̰�, ���� ��� ���� ���� ���� ���°�
        // ���� ���� ���¿� ���ٸ� (��, �ǵ����� ��) �ش� ��ġ�� �� ���� ���·� ����
        // �� ������ �� �˻� �Լ�(IsMoveLegal)���� �̹� ����Ǿ���,
        // ���⼭�� ���� �Ͽ� �а� �Ǵ��� ���θ� �Ǵ��ϱ� ���� koProhibitedState�� �����մϴ�.
        // ���� ������ '�� �ϳ��� ���� ����� ���' koProhibitedState�� �����ϴµ�,
        // ���� ��Ȯ�� ���Ǵ� '������ ���� ���¿� ������ ���� ���¸� ����� ����'�Դϴ�.
        // ����, PlaceStone �Լ� ������ koProhibitedState�� �����ϴ� ����� ���� ������ ����� �մϴ�.
        // IsMoveLegal���� �� �˻縦 ����ߴٴ� ���� ���� ���� ���� �а� �ƴ϶�� ��.
        // ����, PlaceStone���� koProhibitedState�� ������Ʈ�ϴ� ������ ������ ���� �����ؾ� �մϴ�.
        // "���� ���� ���� ���� ���� ���� ��ȹ�Ǿ� ���� ���°� �ٲ���ٸ�, ���� ���� �� �˻縦 ���� ���� ���� ���¸� �����Ѵ�."

        // PlaceStone �Լ����� koProhibitedState ���� ���� ����
        // ���� ���� �� ���� ���� �ִٸ�, ���� ���� ���¸� koProhibitedState�� �����մϴ�.
        // ���� �Ͽ� IsMoveLegal���� �� koProhibitedState�� ��ġ�ϴ� ������ �з� ���ֵ˴ϴ�.
        if (capturedCount > 0) // ���� ��Ҵٸ�
        {
            koProhibitedState = (int[,])boardState.Clone();
        }
        else // ���� ���� �ʾҴٸ� �д� ����
        {
            koProhibitedState = null;
        }


        // 8. �� ��ȯ
        isBlackTurn = !isBlackTurn;
        UpdateTurnIndicatorUI(); // �� ǥ�� UI ������Ʈ

        // ���� ��ȯ�� ��, ���� �÷��̾��� ���̸� �Է� ��� ���·� ���� (����)
        // ���� ���� �浹 ���̰ų� (�÷��̾� ��), 2�� ���� ����� ��� (���浵 �÷��̾�)
        isWaitingForPlayerInput = (isBlackTurn || !isComputerOpponent);
    }

    // --- �н� ó�� �Լ� ---
    public void PassTurn()
    {
        if (isGameOver) return; // ���� ���� ���¿����� �н��� �� ����

        Debug.Log($"[BadukManager] {(isBlackTurn ? "��" : "��")}�� �н��߽��ϴ�.");
        consecutivePasses++;

        if (consecutivePasses >= 2)
        {
            Debug.Log("[BadukManager] ���� ��� �н��߽��ϴ�. ���� ���� �� �谡�� �����մϴ�.");
            RequestScoreCalculation(); // ���� 2ȸ �н� �� �谡 ����
        }
        else
        {
            isBlackTurn = !isBlackTurn; // �� �ѱ��
            UpdateTurnIndicatorUI(); // UI ������Ʈ

            // �н� �� ���� ���� �÷��̾� ���̸� �Է� ���
            // ��ǻ�� ���� �ƴϰų�, �浹 ���� �� (�÷��̾� ��)
            isWaitingForPlayerInput = (isBlackTurn || !isComputerOpponent);

            // ���� ���� ��ǻ�� ���̰� AI ���� AI ���� ����
            if (!isBlackTurn && isComputerOpponent)
            {
                StartCoroutine(ComputerMoveCoroutine());
            }
        }
    }


    // --- ��Ģ ���� �Լ��� (���� �ڵ� ����) ---

    private bool IsMoveLegal(int x, int y, int playerColor)
    {
        // 1. �̹� ���� �ִ� ���� �� �� ���� (PlaceStone���� �̹� üũ������ ������ġ)
        if (boardState[x, y] != 0) return false;

        // 2. ������ ���忡 ���� ���ƺ���.
        int[,] tempBoard = (int[,])boardState.Clone();
        tempBoard[x, y] = playerColor;

        int opponentColor = (playerColor == 1) ? 2 : 1;
        bool capturesOpponent = false; // ��� ���� ��Ҵ��� ����

        // ĸó�� ������ �ӽ÷� �����Ͽ� �� �˻翡 Ȱ�� (���� �߰�)
        List<Vector2Int> potentialCapturedStones = new List<Vector2Int>();

        int[] dx = { 0, 0, -1, 1 };
        int[] dy = { 1, -1, 0, 0 };

        // 3. ���� �� �ֺ��� ��� ���� �������� Ȯ��
        for (int i = 0; i < 4; i++)
        {
            int nx = x + dx[i];
            int ny = y + dy[i];

            if (nx >= 0 && nx < BOARD_SIZE && ny >= 0 && ny < BOARD_SIZE && tempBoard[nx, ny] == opponentColor)
            {
                var groupInfo = FindGroupAndCountLibertiesOnBoard(nx, ny, tempBoard);
                if (groupInfo.liberties == 0) // ��� �׷��� Ȱ�ΰ� 0�̸� ����
                {
                    capturesOpponent = true;
                    // ���� ���忡�� ���� �� ���� �� potentialCapturedStones�� �߰�
                    foreach (var stone in groupInfo.stones)
                    {
                        tempBoard[stone.x, stone.y] = 0;
                        potentialCapturedStones.Add(stone); // �� �˻縦 ���� �߰�
                    }
                }
            }
        }

        // 4. ����� �˻� (�� ���� ���Ƽ� �� �׷��� Ȱ�ΰ� 0�� �Ǵ� ���)
        // ��, ��� ���� ��� ������� ��� (�� ����)
        var myGroupInfo = FindGroupAndCountLibertiesOnBoard(x, y, tempBoard);
        if (!capturesOpponent && myGroupInfo.liberties == 0)
        {
            Debug.Log($"[BadukManager] ������Դϴ�. ({x},{y})");
            return false; // ��� ���� ���� ���ϸ鼭 �� Ȱ�ΰ� 0�� �Ǹ� �����
        }

        // 5. ��(Ko) ��Ģ �˻� (����)
        // ������ ���� ���¿� ������ ���� ���¸� ����� ������ ����
        if (koProhibitedState != null && AreBoardsEqual(tempBoard, koProhibitedState))
        {
            Debug.Log($"[BadukManager] �� ��Ģ �����Դϴ�. ({x},{y})");
            return false; // ���� �� ���¿� ���� ���� ���°� ���ٸ� �� �� ����
        }

        return true; // ��� ��Ģ�� ����ϸ� �� �� ����
    }

    // �� ��ȹ Ȯ�� �� ����
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
                if (groupInfo.liberties == 0) // Ȱ�ΰ� ������ ����
                {
                    capturedStonesCount += RemoveStones(groupInfo.stones);
                }
            }
        }
        // ���� ���� ���� �׷��� ������� ������ ��� (IsMoveLegal���� ������ ������ ��� ���)
        // �� �κ��� IsMoveLegal���� �̹� ó���ϹǷ� ���⼭�� �������� �ʴ� ���� �� ������.
        // PlaceStone������ ���� ���� ���� ������ ��츸 ó���ϴ� ���� �Ϲ���.
        return capturedStonesCount;
    }

    // �� ���� (���� ���忡��)
    int RemoveStones(List<Vector2Int> stonesToRemove)
    {
        foreach (var pos in stonesToRemove)
        {
            if (boardObjects[pos.x, pos.y] != null)
            {
                Destroy(boardObjects[pos.x, pos.y]);
            }
            boardObjects[pos.x, pos.y] = null;
            boardState[pos.x, pos.y] = 0; // ���� ���µ� 0���� (�� ĭ)
        }
        return stonesToRemove.Count;
    }

    // Ư�� �� �׷��� Ȱ�� ��� (���� ���� ���� ����)
    (List<Vector2Int> stones, int liberties) FindGroupAndCountLiberties(int startX, int startY)
    {
        return FindGroupAndCountLibertiesOnBoard(startX, startY, this.boardState);
    }

    // Ư�� ���� ���¿��� �� �׷��� Ȱ�� ��� (���� ���� �˻� �� ���)
    (List<Vector2Int> stones, int liberties) FindGroupAndCountLibertiesOnBoard(int startX, int startY, int[,] board)
    {
        if (startX < 0 || startX >= BOARD_SIZE || startY < 0 || startY >= BOARD_SIZE)
            return (new List<Vector2Int>(), 0); // ��ȿ���� ���� ��ǥ ó��

        if (board[startX, startY] == 0) return (new List<Vector2Int>(), 0); // �� ĭ�̸� �׷� ����

        var stoneColor = board[startX, startY];
        var stonesInGroup = new List<Vector2Int>();
        var liberties = new HashSet<Vector2Int>(); // Ȱ�δ� �ߺ� ���� ���� ���� HashSet ���
        var q = new Queue<Vector2Int>();
        var visited = new bool[BOARD_SIZE, BOARD_SIZE]; // BFS �湮 üũ

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
                    if (board[nx, ny] == 0) // ������ �� ĭ�� Ȱ��
                    {
                        liberties.Add(new Vector2Int(nx, ny));
                    }
                    else if (board[nx, ny] == stoneColor && !visited[nx, ny]) // ���� �� ���̸� �׷쿡 �߰� Ž��
                    {
                        visited[nx, ny] = true;
                        q.Enqueue(new Vector2Int(nx, ny));
                    }
                }
            }
        }
        return (stonesInGroup, liberties.Count);
    }

    // �� �ٵ��� ���°� ������ �� (�� �˻翡 ���)
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

    // --- UI ������Ʈ �Լ� ---
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
            // �ؽ�Ʈ ���� �Ͽ� ���� ������ �� �ֽ��ϴ�.
            // turnIndicatorText.color = isBlackTurn ? Color.black : Color.white;
        }
    }

    // --- �Ͻ����� ���� �Լ� ---
    public void TogglePause()
    {
        if (isGameOver) return; // ���� ���� ���¿����� �Ͻ����� �Ұ�

        bool isCurrentlyPaused = false;
        if (canvasPauseMenu != null)
        {
            isCurrentlyPaused = canvasPauseMenu.activeSelf;
            canvasPauseMenu.SetActive(!isCurrentlyPaused); // �Ͻ����� ĵ���� Ȱ��ȭ/��Ȱ��ȭ ���
        }

        if (!isCurrentlyPaused) // �Ͻ����� ���·� ��ȯ
        {
            Time.timeScale = 0f; // ���� �ð� ���� (���� ����)
            Debug.Log("[BadukManager] ���� �Ͻ�����");
        }
        else // ���� �簳 ���·� ��ȯ
        {
            Time.timeScale = 1f; // ���� �ð� �簳
            Debug.Log("[BadukManager] ���� �簳");
        }
    }

    public void ResumeGame()
    {
        TogglePause(); // �Ͻ����� ����
    }

    // --- ���� ���� �� �谡 ���� �Լ� ---
    // �谡�� �����ϴ� �Լ� (��ư ���� �Ǵ� ���� �н� �� �ڵ� ȣ��)
    public void RequestScoreCalculation()
    {
        if (isGameOver) return;

        Debug.Log("[BadukManager] �谡 ��û!");
        isGameOver = true;
        Time.timeScale = 0f; // ���� �ð� ���� (UI ��ȣ�ۿ��� ����)

        // �Ͻ����� ĵ������ �����ִٸ� �ݱ�
        if (canvasPauseMenu != null)
        {
            canvasPauseMenu.SetActive(false);
        }

        // �谡 ĵ���� Ȱ��ȭ
        if (canvasScorePanel != null)
        {
            canvasScorePanel.SetActive(true);
            CalculateAndDisplayScore(); // �谡 ����� ����ϰ� ǥ��
        }
        else
        {
            Debug.LogError("[BadukManager] Score Panel Canvas�� �Ҵ���� �ʾҽ��ϴ�!");
        }
    }

    // �谡 ���� ����
    void CalculateAndDisplayScore()
    {
        Debug.Log("[BadukManager] �谡 ���� ��...");
        int blackTerritory = 0;
        int whiteTerritory = 0;

        // �湮 ���� Ȯ���� ���� �迭 (�� ����)
        bool[,] visited = new bool[BOARD_SIZE, BOARD_SIZE];

        // ��� ĭ�� ��ȸ�ϸ� �� ���
        for (int x = 0; x < BOARD_SIZE; x++)
        {
            for (int y = 0; y < BOARD_SIZE; y++)
            {
                if (boardState[x, y] == 0 && !visited[x, y]) // �� ĭ�̰� ���� �湮���� �ʾҴٸ�
                {
                    // BFS�� ���� �ش� �� ĭ �׷��� �����ڸ� ã��
                    (List<Vector2Int> emptyCells, int ownerColor) groupInfo = FindEmptyGroupOwner(x, y, visited);

                    if (groupInfo.ownerColor == 1) // ���� ��
                    {
                        blackTerritory += groupInfo.emptyCells.Count;
                        Debug.Log($"[BadukManager] ���� �߰�: {groupInfo.emptyCells.Count}ĭ");
                    }
                    else if (groupInfo.ownerColor == 2) // ���� ��
                    {
                        whiteTerritory += groupInfo.emptyCells.Count;
                        Debug.Log($"[BadukManager] ���� �߰�: {groupInfo.emptyCells.Count}ĭ");
                    }
                    // ownerColor�� 0�̸�, �� �׷��� ������ ���� �ƴ϶�� �� (���� ���� �Ǵ� �缮)
                }
            }
        }

        // ���� ��� (�� + ���� ��)
        // �Ϲ������� �ѱ� �ٵ��� �鿡�� ��(6.5��)�� �ݴϴ�.
        float komi = 6.5f; // �� ����

        float finalBlackScore = blackTerritory + blackCapturedStones;
        float finalWhiteScore = whiteTerritory + whiteCapturedStones + komi;

        Debug.Log($"[BadukManager] �� �� ����: {finalBlackScore} (��: {blackTerritory}, ���� ��: {blackCapturedStones})");
        Debug.Log($"[BadukManager] �� �� ����: {finalWhiteScore} (��: {whiteTerritory}, ���� ��: {whiteCapturedStones}, ��: {komi})");

        // ��� UI ������Ʈ
        if (blackScoreText != null) blackScoreText.text = $"Black Point : {finalBlackScore}";
        if (whiteScoreText != null) whiteScoreText.text = $"White Point: {finalWhiteScore}";

        if (resultText != null)
        {
            if (finalBlackScore > finalWhiteScore)
            {
                resultText.text = $"Winner Black! ({finalBlackScore} : {finalWhiteScore})";
                resultText.color = Color.black; // �� �¸� �� ���ڻ� ���� (���� ����)
            }
            else if (finalWhiteScore > finalBlackScore)
            {
                resultText.text = $"Winner White! ({finalWhiteScore} : {finalBlackScore})";
                resultText.color = Color.white; // �� �¸� �� ���ڻ� ���� (���� ����)
            }
            else
            {
                resultText.text = $"Null! ({finalBlackScore} : {finalWhiteScore})";
                resultText.color = Color.gray; // ���º� �� ���ڻ� ���� (���� ����)
            }
        }
    }

    // �� ĭ �׷��� �����ڸ� ã�� ���� �Լ� (BFS)
    // ��ȯ��: (�� ĭ���� ���, ������ ����: 1=��, 2=��, 0=����/�߸�)
    (List<Vector2Int> emptyCells, int ownerColor) FindEmptyGroupOwner(int startX, int startY, bool[,] visited)
    {
        List<Vector2Int> emptyCellsInGroup = new List<Vector2Int>();
        HashSet<int> surroundingColors = new HashSet<int>(); // �� �� �׷��� �ѷ��ΰ� �ִ� ������ ����
        Queue<Vector2Int> q = new Queue<Vector2Int>();

        q.Enqueue(new Vector2Int(startX, startY));
        visited[startX, startY] = true; // ������ �湮 ó��

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
                    if (boardState[nx, ny] == 0 && !visited[nx, ny]) // ������ �� ĭ�̸� ��� Ž��
                    {
                        visited[nx, ny] = true;
                        q.Enqueue(new Vector2Int(nx, ny));
                    }
                    else if (boardState[nx, ny] != 0) // ������ ���̸� �� ���� ���� ���
                    {
                        surroundingColors.Add(boardState[nx, ny]);
                    }
                }
            }
        }

        if (surroundingColors.Count == 1) // �� ���� ������ ���θ� �ѷ��ο� �ִٸ� �� ������ ����
        {
            return (emptyCellsInGroup, surroundingColors.First());
        }
        else // ���� ������ �� �Ǵ� �ƹ� ���� �ѷ��ΰ� ���� �ʴٸ� �߸� (���� �Ǵ� ���� ���� ����)
        {
            return (emptyCellsInGroup, 0);
        }
    }

    // ���� ����� �Լ�
    public void RestartGame()
    {
        Time.timeScale = 1f; // �ð� ������ ����
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // ���� �� ��ε�
        Debug.Log("[BadukManager] ���� �����");
    }

    // ���� �޴��� �̵� �Լ�
    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // ���� �ð��� ������� �ǵ��� (�ʼ�!)

        // ���� ���� UI ĵ�������� ��Ȱ��ȭ (�� �� �ε� �� �ڵ����� ���������, ��������� ����)
        if (canvasInGameUI != null) canvasInGameUI.SetActive(false);
        if (canvasPauseMenu != null) canvasPauseMenu.SetActive(false);
        if (canvasScorePanel != null) canvasScorePanel.SetActive(false);

        SceneManager.LoadScene("StartMenu"); // "MainMenu" ������ �̵�
        Debug.Log("[BadukManager] ���� �޴��� �̵�");
    }

    // --- ����Ƽ ������ ����� �׸��� (������) ---
    private void OnDrawGizmosSelected()
    {
        if (boardTransform == null) return;

        var boardRenderer = boardTransform.GetComponent<SpriteRenderer>();
        if (boardRenderer == null) return;

        // ���� ��꿡 ���Ǵ� �������� ����𿡼��� ���
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
                Gizmos.DrawWireSphere(worldPos, 0.05f); // �� �׸��� ������ ���� �� �׸���
            }
        }
    }
    public void ExitGame()
    {
        Time.timeScale = 1f; // Ȥ�� �𸣴� �ð� ������ ����
        Debug.Log("[BadukManager] ���� ����!");

        // ����� ���ӿ����� ���ø����̼� ����
#if UNITY_STANDALONE || UNITY_WEBGL || UNITY_IOS || UNITY_ANDROID // �ٸ� �÷����� �߰� ����
        Application.Quit();
#endif

        // Unity �����Ϳ��� �÷��� ��� ����
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}