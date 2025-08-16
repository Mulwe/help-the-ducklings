using UnityEngine;

public enum DuckType { Flying, Walking } //floating , wandering
[CreateAssetMenu(fileName = "DuckConfig", menuName = "Configs/Prefab/DuckConfig")]
public class DuckConfig : ScriptableObject
{
    [Header("Duck settings:")]
    [SerializeField] private DuckType _duckType;
    [SerializeField] private GameObject _prefab;
    [SerializeField] private Sprite _icon;


    public DuckType DuckType => this._duckType;
    public GameObject Prefab => this._prefab;
    public Sprite Icon => this._icon;


}
