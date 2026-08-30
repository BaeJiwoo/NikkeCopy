# NikkeCopy.Domain

Player, Nikke, InventoryItem, Squad, Stage 등 핵심 게임 규칙과 모델을 둡니다. ASP.NET Core, 데이터베이스, Infrastructure에는 의존하지 않습니다.

## 책임

- 게임의 핵심 엔티티와 값 객체 정의
- 레벨업, 재화 사용, 스쿼드 편성 같은 규칙 보호
- 유효하지 않은 상태 변경 방지
- 기술이나 저장 방식과 무관한 순수한 게임 로직 제공

HTTP 요청 DTO, EF Core `DbContext`, Repository 구현 및 외부 API 호출 코드는 Domain에 두지 않습니다.

## 현재 상태

현재 `Player` 엔티티가 있으며 `Id`, `Name`, `CreatedAt`을 정의합니다. 나머지 게임 모델과 세부 규칙은 아직 구현되지 않았습니다.

```text
NikkeCopy.Domain/
└─ Player/
   └─ Player.cs
```

관련 요구사항은 [유즈케이스 문서](../../Docs/UseCases/README.md)를 참고합니다.
