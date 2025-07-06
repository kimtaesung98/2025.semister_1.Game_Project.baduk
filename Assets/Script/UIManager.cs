using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject gameModePanel; // 게임 모드 선택 패널
    public BadukManager badukManager; // BadukManager 스크립트 참조

    // "컴퓨터와 대전" 버튼을 클릭했을 때 호출될 함수
    public void OnClick_StartAIChallenge()
    {
        badukManager.StartGame(true); // AI 모드로 게임 시작
        gameModePanel.SetActive(false); // 선택 패널 비활성화
    }

    // "2인 대전" 버튼을 클릭했을 때 호출될 함수
    public void OnClick_Start2PChallenge()
    {
        badukManager.StartGame(false); // 2인용 모드로 게임 시작
        gameModePanel.SetActive(false); // 선택 패널 비활성화
    }
}