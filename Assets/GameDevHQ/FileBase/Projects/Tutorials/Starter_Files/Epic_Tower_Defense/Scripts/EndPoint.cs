﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndPoint : MonoBehaviour
{
    public delegate void EndPointReached();
    public static event EndPointReached onEndPointReached;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame SO TRY TO ONLY USE IT FOR PLAYER INPUT
    void Update()
    {
        
    }

    public void SendEndPointReachedNotification()
    {
        //This is super duper short hand for the commented out code below
        onEndPointReached?.Invoke();

        //if (onEvent != null)
        //{
        //    onEvent();
        //}
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Contains("Mech"))
        {
            var enemyScript = other.GetComponent<Enemy>();
            enemyScript.SetToStandby();

            SendEndPointReachedNotification();
        }
    }
}