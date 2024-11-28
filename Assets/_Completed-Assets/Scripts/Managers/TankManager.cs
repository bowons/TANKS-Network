using Photon.Pun;
using System;
using UnityEngine;

namespace Complete
{
    [Serializable]
    public class TankManager : MonoBehaviourPun
    {
        // 이 스크립트는 탱크의 다양한 설정을 관리하는 클래스입니다.
        // GameManager 클래스와 함께 작동되며 탱크가 작동하는 방식입니다.
        // 그리고 플레이어가 자신의 탱크를 제어할 수 있는지 여부를 확인합니다.
        // different phases of the game.

        public Color m_PlayerColor;                             // 플레이어의 탱크가 입혀지게 될 색상
        public Transform m_SpawnPoint;                          // 탱크가 스폰 될 스폰 포인트의 위치와 방향
        [HideInInspector] public int m_PlayerNumber;            // 플레이어의 넘버
        [HideInInspector] public string m_ColoredPlayerText;    // 해당 색상과 플레이어가 일치하도록 나타내는 문자열
        [HideInInspector] public GameObject m_Instance;         // 탱크가 생성될 때 인스턴스에 대한 참조
        [HideInInspector] public int m_Wins;                    // 이 플레이어가 지금까지 얻은 승점의 수
        

        private TankMovement m_Movement;                        // 이 탱크의 TankMovement 컴포넌트를 불러온다.
        private TankShooting m_Shooting;                        // 이 탱크의 TankShooting 컴포넌트를 불러온다.
        private GameObject m_CanvasGameObject;                  // 라운드의 시작 및 종료 단계에 탱크의 UI를 비활성화 하는데 활용
        private TankAPBulletSkill m_APBulletSkills;          // 철갑탄 스킬을 사용할 수 있도록 하는 컴포넌트

        public bool m_APBulletTank;                             // 이 탱크의 액티브 스킬이 철갑탄인지 여부

        private ItemManager m_ItemManager;                      // 아이템 매니저 컴포넌트

        private float m_OriginSpeed = 0;                             // 원래 이동 속도 저장용
        private float m_OriginFireCoolDownDelay = 0;                 // 원래 공격 속도 저장용



        public void Setup()
        {
            // Get references to the components.
            m_Movement = m_Instance.GetComponent<TankMovement>();
            m_Shooting = m_Instance.GetComponent<TankShooting>();
            m_CanvasGameObject = m_Instance.GetComponentInChildren<Canvas>().gameObject;

            // GameManager에서 설정된 플레이어의 넘버를 탱크의 컴포넌트에게 전달
            m_Movement.m_PlayerNumber = m_PlayerNumber;
            m_Shooting.m_PlayerNumber = m_PlayerNumber;

            // <color=#FF0000>PLAYER 1</color> 와 같은 문자열을 저장
            m_ColoredPlayerText = "<color=#" + ColorUtility.ToHtmlStringRGB(m_PlayerColor) + ">PLAYER " + m_PlayerNumber + "</color>";

            // 원래의 이동 속도 저장
            m_OriginSpeed = m_Movement.m_Speed;
            // 원래의 공격 속도 저장
            m_OriginFireCoolDownDelay = m_Shooting.m_FireCoolDownDelay;

            // 이 탱크의 하위 오브젝트들의 MeshRenderer 컴포넌트를 배열로 저장
            /*             MeshRenderer[] renderers = m_Instance.GetComponentsInChildren<MeshRenderer>();

                         // 모든 렌더러를 확인
                         for (int i = 0; i < renderers.Length; i++)
                         {
                             // 해당 렌더러에 플레이어의 색상을 적용
                             renderers[i].material.color = m_PlayerColor;
                         }*/

            foreach (var i in PhotonNetwork.CurrentRoom.Players)
            {
                if ((i.Value.TagObject as GameObject) == m_Instance)
                {
                    m_Instance.transform.Find("PlayerTank" + i.Key).gameObject.SetActive(true);
                } else
                {
                    m_Instance.transform.Find("PlayerTank" + i.Key).gameObject.SetActive(false);
                }
            }

            TransformDelivery();

            
            // 탱크에게 철갑탄 스킬을 적용
            if(m_Movement.photonView.IsMine)
                APBulletTankSet();
        }

        private void TransformDelivery()
        {
            // 아이템 매니저 오브젝트를 찾고 컴포넌트 저장
            GameObject itemManager = GameObject.Find("ItemManager");
            m_ItemManager = itemManager.GetComponent<ItemManager>();

            // 해당 플레이어 넘버에 따라 탱크의 위치 정보를 저장
            m_ItemManager.playerTransform[m_PlayerNumber-1] = m_Instance.GetComponent<Transform>();
        }

        public void APBulletTankSet()
        {
            // 철갑탄 스킬 컴포넌트를 저장
            m_APBulletSkills = m_Instance.GetComponent<TankAPBulletSkill>();
            // 해당 컴포넌트를 설정된 액티브 스킬의 여부에따라 활성 또는 비활성화
            m_APBulletSkills.APBulletSkillSet(m_APBulletTank);
        }


        // 플레이어가 탱크를 제어할 수 없도록 하는 함수
        public void DisableControl ()
        {
            if (m_Instance == null)
                return;

            m_Movement.enabled = false;
            m_Shooting.enabled = false;

            m_CanvasGameObject.SetActive (false);

            if (m_APBulletTank)
                m_APBulletSkills.APBulletSkillSet(false);

            // 이동 속도와 공격 속도 초기화
            m_Shooting.m_FireCoolDownDelay = m_OriginFireCoolDownDelay;
            m_Movement.m_Speed = m_OriginSpeed;
        }


        // 플레이어가 탱크를 제어할 수 있도록 하는 함수
        public void EnableControl ()
        {
            if (m_Instance == null)
                return;

            m_Movement.enabled = true;
            m_Shooting.enabled = true;

            m_CanvasGameObject.SetActive (true);

            if (m_APBulletTank)
                m_APBulletSkills.APBulletSkillSet(true);
        }


        // 라운드 시작 시 탱크를 기본 상태로 전환
        public void Reset ()
        {
            if (m_Instance == null)
                return;

            m_Instance.transform.position = m_SpawnPoint.position;
            m_Instance.transform.rotation = m_SpawnPoint.rotation;

            m_Instance.SetActive (false);
            m_Instance.SetActive (true);
        }
    }
}