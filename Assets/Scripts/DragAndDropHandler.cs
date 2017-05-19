using UnityEngine;
using System.Collections;
public class DragAndDropHandler : MonoBehaviour {

	public static DragAndDropHandler instance = null;

	private bool draggingItem = false;
	private GameObject draggedObject;
	private Vector2 touchOffset;
	private Vector3 draggedObjectScale;
	private Vector3 draggedObjectOldPos;
	private GameObject collidedObj = null;
	private BackgroundManager bgManager;
	private bool active = true;

	void Awake() {
		if (instance == null) {
			instance = this;
		} else if(instance != this) {
			Destroy (gameObject);
		}
	}

	public void deactivate() {
		active = false;
	}

	public void activate() {
		active = true;
	}

	public void Reset() {
		draggingItem = false;
		collidedObj = null;
		draggedObject = null;
	}

	public void setCollidedObject(GameObject collidedObject) {
		this.collidedObj = collidedObject;
	}

	public Object getDraggedObject() {
		return draggedObject;
	}

	void Update () {
		if (active) {
			if (HasInput) {
				DragOrPickUp ();
			} else {
				if (draggingItem)
					DropItem ();
			}
		}
	}

	Vector2 CurrentTouchPosition {
		get	{
			Vector2 inputPos;
			inputPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			return inputPos;
		}
	}

	private void DragOrPickUp()	{
		var inputPosition = CurrentTouchPosition;
		if (draggingItem) {
			draggedObject.transform.position = inputPosition + touchOffset;
		} else {
			RaycastHit2D[] touches = Physics2D.RaycastAll(inputPosition, inputPosition, .5f);
			if (touches.Length > 0) {
				var hit = touches[0];
				if (hit.transform != null && hit.transform.gameObject.tag == "Dot") {
					draggingItem = true;
					draggedObject = hit.transform.gameObject;
					touchOffset = (Vector2)hit.transform.position - inputPosition;
					draggedObjectScale = draggedObject.transform.localScale;
					float scaleInc = draggedObjectScale.x + 0.1f;
					draggedObject.transform.localScale = new Vector3(scaleInc, scaleInc, scaleInc);
					draggedObject.transform.SetAsLastSibling ();
					draggedObjectOldPos = draggedObject.transform.position;
					draggedObjectOldPos = new Vector3 (draggedObjectOldPos.x, draggedObjectOldPos.y, draggedObjectOldPos.z);
					collidedObj = null;
				}
			}
		}
	}

	private bool HasInput {
		get	{
			// returns true if either the mouse button is down or at least one touch is felt on the screen
			return Input.GetMouseButton(0);
		}
	}

	void DropItem() {
		draggingItem = false;
		draggedObject.transform.localScale = draggedObjectScale;

		if (collidedObj == null) {
			draggedObject.transform.position = draggedObjectOldPos;
		} else {
			draggedObject.transform.position = collidedObj.transform.position;
			collidedObj.transform.position = draggedObjectOldPos;

			bgManager = GetComponent<BackgroundManager> ();
			bgManager.checkDotPosition (draggedObject, collidedObj);
		}
		draggedObject = null;
	}
}