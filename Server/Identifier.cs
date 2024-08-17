using Newtonsoft.Json;

public class Identifier
{
    private static readonly uint BYTES_TO_BITS = 8;

    private static readonly uint MAX_SIZE = uint.MaxValue / BYTES_TO_BITS;

    private static readonly uint CACHE_SIZE = 1024;

    private readonly byte[] bitmap;

    private readonly Stack<uint> cache;

    private uint previousID = 0;

    private uint i = 0;

    public Identifier()
    {
        bitmap = new byte[MAX_SIZE];
        cache = new Stack<uint>();
    }

    public uint Allocate()
    {
        if (cache.Count > 0)
        {
            uint id = cache.Pop();
            SetBitmap(id);
            return id;
        }
        else
        {
            while (true)
            {
                uint id = (uint)(((ulong)i + previousID) % uint.MaxValue);

                if (GetBitmap(id) == false)
                {
                    previousID = id;
                    SetBitmap(id);
                    i = 1;
                    return id;
                }

                if (i == uint.MaxValue)
                {
                    break;
                }

                i++;
            }

        }        

        throw new OutOfMemoryException("No unique identifier is available.");
    }

    public void Deallocate(uint id)
    {
        ClearBitmap(id);

        if (cache.Count < CACHE_SIZE)
        {
            cache.Push(id);
        }
    }

    private void SetBitmap(uint id)
    {
        uint arrayIndex = id / BYTES_TO_BITS;
        byte bitIndex = (byte)(id % BYTES_TO_BITS);
        bitmap[arrayIndex] |= (byte)(1 << bitIndex);
    }

    private void ClearBitmap(uint id)
    {
        uint arrayIndex = id / BYTES_TO_BITS;
        byte bitIndex = (byte)(id % BYTES_TO_BITS);
        bitmap[arrayIndex] &= (byte)~(1 << bitIndex);
    }

    public bool GetBitmap(uint id)
    {
        uint arrayIndex = id / BYTES_TO_BITS;
        byte bitIndex = (byte)(id % BYTES_TO_BITS);
        return ((bitmap[arrayIndex] >> bitIndex) & 1) != 0;
    }

    public void Save(string data_directory)
    {
        SerializableIdentifier data = new SerializableIdentifier
        {
            bitmap = bitmap,
            cache = cache.ToArray(),
            previousID = previousID
        };

        string json = JsonConvert.SerializeObject(data);
        File.WriteAllText(data_directory, json);
    }

    public void Load(string data_directory)
    {
        Identifier idManager = new Identifier();
        
        if (File.Exists(data_directory))
        {
            string json = File.ReadAllText(data_directory);
            SerializableIdentifier data = JsonConvert.DeserializeObject<SerializableIdentifier>(json);
            Array.Copy(data.bitmap, bitmap, data.bitmap.Length);
            cache.Clear();

            foreach (uint id in data.cache)
            {
                cache.Push(id);
            }
            previousID = data.previousID;
        }
    }

    [Serializable]
    private class SerializableIdentifier
    {
        public byte[] bitmap;

        public uint[] cache;

        public uint previousID;
    }
}