using UnityEditor;
using UnityEngine;

namespace FIMSpace.FOptimizing
{
    public partial class Optimizer_BaseEditor 
    {

        protected virtual void OnSceneGUI()
        {
            if (Event.current.commandName == "Delete") Debug.Log(target.name + " is deleted");

            if (target == null) return; // Unity 2020 have problems with that

            Optimizer_Base scr = (Optimizer_Base)target;

            if (scr.CullIfNotSee)
                if (drawDetectionSphereHandle)
                {
                    Matrix4x4 m = scr.transform.localToWorldMatrix;
                    Matrix4x4 mw = scr.transform.worldToLocalMatrix;

                    Undo.RecordObject(scr, "Changing position of detection sphere");
                    EditorGUI.BeginChangeCheck();

                    Vector3 pos = m.MultiplyPoint(scr.DetectionOffset);

                    Vector3 scaled = FEditor_TransformHandles.ScaleHandle(Vector3.one * scr.DetectionRadius, pos, Quaternion.identity, .3f, true, true);
                    scr.DetectionRadius = scaled.x;

                    Vector3 transformed = FEditor_TransformHandles.PositionHandle(pos, Quaternion.identity, .3f, true, false);
                    if (Vector3.Distance(transformed, pos) > 0.00001f) scr.DetectionOffset = mw.MultiplyPoint(transformed);
                    EditorGUI.EndChangeCheck();
                }

            if (Get.UseMultiShape)
                OnSceneGUIMultiShape();
        }

    }
}

