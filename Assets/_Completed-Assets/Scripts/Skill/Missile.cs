using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Complete
{
    public class Missile : MonoBehaviour
    {
        public LayerMask m_TankMask;                        // Players 레이어 마스크 저장
        public ParticleSystem explosionParticles;           // 폭발 시 파티클 시스템
        public AudioSource explosionAudio;                  // 폭발 시 오디오

        public float missileStartSpeed = 0.2f;              // 미사일이 생성되었을 때 처음 이동속도
        public float missileDamage = 70f;                   // 미사일의 최대 데미지
        public float missileExpForce = 1200f;               // 미사일의 폭발력
        public float missileExpRadius = 8f;                 // 미사일의 폭발 반경

        [HideInInspector]public Vector3 targetPosition;     // 미사일이 떨어질 위치 정보
        public delegate void DestroyEvent();
        private DestroyEvent destroyEventCallback = null;

        void Start()
        {
 
            // 미사일의 각도를 아랫방향으로 변경
            transform.rotation = Quaternion.Euler(90, 0, 0);
        }

        void Update()
        {
            // 미사일을 현재 위치에서 떨어지게 될 위치로 이동
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(targetPosition.x,-10,targetPosition.z) , missileStartSpeed * Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            // 모든 폭발 반경에 있는 Players 레이어를 가진 충돌체들을 배열에 저장
            Collider[] colliders = Physics.OverlapSphere(transform.position, missileExpRadius, m_TankMask);

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

                    // 타겟의 Rigidbody에 폭발력 전달
                    //targetRigidbody.AddExplosionForce(missileExpForce, transform.position, missileExpRadius);

                    // 타겟의 탱크 체력 컴포넌트 호출
                    TankHealth targetHealth = targetRigidbody.GetComponent<TankHealth>();

                    // 타겟에게 탱크 체력 컴포넌트가 없을 경우 이번 반복문 스킵
                    if (!targetHealth)
                        continue;

                    // 타겟의 폭발을 맞은 위치에 따라 데미지 설정
                    float damage = CalculateDamage(targetRigidbody.position);

                    // 타겟의 체력에 데미지를 가함
                    targetHealth.TakeExplosionDamage(missileExpForce, transform.position, missileExpRadius, damage);
                }
            }

            // 폭발 파티클 시스템의 부모 오브젝트를 제거
            explosionParticles.transform.parent = null;
            // 폭발 파티클의 스케일을 폭발 반경에 따라 변경
            explosionParticles.gameObject.transform.localScale = Vector3.one * missileExpRadius;
            // 폭발 파티클 시스템 작동
            explosionParticles.Play();

            // 폭발 오디오 작동
            explosionAudio.Play();

            // 폭발 파티클의 메인 모듈을 불러오고 설정되어 있는 지속시간에 따라 파티클을 제거
            ParticleSystem.MainModule mainModule = explosionParticles.main;
            Destroy(explosionParticles.gameObject, mainModule.duration);
            Destroy(gameObject);
        }

        private float CalculateDamage (Vector3 targetPosition)
        {
            // 타겟과 미사일과의 거리를 계산 및 저장
            Vector3 explosionToTarget = targetPosition - transform.position;

            // 타겟과 미사일과의 거리 벡터를 float형으로 변환해 저장
            float explosionDistance = explosionToTarget.magnitude;

            // 최대 폭발 반경에서 현재 타겟과 미사일과의 거리의 비율을 계산 및 저장
            float relativeDistance = (missileExpRadius - explosionDistance) / missileExpRadius;

            // 계산된 거리 비율에 최대 데미지를 곱해 거리 비율에 따른 데미지 계산
            float damage = relativeDistance * missileDamage;

            // 계산된 데미지가 0보다 큰지 비교
            damage = Mathf.Max (0f, damage);

            return damage;
        }

        private void OnDisable()
        {
            destroyEventCallback?.Invoke();
        }

        public void SetDestroyEvent(DestroyEvent eventCallback)
        {
            destroyEventCallback = eventCallback;
        }
    }
}
