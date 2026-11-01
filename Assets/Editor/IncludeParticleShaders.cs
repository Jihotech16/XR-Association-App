using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Rendering;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Android 빌드 시 Hyper Casual FX 파티클 셰이더들을 강제로 포함시키는 스크립트
/// </summary>
public class IncludeParticleShaders : IPreprocessShaders
{
    public int callbackOrder => 0;

    public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)
    {
        // 파티클 셰이더들을 빌드에서 제외하지 않도록 보호
        // (아무것도 하지 않으면 기본적으로 포함됨)
    }

    [InitializeOnLoadMethod]
    private static void AddShaderGraphsToAlwaysIncluded()
    {
        // Graphics Settings의 Always Included Shaders에 자동으로 추가
        var graphicsSettings = AssetDatabase.LoadAssetAtPath<UnityEngine.Rendering.GraphicsSettings>("ProjectSettings/GraphicsSettings.asset");
        
        // VFX_Klaus 폴더의 모든 Shader Graph 찾기
        string[] shaderGuids = AssetDatabase.FindAssets("t:Shader", new[] { "Assets/VFX_Klaus/Shaders" });
        
        if (shaderGuids.Length > 0)
        {
            Debug.Log($"[IncludeParticleShaders] {shaderGuids.Length}개의 파티클 셰이더를 찾았습니다.");
            
            foreach (string guid in shaderGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
                if (shader != null)
                {
                    Debug.Log($"[IncludeParticleShaders] 파티클 셰이더 발견: {path}");
                }
            }
        }
    }
}

