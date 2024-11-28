using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;

namespace Complete
{
    public class ShellExplosion : MonoBehaviour
    {
        public LayerMask m_TankMask;                        // Players 레이어 마스크 저장
        public ParticleSystem m_ExplosionParticles;         // 폭발 시 파티클 시스템
        public AudioSource m_ExplosionAudio;                // 폭발 시 오디오
        public float m_MaxDamage = 100f;                    // 포탄의 최대 공격력
        public float m_ExplosionForce = 1000f;              // 포탄의 폭발력
        public float m_MaxLifeTime = 2f;                    // 포탄이 공중에서 살아남을 수 있는 최대 시간
        public float m_ExplosionRadius = 5f;                // 포탄의 폭발 범위
        public float m_ExplosionBonusRadius = 0.5f;
        public Action<float> ExplosiveCallBackAction { get; set; }
        public Action DeleteCallBackAction { get; set; }

        [HideInInspector]public bool boomber = false;
        public float addBoomDelay = 0.7f;

        private void Start ()
        {
            // 포탄 생성 및 공중에서 살아남는 시간 설정
            Destroy (gameObject, m_MaxLifeTime + addBoomDelay * 3);
        }


        private void OnTriggerEnter (Collider other)
        {
            //if(PhotonNetwork.IsMasterClient)
              StartCoroutine(BoomWait()); 
        }

        public void Explosion(float bonusRadius)
        {
            // 모든 폭발 반경에 있는 Players 레이어를 가진 충돌체들을 배열에 저장
            Collider[] colliders = Physics.OverlapSphere(transform.position, m_ExplosionRadius + bonusRadius, m_TankMask);
            // 모든 충돌체에게 적용
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].transform.CompareTag("Barrel"))
                {
                    colliders[i].GetComponentInParent<Barrel>().photonView.RPC("ExpBarrel", RpcTarget.All);
                }
                else if (colliders[i].gameObject.layer == LayerMask.NameToLayer("Players"))
                {
                    // 타겟의 Rigidbody 추출
                    Rigidbody targetRigidbody = colliders[i].GetComponent<Rigidbody>();

                    // 타겟에게 Rigidbody가 없을 경우 이번 반복문 스킵
                    if (!targetRigidbody)
                        continue;

                    //타겟의 Rigidbody에 폭발력 전달
                    //targetRigidbody.AddExplosionForce(m_ExplosionForce, transform.position, m_ExplosionRadius);

                    // 타겟의 탱크 체력 컴포넌트 호출
                    TankHealth targetHealth = targetRigidbody.GetComponent<TankHealth>();

                    // 타겟에게 탱크 체력 컴포넌트가 없을 경우 이번 반복문 스킵
                    if (!targetHealth)
                        continue;

                    // 타겟의 폭발을 맞은 위치에 따라 데미지 설정
                    float damage = CalculateDamage(targetRigidbody.position);

                    // 타겟의 체력에 데미지를 가함
                    targetHealth.TakeExplosionDamage(m_ExplosionForce, transform.position, m_ExplosionRadius, damage);
                }
            }

            // 폭발 파티클 시스템의 부모 오브젝트를 제거
            m_ExplosionParticles.transform.parent = null;

            // 폭발 파티클의 스케일을 폭발 반경에 따라 변경
            m_ExplosionParticles.gameObject.transform.localScale = Vector3.one * (m_ExplosionRadius + bonusRadius);
            // 폭발 파티클 시스템 작동
            m_ExplosionParticles.Play();

            // 폭발 오디오 작동
            m_ExplosionAudio.Play();


            // 붐버 탱크의 포탄이 여러번 터질 것을 대비해 각종 컴포넌트 비활성화
            gameObject.GetComponent<CapsuleCollider>().enabled = false;
            gameObject.GetComponent<Rigidbody>().isKinematic = true;
            gameObject.GetComponent<MeshRenderer>().enabled = false;
        }


        private float CalculateDamage (Vector3 targetPosition)
        {
            // 타겟과 포탄과의 거리를 계산 및 저장
            Vector3 explosionToTarget = targetPosition - transform.position;

            // 타겟과 포탄과의 거리 벡터를 float형으로 변환해 저장
            float explosionDistance = explosionToTarget.magnitude;

            // 최대 폭발 반경에서 현재 타겟과 포탄과의 거리의 비율을 계산 및 저장
            float relativeDistance = (m_ExplosionRadius - explosionDistance) / m_ExplosionRadius;

            // 계산된 거리 비율에 최대 데미지를 곱해 거리 비율에 따른 데미지 계산
            float damage = relativeDistance * m_MaxDamage;

            // 계산된 데미지가 0보다 큰지 비교
            damage = Mathf.Max (0f, damage);

            return damage;
        }

        private float CalculateBonusRadius(float radius)
        {
            // 보너스 폭발 반경의 값 저장용 변수
            float bonusRadius;

            // 원래 폭발 반경 값에서 매개변수의 값만큼 축소
            bonusRadius = m_ExplosionBonusRadius / radius;

            // 축소된 값 반환
            return bonusRadius;
        }

        IEnumerator BoomWait()
        {
            // 폭발 파티클의 메인 모듈을 불러오고 보너스로 적용할 폭발 반경 값 초기화
            float radius = 0f;

            // 폭발 함수 호출 및 보너스 폭발 반경은 미적용
            Explosion(radius);
            //ExplosiveCallBackAction?.Invoke(radius);
            // 딜레이만큼 다음 폭발을 대기
            yield return new WaitForSeconds(addBoomDelay);

            // 이 탄을 발사한 탱크가 붐버 탱크가 아닌지 확인
            if(!boomber)
            {
                // 아니라면 파티클과 게임 오브젝트를 제거
                //DeleteCallBackAction?.Invoke();
                DestroyShellObject();

                yield break;
            }

            // 폭발 반경 값을 보너스 폭발 반경을 증가하여 설정
            radius += CalculateBonusRadius(m_ExplosionBonusRadius);

            // 증가된 폭발 반경 값으로 폭발 함수 호출
            Explosion(radius);
            //ExplosiveCallBackAction?.Invoke(radius);
            // 딜레이만큼 재대기
            yield return new WaitForSeconds(addBoomDelay);

            // 한 번더 폭발 반경 값을 증가
            radius += CalculateBonusRadius(m_ExplosionBonusRadius);

            // 한 번더 폭발 함수 호출
            Explosion(radius);
            //ExplosiveCallBackAction?.Invoke(radius);
            // 딜레이만큼 재대기
            yield return new WaitForSeconds(addBoomDelay);

            // 파티클과 게임 오브젝트를 제거
            //DeleteCallBackAction?.Invoke();
            DestroyShellObject();
        }

        public void DestroyShellObject()
        {
            Destroy(m_ExplosionParticles.gameObject, m_ExplosionParticles.main.duration);
            Destroy(gameObject);
        }
        
    }
}