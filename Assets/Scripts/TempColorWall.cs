using UnityEngine;

public class TempColorWall : MonoBehaviour
{
    [SerializeField] private ColorType _color;

    void Start()
    {
        if (PlayerController.LocalPlayer)
        {
            UpdateColor();
        }
        else
        {
            PlayerController.LocalPlayerCreated += UpdateColor;
        }
    }

    void UpdateColor()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

        if (_color == ColorType.Blue)
        {
            meshRenderer.material.color = new Color(0f, 0f, 1.0f, 0.5f);
        }
        else if (_color == ColorType.Red)
        {
            meshRenderer.material.color = new Color(1.0f, 0f, 0f, 0.5f);
        }

        if (_color == PlayerController.LocalPlayer.Color)
        {
            GetComponent<BoxCollider>().enabled = false;
        }
    }
}
