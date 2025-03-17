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

public enum MoveDirection { Up, Down, Left, Right, Forward, Back } // 벽 이동 방향
public enum MovementType { None, Linear, Oscillating }   //벽움직임의 종류
public enum CollisionHandleType { Passable, Blocked, Bouncy, Deadly }
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
    EventA, EventB, EventC, EventD, EventE, EventF, EventG, EventH
}