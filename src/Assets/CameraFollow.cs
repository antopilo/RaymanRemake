using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform playerObject;
    public Transform playerBehind;

    public float distanceFromObject = 6f;
    public float smoothSpeed = 0.125f;

    void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Backspace))
            CenterCamToPlayer();

        Vector3 lookOnObject = playerObject.position - transform.position;

        transform.forward = Vector3.Lerp(transform.forward, lookOnObject.normalized, smoothSpeed);

        Vector3 playerLastPosition;
        playerLastPosition = playerObject.position - lookOnObject.normalized * distanceFromObject;
        playerLastPosition.y = playerObject.position.y + distanceFromObject / 2;

        transform.position = Vector3.Lerp(transform.position, playerLastPosition, smoothSpeed);
    }

    public void CenterCamToPlayer()
    {
        var lookTarget = playerObject.position - transform.position;

        transform.forward = lookTarget.normalized;
        transform.position = playerBehind.position;

    }
}
