using System;

namespace Unity.Services.Wire.Internal
{
    interface IBackoffStrategy
    {
        /// <summary>
        /// Returns the next delay in seconds.
        /// </summary>
        /// <returns>Seconds</returns>
        float GetNext();

        void Reset();
    }


    class ExponentialBackoffStrategy : IBackoffStrategy
    {
        int m_Attempt;
        float m_Factor;
        const float k_Max = 30f;
        const float k_Min = 0.1f;

        public ExponentialBackoffStrategy()
        {
            m_Attempt = 0;
            m_Factor = 2;
        }

        float GetDuration(int attempt)
        {
            //calculate this duration
            var duration = k_Min * (float)Math.Pow(m_Factor, attempt);
            return duration < k_Min ? k_Min : (duration > k_Max ? k_Max : duration);
        }

        public float GetNext()
        {
            var res =  GetDuration(m_Attempt);
            m_Attempt++;
            return res;
        }

        public void Reset()
        {
            m_Attempt = 0;
        }
    }
}
