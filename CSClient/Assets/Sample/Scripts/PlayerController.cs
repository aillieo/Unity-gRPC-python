using Grpc.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sample
{
    public class PlayerController : MonoBehaviour
    {
        internal Vector3 currentTarget;

        private void Start()
        {
            currentTarget = this.transform.position;   
        }

        private void Update()
        {
            //if (Vector3.SqrMagnitude(currentTarget - transform.position) < 1)
            //{
            //    return;
            //}

            this.transform.position = currentTarget;
        }
    }
}
