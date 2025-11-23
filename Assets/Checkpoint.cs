using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Checkpoint : MonoBehaviour
{
    private void Awake()
    {
        // 自动确保是 Trigger
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        CharacterRespawn pr = other.GetComponent<CharacterRespawn>();
        if (pr != null)
        {
            pr.SetCheckpoint(transform.position);
        }
    }
}
