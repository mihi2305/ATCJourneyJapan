using System;
using System.Collections.Generic;
using UnityEngine;

namespace ATCJourneyJapan.Aircraft
{
    // Small waypoint mover for prototype aircraft. It keeps movement deterministic and easy to replace later.
    public class SimpleRoute
    {
        private readonly Queue<Vector3> waypoints = new Queue<Vector3>();
        private Action onComplete;

        public bool IsMoving { get; private set; }
        public float Speed { get; set; } = 5f;

        public void StartRoute(IEnumerable<Vector3> route, float speed, Action completed = null)
        {
            waypoints.Clear();

            foreach (var point in route)
            {
                waypoints.Enqueue(point);
            }

            Speed = speed;
            onComplete = completed;
            IsMoving = waypoints.Count > 0;

            if (!IsMoving)
            {
                onComplete?.Invoke();
            }
        }

        public void Stop()
        {
            IsMoving = false;
        }

        public void Pause()
        {
            IsMoving = false;
        }

        public void Resume()
        {
            IsMoving = waypoints.Count > 0;
        }

        public void Tick(Transform target, float deltaTime)
        {
            if (!IsMoving || waypoints.Count == 0)
            {
                return;
            }

            var next = waypoints.Peek();
            var current = target.position;
            var updated = Vector3.MoveTowards(current, next, Speed * deltaTime);
            target.position = updated;

            var direction = next - current;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.001f)
            {
                target.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            }

            if (Vector3.Distance(updated, next) <= 0.05f)
            {
                waypoints.Dequeue();
                if (waypoints.Count == 0)
                {
                    IsMoving = false;
                    onComplete?.Invoke();
                }
            }
        }
    }
}
