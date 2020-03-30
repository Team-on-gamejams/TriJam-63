using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour {
	public float maxHp = 100.0f;
	public float takeDamage = 33;
	public float currHp;

	public Slider hpSlider;

	public float speed = 7.5f;
	Transform currTarget;

	private void Awake() {
		maxHp = Random.Range(50, 150);
		currHp = maxHp;

		hpSlider.gameObject.SetActive(false);
		hpSlider.minValue = 0;
		hpSlider.maxValue = maxHp;
		currTarget = Player.instance.transform;
	}

	void Update() {
		if(currTarget != null) {
			Vector3 newPos = currTarget.transform.position - transform.position;
			float angle = Mathf.Atan2(newPos.y, newPos.x) * Mathf.Rad2Deg;
			transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90));

			transform.Translate(transform.up * speed * Time.deltaTime, Space.World);
		}

		hpSlider.transform.parent.localRotation = Quaternion.Euler(-transform.rotation.eulerAngles);
	}

	public void TakeDamage() {
		currHp -= takeDamage;
		if (currHp <= 0) {
			Destroy(gameObject);
		}

		hpSlider.value = currHp;
		hpSlider.gameObject.SetActive(true);
	}

	private void OnCollisionEnter2D(Collision2D collision) {
		if(collision.gameObject.tag == "Player") {
			collision.gameObject.SendMessage("TakeDamage", SendMessageOptions.DontRequireReceiver);
		}
	}
}
