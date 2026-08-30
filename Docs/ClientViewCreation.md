# 클라이언트 View 생성 가이드라인

## 목적

일반 UI 화면은 Scene이 아니라 하나의 Canvas 아래에서 활성화되는 View 프리팹으로 만든다. View 생성과 이동 규칙은 이 문서를 기준으로 일관되게 관리한다.

## 기본 원칙

- 화면 하나는 하나의 View 프리팹으로 관리한다.
- View 전환을 위해 Scene을 새로 로드하지 않는다.
- View 이름이나 대상 GameObject를 문자열로 찾지 않는다.
- 이동 버튼은 대상 View가 아니라 `NavigationKey`만 가진다.
- 조건에 따른 이동은 Controller가 `ViewNavigator.Navigate(...)`를 호출한다.
- View는 서버에 직접 요청하지 않는다. `View → Controller → Model → XxxApi → ApiClient.Instance` 흐름을 따른다.
- 해상도 변경에 대응할 수 있도록 Anchor와 Layout 컴포넌트를 사용하고 고정 화면 좌표에 의존하지 않는다.

## 파일 위치와 이름

```text
Assets/
├─ GameResources/Prefabs/UI/Views/{ViewName}View.prefab
└─ Scripts/MVC/
   ├─ Views/{ViewName}View.cs
   ├─ Controllers/{ViewName}Controller.cs
   ├─ Models/{ViewName}Model.cs
   └─ Navigation/NavigationKey.cs
```

- 화면 식별자는 역할을 나타내는 PascalCase 이름을 사용한다. 예: `Inventory`, `Squad`.
- 프리팹과 View 스크립트 이름에는 `View` 접미사를 붙인다. 예: `InventoryView.prefab`, `InventoryView.cs`.
- 사용자 행동을 나타내는 이동 키에는 명확한 동사를 사용한다. 예: `ShowInventory`, `CloseInventory`.

## 생성 절차

### 1. ViewKey 등록

`Assets/Scripts/MVC/Navigation/NavigationKey.cs`의 `ViewKey`에 새 화면 키를 추가한다.

```csharp
public enum ViewKey
{
    None = 0,
    Auth = 1,
    Main = 2,
    Inventory = 3
}
```

기존 숫자는 변경하거나 재사용하지 않는다. 새 항목에는 새로운 고정 값을 부여한다.

### 2. NavigationKey 등록

새로운 사용자 행동이 필요하면 같은 파일의 `NavigationKey`에 추가한다.

```csharp
public enum NavigationKey
{
    None = 0,
    ShowAuth = 1,
    ShowMain = 2,
    ShowInventory = 3
}
```

`NavigationKey`는 목적지 이름 자체보다 사용자의 행동을 표현해야 한다. 같은 행동을 여러 출발 View에서 재사용할 수 있다.

### 3. View 루트 생성

Canvas의 `ViewRoot` 아래에 빈 UI GameObject를 만들고 다음을 설정한다.

- 이름: `{ViewName}View`
- `RectTransform`: 부모 영역을 채우는 Stretch Anchor
- Offset: Left, Right, Top, Bottom을 기본적으로 `0`
- `ViewPrefab` 컴포넌트 추가
- `ViewPrefab.Key`: 앞에서 만든 `ViewKey` 지정

화면 크기에 따라 배치가 달라져야 하는 콘텐츠에는 `Horizontal/Vertical Layout Group`, `Content Size Fitter`, Anchor 등을 사용한다. 특정 해상도에서만 맞는 절대 좌표 배치는 피한다.

### 4. MVC 구성

- View: UI 참조, 화면 표시, 사용자 입력 이벤트 전달
- Controller: View 이벤트 처리, 입력 검증, Model 호출, 화면 이동 결정
- Model: 화면 상태와 API 호출 결과 관리

View에서 `ApiClient`, `UnityWebRequest`, `PlayerPrefs`를 직접 사용하지 않는다. View 이동 조건도 View 스크립트에 넣지 않는다.

### 5. 프리팹 저장

완성한 View 루트를 다음 위치에 프리팹으로 저장한다.

```text
Assets/GameResources/Prefabs/UI/Views/{ViewName}View.prefab
```

프리팹 루트의 `ViewPrefab.Key`가 등록할 `ViewKey`와 같은지 확인한다.

### 6. ViewGraph 등록

`Assets/Settings/ClientViewGraph.asset`을 선택하고 Inspector에서 설정한다.

1. `Views` 목록에 항목을 추가한다.
2. `Key`에 새 `ViewKey`를 지정한다.
3. `Prefab`에 생성한 View 프리팹을 지정한다.
4. `Transitions` 목록에 이동 규칙을 추가한다.
5. `From`, `Navigation`, `To`를 지정한다.

예시:

```text
From: Main
Navigation: ShowInventory
To: Inventory
```

하나의 출발 View에서 동일한 `NavigationKey`를 두 번 등록할 수 없다.

### 7. 이동 연결

일반 버튼 이동:

1. Button GameObject에 `ViewNavigationButton`을 추가한다.
2. `Button Key`에 실행할 `NavigationKey`를 지정한다.
3. 대상 View나 `On Click()` 이벤트를 직접 연결하지 않는다.

조건부 이동:

```csharp
if (requestSucceeded)
{
    navigator.Navigate(NavigationKey.ShowInventory);
}
```

로그인 성공, 서버 검증, 저장 확인처럼 조건이 필요한 이동은 Controller에서 실행한다.

### 8. Canvas 동기화와 검증

1. View를 사용할 Scene을 연다.
2. `ClientViewGraph.asset`을 선택한다.
3. Inspector의 `Validate`를 눌러 오류를 확인한다.
4. `Sync Canvas`를 눌러 `ViewRoot`의 프리팹 인스턴스와 `ViewNavigator` 바인딩을 갱신한다.
5. Scene을 저장한다.
6. Play Mode에서 모든 진입·복귀 경로를 확인한다.

`Sync Canvas`는 그래프에 없는 오래된 View 인스턴스를 제거할 수 있으므로, View 인스턴스에만 존재하는 수정은 먼저 프리팹에 적용해야 한다.

## 완료 체크리스트

- [ ] `ViewKey`에 중복되지 않는 고정 값을 추가했다.
- [ ] 필요한 `NavigationKey`를 행동 중심 이름으로 추가했다.
- [ ] 프리팹 루트의 `ViewPrefab.Key`가 그래프의 Key와 일치한다.
- [ ] View 프리팹을 표준 디렉터리에 저장했다.
- [ ] 다양한 화면 비율을 고려해 Anchor 또는 Layout을 설정했다.
- [ ] View가 Network 계층을 직접 참조하지 않는다.
- [ ] ViewGraph의 `Views`와 `Transitions`를 등록했다.
- [ ] `Validate`에서 오류가 발생하지 않는다.
- [ ] `Sync Canvas` 후 Scene을 저장했다.
- [ ] 진입, 복귀, 조건부 실패 경로를 Play Mode에서 확인했다.

