using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

// <summary>
/// Player agent that can be control using different input devices
/// </summary>
public class CB_Cam: MonoBehaviour
{
    public void clear(){}
//     GameObject    headObject;
// #if MIDDLEVR
//     public GameObject HeadNode=null;
//     public GameObject HandNode=null;
// #endif

//     public abstract GameObject getHeadObject();
//     public abstract void clear();

// #if MIDDLEVR
//     public override Vector3 Position { get { return HeadNode != null ? new Vector3(HeadNode.transform.position.x, transform.position.y, HeadNode.transform.position.z) : transform.position; } }

//     public override void Translate(Vector3 translation)
//     {
//         if (HandNode != null)
//         {
//             Vector3 forward = HandNode.transform.forward;
//             forward.y = 0;
//             transform.position = transform.position + translation.z * forward;
//         } else
//             base.Translate(translation);
//     }

//     public override void Rotate(Vector3 rotation)
//     {
//         if (HeadNode)
//         {
//             transform.RotateAround(HeadNode.transform.position, transform.up, rotation.y);
//         } else
//             base.Rotate(rotation);
//     }
// #endif
}