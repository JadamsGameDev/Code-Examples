/*************************************************************************
 * 
 * MIXT CONFIDENTIAL
 * ________________
 * 
 *  [2016] - [2018] Mixt Ltd
 *  All Rights Reserved.
 * 
 * NOTICE:  All information contained herein is, and remains
 * the property of Mixt Ltd and its suppliers,
 * if any.  The intellectual and technical concepts contained
 * herein are proprietary to Mixt Ltd
 * and its suppliers and may be covered by U.S. and Foreign Patents,
 * patents in process, and are protected by trade secret or copyright law.
 * Dissemination of this information or reproduction of this material
 * is strictly forbidden unless prior written permission is obtained
 * from Mixt Ltd.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Leap;
using Leap.Unity;
/// <summary>
/// A Gesture is used to easily create complex boolean expressions that check the rotations of any joint in the hands.
/// Gestures can hold multiple Transforms and Transforms hold multiple expressions, when isGesturing is called it will evaluate all transforms and their expreesions and only return true if all are true
/// 
/// Example of use: 
///		Gesture triggerPull;
///		if(triggerPull.IsGesturing()) {
///			fire;
///		}
///		
/// </summary>
public class Gesture : MonoBehaviour {

    [SerializeField]
    RigidHand hand;
	[SerializeField]
	private string name;
	public string Name { get { return name; } }
	[SerializeField]
	List<TransformToBool> transforms;
    
    public bool isPalmUp() {

        return hand.GetHand().PalmNormal.ToVector3().y > 0;

    }
		
	public bool isPalmForward() {

        if (hand.GetHand() == null)
        {
            return false;
        }
		return hand.GetHand().PalmNormal.ToVector3().z > 0.64;

	}
	/// <summary>
	/// Checks all listed TransformToBools and their Expressions
	/// </summary>
	/// <returns>
	/// returns true if all Expressions are true
	/// </returns>
	public bool IsGesturing() {

		bool gesturing = false;
		foreach (TransformToBool tb in transforms) {
			Transform t = tb.toEvaluate;
			foreach (Expresion e in tb.expresions) {

				//inverts the values so the logic will work on anything
				Vector3 angles = t.localEulerAngles;
				if (e.upperLimit < e.lowerLimit) {
					e.upperLimit *= -1;
					e.lowerLimit *= -1;
					angles *= -1;
				}
				switch (e.value) {
					case Expresion.Values.ROTATION_X:
						if (angles.x > e.lowerLimit && angles.x < e.upperLimit) {
							gesturing = true;
						} else {
							return false;
						}
						break;
					case Expresion.Values.ROTATION_Y:
						if (angles.y > e.lowerLimit && angles.y < e.upperLimit) {
							gesturing = true;
						} else {
							return false;
						}
						break;
					case Expresion.Values.ROTATION_Z:
						if (angles.z > e.lowerLimit && angles.z < e.upperLimit) {
							gesturing = true;
							
						} else {
							
							return false;
						}
						break;
					default:
						break;
				}
			} 
		}
		return gesturing;
	}
	
}
/// <summary>
/// A transform that expresions can be refrenced to
/// </summary>
[Serializable]
public class TransformToBool {

	string name;

	public Transform toEvaluate;

	public List<Expresion> expresions;
		
}
/// <summary>
/// A single expression. 
/// 
/// Must be listed in a TransformToBool in order to work
/// </summary>
[Serializable]
public class Expresion {

	public enum Values {ROTATION_X, ROTATION_Y, ROTATION_Z}

	public Values value;
	
	public float upperLimit;
	
	public float lowerLimit;
	
}

