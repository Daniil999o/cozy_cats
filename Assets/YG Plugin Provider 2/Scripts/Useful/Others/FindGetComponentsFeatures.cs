namespace UnityEngine
{
    public static class FindGetComponentsFeatures
    {
        public static bool TryGetComponentInParent<T>(this Component obj, out T component) where T : Object =>
            obj.gameObject.TryGetComponentInParent(out component);
        
        public static bool TryGetComponentInParent<T>(this GameObject obj, out T component) where T : Object {
            component = obj.GetComponentInParent<T>();

            return component != null;
        }
        
        // ##################
        
        public static bool TryGetComponentsInParent<T>(this Component obj, out T[] components) where T : Object =>
            obj.gameObject.TryGetComponentsInParent(out components);
        
        public static bool TryGetComponentsInParent<T>(this GameObject obj, out T[] components) where T : Object {
            components = obj.GetComponentsInParent<T>();

            return components != null && components.Length > 0;
        }
        
        // ##################
        
        public static bool TryGetComponentInChildren<T>(this Component obj, out T component) where T : Object =>
            obj.gameObject.TryGetComponentInChildren(out component);
        
        public static bool TryGetComponentInChildren<T>(this GameObject obj, out T component) where T : Object {
            component = obj.GetComponentInChildren<T>();

            return component != null;
        }
        
        // ##################
        
        public static bool TryGetComponentsInChildren<T>(this Component obj, out T[] components) where T : Object =>
            obj.gameObject.TryGetComponentsInChildren(out components);
        
        public static bool TryGetComponentsInChildren<T>(this GameObject obj, out T[] components) where T : Object {
            components = obj.GetComponentsInChildren<T>();

            return components != null && components.Length > 0;
        }
        
        // ##################

        public static bool TryFindObjectByType<T>(out T component) where T : Object {
            component = GameObject.FindFirstObjectByType<T>(FindObjectsInactive.Include);

            return component != null;
        }

        public static bool TryFindObjectsByType<T>(out T[] components) where T : Object {
            components = GameObject.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            return components != null && components.Length > 0;
        }
        
        // ##################

        public static bool TryFindGameObject(this string gameObjectName, out GameObject obj) {
            obj = GameObject.Find(gameObjectName);

            return obj;
        }
        
        // ##################

        public static string GetFullPath(this GameObject obj) => $"{obj.GetFullParentPath()}/{obj.name}";
        
        public static string GetFullParentPath(this GameObject obj) {
            string path = "";
            Transform cur = obj.transform;

            while (cur.transform.parent) {
                path = $"/{cur.transform.parent.name}{path}";
                cur = cur.transform.parent;
            }
            
            if (path[0] == '/') path = path.Remove(0, 1);

            return path;
        }
    }
}