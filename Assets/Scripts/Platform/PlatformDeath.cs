using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformDeath : MonoBehaviour
{
    public bool isDeadly = true;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isDeadly) return;

        if (collision.collider.CompareTag("Player"))
        {
            CharacterRespawn pr = collision.collider.GetComponent<CharacterRespawn>();
            if (pr != null) pr.Die();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isDeadly) return;

        if (other.CompareTag("Player"))
        {
            CharacterRespawn pr = other.GetComponent<CharacterRespawn>();
            if (pr != null) pr.Die();
        }
    }
}

