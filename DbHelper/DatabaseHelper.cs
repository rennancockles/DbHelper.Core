using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;

namespace DbHelper.Core
{
    public class DatabaseHelper : DbHandler, IDisposable
    {
        bool disposed = false;
        readonly SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);

        private DatabaseHelper(string connectionString, string provider) : base(connectionString, provider)
        {
        }

        public static DatabaseHelper Create(string connectionString, string provider = "System.Data.SqlClient")
        {
            return new DatabaseHelper(connectionString, provider);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                handle.Dispose();
            }

            disposed = true;
        }
    }
}