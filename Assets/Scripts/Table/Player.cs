using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Events;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    [SerializeField] private Level[] _levels = null;
    private ResourcesData _resources = new ResourcesData();

    public UnityAction<int> onChangeMoney = null;
    public UnityAction<int> onChangeExperience = null;
    public UnityAction<int> onChangeLevel = null;
    public UnityAction<int> onChangeTableNumber = null;

    public int Level
    {
        get
        {
            string key = "Level";
            if (!PlayerPrefs.HasKey(key))
                PlayerPrefs.SetInt(key, 1);
            return PlayerPrefs.GetInt(key);
        }
        set
        {
            if (Level != value)
            {
                string key = "Level";
                PlayerPrefs.SetInt(key, value);
                onChangeLevel?.Invoke(value);
            }
        }
    }

    public int TableNumber
    {
        get
        {
            string key = "TableNumber";
            if (!PlayerPrefs.HasKey(key))
                PlayerPrefs.SetInt(key, 0);
            return PlayerPrefs.GetInt(key);
        }
        set
        {
            if (TableNumber != value)
            {
                string key = "TableNumber";
                PlayerPrefs.SetInt(key, value);
                onChangeTableNumber?.Invoke(value);
            }
        }
    }

    public int Money
    {
        get => _resources.GetResourceValue(ResourceId.MONEY);
        set
        {
            _resources.SetResourceValue(ResourceId.MONEY, value);
            onChangeMoney?.Invoke(value);
        }
    }
    public int Experience
    {
        get => _resources.GetResourceValue(ResourceId.EXPERIENCE);
        set
        {
            if (value >= CurrentLevel.experience)
            {
                _resources.SetResourceValue(ResourceId.EXPERIENCE, value - CurrentLevel.experience);
                Level++;
            }
            else
            {
                _resources.SetResourceValue(ResourceId.EXPERIENCE, value);
                onChangeExperience?.Invoke(value);
            }
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }

    public Level GetLevel(int id) => _levels[Mathf.Clamp(id - 1, 0, _levels.Length - 1)];

    public Level CurrentLevel { get => GetLevel(Level); }

    //public bool TrySpend(ResourceId id, int value) => _resources.TrySpend(id, value);
    //public void Add(ResourceId id, int value) => _resources.Add(id, value);
}

public class ResourcesData
{
    public IReadOnlyDictionary<ResourceId, int> Resources => _resources;
    private Dictionary<ResourceId, int> _resources = new Dictionary<ResourceId, int>();

    public void SetResourceValue(ResourceId id, int value)
    {
        if (value < 0)
            throw new ArgumentException($"Resource {id} value {value} cant be less then zero");
        _resources[id] = value;
        SaveResourceData(id, value);
    }

    public int GetResourceValue(ResourceId id)
    {
        if (!_resources.ContainsKey(id))
            _resources[id] = LoadResourceData(id);
        return _resources[id];
    }

    public bool TrySpend(ResourceId id, int value)
    {
        if (_resources.TryGetValue(id, out var amount))
            if (amount > value) _resources[id] = amount - value;
        return false;
    }

    public void Add(ResourceId id, int value)
    {
        if (!_resources.TryGetValue(id, out var amount))
            _resources[id] = value;
        else
            _resources[id] = amount + value;
    }

    private void SaveResourceData(ResourceId id, int value)
    {
        PlayerPrefs.SetInt(id.ToString(), value);
    }

    private int LoadResourceData(ResourceId id)
    {
        if (!PlayerPrefs.HasKey(id.ToString()))
            PlayerPrefs.SetInt(id.ToString(), 0);
        return PlayerPrefs.GetInt(id.ToString());
    }
}

public enum ResourceId
{
    MONEY,
    EXPERIENCE
}
