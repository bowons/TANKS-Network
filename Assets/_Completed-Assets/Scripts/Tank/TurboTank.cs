using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Complete
{
    public class TurboTank : TankFactory
    {

        public float t_Speed = 15f;         // 터보 탱크 클래스의 고유 이동속도

        protected override void TankSetUp()
        {
            tankMovement.m_Speed = t_Speed;
            tankShooting.m_IsTurboTank = true;
        }

        //포톤 뷰 인스턴스시 실행
        public override void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            base.OnPhotonInstantiate(info);

        }
    }
}
