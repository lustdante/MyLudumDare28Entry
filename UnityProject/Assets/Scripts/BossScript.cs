﻿using UnityEngine;
using System.Collections;
using Holoville.HOTween;
using Holoville.HOTween.Plugins;

public enum MOVE_TYPE {
	JUSTRISE,
	BULLETHELL,
	LASEREYE,
	SKYROCKET,
	SKYROCKET2
};

public class BossScript : MonoBehaviour {

	public GameObject Boss;
	Projectile _Bullet_Prefab;
	Projectile _Laser_Prefab;
	Projectile _Scope_Prefab;
	Projectile _Exp_Prefab;
	Projectile _Rock_Prefab;
	Projectile _Rocket_Prefab;

	GameObject _Biglaser_Prefab;
	GameObject _Bigexp_Prefab;

	[HideInInspector]
	public bool makingMove;
	[HideInInspector]
	public bool force_stop;

	public Transform bullet_nozzle;
	public Transform laser_eye;

	public Transform player_pos;

	public tk2dSprite thrust1;
	public tk2dSprite thrust2;

	public tk2dSprite arrow_left;
	public tk2dSprite arrow_mid;
	public tk2dSprite arrow_right;

	public tk2dSprite rocket_nozzle;
	public tk2dSprite rocket_exp;

	private bool rising;

	private CameraScript cam;
	private static BossScript instance;
	public static BossScript Instance
	{
		get {
			if(instance == null)
			{
				Debug.LogError ("Error");
			}
			return instance;
		}
	}
	
	void Awake () {
		instance = this;
		cam = CameraScript.Instance;
		_Bullet_Prefab = (Resources.Load ("Prefabs/Bullet") as GameObject).GetComponent<Projectile>();
		_Laser_Prefab = (Resources.Load ("Prefabs/Laser") as GameObject).GetComponent<Projectile>();
		_Scope_Prefab = (Resources.Load ("Prefabs/Scope") as GameObject).GetComponent<Projectile>();
		_Exp_Prefab = (Resources.Load ("Prefabs/Explosion") as GameObject).GetComponent<Projectile>();
		_Rock_Prefab = (Resources.Load ("Prefabs/Rock") as GameObject).GetComponent<Projectile>();
		_Rocket_Prefab = (Resources.Load ("Prefabs/Rocket") as GameObject).GetComponent<Projectile>();

		_Biglaser_Prefab = (Resources.Load ("Prefabs/Biglaser") as GameObject);
		_Bigexp_Prefab = (Resources.Load ("Prefabs/BigExp") as GameObject);
	}

	public void Restart(){
		Boss.SetActive(false);
		Boss.transform.position = new Vector3(-26.90941f, 0, 0);
	}

	public void PrepareFinale(){
		Boss.SetActive(false);
	}

	public void EndingPos(float cy){
		Boss.transform.position = new Vector3(88, -5, 0);
		Boss.SetActive(true);
		StartCoroutine(Rise(cy, 0, 20.0f));
	}

	public void EndingShot(Vector3 pos){
		StartCoroutine(Finale(pos));
	}

	IEnumerator Finale(Vector3 target){
		GameObject pj = Instantiate(_Biglaser_Prefab, new Vector3(laser_eye.position.x, laser_eye.position.y, 0), Quaternion.identity) as GameObject;
		pj.SetActive(true);
		Tweener twn = HOTween.To (pj.transform, 0.3f, new TweenParms().Prop ("position", target));
		yield return new WaitForSeconds(0.3f);
		Destroy(pj, 0.1f);
		GameObject exp = Instantiate(_Bigexp_Prefab, new Vector3(target.x, target.y, 0), Quaternion.identity) as GameObject;
		exp.SetActive(true);
		tk2dSpriteAnimator anim = exp.GetComponent<tk2dSpriteAnimator>();
		anim.Play ("Explosion");
		while(anim.IsPlaying("Explosion")){
			yield return null;
		}
		Destroy(exp);
	}

	public void Activate(){
		Boss.SetActive(true);
	}

	public void MakeMove(MOVE_TYPE type, float cy) {
		makingMove = true;
		switch(type){
		case MOVE_TYPE.JUSTRISE:
			StartCoroutine(JustRise(cy));
			break;
		case MOVE_TYPE.BULLETHELL:
			StartCoroutine(RiseAndShoot(cy));
			break;
		case MOVE_TYPE.LASEREYE:
			StartCoroutine(RiseAndLaser(cy));
			break;
		case MOVE_TYPE.SKYROCKET:
			StartCoroutine(RiseAndRocket(cy));
			break;
		case MOVE_TYPE.SKYROCKET2:
			StartCoroutine(RiseAndLastRocket(cy));
			break;
		default:
			break;
		}
	}

	IEnumerator JustRise(float cy){
		yield return StartCoroutine(Rise(cy, 20));
		makingMove = false;
	}

	IEnumerator RiseAndShoot(float cy){
		yield return StartCoroutine(Rise(cy, Random.Range(10f, 20f)));

		Shoot(0f, 0.35f);
		Shoot(15f, 0.35f);
		Shoot(-15f, 0.35f);
		yield return new WaitForSeconds(0.5f);
		Shoot(0f, 0.35f);
		Shoot(15f, 0.35f);
		Shoot(-15f, 0.35f);
		yield return new WaitForSeconds(0.5f);
		Shoot(0f, 0.35f);
		Shoot(15f, 0.35f);
		Shoot(-15f, 0.35f);
		makingMove = false;
	}

	IEnumerator RiseAndLaser(float cy){
		yield return StartCoroutine(Rise(cy, Random.Range(10f, 20f)));
		Transform t = Aim (1f, 1f);
		yield return new WaitForSeconds(2f);
		Laser (t.position);
		makingMove = false;
	}

	IEnumerator RiseAndRocket(float cy){
		yield return StartCoroutine(Rise(cy, 10));
		for(int i=0; i<3; i++){
			rocketTimer = 0;
			yield return StartCoroutine(Rocket());
		}
		yield return new WaitForSeconds(1f);
		yield return StartCoroutine(Warning(1.0f, 1));
		FallRocket(1);
		yield return new WaitForSeconds(0.5f);
		yield return StartCoroutine(Warning(1.0f, 0));
		FallRocket(0);
		yield return new WaitForSeconds(0.5f);
		yield return StartCoroutine(Warning(1.0f, 0));
		FallRocket(0);
		makingMove = false;
	}

	IEnumerator RiseAndLastRocket(float cy){
		yield return StartCoroutine(Rise(cy, 10));
		for(int i=0; i<5; i++){
			rocketTimer = 0;
			yield return StartCoroutine(Rocket());
		}
		yield return new WaitForSeconds(1f);
		cam.Shake();
		yield return new WaitForSeconds(1f);
		yield return StartCoroutine(Rise(cy, 10));
		yield return StartCoroutine(Warning(1.0f, 2));
		FallRock(0);
		yield return new WaitForSeconds(0.5f);
		yield return StartCoroutine(Warning(1.0f, 3));
		FallRock(1);
		yield return new WaitForSeconds(0.5f);
		yield return StartCoroutine(Warning(1.0f, 2));
		FallRock(0);
		yield return new WaitForSeconds(0.5f);
		yield return StartCoroutine(Warning(1.0f, 3));
		FallRock(1);
		makingMove = false;
	}

	IEnumerator Rise(float py, float amount){
		rising = true;
		float deltay = py - Boss.transform.position.y + amount;
		StartCoroutine(Thrust());
		Tweener twn = HOTween.To (Boss.transform, 2.0f, new TweenParms().Prop ("position", new Vector3(0, deltay, 0), true));
		while(!twn.isComplete) {
			yield return null;
		}
		rising = false;
	}

	IEnumerator Rise(float py, float amount, float dur){
		rising = true;
		float deltay = py - Boss.transform.position.y + amount;
		StartCoroutine(Thrust());
		Tweener twn = HOTween.To (Boss.transform, dur, new TweenParms().Prop ("position", new Vector3(0, deltay, 0), true));
		while(!twn.isComplete && !force_stop) {
			yield return null;
		}
		twn.Complete();
		force_stop = false;
		rising = false;
	}

	private void Shoot(float angle, float speed){
		Projectile pj = Instantiate(_Bullet_Prefab, new Vector3(bullet_nozzle.position.x, bullet_nozzle.position.y, 0), Quaternion.identity) as Projectile;
		pj.gameObject.SetActive(true);
		pj.Fire(GetAngle() + angle, speed, 3);
	}

	private Transform Aim(float aim, float delay){
		Projectile pj = Instantiate(_Scope_Prefab, new Vector3(player_pos.position.x, player_pos.position.y, 0), Quaternion.identity) as Projectile;
		pj.gameObject.SetActive(true);
		pj.Aim(aim, delay, player_pos);
		return pj.transform;
	}

	private void Laser(Vector3 pos){
		Projectile pj = Instantiate(_Laser_Prefab, new Vector3(laser_eye.position.x, laser_eye.position.y, 0), Quaternion.identity) as Projectile;
		pj.gameObject.SetActive(true);
		pj.Laser(pos, _Exp_Prefab);
	}

	float rocketTimer;
	IEnumerator Rocket(){
		Vector3 rnpos = rocket_nozzle.transform.position;
		rnpos.y -= 1;
		rocket_nozzle.transform.position = rnpos;
		Tweener twn = HOTween.To (rocket_nozzle.transform, 0.3f, new TweenParms().Prop ("position", new Vector3(0, 1f, 0), true));
		Color col = rocket_exp.color;
		col.a = 1f;
		rocket_exp.color = col;
		while(!twn.isComplete) {
			rocketTimer+= Time.deltaTime;
			if(rocketTimer > 0.1f){
				col.a = 0f;
				rocket_exp.color = col;
			}
			yield return null;
		}
		rocketTimer = 0;
	}

	float warnTimer;
	float blinkTimer;
	IEnumerator Warning(float dur, int type){
		Color col;
		while(warnTimer < dur){
			warnTimer += Time.deltaTime;
			blinkTimer += Time.deltaTime;
			if(blinkTimer > 0.1f){
				if(type == 1)
					col = arrow_left.color;
				else
					col = arrow_mid.color;
				col.a = col.a == 0 ? 1.0f : 0.0f;
				if(type == 0){
					arrow_mid.color = col;
				} else if(type == 1){
					arrow_left.color = col;
				} else if(type == 2){
					arrow_mid.color = col;
					arrow_left.color = col;
				} else {
					arrow_mid.color = col;
					arrow_right.color = col;
				}
				blinkTimer = 0;
			}
			yield return null;
		}
		col = arrow_mid.color;
		col.a = 0.0f;
		arrow_mid.color = col;
		arrow_left.color = col;
		arrow_right.color = col;
		warnTimer = 0;
		blinkTimer = 0;
	}

	private void FallRocket(int type){
		Vector3 pos;
		if(type == 0)
			pos = new Vector3(arrow_mid.transform.position.x, arrow_mid.transform.position.y + 5, 0);
		else
			pos = new Vector3(arrow_left.transform.position.x, arrow_left.transform.position.y + 5, 0);
		Projectile pj = Instantiate(_Rocket_Prefab, pos, Quaternion.identity) as Projectile;
		pj.gameObject.SetActive(true);
		pj.Fire(180, 0.5f, 3);
	}

	private void FallRock(int type){
		Vector3 pos;
		if(type == 0)
			pos = new Vector3(arrow_mid.transform.position.x - 1.5f, arrow_mid.transform.position.y + 5, 0);
		else
			pos = new Vector3(arrow_mid.transform.position.x + 1.5f, arrow_mid.transform.position.y + 5, 0);
		Projectile pj = Instantiate(_Rock_Prefab, pos, Quaternion.identity) as Projectile;
		pj.gameObject.SetActive(true);
		pj.Fire(180, 0.5f, 3);
	}

	private float GetAngle(){
		Vector3 dir = player_pos.position - bullet_nozzle.position;
		Vector2 from = Vector2.up;
		Vector2 to = new Vector2(dir.x, dir.y);

		float ang = Vector2.Angle(from, to);
		Vector3 cross = Vector3.Cross(from, to);
		if(cross.z > 0)
			ang = 360 - ang;

		return ang;
	}

	private float thrustCounter;
	IEnumerator Thrust(){
		Color col;
		while(rising){
			thrustCounter += Time.deltaTime;
			if(thrustCounter > 0.06f){
				col = thrust1.color;
				col.a = col.a == 0 ? 1.0f : 0.0f;
				thrust1.color = col;
				thrust2.color = col;
				thrustCounter = 0.0f;
			}
			yield return null;
		}
		col = thrust1.color;
		col.a = 0.0f;
		thrust1.color = col;
		thrust2.color = col;
		yield return null;
	}
}