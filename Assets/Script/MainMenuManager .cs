// MainMenuManager.cs
using UnityEngine;
using UnityEngine.SceneManagement; // 씬 전환 기능을 사용하기 위해 필요

public class MainMenuManager : MonoBehaviour
{
    [Header("UI References")]
    // Inspector에서 MainMenu Canvas를 할당할 public 변수
    public GameObject mainMenuCanvas;

    // 이 스크립트가 활성화될 때(즉, MainMenu 씬이 로드될 때) 한 번 호출됩니다.
    void Awake()
    {
        // Debug.Log("[MainMenuManager] Awake 호출됨."); // 디버깅용

        // 메인 메뉴 캔버스가 할당되어 있다면 활성화합니다.
        // 이는 MainMenu 씬이 로드될 때 Canvas_MainMenu가 항상 보이도록 보장합니다.
        if (mainMenuCanvas != null)
        {
            mainMenuCanvas.SetActive(true);
            // Debug.Log("[MainMenuManager] Main Menu Canvas 활성화."); // 디버깅용
        }
        else
        {
            Debug.LogError("[MainMenuManager] mainMenuCanvas 변수가 Inspector에 할당되지 않았습니다!");
        }

        // 게임 시작 시간을 다시 1로 설정하여 UI 상호작용이 가능하게 합니다.
        Time.timeScale = 1f;
    }

    // "AI 게임 시작" 버튼에 연결될 함수
    public void StartGameAI()
    {
        Debug.Log("[MainMenuManager] AI 게임 시작 버튼 클릭!");
        // 게임 씬으로 전환합니다. "GameScene"은 실제 게임 씬의 이름으로 변경해야 합니다.
        SceneManager.LoadScene("GameScene");
    }

    // "2인 플레이 게임 시작" 버튼에 연결될 함수
    public void StartGame2P()
    {
        Debug.Log("[MainMenuManager] 2인 플레이 게임 시작 버튼 클릭!");
        // 게임 씬으로 전환합니다. "GameScene"은 실제 게임 씬의 이름으로 변경해야 합니다.
        SceneManager.LoadScene("GameScene");
    }

    // "나가기" 버튼에 연결될 함수 (메인 메뉴에도 "나가기" 버튼이 있을 경우)
    public void ExitGame()
    {
        Debug.Log("[MainMenuManager] 게임 종료 버튼 클릭!");

        // 빌드된 게임(Windows, Web, iOS, Android 등)에서 애플리케이션을 종료합니다.
#if UNITY_STANDALONE || UNITY_WEBGL || UNITY_IOS || UNITY_ANDROID
        Application.Quit();
#endif

        // Unity 에디터에서 플레이 모드를 종료합니다.
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}