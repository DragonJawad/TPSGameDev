using UnityEngine;
using System.Collections;

public class AIcharacter : MonoBehaviour {

    NavMeshAgent agent;
    CharacterMovement CharMove;
    
    public Transform target;
    float targetTolerance = 1;

    Vector3 targetPos;

	// Use this for initialization
	void Start () {
        agent = GetComponentInChildren<NavMeshAgent>();
        CharMove = GetComponent <CharacterMovement>();
	}
	
	// Update is called once per frame
	void Update () {
        if (target != null)
        {
            if ((target.position - targetPos).magnitude > targetTolerance)
            {
                targetPos = target.position;
                agent.SetDestination(targetPos);
            }

            agent.transform.position = transform.position;

            CharMove.Move(agent.desiredVelocity);
        }
        else {
            CharMove.Move(Vector3.zero); // Still need to pass a move value
        }
	}
}
