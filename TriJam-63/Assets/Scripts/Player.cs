﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cinemachine;
using TMPro;

public class Player : MonoBehaviour {
	public static Player instance;

	[Header("Moving forces")]
	[SerializeField] float rotationSpeed = 5;
	[Space]
	[SerializeField] [Tooltip("Squared in play mode")] float minMovingMagnitude = 0.05f;
	[SerializeField] float movingSmooth = 0.2f;
	[SerializeField] float moveSpeed = 5f;

	[Space]
	[Header("Dodge")]
	[SerializeField] float dodgeDist = 1.1f;
	[SerializeField] float dodgeDrag = 10.0f;
	[SerializeField] float dodgeInvulnerabilityTime = 0.15f;
	[SerializeField] float dodgeEnergySpend = 40;

	[Space]
	[Header("Attack")]
	[SerializeField] float attackKd = 2.0f;
	[SerializeField] Transform spearAnchor;
	[SerializeField] Attacker spear;
	float currShootingKd = 0;

	[Space]
	[Header("Bats")]
	public bool isCanTakeDamage;
	[SerializeField] float healthMax = 100f;
	float healthCurr = 100f;
	float takedDamage = 10f;
	[SerializeField] float energyMax = 100f;
	float energyCurr = 100f;
	[SerializeField] float energyRegen = 25f;

	[HideInInspector] [SerializeField] Rigidbody2D rb;
	[HideInInspector] [SerializeField] Animator anim;
	[HideInInspector] [SerializeField] Collider2D collider;
	[HideInInspector] [SerializeField] Camera camera;
	[SerializeField] Slider hpSlider;
	[SerializeField] Slider energySlider;

	Vector2 prevVelocity;
	Vector2 addVelocity;
	Vector3 moveInput;
	float defaultDrag;
	float triggerStayTime;

#if UNITY_EDITOR
	private void OnValidate() {
		if (rb == null)
			rb = GetComponent<Rigidbody2D>();
		if (anim == null)
			anim = GetComponent<Animator>();
		if (collider == null)
			collider = GetComponent<Collider2D>();
		if (camera == null)
			camera = FindObjectOfType<Camera>();
	}
#endif

	private void Awake() {
		minMovingMagnitude *= minMovingMagnitude;
		defaultDrag = rb.drag;
		energySlider.value = energyCurr / energyMax;
		hpSlider.value = healthCurr / healthMax;
		instance = this;

		float x = UnityEngine.Random.Range(-1f, 1f);
		float y = UnityEngine.Random.Range(-1f, 1f);
		moveInput = new Vector3(x, y).normalized;
	}

	private void OnTriggerEnter2D(Collider2D collision) {
		float range = 0.35f;
		float x = UnityEngine.Random.Range(-range, range);
		float y = UnityEngine.Random.Range(-range, range);
		moveInput = (-moveInput + new Vector3(x, y).normalized).normalized;
		triggerStayTime = 0.0f;
	}


	private void OnTriggerStay2D(Collider2D collision) {
		triggerStayTime += Time.deltaTime;
		if(triggerStayTime >= 1.0f) {
			float range = 0.1f;
			float x = UnityEngine.Random.Range(-range, range);
			float y = UnityEngine.Random.Range(-range, range);
			moveInput = (collision.transform.position - transform.position + new Vector3(x, y).normalized).normalized;
			triggerStayTime = 0.0f;
		}
	}

	private void OnTriggerExit2D(Collider2D collision) {
		
	}

	private void Update() {
		//moveInput = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
		if(energyCurr < energyMax) {
			energyCurr += energyRegen * Time.deltaTime;
			if(energyCurr > energyMax) {
				energyCurr = energyMax;
			}
			energySlider.value = energyCurr / energyMax;
		}

		if (Input.GetKeyDown(KeyCode.Space))
			Dodge();

		Shoot();
	}

	private void FixedUpdate() {
		RotatePlayer();
		RotateSpear();
		Move();
	}

	void RotatePlayer() {
		Vector3 lookPos = rb.velocity;
		float angle = Mathf.Atan2(lookPos.y, lookPos.x) * Mathf.Rad2Deg;
		transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(new Vector3(0, 0, angle)), rotationSpeed * Time.deltaTime);
	}

	void RotateSpear() {
		Vector3 lookPos = camera.ScreenToWorldPoint(Input.mousePosition) - spearAnchor.position;
		float angle = Mathf.Atan2(lookPos.y, lookPos.x) * Mathf.Rad2Deg;
		spearAnchor.rotation = Quaternion.Slerp(spearAnchor.rotation, Quaternion.Euler(new Vector3(0, 0, angle)), rotationSpeed * Time.deltaTime);
	}

	void Move() {
		if (moveInput.sqrMagnitude >= minMovingMagnitude) {
			Vector2 targetVelocity = moveInput * moveSpeed;
			Vector2 dumpedVeocity = Vector2.SmoothDamp(rb.velocity, targetVelocity, ref prevVelocity, movingSmooth);
			rb.velocity = dumpedVeocity;
		}
		if(addVelocity != Vector2.zero) {
			rb.velocity += addVelocity;
			addVelocity = Vector2.zero;
		}
	}

	void Shoot() {
		if (Input.GetMouseButtonDown(0)) {
			currShootingKd += Time.deltaTime;
			while (currShootingKd >= attackKd) {
				currShootingKd -= attackKd;

				Vector3 mousePos = camera.ScreenToWorldPoint(Input.mousePosition) - transform.position;
				float mouseRot = (Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg + 270) % 360;

				//TODO: attack
			}
		}
	}

	void Dodge() {
		if ((rb.velocity.sqrMagnitude >= minMovingMagnitude) && energyCurr > dodgeEnergySpend) {
			energyCurr -= dodgeEnergySpend;

			isCanTakeDamage = false;
			rb.drag = dodgeDrag;
			Vector3 lookPos = rb.velocity;
			addVelocity += (Vector2)(lookPos * dodgeDist);

			LeanTween.value(rb.drag, defaultDrag, dodgeInvulnerabilityTime)
				.setOnUpdate((float drag) => {
					rb.drag = drag;

				})
				.setOnComplete(() => {
					isCanTakeDamage = true;
				});
		}
	}

	void TakeDamage() {
		if (healthCurr <= 0)
			return;

		healthCurr -= takedDamage;
		hpSlider.value = healthCurr / healthMax;
		if (healthCurr <= 0) {
			Die();
		}
	}

	void Die() {
		LeanTween.cancel(gameObject);
		Destroy(gameObject);
	}
}
