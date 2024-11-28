using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Complete
{
    public class APBullet : MonoBehaviour
    {
        private AudioSource apBulletAudioSource;                        // 철갑탄의 오디오 소스
        public AudioClip fireClip;                                      // 철갑탄 생성 시 재생될 효과음
        public float maxAPBulletDamage;                                 // 철갑탄의 최고 데미지
        private float minAPBulletDamage = 50f;                          // 철갑탄의 최소 데미지

        [HideInInspector] public float currentChargingPower;            // TankShooting에서 불러올 현재 차징 값
        public Action DeleteBulletObject;

        private void Start()
        {
            apBulletAudioSource = gameObject.GetComponent<AudioSource>();

            // 철갑탄 생성과 동시에 효과음 재생
            apBulletAudioSource.PlayOneShot(fireClip);
        }

        private void OnTriggerEnter(Collider other)
        {
            /*if (!PhotonNetwork.IsMasterClient)
                return;*/

            // 철갑탄과 닿은 충돌체의 태그가 없을 경우
            if(other.gameObject.CompareTag("Untagged"))
            {
                // 철갑탄 비활성화
                APBulletDisable();
                //DeleteBulletObject?.Invoke();
            }
            // 철갑탄과 닿은 충돌체의 태그가 플레이어일 경우
            else if(other.gameObject.CompareTag("Player"))
            {
                // 해당 탱크의 리지드바디 저장
                Rigidbody targetRigidbody = other.GetComponent<Rigidbody>();

                // 타겟에게 리지드바디가 없을 경우 함수 종료
                if (!targetRigidbody)
                    return;

/*                // 해당 탱크에게 철갑탄이 날아온 방향으로 밀어내는 힘 부여
                targetRigidbody.AddForce(transform.forward * (currentChargingPower / 2f), ForceMode.Impulse);*/

                // 해당 탱크의 TankHealth 저장
                TankHealth targetHealth = targetRigidbody.GetComponent<TankHealth>();

                // 타겟에게 TankHealth가 없을 경우 함수 종료
                if (!targetHealth)
                    return;

                // 철갑탄의 데미지를 차징 값에 따라 저장
                float damage = currentChargingPower * (maxAPBulletDamage / minAPBulletDamage);

                // 데미지를 탱크에게 부여
                targetHealth.TakeDamage(transform.forward * (currentChargingPower / 2f), ForceMode.Impulse, damage);

                // 철갑탄 비활성화
                APBulletDisable();
                //DeleteBulletObject?.Invoke();
            }
            else if (other.transform.CompareTag("Barrel"))
            {
                other.GetComponentInParent<Barrel>().photonView.RPC("ExpBarrel", RpcTarget.All);
            }
        }

        // 철갑탄의 각종 물리효과와 시각효과 비활성화
        public void APBulletDisable()
        {
            gameObject.GetComponent<Rigidbody>().isKinematic = true;
            gameObject.GetComponent<CapsuleCollider>().enabled = false;
            gameObject.GetComponent<MeshRenderer>().enabled = false;
            gameObject.GetComponent<TrailRenderer>().enabled = false;

            // 철갑탄을 효과음이 끝날때까지 대기 후 제거
            Destroy(gameObject, fireClip.length);
        }
    }
}
