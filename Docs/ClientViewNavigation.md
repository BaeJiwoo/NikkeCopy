# 클라이언트 View 내비게이션

## 결정

클라이언트의 일반 화면 전환은 Scene 전환이 아니라 단일 Canvas 내부 View GameObject의 활성 상태로 관리한다. View 간 허용 경로는 `ViewGraph` ScriptableObject에 방향 그래프로 기록한다.

```text
Button / Controller
        │ NavigationKey
        ▼
ViewNavigator ── (Current View + NavigationKey) ──▶ ViewGraph
        │                                             │
        └──────────── 대상 View 활성화 ◀──────────────┘
```

현재 그래프:

```text
Auth ── ShowMain ──▶ Main
Main ── ShowAuth ──▶ Auth
```

## 구성요소

- `ViewKey`: View Node의 고유 식별자
- `NavigationKey`: Edge를 선택하는 고유 식별자
- `ViewGraph`: 시작 Node, Node 목록, 방향 Edge 목록을 저장하는 에셋
- `ViewPrefab`: 프리팹이 담당하는 `ViewKey`를 표시하는 루트 컴포넌트
- `ViewNavigator`: 현재 Node를 관리하고 그래프의 Edge를 실행
- `ViewNavigationButton`: 버튼 클릭 시 할당된 `NavigationKey`를 전달
- `View Binding`: Scene의 `ViewKey`와 Canvas 내부 GameObject를 연결

그래프 에셋은 `Assets/Settings/ClientViewGraph.asset`에 둔다. 각 Node는 View 프리팹을 참조하며, `Sync Canvas`가 프리팹 인스턴스와 Scene의 `ViewNavigator` 바인딩을 맞춘다.

현재 View 프리팹은 다음 위치에서 관리한다.

```text
Assets/GameResources/Prefabs/UI/Views/
├─ AuthView.prefab
└─ MainView.prefab
```

## View 연결 방법

1. `ViewKey` enum에 새 View 식별자를 추가한다.
2. `NavigationKey` enum에 사용자 동작을 나타내는 Edge 키를 추가한다.
3. View 루트에 `ViewPrefab`을 붙이고 해당 `ViewKey`를 지정한 뒤 프리팹으로 저장한다.
4. `Assets/Settings/ClientViewGraph.asset`을 선택한다.
5. Inspector의 `Views` 목록에 Key와 View 프리팹을 등록한다.
6. `Transitions` 목록에 `From`, `Navigation`, `To` 이동 규칙을 등록한다.
7. Inspector의 `Sync Canvas`를 눌러 Canvas의 `ViewRoot`와 `ViewNavigator` 바인딩을 동기화한다.
8. 일반 이동 버튼에는 `ViewNavigationButton`을 붙이고 `Button Key`를 지정한다.
9. 인증 성공처럼 조건부 이동은 Controller에서 `ViewNavigator.Navigate(key)`를 호출한다.

버튼은 대상 GameObject나 View 이름을 직접 참조하지 않는다.

## 검증 규칙

`ViewGraph` Inspector는 다음 오류를 표시한다.

- `None` Node 또는 Edge Key
- 중복 View Node
- View 프리팹 누락 또는 중복 사용
- 프리팹 루트의 `ViewPrefab.Key`와 Node Key 불일치
- 동일 출발 View에서 중복된 `NavigationKey`
- 존재하지 않는 Node를 참조하는 Edge
- 시작 View 누락
- 시작 View에서 도달할 수 없는 View

런타임에도 그래프와 Scene 바인딩을 재검증하며, 현재 View에서 허용되지 않은 Edge 요청은 이동하지 않고 오류를 기록한다.

## Inspector 편집

별도의 그래프 창은 사용하지 않는다. `ClientViewGraph.asset` Inspector에서 시작 View, View 프리팹 목록과 방향 전환 규칙을 관리한다. `Validate`는 저장 데이터의 오류를 확인하고, `Sync Canvas`는 현재 열린 Scene에 프리팹을 배치해 그래프와 Scene 바인딩의 차이를 해소한다.
