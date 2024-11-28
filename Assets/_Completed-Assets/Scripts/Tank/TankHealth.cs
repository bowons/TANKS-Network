using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Complete
{
    public class TankHealth : MonoBehaviourPun
    {
        public float m_StartingHealth = 100f;               // 탱크의 시작 체력
        public Slider m_Slider;                             // 탱크 체력 슬라이더
        public Image m_FillImage;                           // 탱크 체력 이미지
        public Color m_FullHealthColor = Color.green;       // 최대체력일때 hp 색
        public Color m_ZeroHealthColor = Color.red;         // 최저체력일때 hp 색
        public GameObject m_ExplosionPrefab;                // 탱크 사망 시 생성되는 파티클 프리팹
        
        private AudioSource m_ExplosionAudio;               // 탱크 사망 시 생성되는 효과음
        private ParticleSystem m_ExplosionParticles;        // 파티클 컴포넌트
        private ParticleSystem m_HealParticle;
        [HideInInspector]public float m_CurrentHealth;                      // 탱크의 현재 체력
        private bool m_Dead;                                // 탱크의 사망 여부
        private Rigidbody m_tankRigidBody;

        private void Awake ()
        {
            // 생성시 폭발 파티클을 지정함
            m_ExplosionParticles = Instantiate (m_ExplosionPrefab).GetComponent<ParticleSystem> ();

            // 오디오 소스에서 폭발 음성 지정
            m_ExplosionAudio = m_ExplosionParticles.GetComponent<AudioSource> ();

            // 비활성화 적용
            m_ExplosionParticles.gameObject.SetActive(false);
            // 힐 파티클 지정
            m_HealParticle = this.gameObject.GetComponent<TankMovement>().m_HealParticle;
        }

        // 활성화 시 세팅
        private void OnEnable()
        {
            m_tankRigidBody = gameObject.GetComponent<Rigidbody>();
            m_Dead = false;

            if (PhotonNetwork.IsMasterClient)
            {
                // 현재체력을 시작 체력으로 세팅
                m_CurrentHealth = m_StartingHealth;
                // 체력바 최신화
                photonView.RPC("SetHealthUI", RpcTarget.All, m_CurrentHealth);
                PhotonNetwork.SendAllOutgoingCommands();
            }
        }


        public void TakeExplosionDamage (float explosionForce, Vector3 transformPos, float explosionRadius, float damageAmount)
        {
            if(PhotonNetwork.IsMasterClient)
            {
                // 모든 클라이언트에게 균등한 폭발 적용
                photonView.RPC("AddExplosionForce", RpcTarget.All, explosionForce, transformPos, explosionRadius);
                PhotonNetwork.SendAllOutgoingCommands();
                // 현재 체력에서 데미지를 차감
                m_CurrentHealth -= damageAmount;

                // 체력바 최신화
                photonView.RPC("SetHealthUI", RpcTarget.All, m_CurrentHealth);
                PhotonNetwork.SendAllOutgoingCommands();

            }

        }

        public void TakeDamage(Vector3 force, ForceMode forceMode, float damageAmount)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                // 모든 클라이언트에게 균등한 폭발 적용
                photonView.RPC("AddForce", RpcTarget.All, force, forceMode);
                PhotonNetwork.SendAllOutgoingCommands();
                // 현재 체력에서 데미지를 차감
                m_CurrentHealth -= damageAmount;

                // 체력바 최신화
                photonView.RPC("SetHealthUI", RpcTarget.All, m_CurrentHealth);
                PhotonNetwork.SendAllOutgoingCommands();
            }

        }

        [PunRPC] 
        private void AddExplosionForce(float explosionForce, Vector3 transformPos, float explosionRadius)
        {
            m_tankRigidBody.AddExplosionForce(explosionForce, transformPos, explosionRadius);
        }

        [PunRPC]
        private void AddForce(Vector3 force, ForceMode forceMode)
        {
            m_tankRigidBody.AddForce(force, forceMode);
        }

        [PunRPC]
        private void SetHealthUI (float newHealth)
        {
            //서버로부터 새 체력 정보 수신
            m_CurrentHealth = newHealth;
            // 체력바 값을 현재 체력으로 세팅
            m_Slider.value = newHealth;

            // 체력바의 색상을 현재 체력에 따라 변경
            m_FillImage.color = Color.Lerp (m_ZeroHealthColor, m_FullHealthColor, m_CurrentHealth / m_StartingHealth);

            // 체력이 0이하이고 살아있는 상태인 경우 OnDeath 호출
            if (m_CurrentHealth <= 0f && !m_Dead)
            {
                OnDeath();
            }
        }

        private void OnDeath ()
        {
            // 사망 처리
            m_Dead = true;

            // 죽은 포지션에 사망 파티클 포지션 변경 / 활성화
            m_ExplosionParticles.transform.position = transform.position;
            m_ExplosionParticles.gameObject.SetActive (true);

            // 파티클 플레이
            m_ExplosionParticles.Play ();

            // 효과음 플레이
            m_ExplosionAudio.Play();

            // 탱크 오브젝트 비활성화
            gameObject.SetActive (false);
        }

        public IEnumerator Heal(float recoveryAmount, float particleDuration)
        {
            if(PhotonNetwork.IsMasterClient)
            {
                // 현재 체력에서 더해진 회복량이 시작 체력을 넘어가는지 확인
                if (m_CurrentHealth + recoveryAmount > m_StartingHealth)
                {
                    // 최대 체력이 넘어가지 않도록 회복
                    m_CurrentHealth = m_StartingHealth;
                } else
                {
                    // 현재 체력을 회복량만큼 증가
                    m_CurrentHealth += recoveryAmount;
                }

                // 체력바 최신화
                photonView.RPC("SetHealthUI", RpcTarget.All, m_CurrentHealth);
            }

            // 회복 파티클 재생
            m_HealParticle.Play(); 

            // 회복 파티클을 지속
            yield return new WaitForSeconds(particleDuration);

            // 회복 파티클 재생을 종료
            m_HealParticle.Stop();
        }
    }
}