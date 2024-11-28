using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Complete
{
    public class BarrelManager : MonoBehaviour
    {
        public Transform[] spawnPoints;             // �跲�� ������ ��ġ
        public GameObject barrelPrefab;             // �跲 ������

        private GameObject[] createdBarrels;        // ������ �跲 ������ �����

        private void Start()
        {
            // �迭 ����
            createdBarrels = new GameObject[spawnPoints.Length];
        }

        public void SpawnBarrels()
        {
            
            for(int i=0; i < spawnPoints.Length; i++)
            {
                createdBarrels[i] = PhotonNetwork.Instantiate(barrelPrefab.name, spawnPoints[i].position, spawnPoints[i].rotation);
            }
        }

        public void DestroyBarrels()
        {
            for(int i=0; i < createdBarrels.Length; i++)
            {
                if(createdBarrels[i] == null)
                {
                    continue;
                }
                else
                {
                    PhotonNetwork.Destroy(createdBarrels[i]);
                }
            }
        }
    }
}
