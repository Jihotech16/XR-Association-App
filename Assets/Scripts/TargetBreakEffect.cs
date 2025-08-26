using UnityEngine;
using System.Collections;

public class TargetBreakEffect : MonoBehaviour
{
    [Header("부서지는 효과 설정")]
    [SerializeField] private int pieceCount = 5; // 조각 개수
    [SerializeField] private float explosionForce = 5f; // 폭발 힘
    [SerializeField] private float explosionRadius = 2f; // 폭발 반경
    [SerializeField] private float pieceLifetime = 3f; // 조각 지속 시간
    
    [Header("이펙트")]
    [SerializeField] private ParticleSystem hitParticles; // 충돌 파티클 (Unity Particle System)
    [SerializeField] private AudioSource hitSound; // 충돌 사운드
    [SerializeField] private AudioClip hitAudioClip; // 충돌 사운드 파일 (직접 할당)
    
    void Awake()
    {
        // AudioSource가 없으면 자동으로 찾기
        if (hitSound == null)
        {
            hitSound = GetComponent<AudioSource>();
            
            // 여전히 없으면 자식에서 찾기
            if (hitSound == null)
            {
                hitSound = GetComponentInChildren<AudioSource>();
            }
            
            // 그래도 없으면 새로 생성
            if (hitSound == null)
            {
                hitSound = gameObject.AddComponent<AudioSource>();
                Debug.Log("AudioSource를 새로 생성했습니다.");
            }
        }
        
        // AudioClip이 직접 할당되어 있으면 설정
        if (hitAudioClip != null && hitSound != null)
        {
            hitSound.clip = hitAudioClip;
            Debug.Log($"AudioClip 설정됨: {hitAudioClip.name}");
        }
        
        // Spatial Audio 찾기 및 설정
        SetupSpatialAudio();
        
        // 파티클 시스템 초기화 (생성 시 중지)
        if (hitParticles != null)
        {
            hitParticles.Stop();
            hitParticles.Clear();
            Debug.Log("파티클 시스템 초기화 완료");
        }
    }
    
    // Spatial Audio 설정
    private void SetupSpatialAudio()
    {
        // 모든 AudioSource 찾기 (Spatial Audio 포함)
        AudioSource[] allAudioSources = GetComponentsInChildren<AudioSource>();
        
        foreach (AudioSource audioSource in allAudioSources)
        {
            // Spatial Audio인지 확인 (Play On Awake가 체크되어 있을 가능성)
            if (audioSource.playOnAwake)
            {
                // Play On Awake 해제
                audioSource.playOnAwake = false;
                
                // 이미 재생 중이면 중지
                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                }
                
                // hitSound로 설정
                if (hitSound == null)
                {
                    hitSound = audioSource;
                    Debug.Log($"Spatial Audio를 hitSound로 설정: {audioSource.name}");
                }
            }
        }
    }
    
    public void BreakTarget()
    {
        Debug.Log($"BreakTarget 호출됨: {gameObject.name}");
        
        // 파티클 효과 재생
        if (hitParticles != null)
        {
            hitParticles.Play();
            Debug.Log("파티클 재생됨");
        }
        else
        {
            Debug.LogWarning("Hit Particles가 설정되지 않았습니다!");
        }
        
        // 사운드 재생
        if (hitSound != null)
        {
            // AudioSource 상세 정보 출력
            Debug.Log($"AudioSource 정보: {hitSound.name}");
            Debug.Log($"AudioClip: {hitSound.clip?.name ?? "None"}");
            Debug.Log($"Volume: {hitSound.volume}");
            Debug.Log($"Mute: {hitSound.mute}");
            Debug.Log($"Enabled: {hitSound.enabled}");
            Debug.Log($"AudioSource 활성화 상태: {hitSound.gameObject.activeInHierarchy}");
            
            // AudioSource가 비활성화되어 있으면 활성화
            if (!hitSound.gameObject.activeInHierarchy)
            {
                hitSound.gameObject.SetActive(true);
                Debug.Log("AudioSource를 활성화했습니다.");
            }
            
            // 이미 재생 중이면 중지 후 다시 재생
            if (hitSound.isPlaying)
            {
                hitSound.Stop();
            }
            
            // 사운드 재생 시도
            try
            {
                hitSound.Play();
                Debug.Log($"타겟 충돌 사운드 재생 성공! AudioClip: {hitSound.clip?.name ?? "None"}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"사운드 재생 실패: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning("Hit Sound가 설정되지 않았습니다!");
            
            // 마지막 시도: 모든 AudioSource에서 재생
            AudioSource[] allAudioSources = GetComponentsInChildren<AudioSource>();
            foreach (AudioSource audioSource in allAudioSources)
            {
                if (audioSource.clip != null && audioSource.enabled)
                {
                    audioSource.Play();
                    Debug.Log($"대체 AudioSource로 사운드 재생: {audioSource.name}");
                    break;
                }
            }
        }
        
        // 부서진 조각들 생성
        CreateBreakPieces();
        
        // 원본 타겟 비활성화
        gameObject.SetActive(false);
        
        // 일정 시간 후 완전히 제거
        Destroy(gameObject, pieceLifetime + 1f);
    }
    
    private void CreateBreakPieces()
    {
        Vector3 center = transform.position;
        
        for (int i = 0; i < pieceCount; i++)
        {
            // 랜덤한 조각 생성
            GameObject piece = CreateRandomPiece();
            
            if (piece != null)
            {
                // 랜덤 위치에 배치
                Vector3 randomPosition = center + Random.insideUnitSphere * 0.5f;
                piece.transform.position = randomPosition;
                
                // Rigidbody 추가
                Rigidbody rb = piece.GetComponent<Rigidbody>();
                if (rb == null)
                {
                    rb = piece.AddComponent<Rigidbody>();
                }
                
                // 폭발 힘 적용
                rb.AddExplosionForce(explosionForce, center, explosionRadius);
                
                // 일정 시간 후 제거
                Destroy(piece, pieceLifetime);
            }
        }
    }
    
    private GameObject CreateRandomPiece()
    {
        // 간단한 큐브 조각 생성
        GameObject piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
        piece.name = "TargetPiece";
        
        // 크기 랜덤 설정
        float scale = Random.Range(0.1f, 0.3f);
        piece.transform.localScale = new Vector3(scale, scale, scale);
        
        // 색상 설정 (원본 타겟과 비슷하게)
        Renderer renderer = piece.GetComponent<Renderer>();
        if (renderer != null)
        {
            // 원본 타겟의 색상을 가져와서 조각에 적용
            Renderer targetRenderer = GetComponent<Renderer>();
            if (targetRenderer != null && targetRenderer.material != null)
            {
                renderer.material = targetRenderer.material;
            }
            else
            {
                // 기본 빨간색
                renderer.material.color = Color.red;
            }
        }
        
        return piece;
    }
}
