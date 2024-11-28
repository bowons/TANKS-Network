using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomListItem : MonoBehaviour
{
    private Text roomNameText;
    private Text playerCountText;

    private string roomName;
    private float lastClickTime = 0f;
    private float doubleClickThreshold = 0.3f;

    private Action onJoinCallback; // 더블클릭시 실행될 델리게이트
    private Action<string> roomNameCallback; // 룸네임 지정하는 델리게이트

    //대기방 생성시 바인드 함수
    public void Initialize(RoomInfo roomInfo, Action onJoin, Action<string> roomNameCallback)
    {
        //대기방 버튼의 플레이어 수 설정
        roomNameText = GetComponentsInChildren<Text>()[0];
        playerCountText = GetComponentsInChildren<Text>()[1];

        roomName = roomInfo.Name;
        roomNameText.text = roomInfo.Name;

        playerCountText.text = $"{roomInfo.PlayerCount}/{roomInfo.MaxPlayers}";
        onJoinCallback = onJoin;
        this.roomNameCallback = roomNameCallback;

        //버튼 액션 바인드
        Button panelButton = GetComponentInChildren<Button>();
        if(panelButton != null)
        {
            panelButton.onClick.AddListener(OnPanelClicked);
        }
    }

    //방 정보 업데이트
    public void UpdateRoomInfo(RoomInfo roomInfo)
    {
        if (roomInfo.Name != roomName)
            return;

        roomNameText.text = roomInfo.Name;
        playerCountText.text = $"{roomInfo.PlayerCount}/{roomInfo.MaxPlayers}";
    }

    // 패널 클릭시 함수(더블 클릭 체크)
    private void OnPanelClicked()
    {
        float currentTime = Time.time;

        roomNameCallback(roomName);

        if(currentTime - lastClickTime < doubleClickThreshold)
        {
            onJoinCallback?.Invoke();
        }

        lastClickTime = currentTime;
    }

    public void DisableInteraction()
    {
        Button panelButton = GetComponent<Button>();
        if (panelButton != null)
            panelButton.interactable = false;
    }

    public void EnableInteration()
    {
        Button panelButton = GetComponent<Button>();
        if (panelButton != null)
            panelButton.interactable = true;
    }
}

