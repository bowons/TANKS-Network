using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Complete
{
    public class TankFactory : MonoBehaviour, IPunInstantiateMagicCallback
    {
        private TankShooting m_Shooting;
        private TankMovement m_Movement;
        private TankHealth m_Health;

        private GameObject m_Instance;

        protected TankShooting tankShooting { get => m_Shooting; set => m_Shooting = value; }
        protected TankMovement tankMovement { get => m_Movement; set => m_Movement = value; }
        protected TankHealth tankHealth { get => m_Health; set => m_Health = value; }


        // Start is called before the first frame update
        void Start()
        {
            m_Instance = this.gameObject;

            // 모든 탱크 컴포넌트 호출
            m_Shooting = m_Instance.GetComponent<TankShooting>();
            m_Movement = m_Instance.GetComponent<TankMovement>();
            m_Health = m_Instance.GetComponent<TankHealth>();
            m_Health.m_Slider.maxValue = tankHealth.m_StartingHealth;
            TankSetUp();
        }

        //탱크 셋업 함수
        protected virtual void TankSetUp() { }

        //포톤 네트워크 오브젝트 생성시 태그 설정
        public virtual void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            info.Sender.TagObject = this.gameObject;
        }
    }

}
