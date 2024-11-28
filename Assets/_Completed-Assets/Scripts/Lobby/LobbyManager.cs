using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.Demo.Cockpit;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    private string gameVersion = "1";

    public Text connectionInfo;
    public Text WaitingText;
    public Button startGameButton;
    public GameObject lobbyPannel;
    public GameObject MasterPannel;

    public GameObject roomListScrollView;
    public Transform roomListContent;
    public GameObject roomListItemPrefab;

    public Button createRoomButton;
    public Button joinRoomButton;
    public Button exitRoomButton;
    private byte maxPlayerCount = 2;
    private int roomCnt = 1;

    private Dictionary<string, RoomListItem> roomListItems = new Dictionary<string, RoomListItem>();
    private string selectedRoom = null;


    //씬 동시로드 설정
    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        // 로비 룸 버튼 리스너 설정
        joinRoomButton.onClick.AddListener(JoinRoom);
        createRoomButton.onClick.AddListener(() => { CreateRoom("Waiting Room" + roomCnt++); });
        exitRoomButton.onClick.AddListener(() => { PhotonNetwork.LeaveRoom(); });
        WaitingText.enabled = false;
    }

    private void Update()
    {// 선택된 room 없으면 버튼 비활성화
        if (selectedRoom != null)
        {
            joinRoomButton.interactable = true;
        }
        else
        {
            joinRoomButton.interactable = false;
        }
    }

    // 마스터 서버에 연결이 성공한 경우 자동 실행되는 콜백
    public override void OnConnectedToMaster()
    {
        startGameButton.interactable = true;
        connectionInfo.text = "Connected with Master Server";

        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        MasterPannel.SetActive(false);
        lobbyPannel.SetActive(true);
    }

    // 포톤 마스터서버와 디스커넥트 되었을때 발생하는 콜백
    public override void OnDisconnected(DisconnectCause cause)
    {
        startGameButton.interactable = false;
        connectionInfo.text = "Unable to connect to master server, trying to reconnect...";

        PhotonNetwork.ConnectUsingSettings();
    }

    // 버튼 클릭시 이벤트, 마스터 서버에 접속한다 
    public void Connect()
    {
        startGameButton.interactable = false; //중복 실행 방지
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();
        connectionInfo.text = "Connecting to Master Server...";
    }

    // 룸 참가시 실행
    public override void OnJoinedRoom()
    {
        createRoomButton.interactable = false;
        selectedRoom = null;
        exitRoomButton.interactable = true;

        WaitingText.enabled = true;
        roomListScrollView.SetActive(false);
    }

    // 룸 나갔을때 처리
    public override void OnLeftRoom()
    {
        createRoomButton.interactable = true;
        joinRoomButton.interactable = true;
        exitRoomButton.interactable = false;

        WaitingText.enabled = false;
        roomListScrollView.SetActive(true);
    }

    // 포톤에서 자동 실행, 룸 추가 삭제 시
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo room in roomList) // 업데이트 된 포톤 룸 리스트 조회
        {
            if(room.RemovedFromList || room.PlayerCount == 0) // 대기실 생성되거나 대기실 플레이어가 0명인 경우 리프레시
            {
                if(roomListItems.ContainsKey(room.Name))
                {//대기방 객체 삭제
                    Destroy(roomListItems[room.Name].gameObject);
                    roomListItems.Remove(room.Name);
                }
            } else
            {// 키쌍에서 조회 후 있는 경우
                if (roomListItems.ContainsKey(room.Name))
                {// 대기방 정보 업데이트
                    roomListItems[room.Name].UpdateRoomInfo(room);
                } else
                {// 대기방 생성 및 roomName 바인드
                    GameObject newRoomItem = Instantiate(roomListItemPrefab, roomListContent);
                    RoomListItem roomListItem = newRoomItem.GetComponent<RoomListItem>();
                    roomListItem.Initialize(room, JoinRoom, (roomName) => { 
                        if(roomListItems.ContainsKey(room.Name))
                        {
                            selectedRoom = roomName;
                        }
                    });

                    roomListItems.Add(room.Name, roomListItem);
                }
            }
            
        }
    }
    // 룸 생성
    public void CreateRoom(string roomName)
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = maxPlayerCount;
        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    public void JoinRoom()
    {
        if (selectedRoom != null)
        {
            PhotonNetwork.JoinRoom(selectedRoom);

        }
    }

    public void CloseRoom()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;

        PhotonNetwork.LeaveRoom();
    }

    // 리모트 플레이어 입장시 실행 설정 방인원 모두 차면 스타트
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
            {
                PhotonNetwork.LoadLevel("TankSelectScene");
            }
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 0)
        {
            CloseRoom();
        }
    }
}
