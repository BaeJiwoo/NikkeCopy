# NikkeCopy.Application

게임 유스 케이스와 애플리케이션 서비스를 둡니다. 이 계층은 Domain에만 의존하며 Login, GetPlayer, LevelUpNikke, ChangeSquad, RecruitNikke, UseItem 등의 기능이 추가될 자리입니다.

## 책임

- 하나의 사용자 요청에 필요한 Domain 작업 조정
- Repository와 외부 서비스 인터페이스 선언
- 유스케이스 입력과 결과 모델 정의
- 트랜잭션 경계와 권한 확인 흐름 조정

Application은 EF Core, MySQL, Controller 및 HTTP 형식에 의존하지 않습니다. 실제 DB 구현은 Infrastructure에 둡니다.

## 권장 구조

```text
NikkeCopy.Application/
├─ Abstractions/
│  └─ Persistence/
│     └─ IPlayerRepository.cs
└─ Players/
   ├─ CreatePlayer/
   └─ GetPlayer/
```

## 현재 상태

프로젝트 참조와 계층 구조만 구성되어 있으며 실제 유스케이스와 Repository 인터페이스는 아직 없습니다. 첫 구현 대상으로 계정 생성, 로그인 또는 플레이어 조회 중 하나를 선정해야 합니다.

요구사항은 [유즈케이스 문서](../../Docs/UseCases/README.md)를 참고합니다.
