using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Complete
{
    public class ItemManager : MonoBehaviourPun
    {
        public GameObject[] items;                      // 생성할 아이템 프리팹
        public GameObject[] itemsSilhouette;            // 아이템을 생성하기 전 보이게 될 아이템 실루엣 오브젝트
        public Transform[] playerTransform;             // 플레이어들의 위치 정보
        private GameObject[] itemsGO;                   // 생성된 아이템들을 저장하는 배열
        private GameObject[] itemsSilGO;                // 생성된 아이템 실루엣들을 저장하는 배열

        public float maxDistance = 5f;                  // 플레이어의 위치에서 아이템이 배치될 수 있는 최대 반경

        public float timeBetSpawnMax = 7f;              // 아이템이 생성되기까지 걸리는 최대 시간
        public float timeBetSpawnMin = 2f;              // 아이템이 생성되기까지 걸리는 최소 시간
        private float timeBetSpawn;                     // 아이템의 생성 간격
        public float timeSilhouetteSpawn = 3f;          // 아이템의 실루엣이 보이는 시간
        private float timeDelay;                        // 지난 시간 값

        private Vector3 spawnPosition;                  // 아이템과 실루엣이 생성될 위치 값
        private bool isSpawnSilhouette = false;                 // 실루엣이 생성되었는지 여부
        private GameObject selectedItem;                // 생성될 아이템 오브젝트

        public float itemLifeTime = 7f;                 // 아이템이 필드에 남아있는 시간

        private int playerNumber;                       // 플레이어의 번호를 저장하기 위한 변수
        private Vector3 lastSpawnPosition;              // 마지막으로 아이템이 생성된 위치

        private void Start()
        {
            // 생성된 아이템과 실루엣 배열의 최대값을 설정
            itemsGO = new GameObject[15];
            itemsSilGO = new GameObject[itemsGO.Length];
            // 아이템 매니저를 리셋
            if (PhotonNetwork.IsMasterClient)
                ResetManager();
        }

        private void Update()
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            foreach (Transform transform in playerTransform)
                if (transform == null)
                    return;

            // 지난 시간이 생성 시간을 넘지 못했다면 시간 증가
            if (timeBetSpawn + timeSilhouetteSpawn > timeDelay)
            {
                timeDelay += Time.deltaTime;
            }

            // 현재 시점이 실루엣이 생성될 시간 만큼 도달했을 때
            if (timeDelay >= timeBetSpawn - timeSilhouetteSpawn && !isSpawnSilhouette)
            {
                // 실루엣을 생성
                SilhouetteSpawn();
            }

            // 현재 시점이 생성 간격만큼 지나고 플레이어 위치 정보가 있는지 확인
            if (timeDelay >= timeBetSpawn && playerTransform != null)
            {
                // 생성 간격을 최대/최소 시간의 값의 범위 중 임의의 수로 지정
                timeBetSpawn = Random.Range(timeBetSpawnMin, timeBetSpawnMax);
                // 아이템 생성 함수
                Spawn();

                // 지난 시간을 초기화
                timeDelay = 0;

                // 실루엣 생성 여부를 초기화
                isSpawnSilhouette = false;
            }
        }

        private void SilhouetteSpawn()
        {
            // 임의로 선택될 아이템의 종류 값
            int itemNumber;
            // 실루엣 생성 여부 참
            isSpawnSilhouette = true;

            // 플레이어의 수 만큼 랜덤한 임의의 수 저장
            playerNumber = Random.Range(1, playerTransform.Length);
            // 임의로 정해진 플레이어 근처에서 내비메시 위의 랜덤 위치 가져오기
            spawnPosition = GetRandomPointOnNavMesh(playerTransform[playerNumber].position, maxDistance);
            // 아이템의 위치를 지면에서 띄움
            spawnPosition += Vector3.up * 1f;

            // 임의로 아이템 종류 선택
            itemNumber = Random.Range(0, items.Length);
            // 임의로 선택된 아이템을 저장
            selectedItem = items[itemNumber];
            // 해당 아이템의 실루엣을 정해진 랜덤 위치에 생성
            GameObject itemShilhouette = PhotonNetwork.Instantiate(itemsSilhouette[itemNumber].name, spawnPosition, Quaternion.identity);

            // 생성된 아이템 실루엣을 배열에 저장
            SilhouetteStorage(itemShilhouette);

            // 생성된 아이템 실루엣을 지정된 시간 후 제거
            StartCoroutine(NetworkDestroy(itemShilhouette, timeSilhouetteSpawn));
        }

        IEnumerator NetworkDestroy(GameObject obj, float time)
        {
            yield return new WaitForSeconds(time);
            PhotonNetwork.Destroy(obj);
        }

        private void Spawn()
        {
            // 해당 아이템을 정해진 랜덤 위치에 생성
            GameObject item = PhotonNetwork.Instantiate(selectedItem.name, spawnPosition, Quaternion.identity);

            // 생성된 아이템을 배열에 저장
            ItemStorage(item);

            // 생성된 아이템을 지정된 시간 후 제거
            NetworkDestroy(item, itemLifeTime);
        }

        // 생성된 아이템을 배열에 저장하는 함수
        private void ItemStorage(GameObject item)
        {
            // 아이템 배열의 수만큼 실행
            for(int i=0; i < itemsGO.Length; i++)
            {
                // 해당 번호의 배열에 아이템 오브젝트가 없다면
                if(itemsGO[i] == null)
                {
                    // 아이템 오브젝트 저장
                    itemsGO[i] = item;
                    // 함수 종료
                    return;
                }
                // 해당 번호의 배열에 아이템 오브젝트가 있다면
                else if(itemsGO[i] != null)
                {
                    // 다음 배열
                    continue;
                }
            }
        }

        // 생성된 아이템 실루엣을 배열에 저장하는 함수
        private void SilhouetteStorage(GameObject item)
        {
            // 아이템 배열의 수만큼 실행
            for (int i = 0; i < itemsSilGO.Length; i++)
            {
                // 해당 번호의 배열에 아이템 오브젝트가 없다면
                if (itemsSilGO[i] == null)
                {
                    // 아이템 오브젝트 저장
                    itemsSilGO[i] = item;
                    // 함수 종료
                    return;
                }
                // 해당 번호의 배열에 아이템 오브젝트가 있다면
                else if (itemsSilGO[i] != null)
                {
                    // 다음 배열
                    continue;
                }
            }
        }

        private Vector3 GetRandomPointOnNavMesh(Vector3 center, float distance)
        {
            // center를 중심으로 반지름이 distance인 구 안에서 랜덤한 위치 하나를 지정
            Vector3 randomPos = Random.insideUnitSphere * distance + center;

            // 내비메시 샘플링의 결과 정보를 저장하는 변수
            NavMeshHit hit;
            // distance 반경 안에서 randomPos에 가장 가까운 내비메시 위의 한 점을 찾음
            NavMesh.SamplePosition(randomPos, out hit, distance, NavMesh.AllAreas);

            // 마지막 생성 위치가 null이 아닌지 확인
            if(lastSpawnPosition != null)
            {
                // 마지막 생성 위치가 현재 지정된 점과 같은지 확인
                if(lastSpawnPosition == hit.position)
                {
                    // 함수 재호출
                    GetRandomPointOnNavMesh(playerTransform[playerNumber].position, maxDistance);
                }
            }

            // 마지막으로 아이템이 생성된 위치를 저장
            lastSpawnPosition = hit.position;

            // 찾은 점 반환
            return hit.position;
        }

        public void ResetManager()
        {
            // 생성 간격 초기화
            timeBetSpawn = Random.Range(timeBetSpawnMin, timeBetSpawnMax);

            // 생성 위치 초기화
            spawnPosition = Vector3.zero;

            // 실루엣 생성 여부 초기화
            isSpawnSilhouette = false;

            // 지난 시간을 초기화
            timeDelay = 0;
        }

        public void DestroyItmes()
        {
            // 배열에 존재하는 아이템 모두 제거
            for(int i=0; i < itemsGO.Length; i++)
            {
                if(itemsGO[i] == null)
                {
                    continue;
                }
                else
                {
                    PhotonNetwork.Destroy(itemsGO[i]);
                }
            }

            // 배열에 존재하는 아이템 실루엣 모두 제거
            for (int i = 0; i < itemsSilGO.Length; i++)
            {
                if (itemsSilGO[i] == null)
                {
                    continue;
                }
                else
                {
                    PhotonNetwork.Destroy(itemsSilGO[i]);
                }
            }
        }
    }
}
