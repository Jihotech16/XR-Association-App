using UnityEngine;
using UnityEngine.Audio;

public class ThrowableRespawn : MonoBehaviour
{
    [Header("Respawn")]
    public Transform spawnPoint;        // 테이블 위 스폰 지점
    public float respawnDelay = 2.0f;   // 바닥에 닿은 후 재생성까지의 시간 (초)
    public bool recreateObject = false; // 완전히 새로 생성할지 여부
    
    [Header("Detection")]
    public string groundTag = "Ground"; // 바닥 태그 이름
    public string targetTag = "Target"; // 타겟 태그 이름
    public string wallTag = "Wall"; // 벽 태그 이름
    
    [Header("Sound Effects")]
    public AudioClip collisionSound;    // 물체에 부딪힐 때 효과음 (이것만 설정하면 됨)
    public float minCollisionVelocity = 1.0f; // 소리가 나는 최소 충돌 속도
    public AudioMixerGroup audioMixerGroup; // Spatial Audio 믹서 그룹
    
    [Header("Particle Effects")]
    public ParticleSystem collisionParticleEffect; // 충돌 파티클 이펙트
    public ParticleSystem groundHitEffect; // 바닥 충돌 파티클 이펙트
    
    [Header("Physics Materials")]
    public PhysicsMaterial bounceMaterial; // 탄성 물리 재질

    Rigidbody _rb;
    bool isRespawning = false;          // 리스폰 중인지 확인
    bool hasHitGround = false;          // 바닥에 닿았는지 확인
    float groundHitTime = 0f;           // 바닥에 닿은 시간 기록
    
    // 파티클 중복 생성 방지
    private float lastGroundHitTime = 0f;
    private float lastCollisionTime = 0f;
    private float particleCooldown = 0.1f; // 파티클 생성 쿨다운 (초)

    void Awake() 
    {
        _rb = GetComponent<Rigidbody>();
    }

    void OnEnable() 
    {
        ResetToSpawn();  // 항상 켜질 때 안전 위치로
        ApplyBounceMaterial(); // 탄성 재질 적용
    }

    void Update()
    {
        // 바닥에 닿은 후 지정된 시간이 지나면 재생성
        if (hasHitGround && Time.time - groundHitTime >= respawnDelay)
        {
            StartRespawn();
        }
    }
    
    // 물체와 충돌했을 때 호출 (Collider가 있을 때)
    void OnCollisionEnter(Collision collision)
    {
        // 충돌 강도가 충분할 때만 소리 재생
        if (ShouldPlayCollisionSound(collision))
        {
            PlayCollisionSound();
            PlayCollisionParticleEffect(collision.contacts[0].point);
        }
        
        // 타겟 충돌인 경우 TargetCollision에서 처리하므로 여기서는 아무것도 하지 않음
        if (collision.gameObject.CompareTag(targetTag))
        {
            Debug.Log($"타겟 충돌 감지: {gameObject.name} -> {collision.gameObject.name} (TargetCollision에서 처리)");
            return; // TargetCollision에서 처리하므로 여기서는 리턴만
        }
        // 바닥 충돌인 경우 리스폰 로직 실행
        else if (collision.gameObject.CompareTag(groundTag) && !hasHitGround)
        {
            PlayGroundHitEffect(collision.contacts[0].point);
            OnGroundHit();
        }
        // 벽 충돌인 경우 파티클 이펙트만 재생 (리스폰하지 않음)
        else if (collision.gameObject.CompareTag(wallTag))
        {
            PlayCollisionParticleEffect(collision.contacts[0].point);
            Debug.Log($"벽에 충돌: {gameObject.name} -> {collision.gameObject.name}");
        }
    }
    
    // 물체와 트리거 충돌했을 때 호출 (IsTrigger가 true일 때)
    void OnTriggerEnter(Collider other)
    {
        // 트리거 충돌은 소리 재생하지 않음 (일반적으로 부드러운 충돌)
        
        // 타겟 충돌인 경우 TargetCollision에서 처리하므로 여기서는 아무것도 하지 않음
        if (other.CompareTag(targetTag))
        {
            Debug.Log($"타겟 트리거 충돌 감지: {gameObject.name} -> {other.name} (TargetCollision에서 처리)");
            return; // TargetCollision에서 처리하므로 여기서는 리턴만
        }
        // 바닥 충돌인 경우 리스폰 로직 실행
        else if (other.CompareTag(groundTag) && !hasHitGround)
        {
            OnGroundHit();
        }
        // 벽 트리거 충돌인 경우 (트리거는 파티클 재생하지 않음)
        else if (other.CompareTag(wallTag))
        {
            Debug.Log($"벽 트리거 충돌: {gameObject.name} -> {other.name}");
        }
    }
    
    
    // 바닥에 닿았을 때 처리
    void OnGroundHit()
    {
        hasHitGround = true;
        groundHitTime = Time.time;
        
        Debug.Log($"바닥에 닿음: {gameObject.name}, {respawnDelay}초 후 재생성됩니다.");
    }
    
    // 충돌 강도 체크 (소리를 재생할지 결정)
    bool ShouldPlayCollisionSound(Collision collision)
    {
        // 현재 물체의 속도가 최소 임계값보다 클 때만
        if (_rb.linearVelocity.magnitude >= minCollisionVelocity)
        {
            return true;
        }
        
        // 상대 물체의 속도도 체크 (상대가 움직이고 있을 때)
        Rigidbody otherRb = collision.rigidbody;
        if (otherRb != null && otherRb.linearVelocity.magnitude >= minCollisionVelocity)
        {
            return true;
        }
        
        return false;
    }
    
    // 충돌 효과음 재생 (Spatial Audio 믹서 사용)
    void PlayCollisionSound()
    {
        if (collisionSound != null)
        {
            // AudioSource가 없으면 자동으로 찾거나 생성
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            // VR용 3D Spatial Audio 설정
            audioSource.spatialBlend = 1.0f; // 3D Sound
            audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            audioSource.minDistance = 1f;
            audioSource.maxDistance = 20f;
            audioSource.volume = 1f;
            audioSource.playOnAwake = false;
            
            // Spatial Audio 믹서 그룹 설정
            if (audioMixerGroup != null)
            {
                audioSource.outputAudioMixerGroup = audioMixerGroup;
            }
            
            audioSource.PlayOneShot(collisionSound);
        }
    }

    public void ResetToSpawn()
    {
        if (!spawnPoint) return;

        // 잡고 있던 손/인터랙터 영향 최소화
        _rb.isKinematic = true;
        transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
        _rb.isKinematic = false;
        
        // Kinematic 해제 후 velocity 초기화 (Kinematic 상태에서는 velocity 설정 불가)
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        
        // 리스폰되면 바닥 충돌 상태 해제
        hasHitGround = false;
    }

    // 수동으로 리스폰할 때 사용
    public void ManualRespawn()
    {
        if (!isRespawning)
        {
            StartRespawn();
        }
    }
    
    // 리스폰 시작 (즉시)
    public void StartRespawn()
    {
        StartRespawnWithDelay(0f); // 즉시 리스폰
    }
    
    // 리스폰 시작 (지연 시간 지정 가능)
    public void StartRespawnWithDelay(float delay)
    {
        if (!isRespawning)
        {
            isRespawning = true;
            
            // 즉시 Meta XR 컴포넌트들을 안전하게 비활성화
            SafeDisableMetaXRComponents(gameObject);
            
            // 모든 자식 오브젝트 비활성화
            SetAllChildrenActive(gameObject, false);
            
            Invoke(nameof(Respawn), delay);
        }
    }
    
    // 모든 자식 오브젝트 활성화/비활성화 (메인 오브젝트는 유지)
    private void SetAllChildrenActive(GameObject obj, bool active)
    {
        if (obj == null) return;
        
        // 메인 오브젝트는 활성화 상태 유지하고, 자식들만 개별적으로 처리
        // (메인 오브젝트를 비활성화하면 자식들의 활성화 상태가 제대로 복원되지 않음)
        Transform[] allChildren = obj.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren)
        {
            if (child != null && child != obj.transform)
            {
                child.gameObject.SetActive(active);
            }
        }
        
        // 추가로 Collider와 Renderer도 비활성화 (메인 오브젝트 포함)
        if (!active)
        {
            // 비활성화 시에는 충돌과 렌더링도 막음
            Collider[] colliders = obj.GetComponentsInChildren<Collider>(true);
            foreach (Collider col in colliders)
            {
                if (col != null) col.enabled = false;
            }
            
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer rend in renderers)
            {
                if (rend != null) rend.enabled = false;
            }
        }
        else
        {
            // 활성화 시에는 충돌과 렌더링도 다시 켬
            Collider[] colliders = obj.GetComponentsInChildren<Collider>(true);
            foreach (Collider col in colliders)
            {
                if (col != null) col.enabled = true;
            }
            
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer rend in renderers)
            {
                if (rend != null) rend.enabled = true;
            }
        }
        
        Debug.Log($"자식 오브젝트들 {(active ? "활성화" : "비활성화")} 완료: {obj.name}");
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
            // 먼저 모든 자식과 Renderer를 활성화
            SetAllChildrenActive(gameObject, true);
            ForceActivateAllChildren(gameObject);
            ResetToSpawn();
            
            // Meta XR 컴포넌트들 완전히 재활성화
            StartCoroutine(DelayedEnableMetaXRComponents());
        }
        
        isRespawning = false;
        Debug.Log($"물체 리스폰 완료: {gameObject.name}");
    }
    
    // Meta XR 컴포넌트들을 지연 후 재활성화 (안전한 방법)
    private System.Collections.IEnumerator DelayedEnableMetaXRComponents()
    {
        // 한 프레임 대기 (물리 시스템이 안정화될 때까지)
        yield return null;
        
        // Meta XR 컴포넌트들 재활성화
        EnableMetaXRComponents(gameObject);
        
        // 추가로 한 프레임 더 대기 후 Collider 상태 확인
        yield return null;
        
        // Collider들이 제대로 활성화되었는지 확인
        ValidateColliders(gameObject);
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
        if (obj == null) return;
        
        // 메인 오브젝트 활성화
        obj.SetActive(true);
        
        // 모든 자식 오브젝트를 강제로 활성화 (비활성화된 것도 포함)
        Transform[] allChildren = obj.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren)
        {
            if (child != null)
            {
                child.gameObject.SetActive(true);
                
                // 각 자식 오브젝트의 Collider들도 활성화
                Collider[] childColliders = child.GetComponents<Collider>();
                foreach (Collider collider in childColliders)
                {
                    if (collider != null)
                    {
                        collider.enabled = true;
                    }
                }
            }
        }
        
        Debug.Log($"강제 활성화 완료: {obj.name} (총 오브젝트 수: {allChildren.Length})");
    }
    
    // Meta XR 인터랙션 컴포넌트들을 안전하게 비활성화
    private void SafeDisableMetaXRComponents(GameObject obj)
    {
        if (obj == null) return;
        
        // 모든 Meta XR 컴포넌트들을 안전하게 비활성화
        var allGrabbables = obj.GetComponentsInChildren<Oculus.Interaction.Grabbable>();
        foreach (var grab in allGrabbables)
        {
            if (grab != null)
            {
                grab.enabled = false;
            }
        }
        
        var allGrabInteractables = obj.GetComponentsInChildren<Oculus.Interaction.GrabInteractable>();
        foreach (var interactable in allGrabInteractables)
        {
            if (interactable != null)
            {
                interactable.enabled = false;
            }
        }
        
        Debug.Log($"Meta XR 컴포넌트 안전 비활성화 완료: {obj.name}");
    }
    
    // Meta XR 인터랙션 컴포넌트들을 활성화
    private void EnableMetaXRComponents(GameObject obj)
    {
        if (obj == null) return;
        
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
            if (grab != null)
            {
                grab.enabled = true;
            }
        }
        
        var allGrabInteractables = obj.GetComponentsInChildren<Oculus.Interaction.GrabInteractable>();
        foreach (var interactable in allGrabInteractables)
        {
            if (interactable != null)
            {
                interactable.enabled = true;
            }
        }
        
        Debug.Log($"Meta XR 컴포넌트 활성화 완료: {obj.name}");
    }
    
    // Collider들이 제대로 활성화되었는지 확인
    private void ValidateColliders(GameObject obj)
    {
        if (obj == null) return;
        
        // 모든 Collider 확인
        Collider[] allColliders = obj.GetComponentsInChildren<Collider>();
        foreach (Collider collider in allColliders)
        {
            if (collider != null)
            {
                // Collider가 활성화되어 있는지 확인
                if (!collider.enabled)
                {
                    collider.enabled = true;
                    Debug.Log($"Collider 재활성화: {collider.name}");
                }
                
                // MeshCollider의 경우 Convex 설정 확인
                MeshCollider meshCollider = collider as MeshCollider;
                if (meshCollider != null && !meshCollider.convex)
                {
                    meshCollider.convex = true;
                    Debug.Log($"MeshCollider Convex 설정: {meshCollider.name}");
                }
            }
        }
        
        Debug.Log($"Collider 검증 완료: {obj.name} (총 {allColliders.Length}개)");
    }
    
    // 스폰 포인트 설정 (public 메서드)
    public void SetSpawnPoint(Transform newSpawnPoint)
    {
        spawnPoint = newSpawnPoint;
    }
    
    // 수동으로 바닥 충돌 상태 리셋 (필요시 사용)
    public void ResetGroundHit()
    {
        hasHitGround = false;
        Debug.Log($"바닥 충돌 상태 리셋: {gameObject.name}");
    }
    
    // 충돌 파티클 이펙트 재생
    private void PlayCollisionParticleEffect(Vector3 hitPoint)
    {
        if (collisionParticleEffect != null)
        {
            // 쿨다운 체크 (중복 생성 방지)
            if (Time.time - lastCollisionTime < particleCooldown)
            {
                return;
            }
            lastCollisionTime = Time.time;
            
            // 충돌 지점에서 파티클 재생
            ParticleSystem effect = Instantiate(collisionParticleEffect, hitPoint, Quaternion.identity);
            
            // 파티클 재생 전에 설정 확인
            var main = effect.main;
            
            // 파티클이 이미 재생 중이면 중복 생성 방지
            if (effect.isPlaying)
            {
                Destroy(effect.gameObject);
                return;
            }
            
            effect.Play();
            
            // 파티클이 끝나면 자동으로 제거 (더 안전한 방법)
            float totalLifetime = main.duration + main.startLifetime.constantMax + 1f; // 여유시간 추가
            Destroy(effect.gameObject, totalLifetime);
            
            Debug.Log($"충돌 파티클 이펙트 재생: {hitPoint}");
        }
    }
    
    // 바닥 충돌 파티클 이펙트 재생
    private void PlayGroundHitEffect(Vector3 hitPoint)
    {
        if (groundHitEffect != null)
        {
            // 쿨다운 체크 (중복 생성 방지)
            if (Time.time - lastGroundHitTime < particleCooldown)
            {
                return;
            }
            lastGroundHitTime = Time.time;
            
            // 바닥 충돌 지점에서 파티클 재생
            ParticleSystem effect = Instantiate(groundHitEffect, hitPoint, Quaternion.identity);
            
            // 파티클 재생 전에 설정 확인
            var main = effect.main;
            
            // 파티클이 이미 재생 중이면 중복 생성 방지
            if (effect.isPlaying)
            {
                Destroy(effect.gameObject);
                return;
            }
            
            effect.Play();
            
            // 파티클이 끝나면 자동으로 제거 (더 안전한 방법)
            float totalLifetime = main.duration + main.startLifetime.constantMax + 1f; // 여유시간 추가
            Destroy(effect.gameObject, totalLifetime);
            
            Debug.Log($"바닥 충돌 파티클 이펙트 재생: {hitPoint}");
        }
    }
    
    // 모든 Collider에 탄성 재질 적용
    private void ApplyBounceMaterial()
    {
        if (bounceMaterial != null)
        {
            // 모든 Collider 찾기 (자식 포함)
            Collider[] allColliders = GetComponentsInChildren<Collider>();
            
            foreach (Collider collider in allColliders)
            {
                if (collider != null)
                {
                    collider.material = bounceMaterial;
                    Debug.Log($"탄성 재질 적용: {collider.name}");
                }
            }
            
            Debug.Log($"총 {allColliders.Length}개 Collider에 탄성 재질 적용 완료");
        }
    }
}
