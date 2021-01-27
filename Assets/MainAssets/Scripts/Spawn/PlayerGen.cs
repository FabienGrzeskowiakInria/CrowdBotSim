using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrowdMP.Core
{
    public class PlayerGen : MonoBehaviour
    {
        [Tooltip("Name of the player's mesh")]
        public GameObject playerMesh; // = "Player";
        [Tooltip("Radius of the player during the trial")]
        public float playerRadius = 0.33f;
        //public Vector3 playerPosition = new Vector3();
        //public Vector3 playerRotation = new Vector3();

        [Tooltip("Is the player part of the simulation during the trial, or just a camera")]
        public bool in_sim = true;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public virtual TrialPlayer createPlayer()
        {
             if(in_sim){
                TrialRegularPlayer player = new TrialRegularPlayer();
                player.mesh = playerMesh.name;
                player.radius = playerRadius;
                player.Position.vect = gameObject.transform.position;
                player.Rotation.vect = gameObject.transform.rotation.eulerAngles;

                ControlLawGen lawGen = gameObject.GetComponent<ControlLawGen>();
                if (lawGen!=null)
                    player.xmlControlLaw = lawGen.createControlLaw();
                else
                    player.xmlControlLaw = new LawCamControlEditor();

                player.xmlControlSim = null;

                return player;
            }
            else
            {
                TrialCamPlayer player = new TrialCamPlayer();
                player.mesh = playerMesh.name;
                player.radius = playerRadius;
                player.Position.vect = gameObject.transform.position;
                player.Rotation.vect = gameObject.transform.rotation.eulerAngles;

                player.in_sim = in_sim;

                ControlLawGen lawGen = gameObject.GetComponent<ControlLawGen>();
                if (lawGen!=null)
                    player.xmlControlLaw = lawGen.createControlLaw();
                else
                    player.xmlControlLaw = new LawCamControlEditor();

                player.xmlControlSim = null;

                return player;
            }
        }
    }
}
