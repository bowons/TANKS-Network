using Photon.Pun;
using System.Collections;
using UnityEngine;

namespace Complete
{
    public class TankMovement : MonoBehaviourPun, IPunObservable
    {
        public int m_PlayerNumber = 1;              // 플레이어 넘버 설정
        public float m_Speed = 12f;                 // 탱크의 이동 속도
        public float m_TurnSpeed = 180f;            // 탱크의 회전 속도
        public AudioSource m_MovementAudio;         // 탱크 이동 관련 오디오
        public AudioClip m_EngineIdling;            // 대기 상태 효과음
        public AudioClip m_EngineDriving;           // 이동 상태 효과음
		public float m_PitchRange = 0.2f;           // 효과음 빠르기 변화 시킬 값

        private string m_MovementAxisName;          // 전/후 움직임을 입력받을 axis 이름
        private string m_TurnAxisName;              // 좌/우 움직임을 입력받을 axis 이름
        private Rigidbody m_Rigidbody;              // Reference used to move the tank.
        private float m_MovementInputValue;         // 전/후 움직임 입력 값
        private float m_TurnInputValue;             // 좌/우 움직임 입력 값
        private float m_OriginalPitch;              // 기존 효과음의 빠르기
        public ParticleSystem[] m_particleSystems; // 탱크의 파티클 시스템을 모두 불러올 배열
        public ParticleSystem m_SpeedParticle;
        public ParticleSystem m_HealParticle;

        [HideInInspector] public bool m_IsSpeedUp;

        private Vector3 remotePos = new Vector3();
        private Quaternion remoteRot = new Quaternion(); // 2p의 탱크 트랜스폼 정보를 담을 구조체

        private void Awake ()
        {
            m_Rigidbody = GetComponent<Rigidbody> ();
        }


        private void OnEnable ()
        {
            // kinematic(물리요소의 효과 제거) 설정 해제
            m_Rigidbody.isKinematic = false;

            // 이동 관련 값 초기화
            m_MovementInputValue = 0f;
            m_TurnInputValue = 0f;

            // 탱크의 자식 오브젝트의 모든 파티클 시스템을 받아온 후 배열에 배치하고 모든 파티클을 작동
            m_particleSystems = GetComponents<ParticleSystem>();
            for (int i = 0; i < m_particleSystems.Length; ++i)
            {
                m_particleSystems[i].Play();
            }
        }


        private void OnDisable ()
        {
            // 사망 시 kinematic 설정
            m_Rigidbody.isKinematic = true;

            // 사망 시 파티클 시스템 배열에 저장해 둔 모든 파티클 시스템 작동 중지
            for(int i = 0; i < m_particleSystems.Length; ++i)
            {
                m_particleSystems[i].Stop();
            }
        }

        //네트워크로 변경 시 수정해야함
        private void Start ()
        {
            // 플레이어 넘버에 따라 전/후 및 좌/우 입력 받을 Axis 이름 설정
            m_MovementAxisName = "VerticalUI";
            m_TurnAxisName = "HorizontalUI";

            // 시작 시 현재의 오디오 빠르기를 오리지널 빠르기로 지정
            m_OriginalPitch = m_MovementAudio.pitch;
        }


        private void Update ()
        {
            // 플레이어의 넘버에 따른 axis 설정을 변수에 할당
            m_MovementInputValue = Input.GetAxis (m_MovementAxisName);
            m_TurnInputValue = Input.GetAxis (m_TurnAxisName);

            EngineAudio();
        }


        private void EngineAudio ()
        {
            // 아무런 이동 입력 값을 받지 않았을 때
            if (Mathf.Abs (m_MovementInputValue) < 0.1f && Mathf.Abs (m_TurnInputValue) < 0.1f)
            {
                // 현재 오디오 클립이 이동 상태의 효과음이라면
                if (m_MovementAudio.clip == m_EngineDriving)
                {
                    // 오디오 클립을 대기 상태의 효과음으로 변경 / +-의 효과음 빠르기 변화 값을 적용한 오디오를 적용 및 작동
                    m_MovementAudio.clip = m_EngineIdling;
                    m_MovementAudio.pitch = Random.Range (m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                    m_MovementAudio.Play ();
                }
            }
            else
            {
                // 현재 오디오 클립이 대기 상태의 효과음이라면
                if (m_MovementAudio.clip == m_EngineIdling)
                {
                    // 오디오 클립을 이동 상태의 효과음으로 변경 / +-의 효과음 빠르기 변화 값을 적용한 오디오를 적용 및 작동
                    m_MovementAudio.clip = m_EngineDriving;
                    m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                    m_MovementAudio.Play();
                }
            }
        }


        private void FixedUpdate ()
        {
            // 이동 관련 함수 호출
            if (photonView.IsMine) // 자신의 것이라면? 이동
            {
                Move();
                Turn();
            }
            else // 리모트 객체라면 선형 보간을 통해 이동처리
            {
                transform.position = Vector3.Lerp(transform.position, remotePos, 5 * Time.deltaTime);
                transform.rotation = Quaternion.Lerp(transform.rotation, remoteRot, 5 * Time.deltaTime);
            }
        }


        private void Move ()
        {
            // 입력 받은 전/후 이동 값에 따라 이동할 방향 및 속도 설정
            Vector3 movement = transform.forward * m_MovementInputValue * m_Speed * Time.deltaTime;

            // 이동 값을 토대로 탱크 이동
            m_Rigidbody.MovePosition(m_Rigidbody.position + movement);
        }


        private void Turn ()
        {
            // 입력 받은 좌/우 이동 값에 따라 회전할 방향 및 속도 설정
            float turn = m_TurnInputValue * m_TurnSpeed * Time.deltaTime;

            // 회전값을 y축 방향으로 설정
            Quaternion turnRotation = Quaternion.Euler (0f, turn, 0f);

            // 회전 값을 토대로 탱크 회전
            m_Rigidbody.MoveRotation (m_Rigidbody.rotation * turnRotation);
        }

        //컴포넌트의 위치를 동기화 한다.
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            // 내가 데이터를 보내는 중이라면
            if (stream.IsWriting) 
            {
                stream.SendNext(transform.position);
                stream.SendNext(transform.rotation);
            }
            // 내가 데이터를 받는 중이라면 
            else 
            {
                remotePos = (Vector3)stream.ReceiveNext();
                remoteRot = (Quaternion)stream.ReceiveNext();
            }
        }

        public IEnumerator SpeedUp(float BonusSpeed, float Duration)
        {
            // 탱크의 이동 속도 증가 여부를 변경
            m_IsSpeedUp = true;

            // 탱크의 원래 이동속도를 저장
            float originSpeed = m_Speed;
            if(photonView.IsMine)
            {// 내 탱크에게만
                m_Speed = m_Speed * BonusSpeed;
            }
            m_SpeedParticle.Play();

            // 가스의 효과를 지속
            yield return new WaitForSeconds(Duration);

            // 탱크를 원래의 이동속도로 변경 및 파티클 재생을 종료
            if (photonView.IsMine)
            {
                m_Speed = originSpeed;
            }
            m_SpeedParticle.Stop();
            
            // 탱크의 이동 속도 증가 여부 재설정
            m_IsSpeedUp = false;
        }

    }
}