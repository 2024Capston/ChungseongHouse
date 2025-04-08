using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class RenderLine : MonoBehaviour
{
    // Use this for initialization
    [SerializeField] float _lineWidth = 5f;
    public LineRenderer lineRenderer;
    [SerializeField] private LaserGun _laserGun;
    GameObject _emptyChild;

    private void Awake()
    {
        if (_laserGun == null)
            _laserGun = GetComponent<LaserGun>();
        lineRenderer = transform.Find("RayStartPoint").GetComponent<LineRenderer>();
    }
    void Start()
    {
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;
        lineRenderer.startWidth = _lineWidth;
        lineRenderer.endWidth = _lineWidth;
        
    }
    void Update()
    {
        // if (clicked && GameManager.turn % 2 == 1 && tag == "Red")
        // if (clicked)
        // {
        //     Vector3 scrSpace = Camera.main.WorldToScreenPoint(transform.position);
        //     Vector3 offset = new Vector3(scrSpace.x - Input.mousePosition.x, scrSpace.y - Input.mousePosition.y, 0) / 50;
        //     lineRenderer.SetPosition(0, transform.position);
        //     lineRenderer.SetPosition(1, transform.position + offset);
        // }
        // if(clicked && GameManager.turn % 2 == 0 && tag == "Blue") {
        //         Vector3 scrSpace = Camera.main.WorldToScreenPoint(transform.position);
        //         Vector3 offset = new Vector3(scrSpace.x - Input.mousePosition.x, 0, scrSpace.y - Input.mousePosition.y);
        //         lineRenderer.SetPosition(0, transform.position);
        //         lineRenderer.SetPosition(1, transform.position - offset);
        // }
    }
    public IEnumerator DrawLay(Vector3 endPoint)
    {
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, _laserGun.GunPoint.position);
        lineRenderer.SetPosition(1, endPoint);
        yield return new WaitForSeconds(0.5f);
        lineRenderer.SetPosition(0, _laserGun.GunPoint.transform.position);
        lineRenderer.SetPosition(1, _laserGun.GunPoint.transform.position);
        lineRenderer.enabled = false;
    }
}