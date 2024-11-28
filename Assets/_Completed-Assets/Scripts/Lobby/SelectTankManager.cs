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
        private string[] mapType = { "DESERT","CITY","TOWN" }; // 맵 종류 설정
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
        //맵 선택 RPC 함수
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
        
        //레디 버튼 클릭시 액션
        public void ReadyOnButton()
        {// 탱크 미선택시
            if (TankDataManager.instance.currentTank == Tank.None)
            {
                StartCoroutine(SelectPlz());
                return;
            }//스킬 미선택시
            else if(TankDataManager.instance.currentSkill == Skill.None)
            {
                StartCoroutine(SelectSkillPlz());
                return;
            }

            GetReady();
        }
        // 사용할 탱크 모델 생성 함수
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
        // UI 설정
        private void SetUI()
        {
            tankTypeTxt.text = tankType;
        }
        //사운드 설정
        private void SoundEffect()
        {
            audioSource.clip = selectClip;
            audioSource.Play();
        }

        private void GetReady()
        {
            progressTxt.text = "WAITING THE OTHER PLAYER...";
            //마스터 클라이언트가 아니라면 Ready 정보를 송신한다.
            if (!PhotonNetwork.IsMasterClient) {
                photonView.RPC("SendClientReadyStat", RpcTarget.MasterClient);
            }
            else
            {//마스터 클라이언트의 경우 Ready 요청 수행
                SendClientReadyStat();
            }
            ReadyFlag = false;
        }

        [PunRPC]
        private void SendClientReadyStat()
        {// Ready Count를 증가시키고 모두 준비되면 게임 시작
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
