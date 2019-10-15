﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallManager : MonoBehaviour
{
    public bool m_active = false;
    public int m_spawnInterval = 15;
    public GameObject m_ball = null;
    public List<GameObject> m_spawnPoints = new List<GameObject>();

    private bool m_isSpawning = false;
    private int m_currentBallCount = 0;
    private List<GameObject> m_ballPool = new List<GameObject>();
    private Vector3 m_ballPoolPosition = new Vector3(999f,999f,999f);
    private int m_ballPoolSize = 100;
    IEnumerator m_ballSpawner;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(m_ball, "Ball prefab needed in BallManager Script");
        GameObject pool = new GameObject();
        pool.name = "Ball Pool";

        for(int i = 0; i < m_ballPoolSize; ++i)
        {
            var newBall = Instantiate(m_ball,pool.transform,false);
            newBall.SetActive(false);
            m_ballPool.Add(newBall);
        }
        if(m_active)
        {
         m_ballSpawner = BallSpawner();
         StartCoroutine("BallSpawner");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!m_active && m_isSpawning)
        {
            StopCoroutine(m_ballSpawner);
        }
        else if(m_active && !m_isSpawning)
        {
            StartCoroutine(m_ballSpawner);
        }

        if(Input.GetKeyDown(KeyCode.P))
        {
            RemoveBall(m_ballPool[2]);
        }
    }

    // removes specific ball and reorganises the pool
    void RemoveBall(GameObject ball)
    {
        ball.SetActive(false);
        m_currentBallCount--;

        for(int i = 0; i < m_currentBallCount; ++i)
        {
            if(!m_ballPool[i].activeSelf)
            {
                int count = i;
                while(m_ballPool[count + 1].activeSelf)
                {
                    var temp = m_ballPool[count + 1];
                    m_ballPool[count + 1] = m_ballPool[count];
                    m_ballPool[count] = temp;
                    count++;
                }
            }
        }
    }

    public void ResetBalls()
    {
        foreach(GameObject ball in m_ballPool)
        {
            ball.SetActive(false);
            ball.transform.position = m_ballPoolPosition;
            ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
        m_currentBallCount = 0;
        StopCoroutine("BallSpawner");
        StartCoroutine("BallSpawner");
    }

    // ball spawner will spawn a set of balls every m_spawnInterval and only spawn if there is no player or ball in the way
    // instead of instantiating and destroy setup an object pool
    IEnumerator BallSpawner()
    {
        m_isSpawning = true;
        while(true)
        {
            // loops through all the spawn points
            foreach(var point in m_spawnPoints)
            {
                // activates the ball
                var tempBall = m_ballPool[m_currentBallCount];
                tempBall.SetActive(true);
                tempBall.transform.position = point.transform.position;
                m_currentBallCount++;

                // Checks if there is already a ball or player
                // on the spawn point
                var colliders = Physics.OverlapSphere(point.transform.position,.5f);
                int ballCount = 0; // used to grab whats inside the sphere
                foreach(var obj in colliders)
                {
                    if(obj.gameObject.layer == 9)
                    {
                        ballCount++;
                    }
                    else if(obj.gameObject.layer == 10)
                    {
                        ballCount++;
                    }
                }

                // sends the ball back to the pool
                if(ballCount > 1)
                {
                    tempBall.transform.position = m_ballPoolPosition;
                    tempBall.SetActive(false);
                    m_currentBallCount--;
                }
            }
            for(int i = m_spawnInterval; i > 0; i--)
            {
                // if the game is in a paused state then this will run
                // basically resuming the corroutine every 1 second
                // to check if the game has started yet
                if(!RoundManager.m_isPlaying)
                {
                    i++;
                    yield return new WaitForSeconds(1f);
                    continue;
                }
                Debug.Log(i);
                yield return new WaitForSeconds(1f);
            }

            if(m_spawnInterval == 0)
            {
                yield return new WaitForSeconds(m_spawnInterval);
            }
        }
    }

    void OnDrawGizmos()
    {
        foreach(var point in m_spawnPoints)
        {
            Gizmos.DrawWireSphere(point.transform.position,.5f);
        }
    }

}
