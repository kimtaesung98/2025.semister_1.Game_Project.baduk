// MainMenuManager.cs
using UnityEngine;
using UnityEngine.SceneManagement; // �� ��ȯ ����� ����ϱ� ���� �ʿ�

public class MainMenuManager : MonoBehaviour
{
    [Header("UI References")]
    // Inspector���� MainMenu Canvas�� �Ҵ��� public ����
    public GameObject mainMenuCanvas;

    // �� ��ũ��Ʈ�� Ȱ��ȭ�� ��(��, MainMenu ���� �ε�� ��) �� �� ȣ��˴ϴ�.
    void Awake()
    {
        // Debug.Log("[MainMenuManager] Awake ȣ���."); // ������

        // ���� �޴� ĵ������ �Ҵ�Ǿ� �ִٸ� Ȱ��ȭ�մϴ�.
        // �̴� MainMenu ���� �ε�� �� Canvas_MainMenu�� �׻� ���̵��� �����մϴ�.
        if (mainMenuCanvas != null)
        {
            mainMenuCanvas.SetActive(true);
            // Debug.Log("[MainMenuManager] Main Menu Canvas Ȱ��ȭ."); // ������
        }
        else
        {
            Debug.LogError("[MainMenuManager] mainMenuCanvas ������ Inspector�� �Ҵ���� �ʾҽ��ϴ�!");
        }

        // ���� ���� �ð��� �ٽ� 1�� �����Ͽ� UI ��ȣ�ۿ��� �����ϰ� �մϴ�.
        Time.timeScale = 1f;
    }

    // "AI ���� ����" ��ư�� ����� �Լ�
    public void StartGameAI()
    {
        Debug.Log("[MainMenuManager] AI ���� ���� ��ư Ŭ��!");
        // ���� ������ ��ȯ�մϴ�. "GameScene"�� ���� ���� ���� �̸����� �����ؾ� �մϴ�.
        SceneManager.LoadScene("GameScene");
    }

    // "2�� �÷��� ���� ����" ��ư�� ����� �Լ�
    public void StartGame2P()
    {
        Debug.Log("[MainMenuManager] 2�� �÷��� ���� ���� ��ư Ŭ��!");
        // ���� ������ ��ȯ�մϴ�. "GameScene"�� ���� ���� ���� �̸����� �����ؾ� �մϴ�.
        SceneManager.LoadScene("GameScene");
    }

    // "������" ��ư�� ����� �Լ� (���� �޴����� "������" ��ư�� ���� ���)
    public void ExitGame()
    {
        Debug.Log("[MainMenuManager] ���� ���� ��ư Ŭ��!");

        // ����� ����(Windows, Web, iOS, Android ��)���� ���ø����̼��� �����մϴ�.
#if UNITY_STANDALONE || UNITY_WEBGL || UNITY_IOS || UNITY_ANDROID
        Application.Quit();
#endif

        // Unity �����Ϳ��� �÷��� ��带 �����մϴ�.
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}