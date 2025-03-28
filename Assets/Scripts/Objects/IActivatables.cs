using UnityEngine;

/// <summary>
/// 간접적으로 활성화할 수 있는 물체에 대한 Interface
/// </summary>
public interface IActivatable
{
    /// <summary>
    /// 물체를 활성화할 수 있는지 확인한다.
    /// </summary>
    /// <param name="activator">활성화 주체</param>
    /// <returns>활성화 가능 여부</returns>
    public bool IsActivatable(GameObject activator = null);

    /// <summary>
    /// 물체를 활성화한다.
    /// </summary>
    /// <param name="activator">활성화 주체</param>
    public bool Activate(GameObject activator = null);

    /// <summary>
    /// 물체를 비활성화한다.
    /// </summary>
    /// <param name="activator">비활성화 주체</param>
    public bool Deactivate(GameObject activator = null);
}