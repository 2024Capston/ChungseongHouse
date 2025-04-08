/// <summary>
/// Scene의 종류
/// </summary>
public enum SceneType
{
    Title,
    Home,
    Lobby,
    InGame,
}

public enum StageName
{
    Floor1_1,
    Floor1_2,
    Floor1_3,
    Floor1_4,
    Floor1_5,
    Floor1_6,
    Size
}

public enum ColorType
{
    None = 0,
    Blue = 1,
    Red = 2,
    Purple = 3
}

/// <summary>
/// 버튼 작동 방식
/// </summary>
public enum ButtonType
{
    Persistent, // 한 번 누르면 계속 활성화 상태 유지
    Toggle,     // 활성/비활성 토글
    Temporary   // 누르고 일정 시간이 지나면 비활성화
}

/// <summary>
/// 이벤트 버스에 사용할 이벤트 종류
/// </summary>
public enum EventType
{
    EventA, EventB, EventC, EventD, EventE, EventF, EventG, EventH, EventI, EventJ, EventK, EventL, EventM, EventN, EventO, EventP
}

/// <summary>
/// 플레이어 리깅 애니메이션에서 사용할 팔 종류
/// </summary>
public enum ArmType
{
    LeftArm, RightArm, BothArms
}