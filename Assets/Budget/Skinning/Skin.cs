using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Budget
{
    public class Skin : ScriptableObject
    {
        public unsafe class Store
        {
            protected readonly TextureView _mView;

            public float* Source => (float*)_mView.Source.GetUnsafePtr();

            public Store()
            {
                _mView = new TextureView(1);
            }
        }

        private Store _mPersistent;
        public Store Persistent
        {
            get
            {
                if (_mPersistent == null)
                {
                    _mPersistent = new Store();
                }
                return _mPersistent;
            }
        }

        [HideInInspector]
        public string[] Joints;
    }
}