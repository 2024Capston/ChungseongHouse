using UnityEngine;

public class NetworkSyncInterpolator : MonoBehaviour
{
    [SerializeField] private float _lerpSpeed = 10f;

    public GameObject Target
    {
        get => _target;
        set => _target = value;
    }
    private GameObject _target;

    void Update()
    {
        if (_target)
        {
            float posDif = Vector3.Distance(transform.position, _target.transform.position);
            float rotDif = 1f - Quaternion.Dot(transform.rotation, _target.transform.rotation);

            if (posDif > 0.0000001f || rotDif > 0.0000001f)
            {
                transform.position = Vector3.Lerp(transform.position, _target.transform.position, Time.deltaTime * _lerpSpeed);
                transform.rotation = Quaternion.Slerp(transform.rotation, _target.transform.rotation, Time.deltaTime * _lerpSpeed);
            }
        }
    }
}
