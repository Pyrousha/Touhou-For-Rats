using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Path : MonoBehaviour
{
    [Serializable]
    public struct PathPoint
    {
        public PathPoint(Transform point, float timeToWait)
        {
            this.point = point;
            this.timeToWait = timeToWait;
        }

        [field: SerializeField] public Transform point { get; private set; }
        [field: SerializeField] public float timeToWait { get; private set; }
    }

    [field: SerializeField] public float MoveSpeedAdditiveOverride { get; private set; }
    [field: SerializeField] public float BulletSpeedAdditiveOverride { get; private set; }
    [field: SerializeField] public bool OffsetLastPos { get; private set; }
    [field: SerializeField] public List<PathPoint> points { get; private set; }

    public void GeneratePath()
    {
        points = new List<PathPoint>();
        for (int i = 0; i < transform.childCount; i++)
        {
            points.Add(new PathPoint(transform.GetChild(i), 0));
        }
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(Path))]
class PathEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Generate Path"))
            ((Path)target).GeneratePath();
    }
}
#endif
