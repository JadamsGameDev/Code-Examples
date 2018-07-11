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
using System;
using CnControls;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;


public class PlayerControl : NetworkBehaviour {
	
	public int playerID;
	[SerializeField]
	float differenceToPillboxRange = 3f;
	public bool canControl = true;
	public float damage = 0.5f;
	GameObject projectile;
	[SerializeField]
	List<Transform> shotPositions;
	[SerializeField]
	float shootVelocity = 10;
	Animator animator;
	[SerializeField]
	Transform turret;
	[SerializeField]
	float shotDelay = 2f;
	bool canShoot = false;
	bool cr_Running = false;
	Transform mainCameraPos;
	Vector2 input;
	float pillBoxRange = 0f;
    MFManager mfm;

    public List<GameObject> activeBulletList = new List<GameObject>();
    public List<GameObject> idleBulletList = new List<GameObject>();

    

	//authenticates player and initializes all required refrences like projectiles, enemy pillboxes, network control and scoreboard 
	IEnumerator Start () {
		ScoreManager.AddNewPlayer ();
		playerID = Int32.Parse(GetComponent<NetworkIdentity> ().netId.ToString ());
		projectile = Resources.Load ("Prefabs/ProjectileTank", typeof(GameObject)) as GameObject;
		animator = gameObject.GetComponent<Animator> ();
		HandleNetworkInput ();
		
		mainCameraPos = Camera.main.transform;
		//muzzle flash
        mfm = GetComponent<MFManager>();
        while (FindObjectOfType<PillboxTrackingSystem>() == null)
        {
            yield return null;
        }
        foreach (PillboxTrackingSystem pTS in FindObjectsOfType<PillboxTrackingSystem>()) {
            pTS.SetupLocal();
        }
        if (isLocalPlayer) {
			
			GetComponent<HealthSystem> ().Respawn ();
		}
        
    }

    
	//handles who controls this player
	public void HandleNetworkInput () {
		if (!isLocalPlayer) {
			canControl = false;
		} else {
            EventTrigger trigger = GameObject.Find("Deploy Button").GetComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerDown;
            entry.callback.AddListener ((data) => { GetComponent<DeployPillbox>().SetCanDeploy(); } );
            trigger.triggers.Add(entry);
            
			#if !UNITY_EDITOR
			StartCoroutine(FindObjectOfType<PlaceScene> ().TurnOffUI ());
			#endif
		}
	}

	//handles shooting and movement
	void Update () {
       

		if(!canControl){
			return;
		}
		
		Vector3 rawInput = new Vector3(CnInputManager.GetAxis ("Horizontal"),0,CnInputManager.GetAxis ("Vertical"));
		float difference = (Quaternion.Inverse (transform.rotation) * mainCameraPos.rotation).eulerAngles.y;
		Vector3 rotatedVector = Quaternion.Euler (0, difference, 0) * rawInput;	
		input = new Vector2 (rotatedVector.x, rotatedVector.z);
		
		animator.SetFloat ("Horizontal", input.x);
		animator.SetFloat("Vertical", input.y);
		Vector3 Shootinput = new Vector3 (CnInputManager.GetAxis ("AimHorizontal"),0, CnInputManager.GetAxis ("AimVertical"));

		Shootinput = Quaternion.Euler (0, mainCameraPos.eulerAngles.y, 0) * Shootinput;
		if (Shootinput.x == 0f &&  Shootinput.z == 0f) {
			canShoot = false;
			return;
		}
		turret.rotation = Quaternion.LookRotation (Shootinput);
		if (Shootinput.magnitude > 0.8) {
				canShoot = true;
			if(cr_Running == false)
				StartCoroutine (ShootDelay ());
		} else {
			canShoot = false;
		}

	}
	IEnumerator ShootDelay () {
		cr_Running = true;
		while (canShoot == true) {
			CmdShoot ();
			yield return new WaitForSeconds (shotDelay);
			continue;
		}
		cr_Running = false;
	}
	void FixedUpdate () {


		if (isLocalPlayer)
			CmdSyncPlayer (input.x, input.y, turret.rotation.eulerAngles.y, transform.position, transform.eulerAngles.y);



	}
	//syncs player state to server
	[Command]
	public void CmdSyncPlayer (float inputX, float inputY, float turretAngle, Vector3 prefferedPosition, float prefferedAngle) {
		RpcSyncPlayer (inputX, inputY, turretAngle, prefferedPosition, prefferedAngle);
	}
	//syncs player state to clients
	[ClientRpc]
	public void RpcSyncPlayer (float inputX, float inputY, float turretAngle, Vector3 prefferedPosition, float prefferedAngle) {
		if (isLocalPlayer) {
			return;
		}
		if (inputX == null) {
			return;
		}
        animator.SetFloat("Horizontal", inputX);
        animator.SetFloat("Vertical", inputY);
        turret.eulerAngles = new Vector3(0, turretAngle, 0);

        Quaternion lookDirection = Quaternion.LookRotation (new Vector3(0,prefferedAngle,0), Vector3.up);
		float difference = (Quaternion.Inverse (lookDirection) * transform.rotation).eulerAngles.y;
		//if the player is too far from its actual rotation this corrects the rotation 
		if ((difference > 2 && difference <= 180) || (difference < 358 && difference > 180)) {
			transform.eulerAngles = new Vector3 (0, prefferedAngle, 0);
		}
		if (Vector3.Distance (prefferedPosition, transform.position) > 0.0002f) {
			transform.position = prefferedPosition;
            print("called");
		}
	}

	public void OnClientStart () {
		ClientScene.RegisterPrefab (projectile);
		//playerID = GetComponent<NetworkIdentity> ().netId.ToString()

	}

	[Command]
	public void CmdShoot () {
		RpcShoot ();
	}

	[ClientRpc]
	public void RpcShoot () {
		if (pillBoxRange == 0) {
			pillBoxRange = FindObjectOfType<PillboxTrackingSystem> ().range;

		}
		if (GetComponent<HealthSystem> ().isDead == false) {
			foreach (Transform t in shotPositions) {
                mfm.PlayerRelocateParticleSystem(t.position, t.rotation, gameObject);
				RelocateBullet(t.position, t.rotation, damage);								
			}
			turret.GetComponent<Animator>().Play ("Attack");
		}
	}
	//reuses a bullet from a pool of already spawned bullets
	void RelocateBullet(Vector3 pos, Quaternion rot, float damage) {
        if (idleBulletList.Count > 0) {
            idleBulletList[0].transform.position = pos;
            idleBulletList[0].transform.rotation = rot;
            idleBulletList[0].SetActive(true);
            Projectile shotProjectile = idleBulletList[0].GetComponent<Projectile>();
            shotProjectile.range = pillBoxRange - differenceToPillboxRange;
            shotProjectile.velocity = shootVelocity;
            shotProjectile.iD = playerID;
            StartCoroutine(shotProjectile.BulletLife(gameObject));
            activeBulletList.Add(idleBulletList[0]);
            idleBulletList.Remove(idleBulletList[0]);
			shotProjectile.damage = damage;
        }
        else {
            GameObject newBullet = Instantiate(projectile, pos, rot) as GameObject;
            activeBulletList.Add(newBullet);
            Projectile shotProjectile = newBullet.GetComponent<Projectile>();
            shotProjectile.range = pillBoxRange - differenceToPillboxRange;
            shotProjectile.velocity = shootVelocity;
            shotProjectile.iD = playerID;
            StartCoroutine(shotProjectile.BulletLife(gameObject));
			shotProjectile.damage = damage;
        }
    }
}


