using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject gameModePanel; // ���� ��� ���� �г�
    public BadukManager badukManager; // BadukManager ��ũ��Ʈ ����

    // "��ǻ�Ϳ� ����" ��ư�� Ŭ������ �� ȣ��� �Լ�
    public void OnClick_StartAIChallenge()
    {
        badukManager.StartGame(true); // AI ���� ���� ����
        gameModePanel.SetActive(false); // ���� �г� ��Ȱ��ȭ
    }

    // "2�� ����" ��ư�� Ŭ������ �� ȣ��� �Լ�
    public void OnClick_Start2PChallenge()
    {
        badukManager.StartGame(false); // 2�ο� ���� ���� ����
        gameModePanel.SetActive(false); // ���� �г� ��Ȱ��ȭ
    }
}