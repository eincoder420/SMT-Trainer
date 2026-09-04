using System;
using UnityEngine;

namespace SamanthaTrainer.Payload
{
    // Entry points invoked by the injector through mono_runtime_invoke.
    // Both must be public, static and parameterless: the injector looks them up with
    // mono_class_get_method_from_name(klass, name, 0) and invokes with a null argument
    // array, so no MonoString marshalling is needed on the injector side.
    public static class Loader
    {
        private static GameObject _host;

        public static void Init()
        {
            try
            {
                if (_host != null) return; // already loaded

                _host = new GameObject("SMT-Trainer");
                UnityEngine.Object.DontDestroyOnLoad(_host);
                _host.hideFlags = HideFlags.HideAndDontSave;
                _host.AddComponent<TrainerBehaviour>();

                Debug.Log("[SMT-Trainer] payload loaded");
            }
            catch (Exception ex)
            {
                Debug.LogError("[SMT-Trainer] load failed: " + ex);
            }
        }

        public static void Unload()
        {
            try
            {
                if (_host != null)
                {
                    UnityEngine.Object.Destroy(_host);
                    _host = null;
                }
                Debug.Log("[SMT-Trainer] payload unloaded");
            }
            catch (Exception ex)
            {
                Debug.LogError("[SMT-Trainer] unload failed: " + ex);
            }
        }
    }
}
