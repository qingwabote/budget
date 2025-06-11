using UnityEngine;

namespace Budget
{
    public class Skin : ScriptableObject
    {
        public class Store
        {
            protected readonly TextureView<float> _mView;

            public Store()
            {

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