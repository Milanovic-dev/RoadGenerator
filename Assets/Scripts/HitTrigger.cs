using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NimiMobilePack;

public class HitTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        
        if(other.CompareTag("Player"))
        {
            PlayerControl pc = PlayerControl.instance;

            if (pc == null) return;

            if(pc.CarManager.isInFastLane)
            {
                //NLevelManager.RestartScene();
            }

            gameObject.GetComponent<Collider>().enabled = false;
            StartCoroutine(FlipCar());
        }
    }

    IEnumerator FlipCar()
    {
        EntityMover em = GetComponent<EntityMover>();
        Animator Animator = transform.GetComponentInChildren<Animator>();

        em.doMove = false;
        Animator.Play("Flip");
        yield return new WaitForSecondsRealtime(Random.Range(0.5f,3.5f));
        Animator.Play("Idle");
        em.dist = 0f;
        em.doMove = true;
    }
}
