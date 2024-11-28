using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;

namespace Complete
{
    public class SelectTankManager : MonoBehaviourPun
    {
        public GameObject[] tankModels;
        public Transform tankModelSpawnPos;
        public Image mapImage;
        public Text mapText;
        public Text tankTypeTxt;
        public Button readyButton;
        public Text progressTxt;
        public AudioClip selectClip;
        private AudioSource audioSource;
        public Sprite[] mapSprite;

        private GameObject currentTankModel;
        private string tankType;
        private string[] mapType = { "DESERT","CITY","TOWN" }; // �� ���� ����
        private string selectedMap = "Stage0";

        private int readyCount = 0;
        private bool readyFlag = false;

        private bool ReadyFlag 
        { 
            get { return readyFlag; } 
            set 
            { 
                readyFlag = value;
                readyButton.interactable = readyFlag;
            } 
        }


        private void Start()
        {
            audioSource = gameObject.GetComponent<AudioSource>();

            TankDataManager.instance.currentTank = Tank.None;
            //TankDataManager.instance.tankDatas = new TankData[PhotonNetwork.CurrentRoom.PlayerCount];
            tankType = "None";
            progressTxt.text = "CHOOSE YOUR TANK!";
            if(PhotonNetwork.IsMasterClient)
            {
                int index = Random.Range(0,3);
                photonView.RPC("MapSelect", RpcTarget.All, index);
            }
        }
        //�� ���� RPC �Լ�
        [PunRPC]
        private void MapSelect(int index)
        {
            mapImage.sprite = mapSprite[index];
            mapText.text = mapType[index];
            selectedMap = "Stage" + index;
        }

        public void SelectTurboTank()
        {
            TankDataManager.instance.currentTank = Tank.Turbo;
            ModelCreate(tankModels[0]);

            tankType = "Turbo Tank";
            SetUI();

            SoundEffect();
        }
        public void SelectBoomberTank()
        {
            TankDataManager.instance.currentTank = Tank.Boomber;
            ModelCreate(tankModels[1]);

            tankType = "Boomber Tank";
            SetUI();

            SoundEffect();
        }
        public void SelectSnipingTank()
        {
            TankDataManager.instance.currentTank = Tank.Sniping;
            ModelCreate(tankModels[2]);

            tankType = "Sniping Tank";
            SetUI();

            SoundEffect();
        }
        
        //���� ��ư Ŭ���� �׼�
        public void ReadyOnButton()
        {// ��ũ �̼��ý�
            if (TankDataManager.instance.currentTank == Tank.None)
            {
                StartCoroutine(SelectPlz());
                return;
            }//��ų �̼��ý�
            else if(TankDataManager.instance.currentSkill == Skill.None)
            {
                StartCoroutine(SelectSkillPlz());
                return;
            }

            GetReady();
        }
        // ����� ��ũ �� ���� �Լ�
        private void ModelCreate(GameObject model)
        {
            if (currentTankModel == model)
            {
                return;
            }
            else if (currentTankModel != model)
            {
                Destroy(currentTankModel);

                currentTankModel = Instantiate(model, tankModelSpawnPos.position, tankModelSpawnPos.rotation);
                currentTankModel.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
            }
            else if (currentTankModel == null)
            {
                currentTankModel = Instantiate(model, tankModelSpawnPos.position, tankModelSpawnPos.rotation);
                currentTankModel.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
            }
        }
        // UI ����
        private void SetUI()
        {
            tankTypeTxt.text = tankType;
        }
        //���� ����
        private void SoundEffect()
        {
            audioSource.clip = selectClip;
            audioSource.Play();
        }

        private void GetReady()
        {
            progressTxt.text = "WAITING THE OTHER PLAYER...";
            //������ Ŭ���̾�Ʈ�� �ƴ϶�� Ready ������ �۽��Ѵ�.
            if (!PhotonNetwork.IsMasterClient) {
                photonView.RPC("SendClientReadyStat", RpcTarget.MasterClient);
            }
            else
            {//������ Ŭ���̾�Ʈ�� ��� Ready ��û ����
                SendClientReadyStat();
            }
            ReadyFlag = false;
        }

        [PunRPC]
        private void SendClientReadyStat()
        {// Ready Count�� ������Ű�� ��� �غ�Ǹ� ���� ����
            readyCount++;
            if(readyCount == PhotonNetwork.CurrentRoom.PlayerCount)
            {
                photonView.RPC("StartGameProgress", RpcTarget.All);
            }
        }

        [PunRPC]
        public void StartGameProgress()
        {
            StartCoroutine(GameProgress());
        }

        IEnumerator GameProgress()
        {
            progressTxt.text = "THE GAME WILL START SOON!";

            yield return new WaitForSeconds(5);

            if (PhotonNetwork.IsMasterClient)
                PhotonNetwork.LoadLevel(selectedMap);

        }

        IEnumerator SelectPlz()
        {
            progressTxt.text = "PLEASE SELECT A TANK!!!!!!!!!";

            yield return new WaitForSeconds(2);

            progressTxt.text = "CHOOSE YOUR TANK!";
        }

        IEnumerator SelectSkillPlz()
        {
            progressTxt.text = "PLEASE SELECT YOUR ACTIVE SKILL!!!!";

            yield return new WaitForSeconds(2);

            progressTxt.text = "CHOOSE YOUR TANK!";
        }
    }
}
