using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Complete
{
    public class SelectSkillManager : MonoBehaviour
    {
        public Text[] skillTypeTxt;
        public AudioClip selectClip;
        private AudioSource audioSource;
        
        //ö��ź�� �̻��� ���� �Ŵ���, ������ ����, RPC ó���� ���� TankDataManager�� GameManager���� ������
        private void Start()
        {
            audioSource = gameObject.GetComponent<AudioSource>();

            TankDataManager.instance.currentSkill = Skill.None;
            for(int i=0; i < skillTypeTxt.Length; i++)
            {
                skillTypeTxt[i].enabled = false;
            }
        }

        public void SelectMissileSkill()
        {
            TankDataManager.instance.currentSkill = Skill.Missile;

            SetUI((int)Skill.Missile);

            SoundEffect();
        }

        public void SelectAPSkill()
        {
            TankDataManager.instance.currentSkill = Skill.AP;

            SetUI((int)Skill.AP);

            SoundEffect();
        }

        private void SetUI(int number)
        {
            for(int i=0; i < skillTypeTxt.Length; i++)
            {
                if(number == i)
                {
                    skillTypeTxt[number].enabled = true;
                }
                else if(number != i)
                {
                    skillTypeTxt[i].enabled = false;
                }
            }
        }

        private void SoundEffect()
        {
            audioSource.clip = selectClip;
            audioSource.Play();
        }
    }
}
