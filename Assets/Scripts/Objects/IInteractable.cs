/// <summary>
/// 플레이어가 상호작용할 수 있는 물체에 대한 Interface
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Outline 컴포넌트에 대한 레퍼런스.
    /// </summary>
    public Outline Outline { get; set; }

    /// <summary>
    /// 플레이어와 상호작용할 수 있는지 확인한다.
    /// </summary>
    /// <param name="player">플레이어</param>
    /// <returns>상호작용 가능 여부</returns>
    public bool IsInteractable(PlayerController player);

    /// <summary>
    /// 플레이어와 상호작용을 시작한다.
    /// </summary>
    /// <param name="player">플레이어</param>
    public bool StartInteraction(PlayerController player);

    /// <summary>
    /// 플레이어와 상호작용을 중단한다.
    /// </summary>
    /// <param name="player">플레이어</param>
    public bool StopInteraction(PlayerController player);
}