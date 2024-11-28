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

    private Action onJoinCallback; // ����Ŭ���� ����� ��������Ʈ
    private Action<string> roomNameCallback; // ����� �����ϴ� ��������Ʈ

    //���� ������ ���ε� �Լ�
    public void Initialize(RoomInfo roomInfo, Action onJoin, Action<string> roomNameCallback)
    {
        //���� ��ư�� �÷��̾� �� ����
        roomNameText = GetComponentsInChildren<Text>()[0];
        playerCountText = GetComponentsInChildren<Text>()[1];

        roomName = roomInfo.Name;
        roomNameText.text = roomInfo.Name;

        playerCountText.text = $"{roomInfo.PlayerCount}/{roomInfo.MaxPlayers}";
        onJoinCallback = onJoin;
        this.roomNameCallback = roomNameCallback;

        //��ư �׼� ���ε�
        Button panelButton = GetComponentInChildren<Button>();
        if(panelButton != null)
        {
            panelButton.onClick.AddListener(OnPanelClicked);
        }
    }

    //�� ���� ������Ʈ
    public void UpdateRoomInfo(RoomInfo roomInfo)
    {
        if (roomInfo.Name != roomName)
            return;

        roomNameText.text = roomInfo.Name;
        playerCountText.text = $"{roomInfo.PlayerCount}/{roomInfo.MaxPlayers}";
    }

    // �г� Ŭ���� �Լ�(���� Ŭ�� üũ)
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

