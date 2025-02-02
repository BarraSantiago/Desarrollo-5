using System.Linq;
using UnityEngine;

namespace Utils
{
    public class Initializer : MonoBehaviour
    {
        private IInitializable[] _initializables;
        
        public void InitializeAll()
        {
            _initializables = FindObjectsOfType<MonoBehaviour>().OfType<IInitializable>().ToArray();
            foreach (IInitializable initializable in _initializables)
            {
                initializable.Initialize();
            }
        }
        
        public void DeinitializeAll()
        {
            foreach (IInitializable initializable in _initializables)
            {
                initializable.Deinitialize();
            }
        }
    }
}