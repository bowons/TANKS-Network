using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 탱크의 종류 열거
public enum Tank
{
    Turbo, Boomber, Sniping, None
}

public enum Skill
{
    Missile, AP, None
}

namespace Complete
{
    public struct TankData
    {
        Tank tank;
        Skill skill;
    }

    public class TankDataManager : MonoBehaviour
    {
        private readonly string[] tankString = { "TurboTank", "BoomberTank", "SnipingTank", "None" };

        public TankData[] tankDatas;
        public static TankDataManager instance;                       // 이 컴포넌트의 인스턴스 선언
        private void Awake()
        {
            // 인스턴스 설정이 되어있지 않다면 인스턴스 설정
            if (instance == null)
                instance = this;
            else if (instance != null)
                return;
            // 씬 전환 이후에 이 게임 오브젝트가 제거되지 않도록 설정
            DontDestroyOnLoad(gameObject);
        }

        public Tank currentTank;

        public Skill currentSkill;

        public string getTankName(Tank tank)
        {
            return tankString[(int)tank];
        }
    }

    
}
