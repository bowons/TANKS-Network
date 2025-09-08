# TANKS-Network

Unity 공식 튜토리얼 게임 **TANKS**를 기반으로, 카메라워크와 이펙트 개선, 추가 에셋 활용을 통해 게임 요소를 확장했습니다.  
특히 Photon 네트워크 라이브러리를 적용하여 **싱글플레이 프로젝트를 멀티플레이 네트워크 게임으로 변환**했습니다.

해당 프로젝트는 2인 협업으로 진행되었으며, 저는  
- 팀원이 구현한 로컬 기능을 Photon 네트워크 환경에 맞게 수정·확장  
- 프로젝트 내 담당 파트 부분 집중적으로 리팩터링
을 담당했습니다.

스크립트는 `/Assets/_Completed-Assets/Scripts` 경로에 있으며, Photon 적용을 위해 `Resources` 폴더에 추가 에셋을 사용했습니다.

---
## TANKS! 2P 동영상

---

## 주요 구현 기능

- **객체에 PhotonView 부여**
  - 네트워크 동작을 위해 Object와 Prefab에 PhotonView를 부여
  - 고정 배치된 탱크와 기름통에는 고정된 PhotonView ID를 사용
  - Instantiate로 생성되는 아이템은 동적으로 PhotonView가 자동 할당되도록 설정

- **탱크 시스템 (TankFactory)**
  - TankFactory를 상속받아 탱크 특성을 설정
  - `BoomberTank`, `SniperTank`, `TurboTank` 3가지 탱크를 생성 시 기본 속성을 부여한 후 Instantiate 처리

- **스킬(Skill) 동기화**
  - 팀원이 구현한 스킬 동작에 PunRPC를 적용
  - 두 플레이어 간 스킬 사용이 네트워크 상에서 동일하게 동기화되도록 구현

- **체력(Health) 처리**
  - 체력 변화는 반드시 동기화가 필요하기 때문에 마스터 클라이언트에게 위임
  - 두 플레이어가 동일한 결과를 보장하도록 서버 역할 수행

- **슈팅(Shooting) 및 피격 처리**
  - 탱크 발사 시 Shell 생성과 이펙트는 각 클라이언트에서 처리
  - 실제 피격 판정은 마스터 클라이언트에 위임하여 결과 일관성을 확보

- **아이템 시스템 리팩터링**
  - 각 아이템이 직접 탱크에 효과를 주던 방식을 개선
  - `TankItem.cs` 클래스에서 모든 아이템 효과를 통합 관리하도록 리팩터링

- **로비(Lobby) 구현**
  - 네트워크 게임을 위한 대기방과 로비 구현
  - 플레이어 목록 표시, 방 생성/입장/퇴장 기능 추가

- **매니저(Manager) 클래스 네트워크 적용**
  - GameManager, TankManager, ItemManager, BarrelManager에 네트워크 동작 적용
  - 특히 Barrel 설치물이 의도대로 동작하지 않아 디버깅 과정에서 많은 시행착오를 겪음

---

## 기술 스택

- Engine: Unity (2021.x)
- Networking: Photon PUN 2
- Language: C#
- Version Control: PlasticSCM

---
