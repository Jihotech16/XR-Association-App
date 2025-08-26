using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TargetMiniGame : MonoBehaviour
{
    [Header("게임 설정")]
    [SerializeField] private float gameTime = 30f; // 게임 시간 (초)
    
    [Header("UI 요소")]
    [SerializeField] private TextMeshProUGUI timerText; // 타이머 텍스트 (TMP)
    [SerializeField] private TextMeshProUGUI scoreText; // 점수 텍스트 (TMP)
    [SerializeField] private TextMeshProUGUI gameOverText; // 게임 오버 텍스트 (TMP)
    
    [Header("게임 오브젝트")]
    [SerializeField] private GameObject targetPrefab; // 원판 프리팹
    [SerializeField] private Transform spawnArea; // 스폰 영역
    [SerializeField] private ObjectSpawner objectSpawner; // 물체 스폰 관리자
    
    [Header("스폰 설정")]
    [SerializeField] private float spawnInterval = 2f; // 원판 스폰 간격
    [SerializeField] private float targetLifetime = 5f; // 원판 지속 시간
    [SerializeField] private Vector3 spawnAreaSize = new Vector3(2f, 0f, 2f); // 스폰 영역 크기
    
    private float currentTime;
    private int currentScore;
    private bool isGameActive = false;
    private bool isGameOver = false;
    private Coroutine spawnCoroutine;
    private Coroutine timerCoroutine;
    
    void Start()
    {
        // 초기 UI 설정
        if (gameOverText != null)
            gameOverText.gameObject.SetActive(false);
        
        UpdateUI();
    }
    
    public void StartGame()
    {
        if (isGameActive) return;
        
        Debug.Log("타겟 미니게임 시작!");
        isGameActive = true;
        isGameOver = false;
        currentTime = gameTime;
        currentScore = 0;
        
        // UI 초기화
        if (gameOverText != null)
            gameOverText.gameObject.SetActive(false);
        
        UpdateUI();
        
        // 물체 스폰
        if (objectSpawner != null)
        {
            objectSpawner.OnGameRestart();
            Debug.Log("Object Spawner를 통해 물체 스폰됨");
        }
        else
        {
            Debug.LogError("Object Spawner가 설정되지 않았습니다! Inspector에서 연결해주세요.");
        }
        
        // 타이머 시작
        timerCoroutine = StartCoroutine(GameTimer());
        
        // 원판 스폰 시작
        spawnCoroutine = StartCoroutine(SpawnTargets());
    }
    
    public void EndGame()
    {
        if (!isGameActive) return;
        
        Debug.Log($"게임 종료! 최종 점수: {currentScore}");
        isGameActive = false;
        isGameOver = true;
        
        // 코루틴 정지
        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);
            
        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);
        
        // 기존 원판들 제거
        GameObject[] existingTargets = GameObject.FindGameObjectsWithTag("Target");
        foreach (GameObject target in existingTargets)
        {
            Destroy(target);
        }
        
        // 물체들 제거
        if (objectSpawner != null)
            objectSpawner.OnGameEnd();
        
        // 게임 오버 UI 표시
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
            gameOverText.text = $"게임 종료!\n최종 점수: {currentScore}\n\n버튼을 눌러 재시작";
        }
    }
    
    public void RestartGame()
    {
        if (isGameOver)
        {
            EndGame(); // 기존 게임 정리
            StartGame(); // 새 게임 시작
        }
    }
    
    private IEnumerator GameTimer()
    {
        while (currentTime > 0 && isGameActive)
        {
            yield return new WaitForSeconds(1f);
            currentTime--;
            UpdateUI();
        }
        
        if (isGameActive)
        {
            EndGame();
        }
    }
    
    private IEnumerator SpawnTargets()
    {
        while (isGameActive)
        {
            SpawnTarget();
            yield return new WaitForSeconds(spawnInterval);
        }
    }
    
    private void SpawnTarget()
    {
        if (targetPrefab == null || spawnArea == null) return;
        
        // 랜덤 위치 계산
        Vector3 randomPosition = spawnArea.position + new Vector3(
            Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
            Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2),
            Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2)
        );
        
        // 원판 생성 (세워서 생성)
        GameObject target = Instantiate(targetPrefab, randomPosition, Quaternion.Euler(90, 0, 0));
        target.tag = "Target";
        
        // 원판에 충돌 감지 컴포넌트 추가
        TargetCollision targetCollision = target.GetComponent<TargetCollision>();
        if (targetCollision == null)
        {
            targetCollision = target.AddComponent<TargetCollision>();
        }
        targetCollision.Initialize(this);
        
        // 일정 시간 후 자동 제거
        Destroy(target, targetLifetime);
    }
    
    public void AddScore(int points)
    {
        if (!isGameActive) return;
        
        currentScore += points;
        UpdateUI();
        
        Debug.Log($"점수 획득! +{points} (총점: {currentScore})");
    }
    
    private void UpdateUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60);
            int seconds = Mathf.FloorToInt(currentTime % 60);
            string timerString = string.Format("{0:00}:{1:00}", minutes, seconds);
            timerText.text = timerString;
            Debug.Log($"타이머 업데이트: {timerString}");
        }
        else
        {
            Debug.LogWarning("timerText가 null입니다!");
        }
        
        if (scoreText != null)
        {
            string scoreString = $"Score: {currentScore}";
            scoreText.text = scoreString;
            Debug.Log($"점수 업데이트: {scoreString}");
        }
        else
        {
            Debug.LogWarning("scoreText가 null입니다!");
        }
    }
}
