using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MedievalVR.Combat;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using Object = UnityEngine.Object;

namespace MedievalVR.Editor
{
    [InitializeOnLoad]
    public static class CombatValidation
    {
        const string PendingTest = "MedievalVR.CombatValidation.Pending";
        public const string ReportPath = "Temp/InitialCombat/validation.txt";
        public static readonly string[] WeaponNames =
            { "Sword_01", "Shield_01", "Dagger_01", "Ax_01", "Hammer_01", "Bow_01", "Arrow_01" };

        static CombatValidation()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        [MenuItem("MedievalVR/Combat/Validate prefabs")]
        public static void ValidatePrefabs()
        {
            foreach (string name in WeaponNames)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/_MedievalVR/Prefabs/Weapons/{name}.prefab");
                Require(prefab != null, name + ": missing prefab");
                var grab = prefab.GetComponent<XRGrabInteractable>();
                var body = prefab.GetComponent<Rigidbody>();
                Require(grab != null && body != null && !body.isKinematic && body.useGravity, name + ": physics components");
                Require(grab.attachTransform != null && grab.attachTransform.name == "Mango" && !grab.useDynamicAttach,
                    name + ": fixed handle attachment");
                Require(grab.selectMode == InteractableSelectMode.Single && !grab.trackScale, name + ": single grip and fixed scale");
                Require(grab.movementType == XRBaseInteractable.MovementType.VelocityTracking, name + ": velocity tracking");
                Require(grab.colliders.Count == 1 && grab.colliders[0] is BoxCollider &&
                    grab.colliders[0].name == "ZonaAgarre", name + ": handle-only interaction collider");
                var mesh = prefab.GetComponent<MeshCollider>();
                Require(mesh != null && mesh.enabled && mesh.convex && !mesh.isTrigger, name + ": solid body collider");
                Require(prefab.GetComponent<WeaponImpact>() != null, name + ": impact damage profile");
                Require(prefab.transform.position.sqrMagnitude < .0001f, name + ": reusable prefab origin");
                foreach (Transform child in prefab.GetComponentsInChildren<Transform>(true))
                    Require(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject) == 0,
                        name + ": missing script on " + child.name);
            }

            var target = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_MedievalVR/Prefabs/Combat/RedTestTarget.prefab");
            Require(target != null && target.GetComponent<DamageableTarget>() != null, "Target: health component");
            Require(target.GetComponent<BoxCollider>() != null && target.GetComponent<Rigidbody>().isKinematic,
                "Target: stationary collision receiver");
            Require(target.GetComponentInChildren<TargetHealthDisplay>() != null && target.GetComponentInChildren<TMP_Text>() != null,
                "Target: world-space health display");
            Debug.Log("[MedievalVR] Prefab validation passed: 7 weapons and 1 target.");
        }

        [MenuItem("MedievalVR/Combat/Run Play Mode checks")]
        public static void RunPlayModeChecks()
        {
            Require(!EditorApplication.isPlayingOrWillChangePlaymode, "Stop Play before starting checks.");
            Require(SceneManager.GetActiveScene().path.EndsWith("/MainGame.unity") &&
                !SceneManager.GetActiveScene().isDirty, "Save MainGame before starting checks.");
            ValidatePrefabs();
            Directory.CreateDirectory("Temp/InitialCombat");
            File.WriteAllText(ReportPath, "RUNNING\n");
            SessionState.SetBool(PendingTest, true);
            EditorApplication.EnterPlaymode();
        }

        static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(PendingTest, false)) return;
            SessionState.SetBool(PendingTest, false);
            var runner = new GameObject("Combat validation runner") { hideFlags = HideFlags.DontSave };
            runner.AddComponent<CombatSmokeRunner>().Begin();
        }

        public static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }

    public sealed class CombatTestInteractor : XRBaseInteractor
    {
        public XRGrabInteractable Target;
        public bool Holding = true;
        public override bool isSelectActive => Holding;

        public override void GetValidTargets(List<IXRInteractable> targets)
        {
            targets.Clear();
            if (Target != null) targets.Add(Target);
        }
    }

    public sealed class CombatSmokeRunner : MonoBehaviour
    {
        readonly List<string> passed = new List<string>();
        readonly List<GameObject> temporaryObjects = new List<GameObject>();

        public void Begin()
        {
            StartCoroutine(SafeRun());
        }

        IEnumerator SafeRun()
        {
            IEnumerator suite = Run();
            while (true)
            {
                bool next = false;
                object wait = null;
                Exception failure = null;
                try
                {
                    next = suite.MoveNext();
                    if (next) wait = suite.Current;
                }
                catch (Exception exception) { failure = exception; }
                if (failure != null)
                {
                    Finish(failure);
                    yield break;
                }
                if (!next) break;
                yield return wait;
            }
            Finish(null);
        }

        IEnumerator Run()
        {
            var manager = Object.FindFirstObjectByType<XRInteractionManager>();
            CombatValidation.Require(manager != null, "Missing XR Interaction Manager.");
            int index = 0;
            foreach (string name in CombatValidation.WeaponNames)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/_MedievalVR/Prefabs/Weapons/{name}.prefab");
                GameObject weapon = Keep(Object.Instantiate(prefab, new Vector3(100f + index++ * 3f, 4f, 0f), Quaternion.identity));
                var grab = weapon.GetComponent<XRGrabInteractable>();
                var body = weapon.GetComponent<Rigidbody>();
                var handObject = Keep(new GameObject("Test hand"));
                handObject.transform.position = grab.attachTransform.position + new Vector3(.1f, .05f, .05f);
                var hand = handObject.AddComponent<CombatTestInteractor>();
                hand.interactionManager = manager;
                hand.attachTransform = handObject.transform;
                hand.Target = grab;
                manager.SelectEnter((IXRSelectInteractor)hand, (IXRSelectInteractable)grab);
                for (int i = 0; i < 12; i++) yield return new WaitForFixedUpdate();
                CombatValidation.Require(grab.isSelected, name + ": selection failed");
                CombatValidation.Require(Vector3.Distance(grab.attachTransform.position, handObject.transform.position) < .04f,
                    name + ": handle did not align with the hand");
                foreach (Collider collider in weapon.GetComponents<Collider>())
                    CombatValidation.Require(!manager.TryGetInteractableForCollider(collider, out var found) || found != grab,
                        name + ": the blade/body incorrectly permits grabbing");
                CombatValidation.Require(manager.TryGetInteractableForCollider(grab.colliders[0], out var handleTarget) && handleTarget == grab,
                    name + ": handle collider not registered");

                handObject.transform.position += new Vector3(.15f, .12f, .05f);
                handObject.transform.rotation = Quaternion.Euler(20f, 40f, -10f);
                for (int i = 0; i < 16; i++) yield return new WaitForFixedUpdate();
                CombatValidation.Require(Vector3.Distance(grab.attachTransform.position, handObject.transform.position) < .04f &&
                    Quaternion.Angle(grab.attachTransform.rotation, handObject.transform.rotation) < 4f,
                    name + ": following the hand failed");

                hand.Holding = false;
                hand.Target = null;
                manager.SelectExit((IXRSelectInteractor)hand, (IXRSelectInteractable)grab);
                float releaseHeight = body.position.y;
                for (int i = 0; i < 25; i++) yield return new WaitForFixedUpdate();
                CombatValidation.Require(!grab.isSelected && !body.isKinematic && body.useGravity && body.position.y < releaseHeight - .1f,
                    name + ": releasing did not restore gravity");
                passed.Add(name + ": handle-only selection, pose tracking and gravity after release");
                Object.Destroy(weapon);
                Object.Destroy(handObject);
                yield return null;
            }

            var targetPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_MedievalVR/Prefabs/Combat/RedTestTarget.prefab");
            var targetObject = Keep(Object.Instantiate(targetPrefab, new Vector3(100f, 3f, 10f), Quaternion.identity));
            var target = targetObject.GetComponent<DamageableTarget>();
            Vector3 center = targetObject.transform.position;
            yield return null;
            CombatValidation.Require(target.CurrentHealth == 100f, "Target did not start with 100 health.");

            GameObject shot = Sphere(center + Vector3.back * .9f, Vector3.forward * 2f);
            for (int i = 0; i < 30; i++) yield return new WaitForFixedUpdate();
            CombatValidation.Require(target.CurrentHealth == 80f, "An ordinary object did not deal exactly 20 collision damage.");
            CombatValidation.Require(target.GetComponentInChildren<TMP_Text>().text.Contains("80 / 100"), "Health label was not updated.");
            for (int i = 0; i < 30; i++) yield return new WaitForFixedUpdate();
            CombatValidation.Require(target.CurrentHealth == 80f, "Resting contact repeatedly damaged the target.");
            passed.Add("Ordinary collision deals damage once; resting contact does not drain health; label updates");
            Object.Destroy(shot);
            yield return null;

            shot = Sphere(center + Vector3.back * .49f, Vector3.forward * .1f);
            for (int i = 0; i < 55; i++) yield return new WaitForFixedUpdate();
            CombatValidation.Require(target.CurrentHealth == 80f, "A slow touch incorrectly dealt impact damage.");
            passed.Add("Slow touches below the impact threshold do not deal damage");
            Object.Destroy(shot);
            yield return null;

            var hammerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_MedievalVR/Prefabs/Weapons/Hammer_01.prefab");
            shot = Keep(Object.Instantiate(hammerPrefab, center + new Vector3(0f, -.2f, -1.1f), Quaternion.identity));
            var hammerBody = shot.GetComponent<Rigidbody>();
            hammerBody.useGravity = false;
            hammerBody.linearVelocity = Vector3.forward * 3f;
            for (int i = 0; i < 40; i++) yield return new WaitForFixedUpdate();
            CombatValidation.Require(target.CurrentHealth == 50f, "Hammer collision did not use its 30-damage profile.");
            passed.Add("A real hammer prefab collision uses the weapon damage profile");
            Object.Destroy(shot);
            yield return null;

            target.ResetHealth();
            shot = Sphere(center + Vector3.back * .9f, Vector3.forward * 2f);
            GameObject secondShot = Sphere(center + Vector3.forward * .9f, Vector3.back * 2f);
            for (int i = 0; i < 30; i++) yield return new WaitForFixedUpdate();
            CombatValidation.Require(target.CurrentHealth == 60f, "Two different sources incorrectly shared a hit cooldown.");
            passed.Add("Independent objects can hit during the same interval");
            Object.Destroy(shot);
            Object.Destroy(secondShot);
            yield return null;

            target.ResetHealth();
            shot = Sphere(center + Vector3.back * .9f, Vector3.forward * 2f);
            shot.AddComponent<BoxCollider>().size = Vector3.one * .9f;
            for (int i = 0; i < 30; i++) yield return new WaitForFixedUpdate();
            CombatValidation.Require(target.CurrentHealth == 80f, "A compound collider counted a single impact multiple times.");
            passed.Add("Multiple colliders on one object do not multiply the impact damage");
            Object.Destroy(shot);
            yield return null;

            target.ResetHealth();
            shot = Keep(new GameObject("Rotating impact source"));
            shot.transform.position = center + new Vector3(-.8f, 0f, -.5f);
            var shape = shot.AddComponent<BoxCollider>();
            shape.center = new Vector3(.75f, 0f, 0f);
            shape.size = new Vector3(.15f, .1f, .1f);
            var spinBody = shot.AddComponent<Rigidbody>();
            spinBody.useGravity = false;
            spinBody.angularDamping = 0f;
            spinBody.centerOfMass = Vector3.zero;
            shot.AddComponent<WeaponImpact>();
            spinBody.angularVelocity = new Vector3(0f, -4f, 0f);
            for (int i = 0; i < 20; i++) yield return new WaitForFixedUpdate();
            CombatValidation.Require(target.CurrentHealth == 80f, "A rotational strike without linear velocity did not deal damage.");
            passed.Add("Rotational strikes count even when the weapon pivot has no linear velocity");
            Object.Destroy(shot);
            yield return null;

            target.ResetHealth();
            int deaths = 0;
            float healthAtDeath = -1f;
            target.Died += dead => { deaths++; healthAtDeath = dead.CurrentHealth; };
            for (int hit = 0; hit < 5; hit++)
            {
                shot = Sphere(center + Vector3.back * .9f, Vector3.forward * 2f);
                for (int i = 0; i < 30; i++) yield return new WaitForFixedUpdate();
                if (hit < 4) CombatValidation.Require(target != null && target.CurrentHealth == 100f - (hit + 1) * 20f,
                    "Repeated collisions did not reduce health correctly.");
                Object.Destroy(shot);
                yield return null;
            }
            CombatValidation.Require(targetObject == null && deaths == 1 && healthAtDeath == 0f,
                "The target was not destroyed exactly once at zero health.");
            passed.Add("Five separate 20-damage collisions destroy the entire target and its health display");
        }

        GameObject Keep(GameObject value)
        {
            temporaryObjects.Add(value);
            return value;
        }

        GameObject Sphere(Vector3 position, Vector3 velocity)
        {
            GameObject result = Keep(GameObject.CreatePrimitive(PrimitiveType.Sphere));
            result.name = "Collision test object";
            result.transform.position = position;
            result.transform.localScale = Vector3.one * .2f;
            var body = result.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.linearVelocity = velocity;
            return result;
        }

        void Finish(Exception failure)
        {
            foreach (GameObject value in temporaryObjects)
                if (value != null) Object.Destroy(value);
            string report = (failure == null ? "PASS" : "FAIL") + "\n" + string.Join("\n", passed);
            if (failure != null) report += "\n" + failure;
            File.WriteAllText(CombatValidation.ReportPath, report);
            if (failure == null) Debug.Log("[MedievalVR] All combat checks passed. " + passed.Count + " checks.");
            else Debug.LogException(failure);
            EditorApplication.delayCall += EditorApplication.ExitPlaymode;
        }
    }
}
