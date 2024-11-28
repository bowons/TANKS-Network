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
        
        //철갑탄과 미사일 선택 매니저, 사용되지 않음, RPC 처리로 인해 TankDataManager와 GameManager에서 설정함
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
