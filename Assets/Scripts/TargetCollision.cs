using UnityEngine;

public class TargetCollision : MonoBehaviour
{
    [Header("충돌 설정")]
    [SerializeField] private int scoreValue = 10; // 원판당 점수
    [SerializeField] private GameObject hitEffect; // 충돌 효과 (선택사항)
    
    private TargetMiniGame gameManager;
    private TargetBreakEffect breakEffect;
    private bool isHit = false;
    
    public void Initialize(TargetMiniGame manager)
    {
        gameManager = manager;
        
        // 부서지는 이펙트 컴포넌트 가져오기
        breakEffect = GetComponent<TargetBreakEffect>();
        if (breakEffect == null)
        {
            breakEffect = gameObject.AddComponent<TargetBreakEffect>();
        }
        
        Debug.Log($"원판 초기화 완료: {gameObject.name}");
    }
    
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Trigger 충돌 감지: {other.name} -> {gameObject.name}");
        
        // 충돌한 물체 제거
        DestroyCollidingObject(other.gameObject);
        
        HitTarget(); // 간단하게 바로 점수 추가
    }
    
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Collision 충돌 감지: {collision.gameObject.name} -> {gameObject.name}");
        
        // 충돌한 물체 제거
        DestroyCollidingObject(collision.gameObject);
        
        HitTarget(); // 간단하게 바로 점수 추가
    }
    
    private void HitTarget()
    {
        if (isHit) 
        {
            Debug.Log("이미 맞춘 원판입니다.");
            return;
        }
        
        isHit = true;
        Debug.Log($"원판 맞춤! +{scoreValue}점");
        
        // 점수 추가
        if (gameManager != null)
        {
            gameManager.AddScore(scoreValue);
        }
        else
        {
            Debug.LogError("GameManager가 null입니다!");
        }
        
        // 부서지는 이펙트 재생
        if (breakEffect != null)
        {
            breakEffect.BreakTarget();
        }
        else
        {
            // 기존 충돌 효과 재생
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, transform.rotation);
            }
            
            // 원판 제거
            Destroy(gameObject);
        }
    }
    
    // 충돌한 물체 제거를 위한 메서드
    private void DestroyCollidingObject(GameObject collidingObject)
    {
        if (collidingObject != null)
        {
            // Meta XR 인터랙션 컴포넌트들을 먼저 비활성화
            DisableMetaXRComponents(collidingObject);
            
            // 던진 물체가 리스폰 가능한지 확인
            ThrowableRespawn respawn = collidingObject.GetComponent<ThrowableRespawn>();
            if (respawn != null)
            {
                // 모든 자식 오브젝트를 포함하여 비활성화
                SetAllChildrenActive(collidingObject, false);
                respawn.StartRespawn();
                Debug.Log($"충돌한 물체 리스폰 시작: {collidingObject.name} (자식 오브젝트 수: {collidingObject.transform.childCount})");
            }
            else
            {
                // 리스폰 컴포넌트가 없으면 바로 제거
                Destroy(collidingObject);
                Debug.Log($"충돌한 물체 제거: {collidingObject.name}");
            }
        }
    }
    
    // Meta XR 인터랙션 컴포넌트들을 비활성화
    private void DisableMetaXRComponents(GameObject obj)
    {
        // Grabbable 컴포넌트 비활성화
        var grabbable = obj.GetComponent<Oculus.Interaction.Grabbable>();
        if (grabbable != null)
        {
            grabbable.enabled = false;
        }
        
        // GrabInteractable 컴포넌트 비활성화
        var grabInteractable = obj.GetComponent<Oculus.Interaction.GrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.enabled = false;
        }
        
        // 모든 자식 오브젝트에서도 비활성화
        var allGrabbables = obj.GetComponentsInChildren<Oculus.Interaction.Grabbable>();
        foreach (var grab in allGrabbables)
        {
            grab.enabled = false;
        }
        
        var allGrabInteractables = obj.GetComponentsInChildren<Oculus.Interaction.GrabInteractable>();
        foreach (var interactable in allGrabInteractables)
        {
            interactable.enabled = false;
        }
        
        Debug.Log($"Meta XR 컴포넌트 비활성화 완료: {obj.name}");
    }
    
    // 모든 자식 오브젝트 활성화/비활성화 (재귀적)
    private void SetAllChildrenActive(GameObject obj, bool active)
    {
        obj.SetActive(active);
        
        // 모든 자식 오브젝트도 처리
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            Transform child = obj.transform.GetChild(i);
            SetAllChildrenActive(child.gameObject, active);
        }
    }
}
