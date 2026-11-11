using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

[DisallowMultipleComponent]
public sealed class MovimientoConBrazos : MonoBehaviour, IXRInputValueReader<Vector2>
{
    [SerializeField, Range(0.1f, 0.6f)] float umbralBalanceo = 0.22f;
    [SerializeField, Range(0.7f, 2f)] float balanceoParaCorrer = 1.2f;

    const float RecorridoMinimo = 0.05f;
    const float PausaEntreFases = 0.85f;
    const float PausaAlInvertir = 0.18f;

    DynamicMoveProvider movimiento;
    XRInputValueReader<Vector2> entradaIzquierda;
    TrackedPoseDriver visor, manoIzquierda, manoDerecha;
    Vector3 anteriorIzquierda, anteriorDerecha;
    Vector2 lectura;
    int ultimoFrame, faseActual, ultimaFase;
    bool hayMuestra, hayRitmo;
    float recorrido, tiempoUltimaFase, tiempoUltimoBalanceo, tiempoUltimaMuestra;
    float objetivo, intensidad;

    void OnEnable()
    {
        movimiento = GetComponent<DynamicMoveProvider>();
        visor = movimiento?.headTransform?.GetComponent<TrackedPoseDriver>();
        manoIzquierda = movimiento?.leftControllerTransform?.GetComponent<TrackedPoseDriver>();
        manoDerecha = movimiento?.rightControllerTransform?.GetComponent<TrackedPoseDriver>();
        entradaIzquierda = movimiento?.leftHandMoveInput;

        if (visor == null || manoIzquierda == null || manoDerecha == null ||
            entradaIzquierda == null || entradaIzquierda.bypass != null)
        {
            Debug.LogWarning("Revisa Dynamic Move Provider en Locomotion/Move.", this);
            enabled = false;
            return;
        }

        ultimoFrame = -1;
        ReiniciarGesto();
        entradaIzquierda.bypass = this;
    }

    void OnDisable()
    {
        if (ReferenceEquals(entradaIzquierda?.bypass, this))
            entradaIzquierda.bypass = null;
    }

    Vector2 IXRInputValueReader<Vector2>.ReadValue() => LeerMovimiento();

    bool IXRInputValueReader<Vector2>.TryReadValue(out Vector2 valor)
    {
        valor = LeerMovimiento();
        return valor != Vector2.zero;
    }

    Vector2 LeerMovimiento()
    {
        int frame = Time.frameCount;
        if (frame == ultimoFrame)
            return lectura;

        if (frame != ultimoFrame + 1)
            ReiniciarGesto();
        ultimoFrame = frame;

        // Dentro del bypass, el lector devuelve el stick original.
        lectura = entradaIzquierda.ReadValue();
        if (lectura != Vector2.zero || movimiento.rightHandMoveInput.ReadValue() != Vector2.zero)
        {
            ReiniciarGesto();
            return lectura;
        }

        lectura = new Vector2(0f, MedirBalanceo());
        return lectura;
    }

    float MedirBalanceo()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f || dt > 0.1f || !TienePosicion(visor) ||
            !TienePosicion(manoIzquierda) || !TienePosicion(manoDerecha))
        {
            ReiniciarGesto();
            return 0f;
        }

        // El visor y los mandos comparten Camera Offset.
        Vector3 cabeza = visor.transform.localPosition;
        Vector3 izquierda = manoIzquierda.transform.localPosition - cabeza;
        Vector3 derecha = manoDerecha.transform.localPosition - cabeza;
        Vector3 frente = Vector3.ProjectOnPlane(
            visor.transform.localRotation * Vector3.forward, Vector3.up);
        Vector3 lateral = Vector3.Cross(Vector3.up, frente.normalized);

        if (frente.sqrMagnitude < 0.04f || izquierda.y > -0.25f || derecha.y > -0.25f ||
            Vector3.Dot(izquierda, lateral) > -0.10f || Vector3.Dot(derecha, lateral) < 0.10f)
        {
            ReiniciarGesto();
            return 0f;
        }

        frente.Normalize();
        float deltaIzquierda = Vector3.Dot(izquierda - anteriorIzquierda, frente);
        float deltaDerecha = Vector3.Dot(derecha - anteriorDerecha, frente);
        anteriorIzquierda = izquierda;
        anteriorDerecha = derecha;

        if (!hayMuestra)
        {
            hayMuestra = true;
            return 0f;
        }

        float ahora = Time.time;
        float avance = Mathf.Min(Mathf.Abs(deltaIzquierda), Mathf.Abs(deltaDerecha));
        float rapidez = avance / dt;

        if (deltaIzquierda * deltaDerecha < 0f && rapidez >= umbralBalanceo)
        {
            int fase = deltaIzquierda > 0f ? 1 : -1;
            if (fase != faseActual || ahora - tiempoUltimaMuestra > 0.12f)
            {
                faseActual = fase;
                recorrido = 0f;
            }

            tiempoUltimaMuestra = ahora;
            recorrido += avance;
            if (fase != ultimaFase && recorrido >= RecorridoMinimo)
            {
                hayRitmo = ultimaFase != 0 && ahora - tiempoUltimaFase <= PausaEntreFases;
                ultimaFase = fase;
                tiempoUltimaFase = ahora;
            }

            if (hayRitmo && ahora - tiempoUltimaFase <= PausaEntreFases)
            {
                tiempoUltimoBalanceo = ahora;
                float nivel = Mathf.InverseLerp(umbralBalanceo, balanceoParaCorrer, rapidez);
                objetivo = Mathf.Lerp(0.28f, 1f, nivel);
            }
        }

        float destino = ahora - tiempoUltimoBalanceo <= PausaAlInvertir ? objetivo : 0f;
        intensidad = Mathf.MoveTowards(intensidad, destino, dt * (destino > 0f ? 2.5f : 7f));
        return intensidad;
    }

    static bool TienePosicion(TrackedPoseDriver pose)
    {
        if (pose == null || !pose.isActiveAndEnabled)
            return false;

        var estado = pose.trackingStateInput.action;
        return estado != null && (estado.ReadValue<int>() & (int)InputTrackingState.Position) != 0;
    }

    void ReiniciarGesto()
    {
        hayMuestra = false;
        hayRitmo = false;
        faseActual = 0;
        ultimaFase = 0;
        recorrido = 0f;
        objetivo = 0f;
        intensidad = 0f;
    }
}