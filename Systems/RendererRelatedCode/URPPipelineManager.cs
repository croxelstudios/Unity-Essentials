using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

public class URPPipelineManager : MonoBehaviour
{
    Dictionary<ScriptableRendererFeature, bool> originalStates;

    void OnDisable()
    {
        if (originalStates != null)
            foreach (KeyValuePair<ScriptableRendererFeature, bool> pair in originalStates)
                pair.Key.SetActive(pair.Value);
    }

    public void ActivateFeature(ScriptableRendererFeature feature)
    {
        RecordFeatureState(feature);
        feature.SetActive(true);
    }

    public void DeactivateFeature(ScriptableRendererFeature feature)
    {
        RecordFeatureState(feature);
        feature.SetActive(false);
    }

    void RecordFeatureState(ScriptableRendererFeature feature)
    {
        originalStates = originalStates.CreateAdd(feature, feature.isActive);
    }
}
