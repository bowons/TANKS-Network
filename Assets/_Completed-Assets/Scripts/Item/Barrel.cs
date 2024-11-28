using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Complete
{
    public class Barrel : MonoBehaviourPun
    {
        public LayerMask targetMask;                        // 타겟 레이어 마스크 저장
        public float barrelDamage = 100f;                   // 배럴의 폭발 데미지
        public float barrelExplosionForce = 1000f;          // 배럴의 폭발력
        public float barrelExplosionRadius = 7f;            // 배럴의 폭발 반경

        public ParticleSystem barrelExplosionParticles;     // 폭발 시 파티클 시스템
        public float barrelExpParticleRadius = 5f;          // 폭발 파티클의 반경
        //public AudioSource barrelExplosionAudio;          // 폭발 시 오디오

        private GameObject instance;     // 배럴이 생성될 때 인스턴스에 대한 참조

        private void Start()
        {
            instance = this.gameObject;
        }

        IEnumerator Explosion()
        {
            // 모든 폭발 반경에 있는 Players, Barrels 레이어를 가진 충돌체들을 배열에 저장
            Collider[] colliders = Physics.OverlapSphere(transform.position, barrelExplosionRadius, targetMask);

            // 모든 충돌체에게 적용
            for (int i = 0; i < colliders.Length; i++)
            {
                // 타겟의 레이어가 Barrels인지 확인
                if (colliders[i].gameObject.layer == LayerMask.NameToLayer("Barrel"))
                {
                    /*// 폭발 처리를 해야할 오브젝트가 인스턴스라면 스킵
                    if(colliders[i].transform.parent.gameObject == instance)
                    {
                        continue;
                    }*/

                    // 배럴에 폭발 처리
                    colliders[i].GetComponentInParent<Barrel>().photonView.RPC("ExpBarrel", RpcTarget.All);
                }
                // 타겟의 레이어가 Players인지 확인
                else if (colliders[i].gameObject.layer == LayerMask.NameToLayer("Players"))
                {
                    // 타겟의 Rigidbody 추출
                    Rigidbody targetRigidbody = colliders[i].GetComponent<Rigidbody>();

                    // 타겟에게 Rigidbody가 없을 경우 이번 반복문 스킵
                    if (!targetRigidbody)
                        continue;

                    // 타겟의 Rigidbody에 폭발력 전달
                    /*targetRigidbody.AddExplosionForce(barrelExplosionForce, transform.position, barrelExplosionRadius);*/

                    // 타겟의 탱크 체력 컴포넌트 호출
                    TankHealth targetHealth = targetRigidbody.GetComponent<TankHealth>();

                    // 타겟에게 탱크 체력 컴포넌트가 없을 경우 이번 반복문 스킵
                    if (!targetHealth)
                        continue;

                    // 타겟의 폭발을 맞은 위치에 따라 데미지 설정
                    float damage = CalculateDamage(targetRigidbody.position);

                    // 타겟의 체력에 데미지를 가함
                    /*targetHealth.TakeDamage(damage);*/
                    targetHealth.TakeExplosionDamage(barrelExplosionForce, transform.position, barrelExplosionRadius, damage); 
                }
            }

            // 폭발 파티클 시스템의 부모 오브젝트를 제거
            barrelExplosionParticles.transform.parent = null;

            // 폭발 파티클의 스케일을 폭발 반경에 따라 변경
            barrelExplosionParticles.gameObject.transform.localScale = Vector3.one * barrelExpParticleRadius;
            // 폭발 파티클 시스템 작동
            barrelExplosionParticles.Play();

            // 폭발 오디오 작동
            //barrelExplosionAudio.Play();

            // 폭발 파티클의 메인 모듈을 불러오고 설정되어 있는 지속시간에 따라 파티클을 제거
            ParticleSystem.MainModule mainModule = barrelExplosionParticles.main;
            Destroy(barrelExplosionParticles.gameObject, mainModule.duration);
            // 배럴 오브젝트 제거
            Destroy(gameObject);

            yield break;
        }

        [PunRPC]
        public void ExpBarrel()/*Collider barrelCollider*/
        {
            /*// 타겟 배럴의 Barrel 컴포넌트 참조
            Barrel targetBarrel = barrelCollider.GetComponentInParent<Barrel>();
            // 오버플로우 방지
            barrelCollider.enabled = false;*/
            // 타겟 배럴 Explosion 함수 호출
            if(GetComponentInChildren<Collider>().enabled == true)
            {
                GetComponentInChildren<Collider>().enabled = false;

                StartCoroutine(Explosion());
            }
            
        }

        private float CalculateDamage(Vector3 targetPosition)
        {
            // 타겟과 포탄과의 거리를 계산 및 저장
            Vector3 explosionToTarget = targetPosition - transform.position;

            // 타겟과 포탄과의 거리 벡터를 float형으로 변환해 저장
            float explosionDistance = explosionToTarget.magnitude;

            // 최대 폭발 반경에서 현재 타겟과 포탄과의 거리의 비율을 계산 및 저장
            float relativeDistance = (barrelExplosionRadius - explosionDistance) / barrelExplosionRadius;

            // 계산된 거리 비율에 최대 데미지를 곱해 거리 비율에 따른 데미지 계산
            float damage = relativeDistance * barrelDamage;

            // 계산된 데미지가 0보다 큰지 비교
            damage = Mathf.Max(0f, damage);

            return damage;
        }
    }
}
