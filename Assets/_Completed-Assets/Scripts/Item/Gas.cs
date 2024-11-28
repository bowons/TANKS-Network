using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Complete
{
    public class Gas : ItemBase
    {
        public float bonusSpeed;

        public override void UseItem(TankItem tank)
        {
            tank.GetGas(this);
        }
    }
}
