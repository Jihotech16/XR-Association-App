// 2025-08-26 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class ObjectSpawner : MonoBehaviour
{
    [Header("스폰 설정")]
    [SerializeField] private GameObject[] throwablePrefabs; // 던질 수 있는 물체 프리팹들
    [SerializeField] private Transform[] spawnPoints; // 스폰 위치들
    [SerializeField] private int objectsPerSpawn = 3; // 스폰할 물체 개수
    
    [Header("자동 스폰")]
    [SerializeField] private bool spawnOnStart = true; // 시작할 때 자동 스폰
    [SerializeField] private float spawnDelay = 1f; // 스폰 지연 시간
    
    private List<GameObject> spawnedObjects = new List<GameObject>();
    
    void Start()
    {
        if (spawnOnStart)
        {
            Invoke(nameof(SpawnObjects), spawnDelay);
        }
    }
    
    public void SpawnObjects()
    {
        // 기존 물체들 제거
        ClearObjects();
        
        // 새로운 물체들 스폰
        for (int i = 0; i < objectsPerSpawn && i < spawnPoints.Length; i++)
        {
            if (throwablePrefabs.Length > 0 && spawnPoints[i] != null)
            {
                // 랜덤하게 물체 선택
                GameObject prefab = throwablePrefabs[UnityEngine.Random.Range(0, throwablePrefabs.Length)];
                
                // 물체 생성
                GameObject spawnedObject = Instantiate(prefab, spawnPoints[i].position, spawnPoints[i].rotation);
                
                // 강제로 모든 자식 오브젝트 활성화
                ForceActivateAllChildren(spawnedObject);
                
                // Meta XR 컴포넌트들 활성화
                EnableMetaXRComponents(spawnedObject);
                
                // ThrowableRespawn 스크립트 추가
                ThrowableRespawn respawn = spawnedObject.GetComponent<ThrowableRespawn>();
                if (respawn == null)
                {
                    respawn = spawnedObject.AddComponent<ThrowableRespawn>();
                }
                
                // 스폰 포인트 설정 (public 메서드 사용)
                respawn.SetSpawnPoint(spawnPoints[i]);
                
                // 리스트에 추가
                spawnedObjects.Add(spawnedObject);
                
                Debug.Log($"물체 스폰: {spawnedObject.name} at {spawnPoints[i].name} (자식 오브젝트 수: {spawnedObject.transform.childCount})");
            }
        }
    }
    
    public void ClearObjects()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        spawnedObjects.Clear();
    }
    
    // 게임 종료 시 호출
    public void OnGameEnd()
    {
        ClearObjects();
    }
    
    // 게임 재시작 시 호출
    public void OnGameRestart()
    {
        SpawnObjects();
    }

    // 모든 자식 오브젝트 활성화/비활성화
    private void SetAllChildrenActive(GameObject obj, bool active)
    {
        obj.SetActive(active);
        foreach (Transform child in obj.transform)
        {
            SetAllChildrenActive(child.gameObject, active);
        }
    }

    // 강제로 모든 자식 오브젝트 활성화 (더 강력한 방법)
    private void ForceActivateAllChildren(GameObject obj)
    {
        // 메인 오브젝트 활성화
        obj.SetActive(true);
        
        // 모든 자식 오브젝트를 강제로 활성화
        Transform[] allChildren = obj.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren)
        {
            child.gameObject.SetActive(true);
        }
        
        Debug.Log($"강제 활성화 완료: {obj.name} (총 오브젝트 수: {allChildren.Length})");
    }

    // Meta XR 컴포넌트들 활성화
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
        
        Debug.Log($"Meta XR 컴포넌트들 활성화 완료: {obj.name}");
    }
}
