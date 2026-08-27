# Unity Client

Unity Hub에서 이 디렉터리 아래의 `NikkeCopy.Unity` 프로젝트를 엽니다. 현재 Unity 6 URP 프로젝트가 생성되어 있습니다.

스크립트 구조:

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

`ServerHealthTest`를 GameObject에 추가하고 Play하면 `/api/health` 응답이 Unity Console에 출력됩니다.
