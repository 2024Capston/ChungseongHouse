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

        meshRenderer.material.SetObjectColor(_color);
        meshRenderer.material.SetPlayerColor(PlayerController.LocalPlayer.Color);

        if (_color == PlayerController.LocalPlayer.Color)
        {
            GetComponent<BoxCollider>().enabled = false;
        }
    }
}
