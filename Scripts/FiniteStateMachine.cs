using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FiniteStateMachine : MonoBehaviour {
    private enum State
    {
        Idle,
        Chase,
        Wander,
        
    }

    public GameObject[] Enemies;
    public GameObject Enemy;

    public float HealthPoints = 100;
    public float chaseDistance = 10f;

    private State _activeState = State.Idle;

    public float Speed = 10f;
    public float sensorLenght = 10f;
    private float attackLength = 1f;
    private float directionValue = 1.0f;
    Collider myCollider;
    private float turnValue = 0.0f;
    private float turnSpeed = 25.0f;
    private int flag = 0;

    public bool attacking = false;
    public bool isDead = false;

    private Vector3 sensorPosition = new Vector3(0f, 0f, 0f);
    private float sensorAngle = 30;

    void Start()
    {
        myCollider = transform.GetComponent<Collider>();
    }
    void Update()
    {
        if (HealthPoints <= 0)
        {
            Die();
        }
        if (FindClosestEnemy() != null)
        {
            FindClosestEnemy();
        }

        StateTransition();
        StateAction();
    }

    public void StateTransition()
    {
        if (_activeState == State.Idle)
        {
            if (Input.GetKeyDown("space"))
            {
                _activeState = State.Wander;
            }
        }
        else if (_activeState == State.Wander)
        {
            if (Input.GetKeyDown("space"))
            {
                _activeState = State.Idle;
            }
            else if (FindClosestEnemy() != null)
            {
                if (Vector3.Distance(FindClosestEnemy().transform.position, transform.position) <= chaseDistance)
                {
                    _activeState = State.Chase;
                }
            }
        }
        else if (_activeState == State.Chase)
        {
            if (FindClosestEnemy() != null)
            {
                if (Vector3.Distance(FindClosestEnemy().transform.position, transform.position) >= chaseDistance)
                {
                    _activeState = State.Wander;
                }
            }
            else if (FindClosestEnemy() == null)
            {
                _activeState = State.Wander;
            }
        }
    }
    public void StateAction()
    {
        if (_activeState == State.Idle)
        {
            return;
        }
        else if (_activeState == State.Wander)
        {
            Wander();
        }
        else if (_activeState == State.Chase)
        {
            Chase();
        }
    }
    private void Wander()
    {

        ObstacleAvoidance();

        //Random wander if not about to hit obstacle
        if (flag == 0)
        {
            turnValue = Random.Range(-10, 10);
            directionValue = 1f;
        }

        Move();

        flag = 0;
        
    }

    private void Chase()
    {
        attacking = false;

        ObstacleAvoidance();

        if (flag == 0)
        {
            if (FindClosestEnemy() != null)
            {
                transform.LookAt(FindClosestEnemy().transform, Vector3.up);
                turnValue = 0;
            }
        }

        Attack();

        if (!attacking)
        {
            Move();
        }

        flag = 0;
        attacking = false;
        turnValue = 0;
    }

    private void Move()
    {
        transform.Rotate(Vector3.up * (turnSpeed * turnValue) * Time.deltaTime);

        transform.position += transform.forward * (Speed * directionValue) * Time.deltaTime;
    }

    private GameObject FindClosestEnemy()
    {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag("Agent");
        if (gos == null)
        {
            return null;
        }
        else
        {
            GameObject closest = null;
            float distance = Mathf.Infinity;
            Vector3 position = transform.position;
            foreach (GameObject go in gos)
            {
                Vector3 diff = go.transform.position - position;
                float curDistance = diff.sqrMagnitude;
                if (curDistance < distance && go != this.gameObject)
                {
                    closest = go;
                    distance = curDistance;
                }
            }
            if (closest == null)
            {
                return null;
            }
            return closest;
        }
    }

    private void ObstacleAvoidance()
    {
        RaycastHit hit;
        Vector3 sensorPos = transform.position + sensorPosition;

        //Front sensor
        if (Physics.Raycast(sensorPos, transform.forward, out hit, sensorLenght))
        {
            if (hit.collider.tag != "Obstacle" || hit.collider == myCollider)
            {
                if (hit.collider.tag == "Agent")
                {
                    directionValue = 0.8f;
                    flag++;
                }
            }

            directionValue = 0.8f;
            flag++;
        }

        //left sensor
        if (Physics.Raycast(sensorPos, Quaternion.AngleAxis(-sensorAngle, transform.up) * transform.forward, out hit, sensorLenght))
        {
            if (hit.collider.tag != "Obstacle" || hit.collider == myCollider)
            {
                if (hit.collider.tag == "Agent")
                {
                    turnValue += 1;
                    directionValue = 0.8f;
                    flag++;
                }
            }
            turnValue += 1;
            directionValue = 0.8f;
            flag++;
        }

        //Right sensor
        if (Physics.Raycast(sensorPos, Quaternion.AngleAxis(sensorAngle, transform.up) * transform.forward, out hit, sensorLenght))
        {
            if (hit.collider.tag != "Obstacle" || hit.collider == myCollider)
            {
                if (hit.collider.tag == "Agent")
                {
                    turnValue -= 1;
                    directionValue = 0.8f;
                    flag++;
                }
            }
            turnValue -= 1;
            directionValue = 0.8f;
            flag++;
        }
        //if left and right sensor both hit
        if (Physics.Raycast(sensorPos, Quaternion.AngleAxis(sensorAngle, transform.up) * transform.forward, out hit, sensorLenght) && Physics.Raycast(sensorPos, Quaternion.AngleAxis(-sensorAngle, transform.up) * transform.forward, out hit, sensorLenght))
        {
            if (hit.collider.tag != "Obstacle" || hit.collider == myCollider)
            {
                if (hit.collider.tag == "Agent")
                {
                    turnValue -= 1;
                    directionValue = 0.5f;
                    flag++;
                }
            }
            turnValue -= 1;
            directionValue = 0.5f;
            flag++;
        }
    }

    private void Attack()
    {
        RaycastHit hit;
        Vector3 sensorPos = transform.position + sensorPosition;

        if (Physics.Raycast(sensorPos, transform.forward, out hit, attackLength))
        {
            if (hit.collider.tag == "Agent")
            {
                Enemy = hit.collider.gameObject;
                attacking = true;
                Enemy.GetComponent<FiniteStateMachine>().HealthPoints -= Random.Range(0, 10);
            }
        }
    }

    private void Die()
    {
        isDead = true;
        Destroy(this.gameObject);
    }
}
