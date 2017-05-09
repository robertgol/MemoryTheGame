using System;
using System.IO;
using SQLite;
using Xamarin.Forms;
using MemoryTheGame.Persistence;
using MemoryTheGame.Droid.Persistence;

[assembly: Dependency(typeof(SQLiteDb))]

namespace MemoryTheGame.Droid.Persistence
{
    public class SQLiteDb : ISQLiteDb
    {
        public SQLiteAsyncConnection GetConnection()
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var path = Path.Combine(documentsPath, "MemoryTheGameSQLite.db3");

            return new SQLiteAsyncConnection(path);
        }
    }
}