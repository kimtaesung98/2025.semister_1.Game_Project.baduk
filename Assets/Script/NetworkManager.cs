using UnityEngine;
using UnityEngine.UI; // Button 사용을 위해 추가
using TMPro; // TextMeshPro 관련 기능 사용을 위해 추가
using Photon.Pun;
using Photon.Realtime; // Player, RoomOptions 등 사용을 위해 추가
using System.Collections.Generic; // List 사용을 위해 추가
using System.Text; // StringBuilder 사용을 위해 추가

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("UI Panels")]
    public GameObject lobbyPanel;
    public GameObject roomPanel;

    [Header("Lobby UI")]
    public TMP_InputField roomNameInput;

    [Header("Room UI")]
    public TMP_Text roomNameText;
    public TMP_Text playerListText;
    public Button startGameButton;

    void Start()
    {
        // 포톤 서버에 접속
        PhotonNetwork.ConnectUsingSettings();
    }

    // --- 포톤 콜백 함수들 ---

    public override void OnConnectedToMaster()
    {
        Debug.Log("마스터 서버 접속 성공");
        PhotonNetwork.JoinLobby(); // 로비에 접속
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("로비 접속 성공");
        lobbyPanel.SetActive(true); // 로비에 들어오면 로비 패널 활성화
        roomPanel.SetActive(false);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("방 참가 성공: " + PhotonNetwork.CurrentRoom.Name);
        lobbyPanel.SetActive(false); // 방에 들어오면 로비 패널 비활성화
        roomPanel.SetActive(true);   // 룸 패널 활성화

        roomNameText.text = "방 이름: " + PhotonNetwork.CurrentRoom.Name;
        UpdatePlayerList();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log(newPlayer.NickName + " 님이 방에 참가했습니다.");
        UpdatePlayerList();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log(otherPlayer.NickName + " 님이 방에서 나갔습니다.");
        UpdatePlayerList();
    }

    public override void OnLeftRoom()
    {
        Debug.Log("방에서 나갔습니다.");
        lobbyPanel.SetActive(true);
        roomPanel.SetActive(false);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"방 참가 실패: {message} (코드: {returnCode})");
    }

    // --- UI 버튼과 연결될 함수들 ---

    public void CreateRoom()
    {
        if (string.IsNullOrEmpty(roomNameInput.text))
        {
            Debug.Log("방 이름을 입력해주세요.");
            return;
        }
        // RoomOptions 설정: 최대 2명, 방 목록에 보이도록 설정
        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 2, IsVisible = true };
        PhotonNetwork.CreateRoom(roomNameInput.text, roomOptions);
    }

    public void JoinRoom()
    {
        if (string.IsNullOrEmpty(roomNameInput.text))
        {
            Debug.Log("방 이름을 입력해주세요.");
            return;
        }
        PhotonNetwork.JoinRoom(roomNameInput.text);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    // --- 기타 함수들 ---

    void UpdatePlayerList()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("플레이어 목록:");

        // 방에 있는 모든 플레이어 목록을 가져와서 텍스트로 표시
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            sb.AppendLine(player.NickName);
        }
        playerListText.text = sb.ToString();

        // 방장(MasterClient)만 게임 시작 버튼을 누를 수 있게 함
        startGameButton.interactable = PhotonNetwork.IsMasterClient;
    }
}