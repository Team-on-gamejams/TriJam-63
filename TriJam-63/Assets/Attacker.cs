using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attacker : MonoBehaviour
{
    [HideInInspector] [SerializeField] Collider2D collider;


#if UNITY_EDITOR
	private void OnValidate() {
		if (collider == null)
			collider = GetComponent<Collider2D>();
	}
#endif

	public void Enable() {
		collider.enabled = true;
	}

	public void Disable() {
		collider.enabled = false;
	}

	private void OnTriggerEnter2D(Collider2D collision) {
		if (collision.gameObject.tag == "Enemy") {
			collision.gameObject.SendMessage("TakeDamage", SendMessageOptions.DontRequireReceiver);
		}
	}
}
