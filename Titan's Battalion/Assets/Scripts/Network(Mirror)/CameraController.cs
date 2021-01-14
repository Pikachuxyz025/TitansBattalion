using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
//using UnityEngine.InputSystem;

public class CameraController : NetworkBehaviour
{
    [SerializeField] private Transform playerCameraTransform = null;
    [SerializeField] private float speed = 20f, screenBoarderThickness = 10f;
    [SerializeField] private Vector2 screenXLimits = Vector2.zero, screenzLimits = Vector2.zero;

    [SyncVar]
    public bool isCameraMoblie = true;

    private Vector2 previousInput;
    public PlayerInfo player;

    public override void OnStartAuthority()
    {
        playerCameraTransform.gameObject.SetActive(true);
    }


    [ClientCallback]
    private void Update()
    {
        if (!hasAuthority || !Application.isFocused) { /*Debug.Log("Section not authorized, application not focused");*/ return; }

        if (isCameraMoblie)
            UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        Vector3 pos = playerCameraTransform.position;

        if (player.playerNum == 1)
        {
            if (Input.GetKey(KeyCode.A))
                pos += Vector3.left * speed * Time.deltaTime;
            if (Input.GetKey(KeyCode.S))
                pos += Vector3.back * speed * Time.deltaTime;
            if (Input.GetKey(KeyCode.D))
                pos += Vector3.right * speed * Time.deltaTime;
            if (Input.GetKey(KeyCode.W))
                pos += Vector3.forward * speed * Time.deltaTime;
        }
        else if (player.playerNum == 2)
        {
            if (Input.GetKey(KeyCode.D))
                pos += Vector3.left * speed * Time.deltaTime;
            if (Input.GetKey(KeyCode.W))
                pos += Vector3.back * speed * Time.deltaTime;
            if (Input.GetKey(KeyCode.A))
                pos += Vector3.right * speed * Time.deltaTime;
            if (Input.GetKey(KeyCode.S))
                pos += Vector3.forward * speed * Time.deltaTime;
        }

        pos.x = Mathf.Clamp(pos.x, screenXLimits.x, screenXLimits.y);
        pos.z = Mathf.Clamp(pos.z, screenzLimits.x, screenzLimits.y);

        playerCameraTransform.position = pos;
    }
}
