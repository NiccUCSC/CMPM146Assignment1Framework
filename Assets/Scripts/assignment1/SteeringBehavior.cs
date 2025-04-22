using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class SteeringBehavior : MonoBehaviour {
	public Vector3 target;

    public Vector4 ghost;
	public float ghostDist = 0f;
	public float ghostSpeed = 15f;
	public KinematicBehavior kinematic;
	public List<Vector3> path;
	// you can use this label to show debug information,
	// like the distance to the (next) target
	public TextMeshProUGUI label;

	public bool temp = false;
	
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start() {
		kinematic = GetComponent<KinematicBehavior>();
		target = transform.position;
		this.path = new();
		EventBus.OnSetMap += SetMap;

        // set ghost position (in R^4)
        float x = transform.position.x;
        float z = transform.position.z;
        float speed = kinematic.speed;
        float theta = Mathf.Atan2(transform.forward.z, transform.forward.x);
        ghost.Set(x, z, speed, theta);
	}

	// Update is called once per frame
	void Update()  {
		// Assignment 1: If a single target was set, move to that target
		//				If a path was set, follow that path ("tightly")

		// you can use kinematic.SetDesiredSpeed(...) and kinematic.SetDesiredRotationalVelocity(...)
		//	to "request" acceleration/decceleration to a target speed/rotational velocity

		ghostDist += Time.deltaTime * ghostSpeed;
		Vector3 ghostTarget = target;
		Vector3 ghostDir = Vector3.Normalize(target - transform.position);
		float remainingDist = ghostDist;
		if (path.Count != 0) {
			ghostTarget = path[0];
			for (int i = 0; remainingDist > 0 && i < path.Count-1; i++) {
				float segmentDist = Vector3.Distance(path[i], path[i+1]) + 1e-6f;
				// Debug.Log($"Segment {i} RemainingDist {remainingDist} SegmentDist {segmentDist}");

				if (remainingDist < segmentDist) {
					float r = remainingDist / segmentDist;
					ghostTarget = r * path[i+1] + (1-r) * path[i];
					ghostDir = Vector3.Normalize(path[i+1] - path[i]);
					// Debug.Log(ghostTarget);
				}
				remainingDist -= segmentDist;
			}
			if (remainingDist > 0 && path.Count != 0) ghostTarget = path[path.Count-1];
		}
		float gx = ghostTarget.x;
		float gz = ghostTarget.z;
		ghostSpeed = ghostSpeed + 0;
		float ghostTheta = Mathf.Atan2(ghostDir.z, ghostDir.x);
		ghost = new(gx, gz, ghostSpeed, ghostTheta);
		// Debug.Log($"Ghost {ghost} Ghost Target {ghostTarget}");

		this.temp = true;
		SetTarget(ghostTarget);
		this.temp = false;

        float alpha = 1.5f;

        float x = transform.position.x;
        float z = transform.position.z;
        float speed = kinematic.speed;
        float theta = Mathf.Atan2(transform.forward.z, transform.forward.x);

        Vector4 pos = new Vector4(x, z, speed, theta);
        Vector4 error = alpha * (ghost - pos);

        float phi = error.x * Mathf.Cos(theta) + error.y * Mathf.Sin(theta);
        float psi = error.w;

        // float signAngle1 = Vector2.SignedAngle(new Vector2(transform.forward.x, transform.forward.z), new Vector2(error.x, error.y)) * Mathf.PI/180;
        // float signAngle2 = Vector2.SignedAngle(new Vector2(transform.forward.x, transform.forward.z), new Vector2(error.x, error.y)) * Mathf.PI/180;
		// float signAngle = Mathf.Abs(signAngle1) < Mathf.Abs(signAngle2) ? signAngle1 : signAngle2;
		float signAngle = Vector2.SignedAngle(new Vector2(transform.forward.x, transform.forward.z), new Vector2(error.x, error.y)) * Mathf.PI/180;
		


        float dist = Mathf.Sqrt(error.x*error.x + error.y*error.y);

        kinematic.SetDesiredSpeed(phi);
        kinematic.SetDesiredRotationalVelocity(-60f * signAngle / Mathf.Min(dist, 20) * 180/Mathf.PI);
	}

	public void SetTarget(Vector3 target)  {
		this.target = target;
		this.path = this.temp ? this.path : new List<Vector3> {this.target};
		EventBus.ShowTarget(target);

        float x = target.x;
        float z = target.z;
        float speed = kinematic.speed;
        float theta = Mathf.Atan2(transform.forward.z, transform.forward.x);
        ghost.Set(x, z, 0, 0);
	}

	public void SetPath(List<Vector3> path)  {
		this.path = path != null ? path : new List<Vector3> {this.target};
		this.ghostDist = -Vector3.Distance(this.path[0], transform.position);
	}

	public void SetMap(List<Wall> outline)  {
		this.path = new List<Vector3>{ transform.position };
		this.target = transform.position;
	}
}
