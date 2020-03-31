using System.Collections;
using System.Collections.Generic;
using UnityEngine;


    public class ListenerResponse : MonoBehaviour
    {
        protected GameEventListener listener;

        private void Awake()
        {
            listener = GetComponent<GameEventListener>();
        }
    }
