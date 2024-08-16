using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ObjectStats : NetworkBehaviour
{
    public NetworkVariable<int>  score = new NetworkVariable<int>(10);
}
