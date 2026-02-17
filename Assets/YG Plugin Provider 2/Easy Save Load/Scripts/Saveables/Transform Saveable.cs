using UnityEngine;

namespace EasySaveLoad
{
    public class TransformSaveable : AutoSaveable
    {
        [Header("Transform Save Settings")]
        [SaveField] [SerializeField] private float x;
        [SaveField] [SerializeField] private float y;
        [SaveField] [SerializeField] private float z;

        protected override void BeforeSave() {
            x = transform.position.x;
            y = transform.position.y;
            z = transform.position.z;
            
            print($"Before save");
        }

        public override void LoadSaveData(object data) {
            base.LoadSaveData(data);

            transform.position = new Vector3(x, y, z);
        }
    }
}