# NikkeCopy.Api

클라이언트의 HTTP 요청을 받고 Application 유스케이스를 호출하는 Presentation 계층입니다. 서버 실행과 의존성 조립도 담당합니다.

## 책임

- Controller와 API 경로 정의
- HTTP 요청·응답 DTO 정의
- 인증·인가 및 공통 미들웨어 설정
- Application과 Infrastructure 서비스 등록
- 환경별 설정 로드

Controller에서 EF Core `DbContext`를 직접 사용하거나 게임 규칙을 구현하지 않습니다.

## 현재 엔드포인트

| 메서드 | 경로 | 설명 |
|---|---|---|
| GET | `/api/health` | 서버 실행 상태 확인 |

`AuthController.cs`는 아직 구현되지 않았습니다.

## 실행

저장소 루트에서 실행합니다.

```text
dotnet run --project Server/NikkeCopy.Api --launch-profile http
```

## 설정

DB 연결 문자열 키는 `ConnectionStrings:DefaultConnection`입니다. 비밀번호는 User Secrets 또는 환경변수로 관리합니다.

```text
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3306;Database=nikke_copy;User=사용자;Password=비밀번호" --project Server/NikkeCopy.Api
```

## 다음 작업

- 계정·로그인 API 계약 및 DTO 작성
- 요청 검증과 전역 오류 처리
- JWT 인증과 권한 설정
- OpenAPI/Swagger 구성
- API 통합 테스트 작성

관련 계약은 [OpenAPI README](../../Contracts/OpenAPI/README.md), 요구사항은 [유즈케이스 문서](../../Docs/UseCases/README.md)를 참고합니다.
