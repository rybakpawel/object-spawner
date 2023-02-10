using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor
{

    public class Spawner : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset _tree;

        private Toggle _activeToggle;
        private Toggle _alignToNormalToggle;
        private LayerMaskField _layerInput;
        private Vector3Field _minRotationInput;
        private Vector3Field _maxRotationInput;
        private FloatField _minScaleInput;
        private FloatField _maxScaleInput;

        private GameObject _prefab;

        [MenuItem("Tools/Spawner")]
        public static void ShowEditor()
        {
            var window = GetWindow<Spawner>();
            window.titleContent = new GUIContent("Spawner");
        }

        private void CreateGUI()
        {
            _tree.CloneTree(rootVisualElement);
            InitFields();
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGui;
        }

        private void OnSceneGui(SceneView sceneView)
        {
            if (!_activeToggle.value) return;
            var evt = Event.current;
            if (evt.type == EventType.MouseDown && evt.button == 0)
            {
                var ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);
                Physics.Raycast(ray, out var raycastHit, Mathf.Infinity, _layerInput.value);
                if (raycastHit.collider)
                {
                    var obj = CreatePrefab(raycastHit.point);
                    ApplyRandomRotation(obj, raycastHit.normal);
                    ApplyRandomScale(obj);
                    Undo.RegisterCreatedObjectUndo(obj, "Prefab Spawned");
                }
            }
        }

        private GameObject CreatePrefab(Vector3 pos)
        {
            var obj = PrefabUtility.InstantiatePrefab(_prefab) as GameObject;
            obj.transform.position = pos;
            return obj;
        }

        private void ApplyRandomRotation(GameObject obj, Vector3 normal)
        {
            var minRotation = _minRotationInput.value;
            var maxRotation = _maxRotationInput.value;
            var alignToNormal = _alignToNormalToggle.value;
            if (alignToNormal)
            {
                obj.transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);
            }

            var rotationEuler = obj.transform.rotation.eulerAngles;
            obj.transform.rotation = Quaternion.Euler(
                rotationEuler.x + Random.Range(minRotation.x, maxRotation.x),
                rotationEuler.y + Random.Range(minRotation.y, maxRotation.y),
                rotationEuler.z + Random.Range(minRotation.z, maxRotation.z)
                );
        }

        private void ApplyRandomScale(GameObject obj)
        {
            var minScale = _minScaleInput.value;
            var maxScale = _maxScaleInput.value;
            obj.transform.localScale = Vector3.one * Random.Range(minScale, maxScale);
        }

        private void InitFields()
        {
            _activeToggle = rootVisualElement.Q<Toggle>("Active");
            _alignToNormalToggle = rootVisualElement.Q<Toggle>("AlignToNormal");
            _layerInput = rootVisualElement.Q<LayerMaskField>("Layer");
            _minRotationInput = rootVisualElement.Q<Vector3Field>("MinRotation");
            _maxRotationInput = rootVisualElement.Q<Vector3Field>("MaxRotation");
            _minScaleInput = rootVisualElement.Q<FloatField>("MinScale");
            _maxScaleInput = rootVisualElement.Q<FloatField>("MaxScale");

            var prefabInput = rootVisualElement.Q<ObjectField>("Prefab");
            prefabInput.RegisterValueChangedCallback(evt =>
            {
                _prefab = evt.newValue as GameObject;
            });
        }
    }
}