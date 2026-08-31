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
   │  ├─ Views/            BaseView와 현재/직전 View 관리
   │  ├─ Controllers/      입력 처리와 Model/View 조정
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

화면 로직은 `View → Controller → Model` 흐름을 따릅니다. View는 `BaseView`를 상속하고, `ViewManager`가 현재 View와 직전 View 및 비동기 진입·이탈을 관리합니다. Controller가 사용자 입력과 화면 전환을 조정하고, Model이 상태 및 Network 계층과의 통신을 담당합니다. View는 Network 계층을 직접 참조하지 않습니다.

서버 통신은 `Model → XxxApi → ApiClient.Instance → ASP.NET Core` 흐름을 따릅니다. `ApiClient`는 게임 세션 전체에서 하나만 존재하는 공유 인스턴스이며 모든 API 계층이 이를 참조합니다. 기본 서버 주소는 `http://localhost:5000`이고 GET, JSON POST, 공통 오류 처리와 Bearer Token 관리를 제공합니다.

게임 에셋은 용도에 따라 `GameResources` 아래에 배치합니다. 런타임 동적 로딩이 꼭 필요한 경우에만 Unity의 예약 폴더인 `Resources`를 별도로 사용합니다.

## 서버 연결 확인

먼저 저장소 루트에서 서버를 실행합니다.

```text
dotnet run --project Server/NikkeCopy.Api --launch-profile http
```

`ServerHealthTest`를 GameObject에 추가하고 Play하면 `/api/health` 응답이 Unity Console에 출력됩니다.

## 인증 화면

인증 UI는 `Assets/GameResources/Prefabs/UI/Views/AuthView.prefab`으로 관리합니다. 기존 `AuthScene.unity`에는 삭제된 View 전환 컴포넌트의 직렬화 흔적이 남아 있어 정리가 필요합니다.

- `CanvasScaler`: `Scale With Screen Size`, 기준 해상도 `1920x1080`
- 중앙 앵커 기반 로그인 패널
- 아이디 및 비밀번호 입력
- 로그인 및 회원가입 버튼
- 인증 진행 및 오류 상태 표시

`AuthModel`은 로그인에 성공하면 JWT Access Token을 `PlayerPrefs`에 저장하고 이후 `ApiClient.Instance` 요청의 Bearer Token으로 사용하도록 구성되어 있습니다. 현재 인증 View와 Controller 연결은 새 ViewManager 구조에 맞춰 다시 구성해야 합니다. 운영 단계에서는 토큰 저장소를 플랫폼 보안 저장소로 교체해야 합니다.

### View 관리

기존 ViewGraph와 키 기반 전환 시스템은 삭제되었습니다. 현재는 다음 구조로 교체 중입니다.

- `BaseView`: View의 비동기 `EnterAsync`, `ExitAsync` 생명주기와 데이터 바인딩 지점
- `ViewManager`: 단일 인스턴스에서 현재 View와 직전 View를 관리
- `raycastBlocker`: 비동기 화면 전환 중 중복 입력 차단
- UniTask: View 전환과 선행 작업의 비동기 실행

`ViewManager.View Prefabs` 목록이 이동 가능한 View를 모두 소유합니다. 일반 버튼은 `ViewPushButton.Target View`에 목록에 등록된 프리팹을 수동으로 할당하며, 개별 View 클래스에는 목적지별 이동 메서드를 두지 않습니다. 선택적인 UniTask 선행 작업이 실패하면 기존 View를 유지합니다. `PopAsync()`와 `BackViewButton`은 이전 View로 돌아가고, `ViewTransition` 파생 컴포넌트로 화면별 진입·이탈 애니메이션을 지정할 수 있습니다. 새 화면 제작 규칙은 [View 생성 가이드라인](../../../Docs/ClientViewCreation.md), 설정과 사용법은 [클라이언트 View 관리 문서](../../../Docs/ClientViewNavigation.md)를 참고합니다.

버튼의 대상이 현재 View와 같거나 `Target View`가 비어 있으면 `ViewPushButton`이 Button을 자동으로 비활성화합니다. 코드에서 같은 View로 직접 Push를 요청해도 ViewManager가 선행 작업과 전환을 시작하지 않습니다.

각 View는 `Created → Entered → Exited → Released` 라이프사이클을 가집니다. `Reuse Mode`가 `Reuse`이면 인스턴스를 캐시해 `Entered ↔ Exited`를 반복하고, `Recreate`이면 Pop할 때 `Released` 후 파괴하여 다음 진입에서 새로 생성합니다.

`ViewManager`의 `Enable Logging`을 활성화하면 Push/Pop, 비동기 선행 작업, 인스턴스 생성과 라이프사이클 상태가 파란색 `[VIEW]` 로그로 Unity Console에 출력됩니다. 전환 실패와 예외는 이 설정과 관계없이 항상 출력됩니다.

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
- BaseView 및 ViewManager 기반 단일 단계 화면 관리
- 비동기 선행 작업, 실패 유지, 전환 애니메이션과 뒤로 가기 지원
- Auth, Main, Inventory, NikkeManagement, Recruitment, Squad View 프리팹 구성
- 에디터 전용 API 요청 및 응답 로그 구성
- 인증 및 게임 기능 API 연동은 서버 API 구현 후 추가 예정

API 계약은 [OpenAPI 디렉터리](../../../Contracts/OpenAPI/README.md), 서버 실행 방법은 [서버 README](../../../Server/README.md)를 참고합니다.
