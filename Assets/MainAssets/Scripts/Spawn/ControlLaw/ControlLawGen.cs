using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrowdMP.Core
{
    public class ControlLawGen : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }


        public virtual CustomXmlSerializer<ControlLaw> createControlLaw()
        {
            LawMoveStraight law = new LawMoveStraight();

            return law;
        }

        public virtual ControlLawGen randDraw(GameObject agent, int id=0)
        {
            return null;
        }

        public virtual ControlLawGen randDraw(GameObject agent, ControlLawGen groupTemplate, int id = 0)
        {
            return randDraw(agent, id);
        }
    }
}
