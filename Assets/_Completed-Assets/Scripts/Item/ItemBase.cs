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
        // ȹ�� ������ ��� ȸ��
        if (!RotateStop)
            transform.Rotate(Vector3.up * Time.deltaTime * RotateSpeed);
    }

    public abstract void UseItem(TankItem tank);

    protected void OnTriggerEnter(Collider other)
    {
        // �÷��̾� �ݶ��̴��� ���� ��
        if (other.gameObject.CompareTag("Player"))
        {
            // �ش� ��ũ�� TankMovement ������Ʈ�� �ӵ����� ��ƼŬ�� ȣ�� �� ����
            TankItem tank = other.GetComponent<TankItem>();

            // �������� �� �� ���� �ʵ��� ��Ȱ��ȭ
            DisableItem();
            UseItem(tank);

            // �������� ���� �ð� �� ����
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
        // ������ �ݶ��̴��� �޽������� ��Ȱ��ȭ
        gameObject.GetComponent<BoxCollider>().enabled = false;
        gameObject.GetComponent<MeshRenderer>().enabled = false;

        // ������ ȸ�� ��Ȱ��ȭ
        this.rotateStop = true;
    }

}
