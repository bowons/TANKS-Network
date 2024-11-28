using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Complete
{
    public class Whistle : ItemBase
    {
        public float highSpeedFireDelay;

        public override void UseItem(TankItem tank)
        {
            tank.GetWhistle(this);
        }
    }
}
