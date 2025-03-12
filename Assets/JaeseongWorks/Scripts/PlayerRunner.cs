using StageJS;
using Cinemachine;
using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerRunner : NetworkBehaviour
{
    // 플레이어 조작에 쓰이는 보조 변수
    //private Rigid
    private Vector3 _pastPosition;

    // [SerializeField] private bool _isStop;
    [SerializeField] public NetworkVariable<Boolean> _isStop = new NetworkVariable<Boolean>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public bool IsStop
    {
        get
        {
            // if (Mathf.Abs(_pastPosition.x-transform.position.x)<.01 && Mathf.Abs(_pastPosition.y-transform.position.y)<.01) 
            // {
            //     return true;
            // }
            // else return false;

            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            if (h != 0 || v != 0)
            {
                return false;
            }
            else return true;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // 정지 상태시 추격하는 몹 안보이게
        GameObject chasingEnemy = GameObject.FindGameObjectWithTag("chasingEnemy");
        if (chasingEnemy)
        {
            if (IsStop)
            {
                ChasingEnemy enemyScript = chasingEnemy.GetComponent<ChasingEnemy>();
                enemyScript.OnStealth();
            }
            else
            {
                ChasingEnemy enemyScript = chasingEnemy.GetComponent<ChasingEnemy>();
                enemyScript.OffStealth();
            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        // 충돌한 게임 오브젝트의 태그가 "Enemy"인지 확인
        if (collision.gameObject.tag == "chasingEnemy")
        {
            //게임 종료
            GameManager.instance.GameOver();
        }
    }
}
