using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

public class BotPetsIniter : MonoBehaviour {
    [SerializeField] private List<Transform> _petsPoints;

    private List<InstancePets> PetsInstancesForBots { get; set; } = new();
    
    [Inject] private List<PetItemConfig> _petsItems;
    [Inject] private GameData _gameData;
    
    [Inject] private PlayerPetsManager _playerPetsManager;

    private void Start() {
        BotSetRandomPets(_petsPoints);
    }
    
    private void BotSetRandomPets(List<Transform> points) {
        int maxCount = Random.Range(_gameData.BotPetCountDiapasone.From, _gameData.BotPetCountDiapasone.To);
        maxCount = Math.Min(maxCount, _gameData.MaxPetsCount);
        
        // 1. Берём всех питомцев, которых бот может иметь
        List<PetItemConfig> availablePets = _petsItems; // либо другой источник
    
        // 2. Создаём случайную выборку
        List<PetItemConfig> randomSelection = new List<PetItemConfig>();
    
        for (int i = 0; i < maxCount; i++) {
            // Выбираем случайного питомца из доступных
            PetItemConfig pet = availablePets[Random.Range(0, availablePets.Count)];
            randomSelection.Add(pet);
        }

        // 3. Сортируем по модификатору, от сильного к слабому
        randomSelection = randomSelection
            .OrderByDescending(p => p.Modifier)
            .ToList();

        // 4. Спавним на точках
        StartCoroutine(SpawnBotsBuisnessBirds(points, randomSelection));
    }
    
    
    private IEnumerator SpawnBotsBuisnessBirds(List<Transform> points, List<PetItemConfig> randomSelection) {
        for (int i = 0; i < randomSelection.Count; i++) {
            PetItemConfig pet = randomSelection[i];
            // GameObject instance = Instantiate(pet.Prefab, points[i].position, Quaternion.identity, points[i]);
            GameObject instance = Instantiate(pet.Prefab, points[i]);
            instance.transform.localRotation = Quaternion.identity;
            PetsInstancesForBots.Add(new InstancePets {
                PetInstance = instance,
                PetInfo = pet
            });
            yield return null;
        }
        PetsInstancesForBots.Clear();
    }
    
}