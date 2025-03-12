using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderLine : MonoBehaviour
{
    // Use this for initialization
    [SerializeField] float _lineWidth = 5f;
    LineRenderer lineRenderer;
    void Start()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.yellow;
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
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, endPoint);
        yield return new WaitForSeconds(1f);
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.position);
        lineRenderer.enabled = false;
    }
}