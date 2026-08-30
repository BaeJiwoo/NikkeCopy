# Unity Client

Unity Hub에서 이 디렉터리 아래의 `NikkeCopy.Unity` 프로젝트를 엽니다. 현재 Unity 6 URP 프로젝트가 생성되어 있습니다.

## 실행 환경

- Unity `6000.3.11f1`
- Universal Render Pipeline
- 로컬 API 기본 주소: `http://localhost:5000`

## 스크립트 구조

```text
NikkeCopy.Unity/Assets/Scripts/
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

통신은 `UI → Service → XxxApi → ApiClient → ASP.NET Core` 흐름을 따릅니다. UI 코드에서 `UnityWebRequest`를 직접 호출하지 않습니다. `ApiClient`의 기본 서버 주소는 `http://localhost:5000`이며 GET, JSON POST, 공통 오류 처리와 Bearer Token 확장 지점을 제공합니다.

## 서버 연결 확인

먼저 저장소 루트에서 서버를 실행합니다.

```text
dotnet run --project Server/NikkeCopy.Api --launch-profile http
```

`ServerHealthTest`를 GameObject에 추가하고 Play하면 `/api/health` 응답이 Unity Console에 출력됩니다.

## 현재 상태

- Health API 연결 확인 가능
- 공통 API 클라이언트와 기능별 네트워크 디렉터리 구성
- 인증 및 게임 기능 API 연동은 서버 API 구현 후 추가 예정

API 계약은 [OpenAPI 디렉터리](../Contracts/OpenAPI/README.md), 서버 실행 방법은 [서버 README](../Server/README.md)를 참고합니다.
