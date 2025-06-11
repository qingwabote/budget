namespace Budget
{
    using UnityEngine;

    public class Transient
    {
        private readonly int _mReset;

        private int _mVersion = Time.frameCount;

        private int _mValue;
        public int Value
        {
            get
            {
                if (_mVersion != Time.frameCount)
                {
                    return _mReset;
                }
                return _mValue;
            }

            set
            {
                _mValue = value;
                _mVersion = Time.frameCount;
            }
        }

        public Transient(int value, int reset)
        {
            _mValue = value;
            _mReset = reset;
        }
    }
}