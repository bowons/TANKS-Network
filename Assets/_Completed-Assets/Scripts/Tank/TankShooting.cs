using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace Complete
{
    public class TankShooting : MonoBehaviourPun
    {
        public int m_PlayerNumber = 1;              // 플레이어 넘버 세팅
        public Rigidbody m_Shell;                   // 총알 리지드바디
        public Transform[] m_FireTransform;         // 총알이 나가는 포지션
        private float m_OriginFireAngleX;           // FireTransform의 원래 X축 각도
        public Slider m_AimSlider;                  // 에임 슬라이더
        public AudioSource m_ShootingAudio;         // 조준 시 오디오
        public AudioClip m_ChargingClip;            // 차징 효과음
        public AudioClip m_FireClip;                // 발사 효과음
        public float m_MinLaunchForce = 15f;        // 최저 차징 시 발사 거리
        public float m_MaxLaunchForce = 30f;        // 최대 차징 시 발사 거리
        public float m_MaxChargeTime = 0.75f;       // 최대 차징할 수 있는 시간
        public float m_FireCoolDownDelay = 0.5f;    // 포탄을 발사하고 다음 포탄을 발사할 때까지의 딜레이
        public float m_ShellSpeed = 2f;

        [HideInInspector]public bool m_IsTurboTank = false; // 터보 탱크인지 여부
        private bool m_IsFirstFire;
        public float m_TurboTankSecondFireBetweenTime = 0.3f;
        private float m_TurboTankSecondFireDelay;

        [HideInInspector] public bool m_IsBoomberTank = false;

        [HideInInspector]public bool m_APBulletSkillIsActivating = false;
        [HideInInspector] public float m_APBulletBonusSpeed;
        public Rigidbody m_APShell;

        private string m_FireButton;                // 발사 버튼
        private float m_CurrentLaunchForce;         // 현재 차징 발사 거리
        private float m_ChargeSpeed;                // 차징 스피드
        private bool m_Fired;                       // 발사 여부
        private float m_CurrentFireDelay;           // 포탄 발사 후 지금까지 지난 시간
        [HideInInspector]public bool m_IsDoubleFire;// 터보 탱크의 두 번째 포탄이 발사 될지에 대한 여부
        private int m_FireTransformNumber = 0;      // 탱크의 n번째 포신 번호

        [HideInInspector] public bool m_IsFireSpeedUp;


        private void OnEnable()
        {
            // 현재 차징 거리를 최저 차징 거리로 설정 / 에임 슬라이더 값을 최저 차징 값으로 설정
            m_CurrentLaunchForce = m_MinLaunchForce;
            m_AimSlider.value = m_MinLaunchForce;

            // 바로 포탄을 발사 할 수 있는 상태로 전환
            m_CurrentFireDelay = m_FireCoolDownDelay;

            if(m_IsTurboTank)
            {
                // 터보 탱크가 두 번째 포탄을 발사 할 수 있도록 전환
                m_IsFirstFire = true;
                m_TurboTankSecondFireDelay = m_TurboTankSecondFireBetweenTime;
                m_IsDoubleFire = false;
            }
        }


        private void Start ()
        {
            // 발사 버튼 스트링 값을 플레이어 넘버에 따라 설정
            m_FireButton = "Fire1";

            // 차징 스피드 설정
            m_ChargeSpeed = (m_MaxLaunchForce - m_MinLaunchForce) / m_MaxChargeTime;

            // 원래 각도 저장
            m_OriginFireAngleX = m_FireTransform[0].eulerAngles.x;
        }


        private void Update ()
        {
            if (!photonView.IsMine)
                return;

            // 에임 슬라이더 값을 최저 값으로 설정
            m_AimSlider.value = m_MinLaunchForce;

            if (m_FireCoolDownDelay > m_CurrentFireDelay)
            {
                // 시간이 지남에따라 현재 발사 딜레이를 증가
                m_CurrentFireDelay += Time.deltaTime;
            }

            // 터보 탱크인지 확인
            if(m_IsTurboTank)
            {
                // 두 번째 포탄이 발사될 차례인지 확인
                if(m_IsDoubleFire)
                {
                    // 두 번째 포탄 발사
                    DoubleFire();
                }
            }

            // 현재 딜레이가 발사 가능 딜레이를 넘지 못했다면 반환
            if(m_FireCoolDownDelay > m_CurrentFireDelay)
                return;
            // 현재 차징 값이 최대 차징 값 이상이고 발사 여부가 true가 되지 않았을 경우
            if (m_CurrentLaunchForce >= m_MaxLaunchForce && !m_Fired)
            {
                // 현재 차징 값이 최대가 되며 바로 발사
                m_CurrentLaunchForce = m_MaxLaunchForce;

                // 터보 탱크이면서 철갑탄 스킬이 발동 중이 아닌지 확인
                if(m_IsTurboTank && !m_APBulletSkillIsActivating)
                {
                    // 포신 갯수의 따라 추가 발사
                    for(int i = 0; i < m_FireTransform.Length; i++)
                    {
                        // 첫 번째 발사가 아니라면
                        if(!m_IsFirstFire)
                        {
                            // 두 번째 탄환이 발사될 차례로 전환 및 두 번째 포신으로 전환
                            m_IsDoubleFire = true;
                            m_FireTransformNumber = i;
                        }
                        // 첫 번째 발사라면
                        if (m_IsFirstFire)
                        {
                            // 기본 탄으로 n번째 포신에서 발사 개시
                            /*Fire(m_FireTransform[i], m_Shell);*/
                            Fire(i, "Shell");
                        }
                    }
                }
                // 아무 탱크나 철갑탄 스킬이 발동 중인지 확인
                else if(m_APBulletSkillIsActivating)
                {
                    // 철갑탄으로 발사 개시
                    /*Fire(m_FireTransform[0], m_APShell);*/
                    Fire(0, "APShell");
                }
                // 아무것도 해당 되지 않는다면
                else
                {
                    // 기본 탄으로 발사 개시
                    /*Fire(m_FireTransform[0], m_Shell);*/
                    Fire(0, "Shell");
                }
            }
            // 발사 버튼을 눌렀을 경우
            else if (Input.GetButtonDown (m_FireButton))
            {
                // 발사 여부를 false로 설정 / 현재 차징 값이 최저 값으로 설정
                m_Fired = false;
                m_CurrentLaunchForce = m_MinLaunchForce;

                // 차징 효과음을 재생
                m_ShootingAudio.clip = m_ChargingClip;
                m_ShootingAudio.Play ();
            }
            // 발사 여부가 false이고 발사 버튼을 누르고 유지하고 있을 경우
            else if (Input.GetButton (m_FireButton) && !m_Fired)
            {
                // 시간이 지남에 따라 차징 스피드 값 만큼 빠르게 현재 차징 값 변경
                m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime;

                // 현재 차징 값을 에임 슬라이더에 적용
                m_AimSlider.value = m_CurrentLaunchForce;
            }
            // 발사 여부가 false이고 발사 버튼을 뗐을 경우
            else if (Input.GetButtonUp (m_FireButton) && !m_Fired)
            {
                // 터보 탱크이면서 철갑탄 스킬이 발동 중이 아닌지 확인
                if (m_IsTurboTank && !m_APBulletSkillIsActivating)
                {
                    // 포신 갯수의 따라 추가 발사
                    for (int i = 0; i < m_FireTransform.Length; i++)
                    {
                        // 첫 번째 발사가 아니라면
                        if (!m_IsFirstFire)
                        {
                            // 두 번째 탄환이 발사될 차례로 전환 및 두 번째 포신으로 전환
                            m_IsDoubleFire = true;
                            m_FireTransformNumber = i;
                        }
                        // 첫 번째 발사라면
                        if (m_IsFirstFire)
                        {
                            // 기본 탄으로 n번째 포신에서 발사 개시
                            /*Fire(m_FireTransform[i], m_Shell);*/
                            Fire(i, "Shell");
                        }
                    }
                }
                // 아무 탱크나 철갑탄 스킬이 발동 중인지 확인
                else if (m_APBulletSkillIsActivating)
                {
                    // 철갑탄으로 발사 개시
                    /*Fire(m_FireTransform[0], m_APShell);*/
                    Fire(0, "APShell");
                }
                // 아무것도 해당 되지 않는다면
                else
                {
                    // 기본 탄으로 발사 개시
                    /*Fire(m_FireTransform[0], m_Shell);*/
                    Fire(0, "Shell");
                }
            }
        }

        //모든 플레이어에게 Shell 생성하는 네트워크 RPC 함수
        [PunRPC]
        private void CreateShellToAll(string shell, int index, bool isBoomberTank, float launchForce)
        {
            // 총알 생성 및 리지드 바디 값 저장
            Vector3 firePosition;
            Quaternion fireRotaion;
            Vector3 forwardPosition;

            Rigidbody shellInstance = null;
            switch (shell)
            {// 일반 탄환인지 철갑탄인지 확인
                case "Shell":
                    m_FireTransform[index].rotation = Quaternion.Euler(m_OriginFireAngleX, m_FireTransform[index].eulerAngles.y, m_FireTransform[index].eulerAngles.z);

                    firePosition = m_FireTransform[index].position;
                    fireRotaion = m_FireTransform[index].rotation;
                    // 탱크 발사 포신 위치에서 쉘 생성
                    shellInstance = Instantiate(m_Shell, firePosition, fireRotaion) as Rigidbody;
                    
                    break;
                case "APShell":
                    m_FireTransform[index].rotation = Quaternion.Euler(0, m_FireTransform[index].eulerAngles.y, m_FireTransform[index].eulerAngles.z);

                    firePosition = m_FireTransform[index].position;
                    fireRotaion = m_FireTransform[index].rotation;
                    // 탱크 발사 포신 위치에서 철갑탄 쉘 생성
                    shellInstance = Instantiate(m_APShell, firePosition, fireRotaion) as Rigidbody;
                    
                    break;
            }

            forwardPosition = m_FireTransform[index].forward;

            // 매개변수의 shell이 기본탄인지와 현재 탱크가 붐버 탱크인지 확인
            if (m_IsBoomberTank && shell == "Shell")
            {
                // 기본탄의 폭발 컴포넌트를 저장
                ShellExplosion shellExp = shellInstance.GetComponent<ShellExplosion>();

                // 포탄 폭발 컴포넌트에게 붐버 탱크 여부를 true로 저장
                shellExp.boomber = isBoomberTank;
            }
            // 매개변수의 shell이 철갑탄인지 확인
            else if (shell == "APShell")
            {
                // 철갑탄의 컴포넌트를 저장
                APBullet apBullet = shellInstance.GetComponent<APBullet>();

                // 철갑탄의 파워에 현재 차징 값을 저장
                apBullet.currentChargingPower = launchForce;
            }

            // 총알에 속도와 방향 적용
            shellInstance.velocity = launchForce * forwardPosition * (m_ShellSpeed + m_APBulletBonusSpeed);

            // 발사 효과음 재생
            m_ShootingAudio.clip = m_FireClip;
            m_ShootingAudio.Play(); 
        }


        [PunRPC]
        private void FireOnServer(int index, string shell, float launchForce)
        {

            if (shell == "APShell")
            {
                // 포탄이 생성되는 위치의 X축 각도를 0(정면)으로 전환

                CreateShellToAll("APShell", index, m_IsBoomberTank, launchForce);
            }
            // 매개변수의 shell이 기본탄인지 확인
            else if (shell == "Shell")
            {
                // 포탄이 생성되는 위치의 X축 각도를 원래의 각도(포물선 각도)로 전환
                ;
                CreateShellToAll("Shell", index, m_IsBoomberTank, launchForce);
            }
        }

        private void Fire (int index, string shell)
        {

            // 발사 여부를 true로 변경
            m_Fired = true;

            // 매개변수의 shell이 철갑탄인지 확인
            
            photonView.RPC("FireOnServer", RpcTarget.All, index, shell, m_CurrentLaunchForce);

            if (!m_IsFirstFire)
            {
                // 발사 이후 현재 차징 값을 최저 값으로 초기화
                m_CurrentLaunchForce = m_MinLaunchForce;
            }

            // 발사 이후 발사 할 수 없는 상태로 전환
            m_CurrentFireDelay = 0f;

            if (shell != "APShell")
            {
                // 발사 이후 두 번째 탄환임을 표시
                m_IsFirstFire = false;
            }
        }

        private void DoubleFire()
        {
            // 첫 번째 탄 발사 후 두 번째 탄을 발사할 때까지의 딜레이가 0보다 크다면 계속 호출
            if(m_TurboTankSecondFireDelay > 0)
            {
                // 두 번째 탄 발사의 딜레이를 시간이 지남에 따라 계속 차감
                m_TurboTankSecondFireDelay -= Time.deltaTime;

                // 두 번째 탄 발사의 딜레이가 0보다 작아졌다면
                if(m_TurboTankSecondFireDelay < 0)
                {
                    // DoubleFire 함수가 계속 돌지 않게 차단
                    m_TurboTankSecondFireDelay = 0f;

                    // 기본탄으로 발사
                    /*Fire(m_FireTransform[m_FireTransformNumber], m_Shell);*/
                    Fire(m_FireTransformNumber, "Shell");

                    // 첫 번째 탄이 발사 될 수 있도록 초기화 및 포신 초기화
                    m_IsFirstFire = true;
                    m_FireTransformNumber = 0;
                    // 두 번째 탄 발사의 딜레이를 설정한 값으로 초기화
                    m_TurboTankSecondFireDelay = m_TurboTankSecondFireBetweenTime;
                    // 두 번재 탄환이 발사 되지 않도록 초기화
                    m_IsDoubleFire = false;
                }
            }
        }

        public IEnumerator FireSpeedUp(float HighSpeedFireDelay, float WhistleDuration)
        {
            // 탱크의 원래 공격 속도를 저장
            m_IsFireSpeedUp = true;
            float originFireSpeed = m_FireCoolDownDelay;

            // 터보 탱크의 두 번째 사격 중간의 딜레이가 공격 속도 증가 딜레이보다 큰지 확인
            if (m_TurboTankSecondFireBetweenTime >= HighSpeedFireDelay)
            {
                // 공격 속도 증가량을 두 번째 사격 중간의 딜레이보다 느리게 적용 / 두 번째 사격 도중에 포탄을 발사하지 않도록 조정
                HighSpeedFireDelay = m_TurboTankSecondFireBetweenTime + 0.1f;
            }
            // 공격 속도 증가량을 적용
            m_FireCoolDownDelay = HighSpeedFireDelay;

            // 호루라기의 효과를 지속
            yield return new WaitForSeconds(WhistleDuration);

            // 공격 속도를 원래의 속도로 전환
            m_FireCoolDownDelay = originFireSpeed;
            m_IsFireSpeedUp = false;
        }
    }
}