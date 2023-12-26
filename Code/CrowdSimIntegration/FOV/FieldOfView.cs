using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FieldOfView : MonoBehaviour
{

    public float viewRadius;
    [Range(0, 360)]
    public float viewAngle;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

    [HideInInspector]
    public List<Transform> visibleTargets = new List<Transform>();

    public float meshResolution;
    public int edgeResolveIterations;
    public float edgeDstThreshold;

    public float maskCutawayDst = .1f;
    public string doorTag;
    public int objectsBetweenCount;
    public int obstacleMaxCount = 4;
    public MeshFilter viewMeshFilter;
    Mesh viewMesh;

    public Vector3 pathCorrection;
    private Dictionary<Transform, bool> doorDictionary = new Dictionary<Transform, bool>();
    private List<Transform> doors;

    void Start()
    {
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;
        pathCorrection = Vector3.zero;
        StartCoroutine("FindTargetsWithDelay", .2f);
    }


    IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
        }
    }

    void LateUpdate()
    {
        DrawFieldOfView();
        DetectAlternatePath();
    }

    void FindVisibleTargets()
    {
        //visibleTargets.Clear ();
        //Collider[] targetsInViewRadius = Physics.OverlapSphere (transform.position, viewRadius, targetMask);
        //
        //for (int i = 0; i < targetsInViewRadius.Length; i++) {
        //	Transform target = targetsInViewRadius [i].transform;
        //	Vector3 dirToTarget = (target.position - transform.position).normalized;
        //	if (Vector3.Angle (transform.forward, dirToTarget) < viewAngle / 2) {
        //		float dstToTarget = Vector3.Distance (transform.position, target.position);
        //		if (!Physics.Raycast (transform.position, dirToTarget, dstToTarget, obstacleMask)) {
        //			visibleTargets.Add (target);
        //		}
        //	}
        //}

        visibleTargets.Clear();
        if (doors != null)
            doors.Clear();
        int rayCount = 20; // Number of rays to cast within the view angle
        float angleIncrement = viewAngle / (float)(rayCount - 1);
        objectsBetweenCount = 0;
        for (int i = 0; i < rayCount; i++)
        {
            float angle = -viewAngle / 2 + i * angleIncrement;

            Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;

            RaycastHit hit;
            if (Physics.Raycast(transform.position, direction, out hit, viewRadius, targetMask))
            {
                if (Vector3.Angle(transform.forward, direction) < viewAngle / 2)
                {
                    Transform target = hit.collider.transform;
                    if (target.CompareTag(doorTag))
                    {
                        // Count objects between the player and the door
                        RaycastHit[] hits = Physics.RaycastAll(transform.position, direction, obstacleMask);
                        objectsBetweenCount = hits.Length;
                        TrackDoors(target, objectsBetweenCount >= obstacleMaxCount);
                    }
                }
            }
        }
    }

    private void DetectAlternatePath()
    {
        foreach (KeyValuePair<Transform, bool> pair in doorDictionary)
        {
            Transform key = pair.Key;
            bool value = pair.Value;

            if (value == true)
            {
                continue;
            }

            if (value == false && pathCorrection != Vector3.up)
            {
                pathCorrection = key.transform.position;
            }
        }
    }

    private void TrackDoors(Transform door, bool blocked)
    {
        if (doorDictionary.ContainsKey(door))
        {
            doorDictionary[door] = blocked;
            return;
        }
        else
            doorDictionary.Add(door, blocked);
    }

    void DrawFieldOfView()
    {
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
        float stepAngleSize = viewAngle / stepCount;
        List<Vector3> viewPoints = new List<Vector3>();
        ViewCastInfo oldViewCast = new ViewCastInfo();
        for (int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i;
            ViewCastInfo newViewCast = ViewCast(angle);

            if (i > 0)
            {
                bool edgeDstThresholdExceeded = Mathf.Abs(oldViewCast.dst - newViewCast.dst) > edgeDstThreshold;
                if (oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDstThresholdExceeded))
                {
                    EdgeInfo edge = FindEdge(oldViewCast, newViewCast);
                    if (edge.pointA != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointA);
                    }
                    if (edge.pointB != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointB);
                    }
                }

            }


            viewPoints.Add(newViewCast.point);
            oldViewCast = newViewCast;
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;
        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]) + Vector3.forward * maskCutawayDst;

            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        viewMesh.Clear();

        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }


    EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;
        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        for (int i = 0; i < edgeResolveIterations; i++)
        {
            float angle = (minAngle + maxAngle) / 2;
            ViewCastInfo newViewCast = ViewCast(angle);

            bool edgeDstThresholdExceeded = Mathf.Abs(minViewCast.dst - newViewCast.dst) > edgeDstThreshold;
            if (newViewCast.hit == minViewCast.hit && !edgeDstThresholdExceeded)
            {
                minAngle = angle;
                minPoint = newViewCast.point;
            }
            else
            {
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }

        return new EdgeInfo(minPoint, maxPoint);
    }


    ViewCastInfo ViewCast(float globalAngle)
    {
        Vector3 dir = DirFromAngle(globalAngle, true);
        RaycastHit hit;

        if (Physics.Raycast(transform.position, dir, out hit, viewRadius, obstacleMask))
        {
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        }
        else
        {
            return new ViewCastInfo(false, transform.position + dir * viewRadius, viewRadius, globalAngle);
        }
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    public struct ViewCastInfo
    {
        public bool hit;
        public Vector3 point;
        public float dst;
        public float angle;

        public ViewCastInfo(bool _hit, Vector3 _point, float _dst, float _angle)
        {
            hit = _hit;
            point = _point;
            dst = _dst;
            angle = _angle;
        }
    }

    public struct EdgeInfo
    {
        public Vector3 pointA;
        public Vector3 pointB;

        public EdgeInfo(Vector3 _pointA, Vector3 _pointB)
        {
            pointA = _pointA;
            pointB = _pointB;
        }
    }

}