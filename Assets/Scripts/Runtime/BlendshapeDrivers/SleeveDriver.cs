using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class SleeveDriver : MonoBehaviour{
    [SerializeField] private AnimationCurve sleeveUpAnimationCurve;
    [SerializeField] private AnimationCurve sleeveDownAnimationCurve;
    [SerializeField] private Transform bone;
    [SerializeField] private float boneMaxLocalRotation = 90;
    [SerializeField] private float boneMinLocalRotation = 10;

    [BlendShapeName(nameof(GetAvailableOptions))]
    public string sleeveDownBlendShapeName;
    [BlendShapeName(nameof(GetAvailableOptions))]
    public string sleeveUpBlendShapeName;

    private SkinnedMeshRenderer skinnedMeshRenderer;
    private int sleeveDownBlendShapeIndex;
    private int sleeveUpBlendShapeIndex;

    public string[] GetAvailableOptions() {
        if(skinnedMeshRenderer == null) {
            skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        }

        int numBlendShapes = skinnedMeshRenderer.sharedMesh.blendShapeCount;
        string[] options = new string[numBlendShapes];

        for (int i = 0 ; i < numBlendShapes; i++) {
            options[i] = skinnedMeshRenderer.sharedMesh.GetBlendShapeName(i);
        }
        return options;
    }
     
    private void Start(){
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        sleeveDownBlendShapeIndex = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(sleeveDownBlendShapeName);
        sleeveUpBlendShapeIndex = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(sleeveUpBlendShapeName);
    }


    private void Update(){
        float angle = bone.localRotation.eulerAngles.z % 360;
        if(angle > 180) {
            angle -= 360;
        }

        if(angle > 0) { //Sleeve down
            float rotation01 = Mathf.InverseLerp(boneMinLocalRotation, boneMaxLocalRotation, angle);
            float weight = sleeveDownAnimationCurve.Evaluate(rotation01);
            skinnedMeshRenderer.SetBlendShapeWeight(sleeveDownBlendShapeIndex, weight);
            //Debug.Log(angle + " " + rotation01);

        } else { //Sleeve Up
            float rotation01 = Mathf.InverseLerp(-boneMaxLocalRotation, -boneMinLocalRotation, angle);
            float weight = sleeveUpAnimationCurve.Evaluate(rotation01);
            skinnedMeshRenderer.SetBlendShapeWeight(sleeveUpBlendShapeIndex, weight);
            //Debug.Log(angle + " " + rotation01);
        }
    }
}
