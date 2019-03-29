using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class MobileKeyboardManager : IService, IMobileKeyboardManager
    {
        private ILoadObjectsManager _loadObjectsManager;

        private GameObject _prefabMobileKeyboard;

        private GameObject _currentMobileKeyboardObject;

        public void Dispose()
        {
            DisposeKeyboardMobile();
        }

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();

            _prefabMobileKeyboard = _loadObjectsManager.GetObjectByPath<GameObject>("Prefab/Plugins/MobileKeyboard");

            InitKeyboardMobile();
        }

        public void Update()
        {
        }

        public void InitKeyboardMobile()
        {
            _currentMobileKeyboardObject = Object.Instantiate(_prefabMobileKeyboard);
        }

        public void DisposeKeyboardMobile()
        {
            Object.Destroy(_currentMobileKeyboardObject);
            _currentMobileKeyboardObject = null;
        }
    }
}
