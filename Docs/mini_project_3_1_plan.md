# 미니 프로젝트 3.1 타워 배치/정보 UI 가이드

## 이번 수정 핵심
- 타워 종류별 사거리 인스펙터 개별 조정 가능
  - `TowerSystemManager`의 타입별 설정에 `range` 추가
  - 아처/캐논/크로스보우/아이스/라이트닝/포이즌 각각 따로 조절 가능
- 설치된 타워 클릭 시 정보 UI 표시
  - 상단: `이름 + 현재 강화 상태` (예: `Archer +0`)
  - 좌측: 타워 이미지
  - 우측: 공격력 / 체력 / 사거리
- 강화 확인 UI(Yes / No) 추가 지원
  - 강화 버튼 클릭 -> 확인 패널 오픈
  - 표시 정보: 소모 재화, 강화 시 공격력 증가량, 성공 확률

## 수정된 기존 스크립트
- `Assets/Scripts/Tower/TowerSystemManager.cs`
  - 타입별 `range` 설정값 추가
  - `TryGetTowerRangeByType`, `TryGetUpgradePreview` 제공
  - 타워 생성 시 `TowerAutoAttack.SetAttackRange(range)` 적용
- `Assets/Scripts/Tower/TowerAutoAttack.cs`
  - `AttackRange` 조회 프로퍼티
  - 외부 설정용 `SetAttackRange(float)` 추가
- `Assets/Scripts/Tower/TowerInstance.cs`
  - 클릭 이벤트(`OnTowerClicked`) 추가
  - 표시용 이름(`DisplayName`) 프로퍼티 추가

## 추가된 UI 스크립트
- `Assets/Scripts/UI/TowerInfoPanelUI.cs`
  - 타워 클릭 시 정보 패널 갱신
  - 강화 확인 패널 열기
  - Yes/No 버튼 처리 (`OnConfirmUpgradeYes`, `OnConfirmUpgradeNo`)

## 유니티 연결 체크리스트
1. 타워 정보 패널 구성(Canvas)
   - `PanelRoot` (타워 정보 패널)
   - `HeaderText` (예: Archer +0)
   - `TowerImage` (Image 컴포넌트)
   - `StatText` (공격력/체력/사거리)
   - `UpgradeButton`
2. 강화 확인 패널 구성
   - `ConfirmPanel`
   - `ConfirmText`
   - Yes 버튼 -> `TowerInfoPanelUI.OnConfirmUpgradeYes`
   - No 버튼 -> `TowerInfoPanelUI.OnConfirmUpgradeNo`
3. `TowerInfoPanelUI`에 참조 연결
   - `towerSystemManager`, `panelRoot`, `headerText`, `towerImage`, `statText`, `upgradeButton`, `confirmPanel`, `confirmText`
4. `TowerSystemManager`에서 타워 타입별 사거리(range) 숫자 조정

## 기대 동작
- 설치된 타워 클릭 시 즉시 정보 UI 표시
- 강화 버튼 클릭 시 확인 안내창(Yes/No) 표시
- Yes 클릭 시 강화 시도 결과 반영(성공/실패 문구)
- 사거리 값은 타워 종류별로 인스펙터에서 즉시 조정 가능
