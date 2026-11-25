using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace MedievalVR.Combat
{
    [RequireComponent(typeof(Rigidbody), typeof(XRGrabInteractable))]
    public sealed class ArrowProjectile : MonoBehaviour
    {
        readonly List<(Collider arrow, Collider obstacle)> ignored = new();
        Rigidbody body;
        XRGrabInteractable grab;
        Collider[] arrowColliders;
        float restoreCollisionsAt;

        public bool IsFlying { get; private set; }
        public Rigidbody Body => body;

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            grab = GetComponent<XRGrabInteractable>();
            arrowColliders = GetComponentsInChildren<Collider>();
        }

        void OnEnable() => grab.selectEntered.AddListener(OnGrabbed);

        void OnDisable()
        {
            grab.selectEntered.RemoveListener(OnGrabbed);
            IsFlying = false;
            RestoreCollisions();
        }

        public void Launch(Vector3 direction, float speed, Collider shooter, XRGrabInteractable bow)
        {
            RestoreCollisions();
            IgnoreCollisionWith(shooter);
            foreach (var obstacle in bow.GetComponentsInChildren<Collider>())
                IgnoreCollisionWith(obstacle);

            restoreCollisionsAt = Time.time + .22f;
            body.isKinematic = false;
            body.useGravity = true;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.linearVelocity = direction * speed;
            body.angularVelocity = Vector3.zero;
            IsFlying = true;
        }

        void FixedUpdate()
        {
            if (ignored.Count > 0 && Time.time >= restoreCollisionsAt) RestoreCollisions();
            if (!IsFlying || body.isKinematic || body.linearVelocity.sqrMagnitude < 1f) return;

            Quaternion turn = Quaternion.FromToRotation(body.rotation * Vector3.up, body.linearVelocity);
            body.MoveRotation(turn * body.rotation);
        }

        void OnCollisionEnter(Collision collision) => IsFlying = false;

        void OnGrabbed(SelectEnterEventArgs args)
        {
            IsFlying = false;
            body.collisionDetectionMode = CollisionDetectionMode.Discrete;
            RestoreCollisions();
        }

        void IgnoreCollisionWith(Collider obstacle)
        {
            if (obstacle == null || !obstacle.enabled || obstacle.isTrigger) return;
            foreach (var arrow in arrowColliders)
            {
                if (Physics.GetIgnoreCollision(arrow, obstacle)) continue;
                Physics.IgnoreCollision(arrow, obstacle);
                ignored.Add((arrow, obstacle));
            }
        }

        void RestoreCollisions()
        {
            foreach (var pair in ignored)
                if (pair.arrow != null && pair.obstacle != null)
                    Physics.IgnoreCollision(pair.arrow, pair.obstacle, false);
            ignored.Clear();
        }
    }
}
