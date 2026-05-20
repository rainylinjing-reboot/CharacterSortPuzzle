using UnityEngine;
using System;

[System.Serializable]
public struct CharacterEntry
{
    public CharacterType type;
    public GameObject prefab; // 각 타입에 맞는 3D 캐릭터 프리팹
}

[CreateAssetMenu(fileName = "CharacterPrefabSet", menuName = "PuzzleGame/CharacterPrefabSet")]
public class CharacterPrefabSet : ScriptableObject
{
    public CharacterEntry[] characterPrefabs;

    // 특정 캐릭터 타입에 맞는 프리팹을 안전하게 반환하는 헬퍼 함수
    public GameObject GetPrefab(CharacterType type)
    {
        if (characterPrefabs == null) return null;

        foreach (var entry in characterPrefabs)
        {
            if (entry.type == type) return entry.prefab;
        }
        return null;
    }
}