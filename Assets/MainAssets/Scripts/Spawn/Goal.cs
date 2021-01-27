using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrowdMP.Core
{
    public class Goal : MonoBehaviour {

        public Goal[] nextPoint;

        // Use this for initialization
        void Start() {

        }

        // Update is called once per frame
        void Update() {

        }

        public Goal getNextPoint(Goal entryPoint)
        {
            if (nextPoint.Length == 0)
                return this;
            if (nextPoint.Length == 1)
                return nextPoint[0];

            int selection = Random.Range(0, nextPoint.Length);
            if (nextPoint[selection] == entryPoint)
                selection = (selection + Random.Range(1, nextPoint.Length)) % nextPoint.Length;

            return nextPoint[selection];
        }


        public Vector3 randPosInArea()
        {
            Vector3 newVec = gameObject.transform.position
                    + gameObject.transform.right * gameObject.transform.lossyScale.x * Random.Range(-0.5f, 0.5f) * 10f
                    + gameObject.transform.forward * gameObject.transform.lossyScale.z * Random.Range(-0.5f, 0.5f) * 10f;

            return newVec;
        }
    }
}
