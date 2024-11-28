using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Complete
{
    public class TankMissileSkill : MonoBehaviourPun
    {
        public GameObject missilePrefab;                        // 생성될 미사일 프리팹
        public GameObject missileLocation;                      // 마우스로 설정하는 미사일이 떨어지게 될 위치
        public Image missileCooldownImg;                        // 미사일 쿨타임 이미지
        public Text missileCooldownTxt;                         // 미사일 쿨타임 텍스트
        private GameObject missileLocationGO;                   // 미사일이 떨어지게 될 위치에 생길 파티클 오브젝트

        private bool skillIsReady = true;                       // 스킬이 준비되었는지 여부
        public float activeSkillReloadDelay = 3f;               // 미사일 스킬의 쿨타임 딜레이
        private float startSkillReloadDelay = 0f;               // 미사일 스킬의 쿨타임이 시작될 초기값

        private Missile missileScript;                          // 미사일의 미사일 컴포넌트

        void Start()
        {
            // 액티브 스킬의 쿨타임 이미지를 채우고 쿨타임 텍스트를 비활성화한 상태로 시작
            missileCooldownImg.fillAmount = activeSkillReloadDelay;
            missileCooldownImg.enabled = false;
            missileCooldownTxt.enabled = false;
            this.enabled = false;
        }

        void Update()
        {
            // 마우스 왼쪽 버튼을 눌렀는지 확인
            if(Input.GetMouseButtonDown(0))
            {
                // 스킬이 준비된 상태가 아니라면 반환
                if(!skillIsReady)
                {
                    return;
                }
                // 스킬이 준비된 상태라면 미사일 생성 함수 호출
                ActiveSkillInstantiate();
            }

        }

        public void SetEnable(bool enable)
        {
            this.enabled = enable;
            this.missileCooldownImg.enabled = enable;
        }

        private void ActiveSkillInstantiate()
        {
            // 마우스로 클릭한 지점의 좌표값을 저장
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Vector3 point = Vector3.zero;

            // 마우스로 클릭한 지점이 10000거리 안에 존재할 경우
            if (Physics.Raycast(ray, out hit, 10000f))
            {
                // 그 지점을 Vector3 값으로 저장
                point = hit.point;
            }

            // 미사일이 생성될 위치를 항상 70정도의 높이의 저장한 지점으로 변경
            Vector3 missileLaunchPosition = new Vector3(point.x, 70, point.z);
            missileLocationGO = Instantiate(missileLocation, new Vector3(missileLaunchPosition.x, 0.1f, missileLaunchPosition.z), Quaternion.Euler(new Vector3(0, 0, 0)));
            // 미사일이 떨어질 위치에 생성될 파티클 저장 및 재생
            ParticleSystem missileLocationParticle = missileLocationGO.GetComponent<ParticleSystem>();
            missileLocationParticle.Play();

            //미사일 생성을 원격 프로시저 호출
            photonView.RPC("CreateMissleToAll", RpcTarget.All, point);
            // 미사일의 타겟 지점을 마우스로 저장한 지점으로 변경
            //missileScript.targetPosition = point;

            StartCoroutine(MissileLaunch(startSkillReloadDelay));
        }

        [PunRPC]
        private void CreateMissleToAll(Vector3 point)
        {
            // 미사일 오브젝트 생성 및 저장
            // 미사일이 생성될 위치를 항상 70정도의 높이의 저장한 지점으로 변경
            Vector3 missileLaunchPosition = new Vector3(point.x, 70, point.z);
            GameObject missile = Instantiate(missilePrefab, missileLaunchPosition, Quaternion.Euler(new Vector3(0,0,0)));
            missileScript = missile.GetComponent<Missile>();
            // 미사일의 타겟 지점을 마우스로 저장한 지점으로 변경
            missileScript.targetPosition = point;
            //missle 스크립트의 델리게이트에 미사일 파티클 삭제 스크립트 삽입
            missileScript.SetDestroyEvent(() => { Destroy(missileLocationGO); });
        }

        IEnumerator MissileLaunch(float cool)
        {

            // 스킬을 준비되지 않은 상태로 전환
            skillIsReady = false;
            // 스킬 쿨타임 텍스트 활성화
            missileCooldownTxt.enabled = true;

            // 현재 흐른 시간이 실제 쿨타임에 도달하지 못했을 경우
            while(cool < activeSkillReloadDelay)
            {
                // 흐른 시간 증가
                cool += Time.deltaTime;
                // 현재 흐른 시간의 내림수가 실제 쿨타임에 도달하지 못했을 경우
                if (Mathf.Floor(activeSkillReloadDelay - cool) >= 0)
                {
                    // 3부터 시작하여 1까지 소숫점 없이 내림수로 쿨타임 표시
                    missileCooldownTxt.text = (Mathf.Floor(activeSkillReloadDelay - cool) + 1).ToString();
                }
                // 현재 흐른 시간 값만큼 쿨타임 이미지를 채움
                missileCooldownImg.fillAmount = (cool / activeSkillReloadDelay);
                yield return new WaitForFixedUpdate();
            }

            // 스킬 쿨타임 텍스트 비활성화 및 스킬을 준비된 상태로 전환
            missileCooldownTxt.enabled = false;
            skillIsReady = true;
        }
    }
}
