using UnityEngine;

namespace SniperStrategyGame.Utilities
{
    public abstract class GenericMonoSingleton<T> : MonoBehaviour where T : GenericMonoSingleton<T>
    {
        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = (T)this;
            DontDestroyOnLoad(gameObject);
        }
    }
}