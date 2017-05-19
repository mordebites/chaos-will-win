using UnityEngine;
using System.Collections;

public class DotCollisionHandler : MonoBehaviour {

	void OnTriggerStay2D(Collider2D coll) {
		if(gameObject.Equals(DragAndDropHandler.instance.getDraggedObject())) {
			if (coll.gameObject.tag == "Dot") {
				DragAndDropHandler.instance.setCollidedObject(coll.gameObject);
			}
		}
	}

	void OnTriggerExit2D(Collider2D coll) {
		if (gameObject.Equals (DragAndDropHandler.instance.getDraggedObject ())) {
			if (coll.gameObject.tag == "Dot") {
				DragAndDropHandler.instance.setCollidedObject (null);
			}
		}
	}
}
