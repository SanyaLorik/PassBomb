using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Architecture_M;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;


[Serializable]
public class InstancePets {
    public GameObject PetInstance;
    public PetItemConfig PetInfo;
}



public class PetsManager : MonoBehaviour {
    [SerializeField] private List<Transform> _playersPetsPoints;
    [SerializeField] private int _maxPetCount;
    
    private GameSave Saver => _gameSave.GetSave<GameSave>();
    
    private Dictionary<PetItemConfig, int> _petToCountDict = new();
    public List<InstancePets> PetsInstances { get; private set; } = new();
    private List<InstancePets> PetsInstancesForBots { get; set; } = new();


    public event Action GetPet;
    
    [Inject] private IGameSave _gameSave;
    [Inject] private List<PetItemConfig> _petsItems;
    [Inject] private GameData _gameData;
    
    private void Start() {
        LoadDataToDict();
        UpdatePetsVisual();
    }

    public void BotSetRandomPets(List<Transform> points) {
        int maxCount = Random.Range(_gameData.PetCount.From, _gameData.PetCount.To);
        maxCount = Math.Min(maxCount, _maxPetCount);
        
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

    
    public void AddPet(PetItemConfig petItem, int newCount = 1, bool updateNow = true) {
        int count = Saver.AddNewPet(petItem.Id, newCount);
        if (updateNow) {
            _gameSave.Save();
        }
        _petToCountDict[petItem] = count;
       
        GetPet?.Invoke();
        if (CheckPetsNeedUpdate(petItem)) {
            UpdatePetsVisual();
        }
    }
    
    
    private IEnumerator SpawnBotsBuisnessBirds(List<Transform> points, List<PetItemConfig> randomSelection) {
        for (int i = 0; i < randomSelection.Count; i++) {
            PetItemConfig pet = randomSelection[i];
            GameObject instance = Instantiate(pet.Prefab, points[i].position, Quaternion.identity, points[i]);
            PetsInstancesForBots.Add(new InstancePets {
                PetInstance = instance,
                PetInfo = pet
            });
            yield return null;
        }
        PetsInstancesForBots.Clear();
    }

    
    private void LoadDataToDict() {
        List<PetsData> boughtPets = Saver.Pets;
        Debug.Log("Кол-во купленных петов:" + boughtPets.Count);
        foreach (var pet in boughtPets) {
            var petItem = GetPetItemById(pet.Id);
            if (petItem != null) {
                _petToCountDict[petItem] = pet.Count;
            }
            else {
                Debug.LogWarning($"Питомец с ID {pet.Id} не найден");
            }
        }
    }

    
    private bool CheckPetsNeedUpdate(PetItemConfig petItem) {
        if (PetsInstances.Count < _maxPetCount) {
            return true;
        }

        return PetsInstances.Any(pet => pet.PetInfo.Modifier < petItem.Modifier);
    }


    private PetItemConfig GetPetItemById(string id) 
        => _petsItems.FirstOrDefault(pet => pet.Id == id);
    


    private void UpdatePetsVisual() {
        var topPets = GetBestPets(_petToCountDict);

        foreach (var pet in PetsInstances) {
            Destroy(pet.PetInstance);   
        }
        PetsInstances.Clear();

        for (var i = 0; i < topPets.Count; i++) {
            PetItemConfig pet = topPets[i];
            GameObject instance = Instantiate(pet.Prefab, _playersPetsPoints[i].position,  Quaternion.identity, _playersPetsPoints[i]);
            PetsInstances.Add(new InstancePets {
                PetInstance = instance,
                PetInfo = pet
            });
        }
        GetPet?.Invoke();
    }

    private List<PetItemConfig> GetBestPets(Dictionary<PetItemConfig, int> petToCountDict) {
        IEnumerable<PetItemConfig> expanded = _petToCountDict
            .SelectMany(pair => 
                Enumerable.Repeat(pair.Key, pair.Value)
            );

        List<PetItemConfig> topPets = expanded
            .OrderByDescending(p => p.Modifier)
            .Take(_maxPetCount)
            .ToList();

        return topPets;
    }
}
