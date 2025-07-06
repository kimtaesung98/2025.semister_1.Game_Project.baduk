using UnityEngine;
using UnityEngine.UI; // Button ����� ���� �߰�
using TMPro; // TextMeshPro ���� ��� ����� ���� �߰�
using Photon.Pun;
using Photon.Realtime; // Player, RoomOptions �� ����� ���� �߰�
using System.Collections.Generic; // List ����� ���� �߰�
using System.Text; // StringBuilder ����� ���� �߰�

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
        // ���� ������ ����
        PhotonNetwork.ConnectUsingSettings();
    }

    // --- ���� �ݹ� �Լ��� ---

    public override void OnConnectedToMaster()
    {
        Debug.Log("������ ���� ���� ����");
        PhotonNetwork.JoinLobby(); // �κ� ����
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("�κ� ���� ����");
        lobbyPanel.SetActive(true); // �κ� ������ �κ� �г� Ȱ��ȭ
        roomPanel.SetActive(false);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("�� ���� ����: " + PhotonNetwork.CurrentRoom.Name);
        lobbyPanel.SetActive(false); // �濡 ������ �κ� �г� ��Ȱ��ȭ
        roomPanel.SetActive(true);   // �� �г� Ȱ��ȭ

        roomNameText.text = "�� �̸�: " + PhotonNetwork.CurrentRoom.Name;
        UpdatePlayerList();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log(newPlayer.NickName + " ���� �濡 �����߽��ϴ�.");
        UpdatePlayerList();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log(otherPlayer.NickName + " ���� �濡�� �������ϴ�.");
        UpdatePlayerList();
    }

    public override void OnLeftRoom()
    {
        Debug.Log("�濡�� �������ϴ�.");
        lobbyPanel.SetActive(true);
        roomPanel.SetActive(false);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"�� ���� ����: {message} (�ڵ�: {returnCode})");
    }

    // --- UI ��ư�� ����� �Լ��� ---

    public void CreateRoom()
    {
        if (string.IsNullOrEmpty(roomNameInput.text))
        {
            Debug.Log("�� �̸��� �Է����ּ���.");
            return;
        }
        // RoomOptions ����: �ִ� 2��, �� ��Ͽ� ���̵��� ����
        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 2, IsVisible = true };
        PhotonNetwork.CreateRoom(roomNameInput.text, roomOptions);
    }

    public void JoinRoom()
    {
        if (string.IsNullOrEmpty(roomNameInput.text))
        {
            Debug.Log("�� �̸��� �Է����ּ���.");
            return;
        }
        PhotonNetwork.JoinRoom(roomNameInput.text);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    // --- ��Ÿ �Լ��� ---

    void UpdatePlayerList()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("�÷��̾� ���:");

        // �濡 �ִ� ��� �÷��̾� ����� �����ͼ� �ؽ�Ʈ�� ǥ��
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            sb.AppendLine(player.NickName);
        }
        playerListText.text = sb.ToString();

        // ����(MasterClient)�� ���� ���� ��ư�� ���� �� �ְ� ��
        startGameButton.interactable = PhotonNetwork.IsMasterClient;
    }
}