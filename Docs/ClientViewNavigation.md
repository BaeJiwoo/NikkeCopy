# 클라이언트 View 관리

## 현재 결정

기존 `ViewGraph`, `ViewKey`, `NavigationKey`, `ViewNavigator`, `ViewNavigationButton`, `ViewPrefab` 기반 시스템은 삭제했다. View 전환 관계를 ScriptableObject 그래프나 Inspector 전환 목록으로 정의하지 않는다.

새 구조는 `BaseView`와 `ViewManager`가 현재 View와 직전 View만 관리한다.

```text
호출 코드
   │ PushAsync<T>()
   ▼
ViewManager
   ├─ 현재 View ExitAsync()
   ├─ 전환 중 입력 차단
   ├─ 선행 비동기 작업 실행
   └─ 새 BaseView 로드 및 EnterAsync()
```

## 구성요소

### BaseView

위치: `Assets/Scripts/MVC/Views/BaseView.cs`

- 모든 관리 대상 View의 기반 클래스
- `Reuse Mode`: 인스턴스를 캐시할지, Pop 이후 해제하고 다음 진입 때 다시 생성할지 선택
- `Lifecycle State`: `None`, `Created`, `Entered`, `Exited`, `Released` 상태 제공
- `BindViewModel()`: 생성 단계에서 한 번 실행되는 데이터 연결 지점
- `OnCreatedAsync()`: 인스턴스 초기화
- `OnEnteringAsync()`: 화면에 진입할 때마다 실행
- `OnExitingAsync()`: 화면에서 이탈할 때마다 실행
- `OnReleasedAsync()`: 인스턴스가 더 이상 사용되지 않기 전 정리

라이프사이클 호출 순서는 ViewManager가 관리하며 View를 직접 생성하든 캐시된 인스턴스를 재사용하든 동일하다.

```text
최초 생성: Created → Entered
다른 화면 이동: Entered → Exited
재사용: Exited → Entered
재생성 정책 종료: Exited → Released → Destroy
```

`Reuse`는 `Created`와 `Released`가 인스턴스 수명 동안 한 번씩 실행되고 진입·이탈은 반복된다. `Recreate`는 직전 화면 기록에서 제외되거나 뒤로 이동할 때 `Released`와 Destroy가 실행되며 다음 이동 때 새 인스턴스로 `Created`부터 다시 시작한다.

### ViewManager

위치: `Assets/Scripts/MVC/Views/ViewManager.cs`

- `ViewManager.Instance`로 접근하는 단일 관리자
- 현재 View와 직전 View를 각각 하나씩 관리
- 전환 중 `raycastBlocker`를 활성화해 중복 입력을 차단
- `PushAsync<T>()`에 전달된 비동기 선행 작업을 순서대로 실행
- UniTask를 사용해 View 생명주기를 비동기로 처리

### View 프리팹

위치: `Assets/GameResources/Prefabs/UI/Views/`

현재 다음 프리팹이 존재한다.

- `AuthView.prefab`
- `MainView.prefab`
- `InventoryView.prefab`
- `NikkeManagementView.prefab`
- `RecruitmentView.prefab`
- `SquadView.prefab`

각 프리팹 루트에는 같은 이름의 `BaseView` 파생 컴포넌트가 연결되어 있다. 개별 View 클래스는 이동할 목적지를 알지 않는다.

## ViewManager 설정

Scene에 `ViewManager`를 하나만 배치하고 Inspector에서 다음을 설정한다.

- `View Root`: 런타임 View 인스턴스가 생성될 부모 Transform
- `Initial View`: Scene에 미리 배치된 최초 `BaseView`
- `View Prefabs`: 이동 가능한 `BaseView` 파생 프리팹 목록
- `Raycast Blocker`: 전환 및 선행 작업 중 입력을 막는 전체 화면 GameObject

`Reuse` View는 동일한 타입을 처음 요청할 때 한 번 생성한 뒤 캐시한다. `Recreate` View는 더 이상 현재 또는 직전 View가 아니면 해제하며 다음 요청에서 다시 생성한다. 이전 기록은 한 단계만 유지한다.

## 버튼 이동 설정

1. `ViewManager.View Prefabs`에 이동 가능한 View 프리팹 컴포넌트를 모두 등록한다.
2. 일반 이동 버튼에 `ViewPushButton`을 추가한다.
3. `Target View`에 목록에 등록한 프리팹의 `BaseView` 파생 컴포넌트를 직접 할당한다.
4. Button의 별도 `On Click()` 이벤트는 설정하지 않는다.

```text
ViewManager.View Prefabs
├─ MainView
├─ InventoryView
├─ NikkeManagementView
├─ RecruitmentView
└─ SquadView

Inventory Button
└─ ViewPushButton.Target View = InventoryView prefab
```

버튼을 누르면 `ViewPushButton`이 `ViewManager.PushAsync(targetView)`를 호출한다. 대상 View 타입이 ViewManager 목록에 없으면 이동하지 않고 오류를 기록한다.

`Target View`가 현재 View와 같은 타입이면 Button의 `Interactable`이 자동으로 비활성화된다. View가 변경되면 `CurrentViewChanged` 이벤트를 통해 활성 `ViewPushButton`이 상태를 다시 계산한다. 대상이 할당되지 않은 버튼도 비활성화된다.

## 앞으로 이동

```csharp
bool moved = await ViewManager.Instance.PushAsync<InventoryView>();
```

현재 View의 `ExitAsync()`가 끝난 뒤 대상 View를 활성화하고 `EnterAsync()`를 실행한다. 기존 현재 View는 직전 View가 되며, 그보다 오래된 이동 기록은 유지하지 않는다. 이미 현재 화면과 같은 타입을 요청하면 이동하지 않는다.

## 비동기 작업 후 이동

`PushAsync<T>()`에 `Func<UniTask>` 작업을 전달하면 모든 작업이 끝난 뒤에만 화면을 전환한다.

```csharp
bool moved = await ViewManager.Instance.PushAsync<InventoryView>(
    () => inventoryModel.LoadAsync(),
    () => inventoryModel.RefreshEquipmentAsync());
```

작업은 등록 순서대로 실행된다. 작업이 실패하면 예외를 발생시켜야 한다. 이 경우 `PushAsync<T>()`는 `false`를 반환하고 현재 View와 직전 View를 변경하지 않는다. 입력 차단은 성공과 실패 모두에서 해제된다.

## 뒤로 가기

```csharp
bool moved = await ViewManager.Instance.PopAsync();
```

현재 View를 닫고 직전 View로 한 단계 돌아간다. 복귀 후 직전 View 기록은 비워지므로 연속 뒤로 가기는 지원하지 않는다. 직전 View가 없거나 전환 중이면 `false`를 반환한다. 공통 `Btn_back.prefab`에는 `BackViewButton`이 연결되어 있어 클릭 시 `PopAsync()`를 실행한다. 뒤로 가기에는 `Target View`를 지정하지 않는다.

## 전환 애니메이션

View 루트에 `ViewTransition` 파생 컴포넌트를 추가하고 `BaseView.Transition`에 할당한다. 기본 제공 `CanvasGroupFadeTransition`은 `CanvasGroup` 알파를 사용하며 진입·이탈 시간과 Animation Curve를 Inspector에서 설정할 수 있다.

다른 애니메이션이 필요하면 `ViewTransition`을 상속해 다음 두 메서드를 구현한다.

```csharp
public override UniTask PlayEnterAsync();
public override UniTask PlayExitAsync();
```

View별 로직은 `OnEnteringAsync()`와 `OnExitingAsync()`에서 구현한다. 전환 애니메이션 호출과 라이프사이클 상태 변경은 BaseView가 보장하므로 파생 View가 직접 처리하지 않는다.

## 로그

`ViewManager.Diagnostics > Enable Logging`으로 일반 View 로그를 켜거나 끈다. 기본값은 활성화다. Unity Console에는 파란색 `[VIEW]` 헤더로 다음 항목이 출력된다.

- Push 요청과 완료된 목적지
- Pop 요청과 복귀한 View
- 전환 전 비동기 작업 개수와 완료
- View 인스턴스 생성과 재사용 정책
- `Created`, `Entered`, `Exited`, `Released` 라이프사이클 변경
- 전환 완료 후 현재 View와 직전 View

Push, Pop 또는 비동기 작업 실패는 `Enable Logging` 설정과 관계없이 오류와 예외를 출력한다. 실패 로그에는 유지된 현재 View가 포함된다.

## 유지할 설계 원칙

- 일반 UI 전환을 위해 Scene을 매번 로드하지 않는다.
- 화면은 `BaseView` 파생 컴포넌트를 가진 프리팹 단위로 관리한다.
- View는 화면 표시와 입력 전달에 집중한다.
- 서버 요청은 `Model → XxxApi → ApiClient.Instance` 흐름을 사용한다.
- 전환 중에는 사용자 입력을 차단하고 성공·실패 여부와 관계없이 다시 해제한다.
- 화면 크기와 비율 변화에 대응하도록 Anchor와 Layout 컴포넌트를 사용한다.
