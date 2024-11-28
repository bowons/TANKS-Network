using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Complete
{
    // 설정될 액티브 스킬을 열거형으로 저장
    public enum ActiveSkill
    {
        Missile, APBullet
    }

    public class GameManager : MonoBehaviourPunCallbacks, IOnEventCallback 
    {
        public int m_NumRoundsToWin = 5;            // 한 플레이어가 달성해야 승리하는 라운드 승점의 수
        public float m_StartDelay = 3f;             // 라운드 시작과 플레이를 시작할 수 있는 타이밍 그 사이의 딜레이
        public float m_EndDelay = 3f;               // 플레이와 라운드 종료 타이밍 그 사이의 딜레이
        public CameraControl m_CameraControl;       // Reference to the CameraControl script for control during different phases.
        public Text m_MessageText;                  // 화면에 띄울 메세지(Win, Draw 등)
        public GameObject[] m_TankPrefab;           // 플레이어가 조종할 탱크 프리팹
        public TankManager[] m_Tanks;               // 플레이어가 탱크를 조종할 수 있도록 하는 TankManager 스크립트를 불러올 배열
        private TankMissileSkill m_MissileSkills;// 미사일을 생성해낼 액티브 스킬 컴포넌트
        private bool[] m_IsMissileSkillEnable;      // 미사일 스킬을 가질 플레이어의 숫자
        public ItemManager m_ItemManager;           // 아이템 매니저 컴포넌트

        private int m_RoundNumber;                  // 현재 라운드 수
        private WaitForSeconds m_StartWait;         // 라운드 시작 딜레이
        private WaitForSeconds m_EndWait;           // 라운드 종료, 게임 종료 딜레이
        private TankManager m_RoundWinner;          // 해당 라운드에서 이긴 플레이어
        private TankManager m_GameWinner;           // 해당 게임에서 이긴 플레이어
        private BarrelManager barrelManager;

        private string[] TankList = {"TurboTank","BoomberTank","SniperTank"}; //네트워크 인스턴스를 생성할수있도록 Resource 폴더내의 탱크의 이름들
        private static int networkPlayerNum; // 네트워크 플레이어 인덱스
        public enum eventEnum { Instantiate = 0 }; //RaiseEvent 사용자 이벤트 enum
        private int readyCount = 0;

        const float k_MaxDepenetrationVelocity = float.PositiveInfinity;
        
        private void Start()
        {
            // 이 줄은 물리 엔진의 변경 사항을 수정합니다
            Physics.defaultMaxDepenetrationVelocity = k_MaxDepenetrationVelocity;
            
            // 시간 딜레이 설정
            m_StartWait = new WaitForSeconds (m_StartDelay);
            m_EndWait = new WaitForSeconds (m_EndDelay);

            /* m_MissileSkills = gameObject.GetComponent<MissileSkillManager>();
            m_IsMissileSkillEnable = new bool[m_Tanks.Length]; */

            if (PhotonNetwork.IsMasterClient)
                GameSetting();

        }

        [PunRPC]
        public void StartGame()
        {
            StartCoroutine(GameLoop());
        }
		
        private void ActiveSkillSet(TankManager tank)
        {
            // 임의의 액티브 스킬을 저장
            ActiveSkill currentSkill;
            currentSkill = (ActiveSkill)TankDataManager.instance.currentSkill;
            
            // 현재 액티브 스킬을 확인
            switch (currentSkill)
            {
                
                case ActiveSkill.Missile:
                    m_MissileSkills = gameObject.GetComponent<TankMissileSkill>();
                    //m_IsMissileSkillEnable[number] = true;
                    m_MissileSkills?.SetEnable(true);
                    tank.m_APBulletTank = false;
                    break;
                case ActiveSkill.APBullet:
                    //m_IsMissileSkillEnable[number] = false;
                    m_MissileSkills?.SetEnable(false);
                    tank.m_APBulletTank = true;
                    break;
                default:
                    m_MissileSkills = gameObject.GetComponent<TankMissileSkill>();
                    //m_IsMissileSkillEnable[number] = true;
                    m_MissileSkills?.SetEnable(true);
                    tank.m_APBulletTank = false;

                    break;
            }
        }

        public void GameSetting()
        {
            barrelManager = GetComponent<BarrelManager>();
            photonView.RPC("SpawnAllTanks", RpcTarget.All);
        }



        [PunRPC]
        //네트워크 정보에 따라 스폰포인트 구분 및 인스턴스 구별 필요, 처리 예정
        private void SpawnAllTanks()
        {
            Transform[] targets = new Transform[PhotonNetwork.CurrentRoom.PlayerCount];
            m_CameraControl.m_Targets = targets;

            foreach (var i in PhotonNetwork.CurrentRoom.Players)
            {
                if (i.Value.Equals(PhotonNetwork.LocalPlayer))
                {
                    /* 탱크의 이름을 가져와 해당 탱크 포톤 인스턴스 생성 */
                    networkPlayerNum = i.Key - 1;
                    m_Tanks[networkPlayerNum].m_Instance =
                        PhotonNetwork.Instantiate(TankDataManager.instance.getTankName(TankDataManager.instance.currentTank)
                        , m_Tanks[networkPlayerNum].m_SpawnPoint.position, m_Tanks[networkPlayerNum].m_SpawnPoint.rotation) as GameObject;

                    m_Tanks[networkPlayerNum].m_PlayerNumber = i.Key;
                    // 해당 탱크에 액티브 스킬을 부여
                    ActiveSkillSet(m_Tanks[networkPlayerNum]);
                    m_Tanks[networkPlayerNum].Setup();

                    /* 포톤 이벤트 발생시키기 */
                    PhotonNetwork.RaiseEvent(((byte)eventEnum.Instantiate), 
                                                new object[] { networkPlayerNum 
                                                                , TankDataManager.instance.getTankName(TankDataManager.instance.currentTank)
                                                                , m_Tanks[networkPlayerNum].m_SpawnPoint.position
                                                                , m_Tanks[networkPlayerNum].m_SpawnPoint.rotation } 
                                                    , new RaiseEventOptions { Receivers = ReceiverGroup.Others }, SendOptions.SendReliable);

                    m_ItemManager.playerTransform[networkPlayerNum] = m_Tanks[networkPlayerNum].m_Instance.transform;
                    m_CameraControl.m_Targets[networkPlayerNum] = m_Tanks[networkPlayerNum].m_Instance.transform;
            }
        }

        }

 
        //네트워크 이벤트 발생시 실행될 메서드
        public void OnEvent(EventData photonEvent)
        {
            eventEnum eventType = (eventEnum)photonEvent.Code;
            
            switch(eventType)
            {
                case eventEnum.Instantiate: // 탱크 프리팹 생성시
                    object[] dataArray = (object[])photonEvent.CustomData;

                    int playerNum = (int)dataArray[0];
/*                    GameObject tankPrefab = m_TankPrefab[playerNum];
                    Vector3 position = (Vector3)dataArray[2];
                    Quaternion rotation = (Quaternion)dataArray[3];*/

                    //마스터 클라이언트로부터 정보를 받고 플레이어 넘버, 인스턴스 좌표와 회전값을 토대로 탱크 생성
                    m_Tanks[playerNum].m_PlayerNumber = playerNum + 1;
                    m_Tanks[playerNum].m_Instance = PhotonNetwork.CurrentRoom.Players[playerNum + 1].TagObject as GameObject;
                    /*ActiveSkillSet(m_Tanks[playerNum]);*/
                    m_Tanks[playerNum].Setup();

                    // 카메라와 아이템매니저 설정
                    m_ItemManager.playerTransform[playerNum] = m_Tanks[playerNum].m_Instance.transform;
                    m_CameraControl.m_Targets[playerNum] = m_Tanks[playerNum].m_Instance.transform;

                    if (PhotonNetwork.IsMasterClient) // 마스터 클라이언트라면. 플레이어 준비 확인
                    {
                        readyCount++;
                        if (readyCount != PhotonNetwork.CurrentRoom.PlayerCount - 1)
                            break;

                        photonView.RPC("StartGame", RpcTarget.All);
                    }
                    break;
                default:
                    break;
            }
        }

        public override void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        public override void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        // 라운드 시작과 진행, 종료를 차례대로 진행하는 라운드 진행 코루틴
        private IEnumerator GameLoop ()
        {
            // 게임 시작 단계가 끝날 때까지 반복되는 코루틴
            yield return StartCoroutine (RoundStarting ());

            // 게임 진행 단계가 끝날 때까지 반복되는 코루틴
            yield return StartCoroutine (RoundPlaying());

            // 게임 종료 단계가 끝날 때까지 반복되는 코루틴
            yield return StartCoroutine (RoundEnding());

            // 라운드가 끝나고 게임의 승자가 정해졌는지 확인
            if (m_GameWinner != null)
            {
                if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
                {
                    PhotonNetwork.Disconnect();
                    SceneManager.LoadScene(0);
                }
                else
                {
                    for (int i = 0; i < m_Tanks.Length; i++)
                    {
                        m_Tanks[i].m_Wins = 0;
                    }
                    m_RoundNumber = 0;
                    StartCoroutine(GameLoop());
                }
            }
            else
            {
                // 게임의 승자가 정해지지 않았다면 하나의 라운드를 더 진행
                // Note that this coroutine doesn't yield.  This means that the current version of the GameLoop will end.
                StartCoroutine (GameLoop ());
            }
        }


        private IEnumerator RoundStarting ()
        {
            // 라운드 시작과 동시에 탱크를 리셋하고 조종할 수 없도록 하는 함수 호출
            ResetAllTanks ();
            DisableTankControl ();

            // 라운드 시작과 동시에 아이템 매니저를 비활성화
            DisableItemManager();

            m_MissileSkills?.SetEnable(false);
            barrelManager?.SpawnBarrels();

            // Snap the camera's zoom and position to something appropriate for the reset tanks.
            m_CameraControl.SetStartPositionAndSize ();

            // 현재 라운드 넘버 증가 및 표시
            m_RoundNumber++;
            m_MessageText.text = "ROUND " + m_RoundNumber;

            // 라운드 시작 딜레이 만큼 대기
            yield return m_StartWait;
        }


        private IEnumerator RoundPlaying ()
        {
            // 라운드 진행과 동시에 모든 탱크를 조종 가능하도록 하는 함수 호출
            EnableTankControl ();

            // 라운드 진행과 동시에 아이템 매니저를 활성화 및 초기화
            EnableItemManager();
            ResetItemManager();

            /*for(int i = 0; i < m_IsMissileSkillEnable.Length; i++)
            {
                if(m_IsMissileSkillEnable[i])
                {
                    m_MissileSkills.enabled = true;
                    break;
                }
                else
                {
                    m_MissileSkills.enabled = false;
                }
            }*/

            m_MissileSkills?.SetEnable(true);

            // 메세지 제거
            m_MessageText.text = string.Empty;

            // 처치당한 탱크가 있는지 확인
            while (!OneTankLeft())
            {
                // 처치당한 탱크가 있다면 해당 진행 라운드 종료
                yield return null;
            }
        }


        private IEnumerator RoundEnding ()
        {
            // 라운드 종료와 동시에 모든 탱크를 조종할 수 없도록 하는 함수 호출
            DisableTankControl ();

            // 라운드 종료와 동시에 아이템 매니저를 비활성화
            DisableItemManager();
            m_MissileSkills?.SetEnable(false);
            barrelManager?.DestroyBarrels();

            // 이전 라운드의 승자를 제거
            m_RoundWinner = null;

            // 라운드 승자가 있는지 확인하는 함수 호출 및 라운드 승자를 설정
            m_RoundWinner = GetRoundWinner ();

            // 승자가 있다면 해당 TankManager 컴포넌트에 승리 포인트 플러스
            if (m_RoundWinner != null)
                m_RoundWinner.m_Wins++;

            // 승자가 있는지 확인하는 함수 호출 및 게임 승자 설정
            m_GameWinner = GetGameWinner ();

            // 설정된 문자열을 출력
            string message = EndMessage ();
            m_MessageText.text = message;

            // 라운드 종료 딜레이만큼 대기
            yield return m_EndWait;
        }


        // 라운드 진행 중에 처치당한 탱크가 있는지 확인하는 함수
        private bool OneTankLeft()
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
                return true;

            // 남은 탱크 수
            int numTanksLeft = 0;

            // 모든 탱크 호출
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                // 탱크가 조종할 수 있는 상태라면 남은 탱크 수를 플러스
                if (m_Tanks[i].m_Instance.activeSelf)
                    numTanksLeft++;
            }

            // 만약 남은 탱크 수가 2대가 아니라면 false 출력
            return numTanksLeft <= 1;
        }
        
        
        // 라운드의 승자가 있는지 확인하는 함수
        // 이 함수는 현재 활성화 상태인 탱크가 1개 이하라는 가정에서 호출
        private TankManager GetRoundWinner()
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == 1) // 현재 연결이 혼자 남았는지 확인
            {
                foreach (var keyPair in PhotonNetwork.CurrentRoom.Players)
                {
                    return m_Tanks[keyPair.Key - 1];
                }
            }

            // 모든 탱크 호출
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                // 현재 활성화 상태인 탱크를 반환
                if (m_Tanks[i].m_Instance.activeSelf)
                    return m_Tanks[i];
            }

            return null;
        }


        // 게임에 승자가 있는지 확인하는 함수
        private TankManager GetGameWinner()
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == 1) // 현재 연결이 혼자 남았는지 확인
            {
                foreach (var keyPair in PhotonNetwork.CurrentRoom.Players)
                {
                    return m_Tanks[keyPair.Key - 1];
                }
            }

            // 모든 탱크 호출
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                // 만약 해당 탱크의 승점이 달성해야 하는 라운드 승점의 수와 같다면 해당 탱크 반환
                if (m_Tanks[i].m_Wins == m_NumRoundsToWin)
                    return m_Tanks[i];
            }
            // 승자가 없다면 null 반환
            return null;
        }

        // 라운드의 결과에 따라 문자열을 반환하는 함수
        private string EndMessage()
        {
            // 라운드 승자가 아무도 없다면 문자열을 DRAW 로 설정
            string message = "DRAW!";

            // 만약 라운드 승자가 있다면 문자열을 승자 탱크의 플레이어 색에 따라 문자열을 플레이어 넘버 WINS THE ROUND 로 설정
            if (m_RoundWinner != null)
                message = m_RoundWinner.m_ColoredPlayerText + " WINS THE ROUND!";

            // 문자열 줄바꿈
            message += "\n\n\n\n";

            // 모든 탱크의 라운드 승리한 수를 플레이어 색에 따라 문자열을 각각 플레이어 넘버 : 플레이어 승점 WINS 로 설정
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                message += m_Tanks[i].m_ColoredPlayerText + ": " + m_Tanks[i].m_Wins + " WINS\n";
            }

            // 만약 게임 승자가 있다면 플레이어 색에 따라 문자열을 플레이어 넘버 WINS THE GAME! 으로 설정
            if (m_GameWinner != null)
                message = m_GameWinner.m_ColoredPlayerText + " WINS THE GAME!";

            return message;
        }


        // 모든 탱크의 위치를 스폰 포인트 위치로 변경
        private void ResetAllTanks()
        {
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                m_Tanks[i].Reset();
            }
        }


        private void EnableTankControl()
        {
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                m_Tanks[i].EnableControl();
            }
        }


        private void DisableTankControl()
        {
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                m_Tanks[i].DisableControl();
            }
        }


        private void DisableItemManager()
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            m_ItemManager.DestroyItmes();
            m_ItemManager.enabled = false;
        }

        private void EnableItemManager()
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            m_ItemManager.enabled = true;
        }

        private void ResetItemManager()
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            m_ItemManager.ResetManager();
        }
    }
}