using System;
using System.Runtime.InteropServices;

public class LevelDBWrapper : IDisposable
{
    [DllImport("./leveldbwrapper.so", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool db_open(string data_directory);

    [DllImport("./leveldbwrapper.so", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool db_close();

    [DllImport("./leveldbwrapper.so", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr db_get(string key);

    [DllImport("./leveldbwrapper.so", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool db_put(string key, string value);

    [DllImport("./leveldbwrapper.so", CallingConvention = CallingConvention.Cdecl)]
    private static extern bool db_delete(string key);

    [DllImport("./leveldbwrapper.so", CallingConvention = CallingConvention.Cdecl)]
    private static extern void db_free(IntPtr ptr);

    public LevelDBWrapper(string dataDirectory)
    {
        if (!db_open(dataDirectory))
        {
            throw new Exception("FAIL");
        }
    }

    ~LevelDBWrapper()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            db_close();
        }
    }

    public string Get(string key)
    {
        IntPtr valuePtr = db_get(key);
        if (valuePtr == IntPtr.Zero)
        {
            return null;
        }
        string value = Marshal.PtrToStringAnsi(valuePtr);
        db_free(valuePtr);
        return value;
    }

    public bool Put(string key, string value)
    {
        return db_put(key, value);
    }

    public bool Delete(string key)
    {
        return db_delete(key);
    }
}