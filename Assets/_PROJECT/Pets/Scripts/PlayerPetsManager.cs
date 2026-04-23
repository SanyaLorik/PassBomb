using System;
using System.Collections.Generic;
using System.Linq;
using Architecture_M;
using UnityEngine;
using Zenject;


public class PlayerPetsManager : MonoBehaviour {
    [SerializeField] private List<Transform> _playersPetsPoints;
    
    private GameSave Saver => _gameSave.GetSave<GameSave>();
    
    private Dictionary<PetItemConfig, int> _petToCountDict = new();
    private List<InstancePets> PetsInstances { get; set; } = new();


    
    [Inject] private IGameSave _gameSave;
    [Inject] private List<PetItemConfig> _petsItems;
    [Inject] private GameData _gameData;
    
    
    private void Start() {
        LoadDataToDict();
        UpdatePetsVisual();
    }
    

    
    public void AddPet(PetItemConfig petItem, int newCount = 1, bool updateNow = true) {
        int count = Saver.AddNewPet(petItem.Id, newCount);
        if (updateNow) {
            _gameSave.Save();
        }
        _petToCountDict[petItem] = count;
       
        if (CheckPetsNeedUpdate(petItem)) {
            UpdatePetsVisual();
        }
    }

    
    private void LoadDataToDict() {
        List<PetsData> boughtPets = Saver.Pets;
        Debug.Log("Кол-во петов:" + boughtPets.Count);
        foreach (var pet in boughtPets) {
            PetItemConfig petItem = GetPetItemById(pet.Id);
            if (petItem != null) {
                _petToCountDict[petItem] = pet.Count;
            }
            else {
                Debug.LogWarning($"Питомец с ID {pet.Id} не найден");
            }
        }
    }

    
    private bool CheckPetsNeedUpdate(PetItemConfig petItem) {
        if (PetsInstances.Count < _gameData.MaxPetsCount) {
            return true;
        }

        return PetsInstances.Any(pet => pet.PetInfo.Modifier < petItem.Modifier);
    }


    private PetItemConfig GetPetItemById(string id) 
        => _petsItems.FirstOrDefault(pet => pet.Id == id);
    


    private void UpdatePetsVisual() {
        List<PetItemConfig> topPets = GetBestPets(_petToCountDict);

        foreach (var pet in PetsInstances) {
            Destroy(pet.PetInstance);   
        }
        PetsInstances.Clear();

        for (var i = 0; i < topPets.Count; i++) {
            PetItemConfig pet = topPets[i];
            // GameObject instance = Instantiate(pet.Prefab, _playersPetsPoints[i].position,  Quaternion.identity, _playersPetsPoints[i]);
            GameObject instance = Instantiate(pet.Prefab, _playersPetsPoints[i]);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            
            PetsInstances.Add(new InstancePets {
                PetInstance = instance,
                PetInfo = pet
            });
        }
    }

    private List<PetItemConfig> GetBestPets(Dictionary<PetItemConfig, int> petToCountDict) {
        IEnumerable<PetItemConfig> expanded = _petToCountDict
            .SelectMany(pair => 
                Enumerable.Repeat(pair.Key, pair.Value)
            );

        List<PetItemConfig> topPets = expanded
            .OrderByDescending(p => p.Modifier)
            .Take(_gameData.MaxPetsCount)
            .ToList();

        return topPets;
    }
}
