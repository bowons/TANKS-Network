using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Complete
{
    public class TankItem : MonoBehaviour
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
        }

        //가스 아이템 사용
        public void GetGas(Gas gas)
        {
            if(!tankMovement.m_IsSpeedUp)
                StartCoroutine(tankMovement.SpeedUp(gas.bonusSpeed, gas.ItemDuration));
        }

        //호루라기 아이템 사용
        public void GetWhistle(Whistle whistle)
        {
            if(tankShooting.photonView.IsMine)
            {
                if (!tankShooting.m_IsFireSpeedUp)
                    StartCoroutine(tankShooting.FireSpeedUp(whistle.highSpeedFireDelay, whistle.ItemDuration));
            }
        }

        //스패너 아이템 사용
        public void GetSpanner(Spanner spanner)
        {
            StartCoroutine(m_Health.Heal(spanner.recoveryAmount, spanner.ItemDuration));
        }

    }

}
