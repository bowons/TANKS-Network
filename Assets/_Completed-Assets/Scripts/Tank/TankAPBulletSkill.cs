using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Complete
{
    public class TankAPBulletSkill : MonoBehaviour
    {
        private bool apBulletActivating = false;                        // 철갑탄 스킬의 활성화 여부
        public float apBulletSkillDuration = 3f;                        // 철갑탄 스킬의 지속 시간
        public float apBulletSkillDelay = 5f;                           // 철갑탄 스킬의 쿨타임
        public float apBulletBonusSpeed = 2f;                           // 철갑탄의 추가 이동속도
        private Image apCooldownImg;                                     // 미사일 쿨타임 이미지
        private Text apCooldownTxt;                                      // 미사일 쿨타임 텍스트

        private TankShooting shooting;

        void Start()
        {// 스킬 이미지 설정
            shooting = gameObject.GetComponent<TankShooting>();
            apCooldownImg = GameObject.Find("APBulletCooldownImg").GetComponent<Image>();
            apCooldownTxt = GameObject.Find("ActiveSkillCooldownTxt").GetComponent<Text>();
        }

        void Update()
        {
            // 키보드 C를 누르고 철갑탄 스킬이 활성화 되지 않았는지 확인
            if (Input.GetKeyDown(KeyCode.C) && !apBulletActivating)
            {
                // 철갑탄 스킬 쿨타임 코루틴 호출
                StartCoroutine(APBulletFireDelay(apBulletSkillDuration));
            }
        }

        public void APBulletSkillSet(bool isTrue)
        {
            // 철갑탄 관련 이미지와 텍스트를 참조
            apCooldownImg = GameObject.Find("APBulletCooldownImg").GetComponent<Image>();
            apCooldownTxt = GameObject.Find("ActiveSkillCooldownTxt").GetComponent<Text>();

            // 액티브 스킬의 쿨타임 이미지를 채우고 쿨타임 텍스트를 비활성화한 상태로 시작
            apCooldownImg.fillAmount = apBulletSkillDelay;
            apCooldownImg.enabled = false;
            apCooldownTxt.enabled = false;

            // 철갑탄 스킬이 설정되어 있는지 확인
            if (isTrue)
            {
                // 철갑탄 스킬 매니저 컴포넌트를 사용가능한 상태로 전환
                this.gameObject.GetComponent<TankAPBulletSkill>().enabled = true;
                // 철갑탄 스킬이 작동중이지 않은 상태로 전환
                apBulletActivating = false;

                // 철갑탄 이미지를 활성화
                apCooldownImg.enabled = true;
            }
            else
            {
                // 철갑탄 스킬 매니저 컴포넌트를 사용불가능한 상태로 전환
                this.gameObject.GetComponent<TankAPBulletSkill>().enabled = false;
                // 철갑탄 스킬이 작동중인 상태로 전환
                apBulletActivating = true;

                // 철갑탄 이미지를 비활성화
                apCooldownImg.enabled = false;
            }
        }

        IEnumerator APBulletFireDelay(float cool)
        {
            // 철갑탄 스킬의 현재 지난 시간
            float delay = 0f;

            // 철갑탄을 작동중인 상태로 전환 및 철갑탄의 속도 보너스 적용
            apBulletActivating = true;
            shooting.m_APBulletSkillIsActivating = true;
            shooting.m_APBulletBonusSpeed = apBulletBonusSpeed;

            // 철갑탄 스킬 지속
            yield return new WaitForSeconds(cool);

            // 철갑탄이 생성되지 않도록 전환 및 속도 보너스 초기화
            shooting.m_APBulletSkillIsActivating = false;
            shooting.m_APBulletBonusSpeed = 0f;

            // 스킬 쿨타임 텍스트 활성화
            apCooldownTxt.enabled = true;

            // 스킬의 쿨타임이 끝나지 않았다면
            while (delay < apBulletSkillDelay)
            {
                // 현재 지난 시간을 증가
                delay += Time.deltaTime;
                // 현재 흐른 시간의 내림수가 실제 쿨타임에 도달하지 못했을 경우
                if (Mathf.Floor(apBulletSkillDelay - delay) >= 0)
                {
                    // 5부터 시작하여 1까지 소숫점 없이 내림수로 쿨타임 표시
                    apCooldownTxt.text = (Mathf.Floor(apBulletSkillDelay - delay) + 1).ToString();
                }
                // 현재 흐른 시간 값만큼 쿨타임 이미지를 채움
                apCooldownImg.fillAmount = (delay / apBulletSkillDelay);
                yield return new WaitForFixedUpdate();
            }

            // 스킬 쿨타임 텍스트 비활성화 및 철갑탄을 작동중이지 않은 상태로 전환
            apCooldownTxt.enabled = false;
            apBulletActivating = false;
        }
    }
}
