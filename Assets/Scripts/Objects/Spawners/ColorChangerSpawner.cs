using Unity.Netcode;
using UnityEngine;

namespace ColorChanger
{
    /// <summary>
    /// 색깔 변환기를 스폰하는 Class
    /// </summary>
    public class ColorChangerSpawner : NetworkObjectSpawner
    {
        [SerializeField] float _changeTime;

        public override void SpawnObject()
        {
            base.SpawnObject();

            _spawnedObject = Instantiate(_prefab);

            _spawnedObject.transform.position = transform.position;
            _spawnedObject.transform.rotation = transform.rotation;
            _spawnedObject.transform.localScale = transform.lossyScale;

            _spawnedObject.GetComponent<NetworkObject>().Spawn();
            _spawnedObject.GetComponent<ColorChangerController>().Initialize(_changeTime);
        }
    }
}
