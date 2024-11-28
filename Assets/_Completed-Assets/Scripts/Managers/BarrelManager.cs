using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Complete
{
    public class BarrelManager : MonoBehaviour
    {
        public Transform[] spawnPoints;             // 배럴이 생성될 위치
        public GameObject barrelPrefab;             // 배럴 프리팹

        private GameObject[] createdBarrels;        // 생성된 배럴 프리팹 저장용

        private void Start()
        {
            // 배열 선언
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
