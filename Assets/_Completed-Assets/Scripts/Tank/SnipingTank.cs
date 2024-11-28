using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Complete
{
    public class SnipingTank : TankFactory
    {
        public float s_Speed;                       
        public float s_MinLaunchForce;        
        public float s_MaxLaunchForce;        
        public float s_ShellSpeed;

        //탱크 속도 조정
        protected override void TankSetUp()
        {
            tankMovement.m_Speed = s_Speed;
            tankShooting.m_ShellSpeed = s_ShellSpeed;
            SnipingTankLaunchForceSet();
        }

        //발사 속도 조정
        public void SnipingTankLaunchForceSet()
        {
            tankShooting.m_MinLaunchForce = s_MinLaunchForce;
            tankShooting.m_MaxLaunchForce = s_MaxLaunchForce;
            tankShooting.m_AimSlider.minValue = s_MinLaunchForce;
            tankShooting.m_AimSlider.maxValue = s_MaxLaunchForce;
        }
    }
}
