using UnityEngine;

public class ThrowableRespawn : MonoBehaviour
{
     [Header("Respawn")]
    public Transform spawnPoint;        // 테이블 위 스폰 지점
    public float fallY = -1.0f;         // 이 Y보다 낮아지면 리스폰
    public float respawnDelay = 0.15f;  // 순간이동 전 안정화 딜레이
    public bool autoRespawn = true;     // 자동 리스폰 여부
    public bool recreateObject = false; // 완전히 새로 생성할지 여부

    Rigidbody _rb;
    bool _queued;
    bool isRespawning = false;          // 리스폰 중인지 확인

    void Awake() => _rb = GetComponent<Rigidbody>();

    void OnEnable() => ResetToSpawn();  // 항상 켜질 때 안전 위치로

    void Update()
    {
        if (autoRespawn && !_queued && transform.position.y < fallY)
        {
            _queued = true;
            Invoke(nameof(ResetToSpawn), respawnDelay);
        }
    }

    public void ResetToSpawn()
    {
        _queued = false;
        if (!spawnPoint) return;

        // 잡고 있던 손/인터랙터 영향 최소화
        _rb.isKinematic = true;
        transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.isKinematic = false;
    }

    // 수동으로 리스폰할 때 사용
    public void ManualRespawn()
    {
        if (!isRespawning)
        {
            StartRespawn();
        }
    }
    
    // 리스폰 시작
    public void StartRespawn()
    {
        if (!isRespawning)
        {
            isRespawning = true;
            Invoke(nameof(Respawn), respawnDelay);
        }
    }
    
    // 실제 리스폰 실행
    private void Respawn()
    {
        Debug.Log($"리스폰 시작: {gameObject.name}");
        
        if (recreateObject)
        {
            // 완전히 새로운 오브젝트 생성
            RecreateObject();
        }
        else
        {
            // 기존 오브젝트 복원
            ForceActivateAllChildren(gameObject);
            ResetToSpawn();
            
            // Meta XR 컴포넌트들 재활성화
            EnableMetaXRComponents(gameObject);
        }
        
        isRespawning = false;
        Debug.Log($"물체 리스폰 완료: {gameObject.name}");
    }
    
    // 완전히 새로운 오브젝트 생성
    private void RecreateObject()
    {
        if (spawnPoint == null) return;
        
        // 현재 오브젝트 제거
        Destroy(gameObject);
        
        // 프리팹에서 새로 생성 (ObjectSpawner가 처리)
        Debug.Log("완전히 새로운 오브젝트 생성 요청");
    }
    
    // 강제로 모든 자식 오브젝트 활성화 (더 강력한 방법)
    private void ForceActivateAllChildren(GameObject obj)
    {
        // 메인 오브젝트 활성화
        obj.SetActive(true);
        
        // 모든 자식 오브젝트를 강제로 활성화 (비활성화된 것도 포함)
        Transform[] allChildren = obj.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren)
        {
            child.gameObject.SetActive(true);
        }
        
        Debug.Log($"강제 활성화 완료: {obj.name} (총 오브젝트 수: {allChildren.Length})");
    }
    
    // Meta XR 인터랙션 컴포넌트들을 활성화
    private void EnableMetaXRComponents(GameObject obj)
    {
        // Grabbable 컴포넌트 활성화
        var grabbable = obj.GetComponent<Oculus.Interaction.Grabbable>();
        if (grabbable != null)
        {
            grabbable.enabled = true;
        }
        
        // GrabInteractable 컴포넌트 활성화
        var grabInteractable = obj.GetComponent<Oculus.Interaction.GrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.enabled = true;
        }
        
        // 모든 자식 오브젝트에서도 활성화
        var allGrabbables = obj.GetComponentsInChildren<Oculus.Interaction.Grabbable>();
        foreach (var grab in allGrabbables)
        {
            grab.enabled = true;
        }
        
        var allGrabInteractables = obj.GetComponentsInChildren<Oculus.Interaction.GrabInteractable>();
        foreach (var interactable in allGrabInteractables)
        {
            interactable.enabled = true;
        }
        
        Debug.Log($"Meta XR 컴포넌트 활성화 완료: {obj.name}");
    }
    
    // 스폰 포인트 설정 (public 메서드)
    public void SetSpawnPoint(Transform newSpawnPoint)
    {
        spawnPoint = newSpawnPoint;
    }
}
