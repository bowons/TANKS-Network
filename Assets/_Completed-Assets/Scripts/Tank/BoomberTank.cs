using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Complete
{
    public class BoomberTank : TankFactory
    {
        public float b_Speed = 10f;         // �չ� ��ũ�� ���� �̵��ӵ�

        protected override void TankSetUp()
        {
            tankMovement.m_Speed = b_Speed;
            tankShooting.m_IsBoomberTank = true;
        }

        public override void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            base.OnPhotonInstantiate(info);
        }
    }
}
