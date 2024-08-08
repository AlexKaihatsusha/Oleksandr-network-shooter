using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Meteor : NetworkBehaviour
{
    private float moveSpeed = 2f;


    public void Init(Vector3 position)
    {
        transform.position = position;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    

    // Update is called once per frame
    void Update()
    {
        transform.position += new Vector3(0f, -1f, 0f) * (moveSpeed * Time.deltaTime);
    }
}
