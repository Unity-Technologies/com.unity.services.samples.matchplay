using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using System;

namespace Unity.Ucg.Usqp
{
    /// <summary>
    /// Data streams can be used to serialize data over the network. The
    /// <c>DataStreamWriter</c> and <c>DataStreamReader</c> classes work together
    /// to serialize data for sending and then to deserialize when receiving.
    /// </summary>
    /// <remarks>
    /// The reader can be used to deserialize the data from a NativeArray<byte>, writing data
    /// to a NativeArray<byte> and reading it back can be done like this:
    /// <code>
    /// using (var data = new NativeArray<byte>(16, Allocator.Persistent))
    /// {
    ///     var dataWriter = new DataStreamWriter(data);
    ///     dataWriter.WriteInt(42);
    ///     dataWriter.WriteInt(1234);
    ///     // Length is the actual amount of data inside the writer,
    ///     // Capacity is the total amount.
    ///     var dataReader = new DataStreamReader(nativeArrayOfBytes.GetSubArray(0, dataWriter.Length));
    ///     var myFirstInt = dataReader.ReadInt();
    ///     var mySecondInt = dataReader.ReadInt();
    /// }
    /// </code>
    ///
    /// There are a number of functions for various data types. If a copy of the writer
    /// is stored it can be used to overwrite the data later on, this is particularly useful when
    /// the size of the data is written at the start and you want to write it at
    /// the end when you know the value.
    ///
    /// <code>
    /// using (var data = new NativeArray<byte>(16, Allocator.Persistent))
    /// {
    ///     var dataWriter = new DataStreamWriter(data);
    ///     // My header data
    ///     var headerSizeMark = dataWriter;
    ///     dataWriter.WriteUShort((ushort)0);
    ///     var payloadSizeMark = dataWriter;
    ///     dataWriter.WriteUShort((ushort)0);
    ///     dataWriter.WriteInt(42);
    ///     dataWriter.WriteInt(1234);
    ///     var headerSize = data.Length;
    ///     // Update header size to correct value
    ///     headerSizeMark.WriteUShort((ushort)headerSize);
    ///     // My payload data
    ///     byte[] someBytes = Encoding.ASCII.GetBytes("some string");
    ///     dataWriter.Write(someBytes, someBytes.Length);
    ///     // Update payload size to correct value
    ///     payloadSizeMark.WriteUShort((ushort)(dataWriter.Length - headerSize));
    /// }
    /// </code>
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct DataStreamWriter
    {
        struct StreamData
        {
            public byte* buffer;
            public int length;
            public int capacity;
            public ulong bitBuffer;
            public int bitIndex;
        }

        [NativeDisableUnsafePtrRestriction] StreamData m_Data;
        internal IntPtr m_SendHandleData;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle m_Safety;
#endif
        public DataStreamWriter(NativeArray<byte> data)
        {
            Initialize(out this, data);
        }

        public NativeArray<byte> AsNativeArray()
        {
            var na = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(m_Data.buffer, Length, Allocator.Invalid);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref na, m_Safety);
#endif
            return na;
        }

        private static void Initialize(out DataStreamWriter self, NativeArray<byte> data)
        {
            self.m_SendHandleData = IntPtr.Zero;

            self.m_Data.capacity = data.Length;
            self.m_Data.length = 0;
            self.m_Data.buffer = (byte*)data.GetUnsafePtr();
            self.m_Data.bitBuffer = 0;
            self.m_Data.bitIndex = 0;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            self.m_Safety = NativeArrayUnsafeUtility.GetAtomicSafetyHandle(data);
#endif
            uint test = 1;
            unsafe
            {
                byte* test_b = (byte*)&test;
                self.m_IsLittleEndian = test_b[0] == 1 ? 1 : 0;
            }
        }

        private int m_IsLittleEndian;
        private bool IsLittleEndian => m_IsLittleEndian != 0;

        private static short ByteSwap(short val)
        {
            return (short)(((val & 0xff) << 8) | ((val >> 8) & 0xff));
        }

        private static int ByteSwap(int val)
        {
            return (int)(((val & 0xff) << 24) | ((val & 0xff00) << 8) | ((val >> 8) & 0xff00) | ((val >> 24) & 0xff));
        }

        /// <summary>
        /// The total size of the data buffer, see <see cref="Length"/> for
        /// the size of space used in the buffer.
        /// </summary>
        public int Capacity
        {
            get
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif
                return m_Data.capacity;
            }
        }

        /// <summary>
        /// The size of the buffer used. See <see cref="Capacity"/> for the total size.
        /// </summary>
        public int Length
        {
            get
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif
                SyncBitData();
                return m_Data.length + ((m_Data.bitIndex + 7) >> 3);
            }
        }

        private void SyncBitData()
        {
            var bitIndex = m_Data.bitIndex;
            if (bitIndex <= 0)
                return;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
            var bitBuffer = m_Data.bitBuffer;
            int offset = 0;
            while (bitIndex > 0)
            {
                m_Data.buffer[m_Data.length + offset] = (byte)bitBuffer;
                bitIndex -= 8;
                bitBuffer >>= 8;
                ++offset;
            }
        }

        public void Flush()
        {
            while (m_Data.bitIndex > 0)
            {
                m_Data.buffer[m_Data.length++] = (byte)m_Data.bitBuffer;
                m_Data.bitIndex -= 8;
                m_Data.bitBuffer >>= 8;
            }

            m_Data.bitIndex = 0;
        }

        public bool WriteBytes(byte* data, int bytes)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
            if (m_Data.length + ((m_Data.bitIndex + 7) >> 3) + bytes > m_Data.capacity)
            {
                return false;
            }
            Flush();
            UnsafeUtility.MemCpy(m_Data.buffer + m_Data.length, data, bytes);
            m_Data.length += bytes;
            return true;
        }

        public bool WriteByte(byte value)
        {
            return WriteBytes((byte*)&value, sizeof(byte));
        }

        public bool WriteShortNetworkByteOrder(short value)
        {
            short netValue = IsLittleEndian ? ByteSwap(value) : value;
            return WriteBytes((byte*)&netValue, sizeof(short));
        }

        public bool WriteUShortNetworkByteOrder(ushort value)
        {
            return WriteShortNetworkByteOrder((short)value);
        }

        public bool WriteIntNetworkByteOrder(int value)
        {
            int netValue = IsLittleEndian ? ByteSwap(value) : value;
            return WriteBytes((byte*)&netValue, sizeof(int));
        }

        public bool WriteUIntNetworkByteOrder(uint value)
        {
            return WriteIntNetworkByteOrder((int)value);
        }
    }

    /// <summary>
    /// The <c>DataStreamReader</c> class is the counterpart of the
    /// <c>DataStreamWriter</c> class and can be be used to deserialize
    /// data which was prepared with it.
    /// </summary>
    /// <remarks>
    /// Simple usage example:
    /// <code>
    /// using (var dataWriter = new DataStreamWriter(16, Allocator.Persistent))
    /// {
    ///     dataWriter.Write(42);
    ///     dataWriter.Write(1234);
    ///     // Length is the actual amount of data inside the writer,
    ///     // Capacity is the total amount.
    ///     var dataReader = new DataStreamReader(dataWriter, 0, dataWriter.Length);
    ///     var context = default(DataStreamReader.Context);
    ///     var myFirstInt = dataReader.ReadInt(ref context);
    ///     var mySecondInt = dataReader.ReadInt(ref context);
    /// }
    /// </code>
    ///
    /// The <c>DataStreamReader</c> carries the position of the read pointer inside the struct,
    /// taking a copy of the reader will also copy the read position. This includes passing the
    /// reader to a method by value instead of by ref.
    ///
    /// See the <see cref="DataStreamWriter"/> class for more information
    /// and examples.
    /// </remarks>
    internal unsafe struct DataStreamReader
    {
        struct Context
        {
            public int m_ReadByteIndex;
            public int m_BitIndex;
        }

        byte* m_bufferPtr;
        Context m_Context;
        int m_Length;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle m_Safety;
#endif

        public DataStreamReader(NativeArray<byte> array)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            m_Safety = NativeArrayUnsafeUtility.GetAtomicSafetyHandle(array);
#endif
            m_bufferPtr = (byte*)array.GetUnsafeReadOnlyPtr();
            m_Length = array.Length;
            m_Context = default;

            uint test = 1;
            unsafe
            {
                byte* test_b = (byte*)&test;
                m_IsLittleEndian = test_b[0] == 1 ? 1 : 0;
            }
        }

        private int m_IsLittleEndian;
        private bool IsLittleEndian => m_IsLittleEndian != 0;

        private static short ByteSwap(short val)
        {
            return (short)(((val & 0xff) << 8) | ((val >> 8) & 0xff));
        }

        private static int ByteSwap(int val)
        {
            return (int)(((val & 0xff) << 24) | ((val & 0xff00) << 8) | ((val >> 8) & 0xff00) | ((val >> 24) & 0xff));
        }

        /// <summary>
        /// Read and copy data to the memory location pointed to, an exception will
        /// be thrown if it does not fit.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="length"></param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the length
        /// will put the reader out of bounds based on the current read pointer
        /// position.</exception>
        public void ReadBytes(byte* data, int length)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif
            if (GetBytesRead() + length > m_Length)
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                throw new System.ArgumentOutOfRangeException();
#else
                UnsafeUtility.MemClear(data, length);
                return;
#endif
            }
            // Restore the full bytes moved to the bit buffer but no consumed
            m_Context.m_ReadByteIndex -= (m_Context.m_BitIndex >> 3);
            m_Context.m_BitIndex = 0;
            UnsafeUtility.MemCpy(data, m_bufferPtr + m_Context.m_ReadByteIndex, length);
            m_Context.m_ReadByteIndex += length;
        }

        public int GetBytesRead()
        {
            return m_Context.m_ReadByteIndex - (m_Context.m_BitIndex >> 3);
        }

        public byte ReadByte()
        {
            byte data;
            ReadBytes((byte*)&data, sizeof(byte));
            return data;
        }

        public short ReadShortNetworkByteOrder()
        {
            short data;
            ReadBytes((byte*)&data, sizeof(short));
            return IsLittleEndian ? ByteSwap(data) : data;
        }

        public ushort ReadUShortNetworkByteOrder()
        {
            return (ushort)ReadShortNetworkByteOrder();
        }

        public int ReadIntNetworkByteOrder()
        {
            int data;
            ReadBytes((byte*)&data, sizeof(int));
            return IsLittleEndian ? ByteSwap(data) : data;
        }

        public uint ReadUIntNetworkByteOrder()
        {
            return (uint)ReadIntNetworkByteOrder();
        }
    }
}
