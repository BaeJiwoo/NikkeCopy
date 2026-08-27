# Architecture

```text
Unity Client
    ↓ HTTP / JSON
NikkeCopy.Api
    ↓
NikkeCopy.Application
    ↓
NikkeCopy.Domain

NikkeCopy.Infrastructure
    ↓
Database / External Services
```

- **Api**: HTTP 요청과 응답을 담당하는 Presentation 계층입니다. Controller에서 데이터베이스에 직접 접근하지 않습니다.
- **Application**: 게임 유스 케이스를 조정하며 Domain에 의존합니다.
- **Domain**: 게임의 핵심 모델과 규칙을 소유하고 다른 프로젝트를 참조하지 않습니다.
- **Infrastructure**: 데이터베이스와 외부 서비스 구현을 담당하며 Application과 Domain에 의존합니다.
- **Contracts**: Client와 Server 사이의 경계이며 구현 코드 대신 API 명세만 공유합니다.
