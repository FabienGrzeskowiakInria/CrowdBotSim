using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrowdMP.Core
{
    public class ControlSimGen : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }


        public virtual CustomXmlSerializer<TrialControlSim> createControlSim(int groupSeed)
        {
            RVOconfig sim = new RVOconfig();

            return sim;
        }

        public virtual ControlSimGen randDraw(GameObject agent, int simID = 0, int groupID = 0)
        {
            return null;
        }
    }
}