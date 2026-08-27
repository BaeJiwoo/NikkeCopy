# NikkeCopy

Unity + ASP.NET Core 기반 수집형 RPG 모작 프로젝트입니다. Client와 Server를 하나의 Git 저장소에서 관리합니다.

## Repository Structure

```text
Client/       Unity 클라이언트
Server/       ASP.NET Core 서버
Contracts/    OpenAPI 및 향후 ProtoBuf 계약
Docs/         아키텍처, API, 개발 문서
```

## Server Requirements

- .NET 10 SDK

## Server 실행 방법

```bash
dotnet restore
dotnet build
dotnet run --project Server/NikkeCopy.Api --launch-profile http
```

### VS Code

저장소 루트를 VS Code로 연 뒤 권장 확장을 설치합니다.

- `Ctrl+Shift+B`: 전체 서버 복원 및 빌드
- `F5`: `NikkeCopy.Api`를 디버그 모드로 실행하고 Health API 열기
- `Terminal → Run Task → server: run`: 디버거 없이 서버 실행
- 실행 및 디버그 목록의 `Client: Attach to Unity`: 실행 중인 Unity Editor에 디버거 연결

## Health Check

```http
GET http://localhost:5000/api/health
```

## Unity

Unity Hub에서 다음 경로를 엽니다.

```text
Client/NikkeCopy.Unity
```

Unity 6 URP 프로젝트가 생성되어 있습니다. Network 계층의 `ApiClient`를 통해 서버에 접근하며 UI에서 `UnityWebRequest`를 직접 호출하지 않습니다.

서버 연결 확인용 `ServerHealthTest` 컴포넌트를 GameObject에 추가한 뒤 Play하면 Unity Console에서 Health API 응답을 확인할 수 있습니다.

## Future Infrastructure

### Docker

서버 및 의존 서비스를 위한 Docker 구성은 배포 요구사항이 정해진 뒤 추가합니다.

### MySQL / EF Core

영속성 요구사항과 스키마가 정해진 뒤 Infrastructure 프로젝트에 EF Core와 MySQL 구현을 추가합니다. 현재 관련 패키지는 설치하지 않습니다.
