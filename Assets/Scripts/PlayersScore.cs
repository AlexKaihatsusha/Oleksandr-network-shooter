using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PlayersScore : NetworkBehaviour
{
   public static PlayersScore Singleton { get; private set; }
   //by default is Server authoritative, and read everyone
   public NetworkVariable<int> score = new NetworkVariable<int>(0);
   public void Awake()
   {
      if (Singleton == null)
      {
         Singleton = new PlayersScore();
      }
   }

   //called by a server
   public void UpdateTheScore(int score)
   {
      //update current value
      this.score.Value += score;
   }
}
