using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpikyPole))]
public class SpikyPoleEditor : Editor {

    private float arrowSpacing = 0.5f;
    private float arrowLength = 0.7f;
    private float arrowThickness = 2;
    private float arrowAngle = 45;
    private Color arrowColor = Color.red;
    private MeshRenderer renderer;
    private SpikyPole spikyPole;
    private Mesh mesh;
    private Material previewMaterial;
    private Shader previewShader;
    private string buttonText;
    private string playPreviewText;
    private string stopPreviewText;
    private bool previewIsPlaying;
    private float spikyPolePreviewScaleFactor = 1.01f;  // Draw previews a little larger to avoid z-fighting.
    private float previewStartTime;
    GUILayoutOption[] buttonOptions;

    private void OnEnable() {
        spikyPole = (SpikyPole)target;
        playPreviewText = "Play preview";
        stopPreviewText = "Stop preview";
        buttonText = playPreviewText;
        buttonOptions = new GUILayoutOption[] {
            GUILayout.MaxWidth(100f),
            GUILayout.MinWidth(100f)
        };
        renderer = spikyPole.GetComponent<MeshRenderer>();
        mesh = spikyPole.GetComponent<MeshFilter>().sharedMesh;
        previewShader = Shader.Find("Shader Graphs/Hologram");
        previewMaterial = new Material(previewShader);
        SceneView.duringSceneGui += DuringSceneViewGUI;
    } 

    private void OnDisable() {
        SceneView.duringSceneGui -= DuringSceneViewGUI;
    }

    private void DuringSceneViewGUI(SceneView sceneView) {
        if (Selection.Contains(spikyPole.gameObject)) {

            if(previewIsPlaying) {
                PreviewSpikyPoleMovement(spikyPole, sceneView.camera);
            }else {
                if (Application.isPlaying) {
                    DrawSpikyPoleInitialPosition(spikyPole, sceneView.camera);
                } else {
                    DrawSpikyPoleTrajectory(spikyPole, animate:true);
                    HandleUtility.Repaint();
                }
                DrawSpikyPoleTargetPosition(spikyPole, sceneView.camera);
            }
        }
    }

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        GUI.enabled = !Application.isPlaying;
        if (GUILayout.Button(buttonText, buttonOptions)) {
            previewIsPlaying = !previewIsPlaying;
            if (previewIsPlaying) {
                buttonText = stopPreviewText;
                previewStartTime = (float)EditorApplication.timeSinceStartup;
            } else {
                buttonText = playPreviewText;
            }
        }
    }
    private void DrawSpikyPoleInitialPosition(SpikyPole spikyPole, Camera camera) {
        Matrix4x4 matrix = Matrix4x4.TRS(spikyPole.InitialPosition, 
                                         spikyPole.transform.rotation, 
                                         spikyPole.transform.localScale * spikyPolePreviewScaleFactor);
        Graphics.DrawMesh(mesh, matrix, previewMaterial, 0, camera);
    }

    private void DrawSpikyPoleTargetPosition(SpikyPole spikyPole, Camera camera) {
        Vector3 targetPosition = Vector3.zero;
        if (Application.isPlaying) {
            targetPosition = spikyPole.InitialPosition + spikyPole.transform.forward * spikyPole.Displacement;
        } else {
            targetPosition = spikyPole.transform.position + spikyPole.transform.forward * spikyPole.Displacement;
        }
        Matrix4x4 matrix = Matrix4x4.TRS(targetPosition, spikyPole.transform.rotation, spikyPole.transform.localScale *spikyPolePreviewScaleFactor);
        Graphics.DrawMesh(mesh, matrix, previewMaterial, 0, camera);
    }

    private void PreviewSpikyPoleMovement(SpikyPole spikyPole, Camera camera) {
        float time = (float)EditorApplication.timeSinceStartup - previewStartTime;
        Vector3 targetPosition = spikyPole.transform.position + 
                                 spikyPole.transform.forward * spikyPole.EvaluateDisplacement(time);
        Matrix4x4 matrix = Matrix4x4.TRS(targetPosition, spikyPole.transform.rotation, spikyPole.transform.localScale * spikyPolePreviewScaleFactor);
        Graphics.DrawMesh(mesh, matrix, previewMaterial, 0, camera);
    }


    private void DrawSpikyPoleTrajectory(SpikyPole spikyPole, bool animate = true) {
        Gizmos.color = Color.red;
        Vector3 startPosition = Vector3.zero;
        if (Application.isPlaying) {
            startPosition = spikyPole.InitialPosition;
        } else {
            startPosition = spikyPole.transform.position;
        }
        startPosition += spikyPole.transform.forward * (renderer.bounds.extents.z + arrowLength);

        float spikyPoleWidth = renderer.bounds.extents.z * 2;
        int numArrows = (int)Math.Floor( (spikyPole.Displacement - spikyPoleWidth - arrowLength) / (arrowSpacing + arrowLength) );
        numArrows = Math.Max(numArrows, 1);
        
        float maxDisplacement = (numArrows)*( arrowSpacing + arrowLength);
        Quaternion arrowRotation = spikyPole.transform.rotation * Quaternion.Euler(0.0f, 0.0f, 90f);
        Vector3 arrowsStartPosition = startPosition + spikyPole.transform.forward * (spikyPoleWidth / 2.0f - arrowLength / 2.0f);
        float time  = (float)EditorApplication.timeSinceStartup % ( maxDisplacement/spikyPole.Speed);
        
        for (int i = 0; i < numArrows; i++) {
            float arrowLocalOffset = (arrowLength + arrowSpacing) * i;
            float animationOffset = spikyPole.Speed * time;
            Vector3 arrowPosition = Vector3.zero;
            if (animate) {
                arrowPosition = arrowsStartPosition + spikyPole.transform.forward * ((arrowLocalOffset + animationOffset) % maxDisplacement);
            } else {
                arrowPosition = arrowsStartPosition + spikyPole.transform.forward * arrowLocalOffset;
            }
            GizmosExtensions.DrawArrowHead(arrowPosition, arrowRotation, arrowAngle, arrowLength, arrowThickness, arrowColor);
        }
    }
}