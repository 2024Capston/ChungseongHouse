using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace ColorChanger
{
    public class CC1_StageManager : StageManager
    {
        [SerializeField] GameObject _doorPrefab;
        [SerializeField] GameObject _buttonPrefab;
        [SerializeField] GameObject _ColorChangerPrefab;
        [SerializeField] GameObject _cubePrefab;
        [SerializeField] GameObject _platformPrefab;

        private List<GameObject> _spawnedNetworkObjects;

        private Action _networkObjectsCreated;
        public Action NetworkObjectsCreated
        {
            get => _networkObjectsCreated;
            set => _networkObjectsCreated = value;
        }

        public override void EndGame()
        {
            Debug.Log("E");
        }

        public override void RestartGame()
        {
            Debug.Log("R");
        }

        public override void StartGame()
        {
            _spawnedNetworkObjects = new List<GameObject>();

            GameObject obj = Instantiate(_doorPrefab);

            obj.transform.position = new Vector3(0, -15, -51);
            obj.transform.rotation = Quaternion.Euler(0, 180, 0);
            obj.transform.localScale = Vector3.one * 10;

            obj.GetComponent<NetworkObject>().Spawn();
            obj.GetComponent<DoorController>().Initialize(false);
            _spawnedNetworkObjects.Add(obj);

            //

            obj = Instantiate(_doorPrefab);

            obj.transform.position = new Vector3(340, -15, 451);
            obj.transform.rotation = Quaternion.Euler(0, 180, 0);
            obj.transform.localScale = Vector3.one * 10;

            obj.GetComponent<NetworkObject>().Spawn();
            obj.GetComponent<DoorController>().Initialize(false);
            _spawnedNetworkObjects.Add(obj);

            //

            obj = Instantiate(_doorPrefab);

            obj.transform.position = new Vector3(340, -15, 189);
            obj.transform.rotation = Quaternion.Euler(0, 180, 0);
            obj.transform.localScale = Vector3.one * 10;

            obj.GetComponent<NetworkObject>().Spawn();
            obj.GetComponent<DoorController>().Initialize(false);
            _spawnedNetworkObjects.Add(obj);

            //

            GameObject platform = Instantiate(_platformPrefab);

            platform.transform.position = new Vector3(-100, -16, 50);
            platform.transform.localScale = Vector3.one * 20;

            Transform[] targets = new Transform[6];

            targets[0] = GameObject.Find("Platform Target 1").transform;
            targets[1] = GameObject.Find("Platform Target 2").transform;
            targets[2] = GameObject.Find("Platform Target 3").transform;
            targets[3] = GameObject.Find("Platform Target 4").transform;
            targets[4] = targets[2];
            targets[5] = targets[1];

            platform.GetComponent<NetworkObject>().Spawn();
            platform.GetComponent<PlatformController>().Initialize(targets, 30f);
            _spawnedNetworkObjects.Add(platform);

            //

            obj = Instantiate(_buttonPrefab);

            obj.transform.position = new Vector3(0, -14, 160);
            obj.transform.rotation = Quaternion.Euler(0, 90, 0);
            obj.transform.localScale = Vector3.one * 5;

            obj.GetComponent<NetworkObject>().Spawn();
            obj.GetComponentInChildren<GenericButtonController>().Initialize(ColorType.Blue, new GameObject[] { platform }, ButtonType.Persistent, 0f, false, 0f, null);
            _spawnedNetworkObjects.Add(obj);

            //

            obj = Instantiate(_ColorChangerPrefab);

            obj.transform.position = new Vector3(-140, 4, 110);
            obj.transform.rotation = Quaternion.Euler(0, 90, 0);

            obj.GetComponent<NetworkObject>().Spawn();
            obj.GetComponent<ColorChangerController>().Initialize(5);
            _spawnedNetworkObjects.Add(obj);

            //

            obj = Instantiate(_ColorChangerPrefab);

            obj.transform.position = new Vector3(-0, 4, 250);
            obj.transform.rotation = Quaternion.Euler(0, 180, 0);

            obj.GetComponent<NetworkObject>().Spawn();
            obj.GetComponent<ColorChangerController>().Initialize(5);
            _spawnedNetworkObjects.Add(obj);

            //

            obj = Instantiate(_ColorChangerPrefab);

            obj.transform.position = new Vector3(180, 4, 250);
            obj.transform.rotation = Quaternion.Euler(0, 180, 0);

            obj.GetComponent<NetworkObject>().Spawn();
            obj.GetComponent<ColorChangerController>().Initialize(5);
            _spawnedNetworkObjects.Add(obj);

            //

            obj = Instantiate(_cubePrefab);

            obj.transform.position = new Vector3(0, -8.9f, 90);

            obj.GetComponent<NetworkObject>().Spawn();
            obj.GetComponent<CubeController>().Initialize(ColorType.Blue);

            //

            obj = Instantiate(_cubePrefab);

            obj.transform.position = new Vector3(20, -8.9f, 90);

            obj.GetComponent<NetworkObject>().Spawn();
            obj.GetComponent<CubeController>().Initialize(ColorType.Red);
            _spawnedNetworkObjects.Add(obj);
        }

        public override void OnNetworkDespawn()
        {
            foreach(GameObject gameObject in _spawnedNetworkObjects)
            {
                gameObject.GetComponent<NetworkObject>().Despawn();
            }
        }
    }
}