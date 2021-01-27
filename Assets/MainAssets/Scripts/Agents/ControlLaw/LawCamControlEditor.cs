using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;


namespace CrowdMP.Core
{
    public class LawCamControlEditor : ControlLaw
    {

        [XmlAttribute]
        public float lookSpeedH;
        [XmlAttribute]
        public float lookSpeedV;
        [XmlAttribute]
        public float zoomSpeed;
        [XmlAttribute]
        public float dragSpeed;

        public LawCamControlEditor()
        {
            lookSpeedH = 2f;
            lookSpeedV = 2f;
            zoomSpeed = 2f;
            dragSpeed = 3f;
        }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public LawCamControlEditor(float rotSpeedH, float rotSpeedV, float fwdSpeed, float sideSpeed)
        {
            lookSpeedH = rotSpeedH;
            lookSpeedV = rotSpeedV;
            zoomSpeed = fwdSpeed;
            dragSpeed = sideSpeed;

        }



        public bool computeGlobalMvt(float deltaTime,
                                        out Vector3 translation,
                                        out Vector3 rotation)
        {
            float yaw = 0f;
            float pitch = 0f;
            translation = new Vector3(0, 0, 0);
            rotation = new Vector3(0, 0, 0);
            //Look around with Right Mouse
            if (Input.GetMouseButton(1))
            {
                yaw = lookSpeedH * Input.GetAxis("Mouse X");
                pitch = - lookSpeedV * Input.GetAxis("Mouse Y");

                rotation = new Vector3(pitch, yaw, 0f);
            }

            //drag camera around with Middle Mouse
            if (Input.GetMouseButton(2))
            {
                translation = new Vector3(-Input.GetAxisRaw("Mouse X") * Time.deltaTime * dragSpeed, -Input.GetAxisRaw("Mouse Y") * Time.deltaTime * dragSpeed, 0);
            }

            //Zoom in and out with Mouse Wheel
            translation= translation + new Vector3(0, 0, Input.GetAxis("Mouse ScrollWheel") * zoomSpeed);

            return true;
        }

        public bool applyMvt(Agent a, Vector3 translation, Vector3 rotation)
        {
            a.transform.eulerAngles += rotation;
            a.Translate(translation);
            return true;
        }

        public void initialize(Agent a)
        {
        }
    }
}