using UnityEngine;

public class TargetPrefabCreator : MonoBehaviour
{
    [Header("원판 설정")]
    [SerializeField] private float targetSize = 0.3f;
    [SerializeField] private Color targetColor = Color.red;
    [SerializeField] private Material targetMaterial;
    
    [Header("생성 버튼")]
    [SerializeField] private bool createTargetPrefab = false;
    
    void Update()
    {
        if (createTargetPrefab)
        {
            CreateTargetPrefab();
            createTargetPrefab = false;
        }
    }
    
    public void CreateTargetPrefab()
    {
        // 원판 오브젝트 생성
        GameObject target = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        target.name = "Target";
        target.tag = "Target";
        
        // 크기 설정
        target.transform.localScale = new Vector3(targetSize, 0.1f, targetSize);
        
        // 위치 설정
        target.transform.position = transform.position;
        
        // 머티리얼 설정
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer != null)
        {
            if (targetMaterial != null)
            {
                renderer.material = targetMaterial;
            }
            else
            {
                // 기본 머티리얼 생성
                Material newMaterial = new Material(Shader.Find("Standard"));
                newMaterial.color = targetColor;
                renderer.material = newMaterial;
            }
        }
        
        // Rigidbody 추가 (중력 영향 받지 않도록)
        Rigidbody rb = target.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
        
        // Collider를 Trigger로 설정
        Collider collider = target.GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
        
        // TargetCollision 스크립트 추가
        target.AddComponent<TargetCollision>();
        
        Debug.Log("원판 프리팹이 생성되었습니다!");
    }
}
