# 클라이언트 View 생성 가이드라인

## 적용 범위

이 문서는 `BaseView`, `ViewManager` 및 View 프리팹 구조를 기준으로 한다.

현재 `AuthView`, `MainView`, `InventoryView`, `NikkeManagementView`, `RecruitmentView`, `SquadView` 클래스가 준비되어 있으며 각 프리팹 루트에 연결되어 있다.

## 파일 위치와 이름

```text
Assets/
├─ GameResources/Prefabs/UI/Views/{ViewName}View.prefab
└─ Scripts/MVC/
   ├─ Views/{ViewName}View.cs
   ├─ Controllers/{ViewName}Controller.cs
   └─ Models/{ViewName}Model.cs
```

- 화면 이름은 역할을 나타내는 PascalCase를 사용한다.
- 프리팹과 View 스크립트에는 `View` 접미사를 사용한다.
- 예: `InventoryView.prefab`, `InventoryView.cs`.

## 생성 절차

### 1. View 스크립트 작성

관리 대상 화면은 `BaseView`를 상속한다.

```csharp
using Cysharp.Threading.Tasks;

public sealed class InventoryView : BaseView
{
    protected override void BindViewModel()
    {
        // UI 이벤트와 화면 데이터를 연결한다.
    }

    protected override async UniTask OnCreatedAsync()
    {
        await UniTask.CompletedTask;
        // 인스턴스 생성 시 한 번 실행
    }

    protected override async UniTask OnEnteringAsync()
    {
        await UniTask.CompletedTask;
        // 화면에 진입할 때마다 실행
    }

    protected override async UniTask OnExitingAsync()
    {
        await UniTask.CompletedTask;
        // 화면에서 이탈할 때마다 실행
    }

    protected override async UniTask OnReleasedAsync()
    {
        await UniTask.CompletedTask;
        // 인스턴스 해제 전 한 번 실행
    }
}
```

- ViewManager가 호출하는 `CreateAsync`, `EnterAsync`, `ExitAsync`, `ReleaseAsync`는 직접 호출하거나 재정의하지 않는다.
- 파생 View는 `OnCreatedAsync`, `OnEnteringAsync`, `OnExitingAsync`, `OnReleasedAsync`만 필요에 따라 재정의한다.
- 서버 요청과 도메인 판단은 Model 또는 Controller가 담당한다.
- View에서 `UnityWebRequest`나 `ApiClient`를 직접 호출하지 않는다.
- 이벤트를 등록했다면 비활성화 또는 파괴 시점에 반드시 해제한다.

### 2. 반응형 View 루트 구성

Canvas 아래에서 View 루트를 만들고 다음을 적용한다.

- 이름: `{ViewName}View`
- `RectTransform`: 부모를 채우는 Stretch Anchor
- 기본 Offset: Left, Right, Top, Bottom `0`
- `{ViewName}View` 컴포넌트 추가
- `Reuse Mode`: 상태를 유지할 화면은 `Reuse`, Pop 후 새로 만들 화면은 `Recreate`
- 화면 배치는 Anchor, Layout Group, Content Size Fitter 등을 우선 사용
- 특정 해상도에서만 맞는 절대 좌표 배치는 피함

### 3. 프리팹 저장

View 루트를 다음 위치에 저장한다.

```text
Assets/GameResources/Prefabs/UI/Views/{ViewName}View.prefab
```

Scene 인스턴스에만 변경 사항을 남기지 말고 재사용할 설정은 프리팹에 적용한다.

### 4. MVC 역할 분리

- View: UI 참조, 화면 표시, 사용자 입력 이벤트
- Controller: 입력 처리, 검증, Model 호출, View 전환 결정
- Model: 상태와 API 결과 관리

서버 통신은 다음 흐름을 따른다.

```text
View → Controller → Model → XxxApi → ApiClient.Instance
```

### 5. ViewManager 연결

1. View 프리팹 루트에 `BaseView` 파생 컴포넌트가 연결됐는지 확인한다.
2. Scene의 `ViewManager.View Prefabs`에 해당 컴포넌트를 등록한다.
3. 최초 화면이면 Scene에 배치하고 `Initial View`에 연결한다.
4. 일반 이동 버튼에 `ViewPushButton`을 추가하고 `Target View`에 이 프리팹의 View 컴포넌트를 할당한다.
5. API 등 선행 작업이 있으면 `Func<UniTask>`로 전달한다.

```csharp
bool moved = await ViewManager.Instance.PushAsync<InventoryView>(
    () => inventoryModel.LoadAsync());
```

선행 작업은 실패 시 예외를 발생시켜야 한다. 실패하면 기존 View가 유지되고 반환값은 `false`다.

### 6. 전환 애니메이션

기본 페이드가 필요하면 View 루트에 `CanvasGroup`과 `CanvasGroupFadeTransition`을 추가하고, View 컴포넌트의 `Transition` 필드에 연결한다. 다른 효과는 `ViewTransition` 파생 컴포넌트로 구현한다.

### 7. 뒤로 가기

공통 `Btn_back.prefab`을 사용하거나 Button에 `BackViewButton`을 추가한다. 별도의 `On Click()` 연결 없이 현재 View를 Pop하고 이전 View로 복귀한다.

## 현재 체크리스트

- [ ] View 스크립트가 `BaseView`를 상속한다.
- [ ] View 프리팹을 표준 디렉터리에 저장했다.
- [ ] 프리팹 루트에 해당 View 컴포넌트를 연결했다.
- [ ] Anchor 또는 Layout을 사용해 화면 비율 변화에 대응한다.
- [ ] View가 Network 계층을 직접 참조하지 않는다.
- [ ] View 이벤트 등록과 해제 시점이 대칭이다.
- [ ] 상태 보존 여부에 맞게 `Reuse Mode`를 선택했다.
- [ ] 생성·진입·이탈·해제 로직을 올바른 라이프사이클 메서드에 배치했다.
- [ ] ViewManager의 `View Prefabs` 또는 `Initial View`에 등록했다.
- [ ] 이동 버튼의 `ViewPushButton.Target View`를 수동으로 할당했다.
- [ ] 전환 애니메이션이 필요하면 `ViewTransition`을 연결했다.
- [ ] 선행 비동기 작업의 실패가 예외로 전달된다.
- [ ] 뒤로 가기가 필요한 화면에 `BackViewButton`을 배치했다.
