using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    public AudioClip footStep;
    public AudioClip jump;
    public AudioClip groundRoll;
    public AudioClip runJump;
    public AudioSource audioSource;

    Random random = new Random();

    public void FootStep()
    {
        audioSource.PlayOneShot(footStep);
    }

    public void Jump()
    {
        if (Random.Range(0.0f, 1.0f) > 0.5)
        {
            audioSource.PlayOneShot(jump);
        } else
        {
            RunJump();
        }
        
    }

    public void GroundRoll()
    {
        audioSource.PlayOneShot(groundRoll);
    }

    public void RunJump()
    {
        audioSource.PlayOneShot(runJump);
    }
}
