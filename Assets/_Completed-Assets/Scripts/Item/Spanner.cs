using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Complete
{
    public class Spanner : ItemBase
    {
        public float recoveryAmount;

        public override void UseItem(TankItem tank)
        {
            tank.GetSpanner(this);
        }
    }
}
