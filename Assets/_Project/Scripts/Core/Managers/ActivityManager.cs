using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine; // Component


//Checks activity behind camera, off if ready to dbe deactivated
public class ActivityManager
{
    private List<Component> _ducks;
    private List<Component> _enemies;

    private Camera _camera;
    private float _checkInterval = 0.2f;
    private float _cullingDistance = 0f;

    private float _viewportOffset = 0.3f;
    private float _distanceOffset = 20f;

    // Поля для 2D
    private Rect _cameraViewRect;
    private Rect _extendedViewRect;

    private MonoBehaviour _coroutineRunner;
    private Coroutine _visibilityCheckCoroutine;

    private Dictionary<GameObject, bool> _originalActiveStates = new Dictionary<GameObject, bool>();


    public ActivityManager(MonoBehaviour runner)
    {
        _coroutineRunner = runner;

    }

    public ActivityManager(MonoBehaviour runner,
                             float intervalInSec,
                             float cullingDistance,
                             float viewportOffset,
                             float _distanceOffset)
    {
        _coroutineRunner = runner;
        this._checkInterval = intervalInSec;
        this._cullingDistance = cullingDistance;
        this._viewportOffset = viewportOffset;
        this._distanceOffset = _distanceOffset;
    }

    public void SetCheckInterval(float intervalInSec) => this._checkInterval = intervalInSec;
    public void SetCullingDistance(float cullingDistance) => this._cullingDistance = cullingDistance;
    public void SetViewportOffset(float viewportOffset) => this._viewportOffset = viewportOffset;
    public void SetDistanceOffset(float distanceOffset) => this._distanceOffset = distanceOffset;


    public void StartMonitoring()
    {
        if (_camera != null && _checkInterval > 0)
        {
            StopMonitoring();
            _visibilityCheckCoroutine = _coroutineRunner.StartCoroutine(VisibilityCheckRoutine(_checkInterval));
        }
        else
            Debug.Log($"{nameof(StartMonitoring)}: camera or checkInterval incorrect");
    }

    public void StopMonitoring()
    {
        if (_visibilityCheckCoroutine != null)
        {
            _coroutineRunner.StopCoroutine(_visibilityCheckCoroutine);
            _visibilityCheckCoroutine = null;
        }
    }
    public void SetTargetCamera(Camera cam)
    {
        this._camera = cam;
    }

    //--------------------------------------------------------------------------------------------------------------------


    private void UpdateCameraRects()
    {
        if (_camera == null) return;

        float height = _camera.orthographicSize * 2f;
        float width = height * _camera.aspect;

        Vector3 cameraPos = _camera.transform.position;

        // Стандартные границы камеры
        _cameraViewRect = new Rect(
            cameraPos.x - width / 2f,
            cameraPos.y - height / 2f,
            width,
            height
        );

        // Расширенные границы с offset
        float extendedWidth = width * (1f + _viewportOffset);
        float extendedHeight = height * (1f + _viewportOffset);

        _extendedViewRect = new Rect(
            cameraPos.x - extendedWidth / 2f,
            cameraPos.y - extendedHeight / 2f,
            extendedWidth,
            extendedHeight
        );
    }

    private bool IsObject2DVisible(GameObject obj)
    {
        Vector3 objPos = obj.transform.position;

        if (_cullingDistance > 0)
        {
            float distance = Vector2.Distance(_camera.transform.position, objPos);
            if (distance > _cullingDistance + _distanceOffset)
                return false;
        }

        return _extendedViewRect.Contains(new Vector2(objPos.x, objPos.y));
    }

    private IEnumerator VisibilityCheckRoutine(float interval)
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);

            if (_camera != null)
            {
                UpdateCameraRects();
                CheckAndUpdateVisibility();
            }
        }
    }

    private void CheckAndUpdateVisibility()
    {
        CheckListVisibility(_ducks);
        CheckListVisibility(_enemies);
    }

    private void CheckListVisibility(List<Component> componentList)
    {
        if (componentList == null) return;

        foreach (Component comp in componentList)
        {
            if (comp == null) continue;

            GameObject obj = comp.gameObject;
            if (!_originalActiveStates.ContainsKey(obj))
                _originalActiveStates[obj] = obj.activeSelf;

            bool shouldBeActive = IsObject2DVisible(obj);
            if (obj.activeSelf != shouldBeActive)
            {
                if (IsObjectReadyToBeDeactivated(obj))
                {
                    // Debug.Log($"{obj.name} is {(!shouldBeActive ? "<color=red>behind</color>" : "<color=green>on</color>")} camera");
                    obj.SetActive(shouldBeActive);
                }
            }
        }
    }
    private bool IsObjectReadyToBeDeactivated(GameObject obj)
    {
        if (obj.TryGetComponent<DuckController>(out var duck))
        {
            if (!duck.IsFollowing && duck.IsAirborne) return true;
        }

        if (obj.TryGetComponent<EnemyController>(out var enemy))
        {
            if (enemy._isPatrolling) return true;
        }
        return false;
    }

    //--------------------------------------------------------------------------------------------------------------------
    private bool AddObject<T>(T component, ref List<Component> list) where T : Component
    {
        if (component == null) return false;
        list ??= new List<Component>();
        if (!ContainsComponent(component, list))
        {
            list.Add(component);
            return true;
        }
        return false;
    }

    private bool ContainsComponent<T>(T component, List<Component> list) where T : Component
    {
        return list.Any(x => x == component);
    }

    public bool AddDuck<T>(T duck) where T : Component
    {
        return AddObject(duck, ref _ducks);
    }

    public bool AddEnemy<T>(T enemyComponent) where T : Component
    {
        return AddObject(enemyComponent, ref _enemies);
    }

    public List<T> GetDucks<T>() where T : Component
    {
        return _ducks?.OfType<T>().ToList() ?? new List<T>();
    }
    public List<T> GetEnemies<T>() where T : Component
    {
        return _enemies?.OfType<T>().ToList() ?? new List<T>();
    }

    public List<Component> GetAllDucks() => _ducks ?? new List<Component>();
    public List<Component> GetAllEnemies() => _enemies ?? new List<Component>();

    public List<Component> AddEnemies<T>(T[] components) where T : Component
    {
        foreach (T component in components)
        {
            AddEnemy(component);
        }
        return _enemies;
    }
    public List<Component> AddDucks<T>(T[] components) where T : Component
    {
        foreach (T component in components)
        {
            AddDuck(component);
        }
        return _ducks;
    }

    public void DeleteComponentFromList<T>(T component, ref List<Component> list) where T : Component
    {
        if (list != null && list.Count > 0 && component != null)
        {
            list.RemoveAll(x => x == component);
        }
    }

    public void DeleteComponentsFromList<T>(List<T> components, ref List<Component> list) where T : Component
    {
        if (list == null || list.Count == 0 || components == null || components.Count == 0)
            return;

        list.RemoveAll(listComponent => components.Any(comp => comp == listComponent));
    }

    public void ChangeComponentActivity<T>(T component, ref List<Component> list, bool isActive) where T : Component
    {
        if (list != null && list.Count > 0 && component != null)
        {
            var target = list.FirstOrDefault(x => x == component);
            if (target != null)
                target.gameObject.SetActive(isActive);
        }
    }

    public void ChangeComponentsActivity<T>(List<T> components, ref List<Component> list, bool isActive) where T : Component
    {
        if (list == null || list.Count == 0 || components == null || components.Count == 0)
            return;

        foreach (var component in components)
        {
            var target = list.FirstOrDefault(x => x == component);
            if (target != null)
                target.gameObject.SetActive(isActive);
        }
    }

    public void ChangeDuckActivity<T>(T component, bool isActive) where T : Component
    {
        ChangeComponentActivity(component, ref _ducks, isActive);
    }

    public void ChangeDucksActivity<T>(List<T> components, bool isActive) where T : Component
    {
        ChangeComponentsActivity(components, ref _ducks, isActive);
    }
    public void ChangeEnemyActivity<T>(T component, bool isActive) where T : Component
    {
        ChangeComponentActivity(component, ref _enemies, isActive);
    }

    public void ChangeEnemiesActivity<T>(List<T> components, bool isActive) where T : Component
    {
        ChangeComponentsActivity(components, ref _enemies, isActive);
    }

    public void DeleteEnemy<T>(T component) where T : Component
    {
        DeleteComponentFromList(component, ref _enemies);
    }

    public void DeleteEnemies<T>(List<T> components) where T : Component
    {
        DeleteComponentsFromList(components, ref _enemies);
    }

    public void DeleteDuck<T>(T component) where T : Component
    {
        DeleteComponentFromList(component, ref _ducks);
    }

    public void DeleteDucks<T>(List<T> components) where T : Component
    {
        DeleteComponentsFromList(components, ref _ducks);
    }

    public void CleanDucks()
    {
        _ducks?.Clear();
    }
    public void CleanEnemies()
    {
        _enemies?.Clear();
    }

    public void CleanAllLists()
    {
        CleanDucks();
        CleanEnemies();
    }

    public void OnDispose()
    {
        CleanAllLists();
        _ducks = null;
        _enemies = null;
    }
}
/* Optional, not used

// pixel perfect
private Vector2 SnapToPixel(Vector2 position)
{
    float pixelsPerUnit = 100f;

    return new Vector2(
        Mathf.Round(position.x * pixelsPerUnit) / pixelsPerUnit,
        Mathf.Round(position.y * pixelsPerUnit) / pixelsPerUnit
    );
}

private Rect GetObject2DBounds(GameObject obj)
{
    if (obj.TryGetComponent<SpriteRenderer>(out var sr))
        return BoundsToRect(sr.bounds);

    if (obj.TryGetComponent<Collider2D>(out var col))
        return BoundsToRect(col.bounds);

    // default rect  
    Vector3 pos = obj.transform.position;
    return new Rect(pos.x - 0.5f, pos.y - 0.5f, 1f, 1f);
}

private Rect BoundsToRect(Bounds bounds)
{
    Vector2 center = bounds.center;
    Vector2 size = bounds.size;
    return new Rect(center.x - size.x / 2f, center.y - size.y / 2f, size.x, size.y);
}
*/
