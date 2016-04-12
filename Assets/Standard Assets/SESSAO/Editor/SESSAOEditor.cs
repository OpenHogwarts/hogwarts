using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(SESSAO))]
public class SESSAOEditor : Editor
{
	SerializedObject serObj;

	SerializedProperty radius;
	SerializedProperty bias;
	SerializedProperty bilateralDepthTolerance;
	SerializedProperty zThickness;
	SerializedProperty occlusionIntensity;
	SerializedProperty sampleDistributionCurve;
	SerializedProperty colorBleedAmount;
	SerializedProperty drawDistance;
	SerializedProperty drawDistanceFadeSize;
	SerializedProperty reduceSelfBleeding;
	SerializedProperty useDownsampling;
	SerializedProperty visualizeSSAO;
	SerializedProperty brightnessThreshold;
	SerializedProperty halfSampling;
	SerializedProperty preserveDetails;

	SESSAO targetInstance;

	void OnEnable()
	{
		serObj = new SerializedObject(target);

		radius = serObj.FindProperty("radius"); 
		bias = serObj.FindProperty("bias");
		bilateralDepthTolerance = serObj.FindProperty("bilateralDepthTolerance");
		zThickness = serObj.FindProperty("zThickness");
		occlusionIntensity = serObj.FindProperty("occlusionIntensity");
		sampleDistributionCurve = serObj.FindProperty("sampleDistributionCurve");
		colorBleedAmount = serObj.FindProperty("colorBleedAmount");
		brightnessThreshold = serObj.FindProperty("brightnessThreshold");
		drawDistance = serObj.FindProperty("drawDistance");
		drawDistanceFadeSize = serObj.FindProperty("drawDistanceFadeSize");
		reduceSelfBleeding = serObj.FindProperty("reduceSelfBleeding");
		useDownsampling = serObj.FindProperty("useDownsampling");
		visualizeSSAO = serObj.FindProperty("visualizeSSAO");
		halfSampling = serObj.FindProperty("halfSampling");
		preserveDetails = serObj.FindProperty("preserveDetails");

		targetInstance = (SESSAO)target;
	}

	public override void OnInspectorGUI()
	{
		serObj.Update();

		GUILayout.BeginVertical();
		GUILayout.Space(5);
		EditorGUILayout.PropertyField(occlusionIntensity);
		EditorGUILayout.PropertyField(colorBleedAmount);
		EditorGUILayout.PropertyField(radius);
		EditorGUILayout.PropertyField(sampleDistributionCurve);
		EditorGUILayout.PropertyField(drawDistance);
		EditorGUILayout.PropertyField(drawDistanceFadeSize);
		EditorGUILayout.PropertyField(bias);
		EditorGUILayout.PropertyField(zThickness);
		EditorGUILayout.PropertyField(bilateralDepthTolerance);
		EditorGUILayout.PropertyField(brightnessThreshold);
		EditorGUILayout.PropertyField(reduceSelfBleeding);
		EditorGUILayout.PropertyField(preserveDetails);
		EditorGUILayout.PropertyField(useDownsampling);
		EditorGUILayout.PropertyField(halfSampling);
		EditorGUILayout.PropertyField(visualizeSSAO);
//		occlusionIntensitySlider.RenderSlider(occlusionIntensity, 0.5f, 5.0f);
//		colorBleedAmountSlider.RenderSlider(colorBleedAmount, 0.0f, 1.0f);
//		radiusSlider.RenderSlider(radius, 0.02f, 5.0f);
//		sampleDistributionCurveSlider.RenderSlider(sampleDistributionCurve, 1.0f, 6.0f);
//		drawDistanceSlider.RenderSlider(drawDistance, targetInstance.attachedCamera.nearClipPlane, targetInstance.attachedCamera.farClipPlane);
//		drawDistanceFadeSizeSlider.RenderSlider(drawDistanceFadeSize, 0.0f, targetInstance.attachedCamera.farClipPlane - targetInstance.attachedCamera.nearClipPlane);
//		biasSlider.RenderSlider(bias, -0.2f, 0.5f);
//		zThicknessSlider.RenderSlider(zThickness, 1.0f, 5.0f);
//		bilateralDepthToleranceSlider.RenderSlider(bilateralDepthTolerance, 0.1f, 3.0f);
//		brightnessThresholdSlider.RenderSlider(brightnessThreshold, 0.1f, 3.0f);
//		reduceSelfBleedingToggle.RenderToggle(reduceSelfBleeding);
//		preserveDetailsToggle.RenderToggle(preserveDetails);
//		useDownsamplingToggle.RenderToggle(useDownsampling);
//		halfSamplingToggle.RenderToggle(halfSampling);
//		visualizeSSAOToggle.RenderToggle(visualizeSSAO);
		GUILayout.Space(5);
		GUILayout.EndVertical();

		serObj.ApplyModifiedProperties();
	}
}
