using Complete;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemBase : MonoBehaviourPun
{
    private bool rotateStop;
    public float rotateSpeed;
    public float itemDuration;

    public bool RotateStop { get => rotateStop; }
    public float RotateSpeed { get => rotateSpeed; }
    public float ItemDuration { get; }


    // Update is called once per frame
    void Update()
    {
        // 획득 전까지 계속 회전
        if (!RotateStop)
            transform.Rotate(Vector3.up * Time.deltaTime * RotateSpeed);
    }

    public abstract void UseItem(TankItem tank);

    protected void OnTriggerEnter(Collider other)
    {
        // 플레이어 콜라이더가 닿을 시
        if (other.gameObject.CompareTag("Player"))
        {
            // 해당 탱크의 TankMovement 컴포넌트와 속도증가 파티클을 호출 및 저장
            TankItem tank = other.GetComponent<TankItem>();

            // 아이템을 두 번 얻지 않도록 비활성화
            DisableItem();
            UseItem(tank);

            // 아이템을 일정 시간 후 삭제
            StartCoroutine(DelayedDestroy(gameObject, 3 + itemDuration));
        }
    }

    private IEnumerator DelayedDestroy(GameObject obj, float time)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            yield return new WaitForSeconds(3 + time);

            PhotonNetwork.Destroy(gameObject);
        }
    }

    public void DisableItem()
    {
        // 아이템 콜라이더와 메쉬렌더러 비활성화
        gameObject.GetComponent<BoxCollider>().enabled = false;
        gameObject.GetComponent<MeshRenderer>().enabled = false;

        // 아이템 회전 비활성화
        this.rotateStop = true;
    }

}
