using UnityEngine;
using UnityEditor;
using System.Linq;

/// <summary>
/// 셰이더 키워드 불일치 문제를 해결하는 에디터 유틸리티
/// </summary>
public class MaterialShaderFixer : EditorWindow
{
    [MenuItem("Tools/Fix Material Shader Keywords")]
    public static void ShowWindow()
    {
        GetWindow<MaterialShaderFixer>("Material Shader Fixer");
    }

    private void OnGUI()
    {
        GUILayout.Label("셰이더 키워드 불일치 문제 해결", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("모든 머티리얼 셰이더 재할당", GUILayout.Height(30)))
        {
            FixAllMaterials();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("URP Lit 셰이더 사용 머티리얼만 수정", GUILayout.Height(30)))
        {
            FixURPLitMaterials();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("머티리얼 키워드 초기화", GUILayout.Height(30)))
        {
            ClearMaterialKeywords();
        }
    }

    private static void FixAllMaterials()
    {
        string[] materialGuids = AssetDatabase.FindAssets("t:Material");
        int fixedCount = 0;

        foreach (string guid in materialGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (mat != null && mat.shader != null)
            {
                Shader currentShader = mat.shader;
                mat.shader = currentShader; // 셰이더를 다시 할당하여 키워드 재계산
                EditorUtility.SetDirty(mat);
                fixedCount++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"총 {materialGuids.Length}개의 머티리얼 중 {fixedCount}개를 수정했습니다.");
        EditorUtility.DisplayDialog("완료", $"{fixedCount}개의 머티리얼을 수정했습니다.", "확인");
    }

    private static void FixURPLitMaterials()
    {
        string[] materialGuids = AssetDatabase.FindAssets("t:Material");
        int fixedCount = 0;

        Shader urpLitShader = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLitShader == null)
        {
            EditorUtility.DisplayDialog("오류", "Universal Render Pipeline/Lit 셰이더를 찾을 수 없습니다.", "확인");
            return;
        }

        foreach (string guid in materialGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (mat != null && mat.shader != null)
            {
                // URP Lit 셰이더를 사용하는 머티리얼만 수정
                if (mat.shader.name.Contains("Universal Render Pipeline/Lit") || 
                    mat.shader.name.Contains("Meta/Lit"))
                {
                    // 셰이더를 다시 할당
                    mat.shader = urpLitShader;
                    EditorUtility.SetDirty(mat);
                    fixedCount++;
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"URP Lit 셰이더를 사용하는 {fixedCount}개의 머티리얼을 수정했습니다.");
        EditorUtility.DisplayDialog("완료", $"{fixedCount}개의 URP Lit 머티리얼을 수정했습니다.", "확인");
    }

    private static void ClearMaterialKeywords()
    {
        string[] materialGuids = AssetDatabase.FindAssets("t:Material");
        int fixedCount = 0;

        foreach (string guid in materialGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (mat != null)
            {
                // 모든 로컬 키워드 제거
                var keywords = mat.shaderKeywords.ToList();
                foreach (var keyword in keywords)
                {
                    mat.DisableKeyword(keyword);
                }
                
                EditorUtility.SetDirty(mat);
                fixedCount++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"총 {fixedCount}개의 머티리얼에서 키워드를 초기화했습니다.");
        EditorUtility.DisplayDialog("완료", $"{fixedCount}개의 머티리얼 키워드를 초기화했습니다.", "확인");
    }
}


