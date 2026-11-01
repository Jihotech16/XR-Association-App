using UnityEngine;
using UnityEngine.Audio;

public class TargetCollision : MonoBehaviour
{
    [Header("충돌 설정")]
    [SerializeField] private int scoreValue = 10; // 원판당 점수
    
    [Header("파티클 이펙트")]
    [SerializeField] private ParticleSystem hitParticleEffect; // 충돌 파티클 이펙트
    
    [Header("사운드 이펙트")]
    [SerializeField] private AudioClip hitSound; // 타겟 맞췄을 때 효과음
    [SerializeField] private AudioMixerGroup audioMixerGroup; // Spatial Audio 믹서 그룹
    
    private TargetMiniGame gameManager;
    private bool isHit = false;
    
    public void Initialize(TargetMiniGame manager)
    {
        gameManager = manager;
        
        Debug.Log($"원판 초기화 완료: {gameObject.name}");
    }
    
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Trigger 충돌 감지: {other.name} -> {gameObject.name}");
        
        // 타겟끼리 충돌하는지 확인
        if (IsTargetCollision(other.gameObject))
        {
            Debug.Log("타겟끼리 충돌 - 점수 추가하지 않음");
            return;
        }
        
        // 충돌한 물체 제거
        DestroyCollidingObject(other.gameObject);
        
        HitTarget(); // 간단하게 바로 점수 추가
    }
    
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Collision 충돌 감지: {collision.gameObject.name} -> {gameObject.name}");
        
        // 타겟끼리 충돌하는지 확인
        if (IsTargetCollision(collision.gameObject))
        {
            Debug.Log("타겟끼리 충돌 - 점수 추가하지 않음");
            return;
        }
        
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
        
        // 파티클 이펙트 재생 (TargetBreakEffect 무시)
        PlayHitParticleEffect();
        
        // 원판 제거
        Destroy(gameObject);
    }
    
    // 충돌한 물체 제거를 위한 메서드
    private void DestroyCollidingObject(GameObject collidingObject)
    {
        if (collidingObject != null)
        {
            // 타겟 맞춤 효과음을 충돌한 물체에서 재생
            PlayHitSoundOnObject(collidingObject);
            
            // 던진 물체가 리스폰 가능한지 확인 (부모 오브젝트에서도 검색)
            ThrowableRespawn respawn = collidingObject.GetComponentInParent<ThrowableRespawn>();
            if (respawn != null)
            {
                // 즉시 리스폰 시작 (안전한 비활성화는 ThrowableRespawn에서 처리)
                respawn.StartRespawn();
                Debug.Log($"충돌한 물체 리스폰 시작: {respawn.gameObject.name} (충돌 지점: {collidingObject.name})");
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
    
    // 충돌 파티클 이펙트 재생
    private void PlayHitParticleEffect()
    {
        if (hitParticleEffect != null)
        {
            // 충돌 위치에서 파티클 재생
            ParticleSystem effect = Instantiate(hitParticleEffect, transform.position, transform.rotation);
            effect.Play();
            
            // 파티클이 끝나면 자동으로 제거
            Destroy(effect.gameObject, effect.main.duration + effect.main.startLifetime.constantMax);
            
            Debug.Log("충돌 파티클 이펙트 재생!");
        }
    }
    
    // 타겟끼리 충돌하는지 확인
    private bool IsTargetCollision(GameObject collidingObject)
    {
        // 충돌한 오브젝트가 타겟 태그를 가지고 있는지 확인
        if (collidingObject.CompareTag("Target"))
        {
            return true;
        }
        
        // 충돌한 오브젝트에 TargetCollision 컴포넌트가 있는지 확인
        TargetCollision targetCollision = collidingObject.GetComponent<TargetCollision>();
        if (targetCollision != null)
        {
            return true;
        }
        
        // 부모 오브젝트에서도 확인
        TargetCollision parentTargetCollision = collidingObject.GetComponentInParent<TargetCollision>();
        if (parentTargetCollision != null)
        {
            return true;
        }
        
        return false;
    }
    
    // 타겟 맞췄을 때 효과음을 충돌한 물체에서 재생
    private void PlayHitSoundOnObject(GameObject targetObject)
    {
        if (hitSound != null)
        {
            // 충돌한 물체에서 AudioSource 찾기 (부모 오브젝트에서도 검색)
            AudioSource audioSource = targetObject.GetComponentInParent<AudioSource>();
            if (audioSource == null)
            {
                // AudioSource가 없으면 부모 오브젝트에 생성
                GameObject parentObject = targetObject.GetComponentInParent<ThrowableRespawn>()?.gameObject;
                if (parentObject != null)
                {
                    audioSource = parentObject.AddComponent<AudioSource>();
                }
                else
                {
                    audioSource = targetObject.AddComponent<AudioSource>();
                }
            }
            
            // VR용 3D Spatial Audio 설정
            audioSource.spatialBlend = 1.0f; // 3D Sound (0 = 2D, 1 = 3D)
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
            
            audioSource.PlayOneShot(hitSound);
            Debug.Log($"타겟 맞춤 효과음 재생! (대상: {targetObject.name})");
        }
    }
}
