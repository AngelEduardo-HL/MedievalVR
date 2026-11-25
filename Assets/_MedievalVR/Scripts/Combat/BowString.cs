using UnityEngine;

namespace MedievalVR.Combat
{
    [RequireComponent(typeof(MeshFilter))]
    public sealed class BowString : MonoBehaviour
    {
        [SerializeField] Material stringMaterial;

        LineRenderer line;
        Vector3 rest;
        Vector3 upperTip;
        Vector3 lowerTip;

        public Vector3 StringRest => transform.TransformPoint(rest);
        public Vector3 ArrowRest => transform.TransformPoint(new Vector3(0f, .055f, .025f));

        void Awake()
        {
            Bounds bounds = GetComponent<MeshFilter>().sharedMesh.bounds;
            float tipX = bounds.center.x < 0f ? bounds.min.x + .009f : bounds.max.x - .009f;
            upperTip = new Vector3(tipX, bounds.max.y, bounds.center.z);
            lowerTip = new Vector3(tipX, bounds.min.y, bounds.center.z);
            rest = new Vector3(tipX, .055f, .025f);

            var stringObject = new GameObject("Cuerda");
            stringObject.transform.SetParent(transform, false);
            line = stringObject.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.sharedMaterial = stringMaterial;
            line.widthMultiplier = .005f;
            line.positionCount = 3;
            line.SetPosition(0, upperTip);
            line.SetPosition(2, lowerTip);
            ResetDraw();
        }

        public void DrawTo(Vector3 worldPosition)
        {
            line.SetPosition(1, transform.InverseTransformPoint(worldPosition));
        }

        public void ResetDraw() => line.SetPosition(1, rest);
    }
}
