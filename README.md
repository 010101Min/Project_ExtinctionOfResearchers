# Party Hard 3D

## 소개
플레이어는 다양한 아이템과 능력을 활용하여 NPC의 추적을 피하며 목표를 달성해야 하며, NPC의 행동 패턴과 상황 변화에 따라 게임 흐름이 달라지도록 설계하였습니다.

## 플레이 화면

- 플레이 영상: [유튜브](https://youtu.be/X6PxECdRqh8?si=W4z_HgnB1mdqgBfz)
- 실행파일 다운 링크: [구글 드라이브](https://drive.google.com/file/d/1z_6DDCl_-gRo-4nk79rfOuIJ_dUDkPHs/view?usp=sharing)

| 시민 NPC 상태 | 경찰에게 추격당하는 플레이어 |
|---|---|
| <img src="https://github.com/user-attachments/assets/52fcb14d-6f62-4680-b957-1a04c1e0e996" width="400"> | <img src="https://github.com/user-attachments/assets/37175dad-fe24-4988-b63f-4c0855691442" width="400"> |

| 플레이어 상호작용(공격) | 아이템 사용(폭탄) |
|---|---|
| <img src="https://github.com/user-attachments/assets/59f6c94d-e884-4fae-a868-5fd6581ce74d" width="400"> | <img src="https://github.com/user-attachments/assets/27775ecc-b23c-4caf-a097-694d7ac36dd5" width="400"> |

| 사건 발생 및 반응 |
|---|
| <img src="https://github.com/user-attachments/assets/424dcbda-de38-4094-9753-995558b0f426" width="700"> |
|1. 시신을 발견하지 못해 평화로운 NPC <br>2. 시신을 발견하고 용의자를 빨간 선으로 의심하는 NPC <br>3. 시신을 신고하기 위해 전화기로 달려가는 NPC|


## 주요 기능
- FSM 기반 NPC 상태 전환 구조 설계
- NavMesh 기반 NPC 이동 및 추적 시스템 구현
- 플레이어/NPC/오브젝트 간 상호작용 시스템 구현
- 플레이 방식 및 클리어 시간 기반 점수 시스템 구현

## 구조 설명

NPCController
- FSM 기반 시민 NPC 상태 및 행동 패턴 관리
- 상태별 Coroutine 실행 구조 구성

PoliceController, EngineerController, ParamedicController
- 역할에 따른 개별 행동 패턴 구현
- 상황 변화에 따른 상태 전환 수행

PlayerController
- 플레이어 이동 및 능력 사용 기능 구현
- 레이저 발사, NPC 도발 및 운반 기능 구현
- 지름길, 폭탄, 소화기, NPC 방출 문 등 아이템 사용 기능 구현

## 설명
Party Hard를 3D 환경으로 재구성한 잠입형 암살 게임입니다.

FSM 기반으로 시민 NPC의 상태와 행동 패턴을 관리하였으며,
상황 변화에 따라 NPC마다 서로 다른 행동을 수행하도록 구현하였습니다.

또한 NavMesh 기반 이동 시스템과 플레이 방식 기반 점수 시스템을 구현하였습니다.

## 개발 중 어려웠던 점

### 플레이어의 달리기 구현
- 스태미너 시스템 구현을 위해 달리기 및 스태미너 회복 기능을 Coroutine 기반으로 관리하였습니다.

### NPC 행동패턴 구현
- NPC의 상태별 행동을 Coroutine으로 관리하고, 상태 변경 시에만 Coroutine을 교체하도록 구성하여 불필요한 연산과 행동 끊김을 최소화하였습니다.
- Update에서 NPC 시야를 지속적으로 검사하고, 플레이어의 이상 행동이 감지될 경우 현재 행동을 중단한 뒤 새로운 상태에 맞는 행동을 수행하도록 구현하였습니다.

### 예외처리
- NPC가 벽 너머의 상황을 감지하는 문제
- 시민 NPC의 신고 타이밍과 플레이어 행동이 충돌하는 상황
- 여러 NPC가 동시에 신고를 시도하는 상황

### 레이어 분리
- 최적화 및 충돌 감지 정확도 향상을 위해 게임 오브젝트의 레이어를 분리하였습니다.
- 이를 통해 NPC 시야 판정, 아이템 감지, 벽 너머 탐지 오류 등을 개선하였습니다.

### 사용 에셋

- 배경 : 3D Scifi Kit Starter Kit (Creepy Cat)
- 인물 모델링 : SIMPLE modular human (255 pixel studios)
- 스카이박스 : 3 Skyboxes (Bright Shining Star)
- 차 모델링 : 4 Low Poly Toon Cars (Space Helmet Studio)
- 포탈 이펙트 : Cartoon FX Remaster Free (Jean Moreno)
- 기타 각종 이펙트 : Simple Particles FX : Toon Effects (Indian Ocean Assets)
- UI : Sci-fi GUI skin (3d.rina)
- BGM : 8Bit Music - 062022 (GWriterStudio)
- SFX : 8 Bits Elements (Game Sound Solutions)
- 폰트 : 둥근모꼴
