# Unity Client

Unity Hub에서 저장소의 `Client/NikkeCopy.Unity` 디렉터리를 엽니다. 현재 Unity 6 URP 프로젝트가 생성되어 있습니다.

## 실행 환경

- Unity `6000.3.11f1`
- Universal Render Pipeline
- 로컬 API 기본 주소: `http://localhost:5000`

## 디렉터리 구조

```text
Assets/
├─ GameResources/
│  ├─ Art/                 스프라이트, 텍스처, 머티리얼
│  ├─ Audio/               배경음과 효과음
│  ├─ Data/                ScriptableObject 기반 게임 데이터
│  └─ Prefabs/
│     └─ UI/Views/         View 단위 UI 프리팹
└─ Scripts/
   ├─ MVC/
   │  ├─ Models/           화면 상태와 도메인 데이터
   │  ├─ Views/            화면 표시와 사용자 입력 전달
   │  ├─ Controllers/      입력 처리와 Model/View 조정
   │  └─ Navigation/       키 기반 View 전환
   ├─ Network/
   │  ├─ ApiClient.cs
   │  ├─ Auth/
   │  ├─ Player/
   │  ├─ Nikke/
   │  ├─ Squad/
   │  └─ Inventory/
   ├─ UI/
   ├─ Game/
   └─ Common/
```

화면 로직은 `View → Controller → Model` 흐름을 따릅니다. Controller가 사용자 입력과 화면 전환을 조정하고, Model이 상태 및 Network 계층과의 통신을 담당합니다. View는 Network 계층을 직접 참조하지 않습니다.

서버 통신은 `Model → XxxApi → ApiClient.Instance → ASP.NET Core` 흐름을 따릅니다. `ApiClient`는 게임 세션 전체에서 하나만 존재하는 공유 인스턴스이며 모든 API 계층이 이를 참조합니다. 기본 서버 주소는 `http://localhost:5000`이고 GET, JSON POST, 공통 오류 처리와 Bearer Token 관리를 제공합니다.

게임 에셋은 용도에 따라 `GameResources` 아래에 배치합니다. 런타임 동적 로딩이 꼭 필요한 경우에만 Unity의 예약 폴더인 `Resources`를 별도로 사용합니다.

## 서버 연결 확인

먼저 저장소 루트에서 서버를 실행합니다.

```text
dotnet run --project Server/NikkeCopy.Api --launch-profile http
```

`ServerHealthTest`를 GameObject에 추가하고 Play하면 `/api/health` 응답이 Unity Console에 출력됩니다.

## 인증 화면

`Assets/Scenes/AuthScene.unity`에 로그인 UI가 미리 배치되어 있으며 첫 번째 Build Scene으로 등록되어 있습니다. 런타임에 UI 오브젝트를 생성하지 않습니다.

- `CanvasScaler`: `Scale With Screen Size`, 기준 해상도 `1920x1080`
- 중앙 앵커 기반 로그인 패널
- 아이디 및 비밀번호 입력
- 로그인 및 회원가입 버튼
- 인증 진행 및 오류 상태 표시

로그인에 성공하면 JWT Access Token을 `PlayerPrefs`에 저장하고 이후 `ApiClient` 요청의 Bearer Token으로 사용합니다. 운영 단계에서는 플랫폼 보안 저장소로 교체해야 합니다.

로그인과 메인 화면은 같은 Scene과 Canvas 안의 `AuthView`, `MainView` 오브젝트로 구성됩니다. 화면 전환 시 Scene을 다시 로드하지 않고 대상 View만 활성화합니다.

### View 전환 그래프

- `ViewKey`: View Node를 구분하는 enum
- `NavigationKey`: 문자열이 아닌 방향 Edge 키
- `ViewGraph`: 시작 View와 `From + NavigationKey → To` Edge를 관리하는 ScriptableObject
- `ViewNavigator`: 현재 View에서 허용된 Edge를 찾아 View GameObject 활성 상태 변경
- `ViewNavigationButton`: 버튼에 고유 키를 할당하고 클릭 이벤트를 네비게이터에 전달
- `ViewPrefab`: 프리팹 루트와 `ViewKey` 연결
- Scene View Binding: `ViewKey → 프리팹 인스턴스` 연결

그래프 에셋은 `Assets/Settings/ClientViewGraph.asset`에서 관리합니다. 에셋 Inspector의 `Views`에 View 프리팹을 등록하고 `Transitions`에 `From + Navigation → To` 규칙을 정의합니다. `Sync Canvas`를 누르면 현재 Scene의 `ViewRoot` 아래 프리팹 인스턴스와 `ViewNavigator` 바인딩이 그래프에 맞게 갱신됩니다. 설정 절차와 검증 규칙은 [클라이언트 View 내비게이션 문서](../../../../Docs/ClientViewNavigation.md)를 참고합니다.

## API 디버깅 로그

Unity Editor에서 실행할 때만 모든 `ApiClient` 요청과 응답을 Console에 출력합니다.

- 요청 헤더: 녹색 `[API REQUEST]`
- 응답 헤더: 주황색 `[API RESPONSE]`
- 출력 정보: HTTP 메서드, URL, 상태 코드, 요청·응답 JSON
- 보호 정보: `password`, `accessToken` 값은 `***`로 마스킹

관련 코드는 `UNITY_EDITOR` 조건부 컴파일을 사용하므로 플레이어 빌드에는 포함되지 않습니다.

## 현재 상태

- Health API 연결 확인 가능
- 공통 API 클라이언트와 기능별 네트워크 디렉터리 구성
- 게임 리소스 유형별 디렉터리 구성
- MVC 기반 화면 관리 디렉터리 구성
- 반응형 Canvas 기반 로그인 및 회원가입 화면 구성
- JWT Access Token 저장 및 Bearer 인증 연결
- 로그인 성공 후 메인 화면 전환
- View 프리팹과 Inspector 기반 화면 연결
- 에디터 전용 API 요청 및 응답 로그 구성
- 인증 및 게임 기능 API 연동은 서버 API 구현 후 추가 예정

API 계약은 [OpenAPI 디렉터리](../../../../Contracts/OpenAPI/README.md), 서버 실행 방법은 [서버 README](../../../../Server/README.md)를 참고합니다.
