using SQLite;
using Xamarin.Forms;
using SkinSelfie.Droid;
using System.IO;
using SkinSelfie.Interfaces;

[assembly: Dependency(typeof(SQLite_Android))]
namespace SkinSelfie.Droid
{
    public class SQLite_Android : ISQLite
    {
        public SQLite_Android()
        {
        }

        #region ISQLite implementation

        public SQLiteConnection GetConnection()
        {
            var fileName = "RandomThought.db3";
            var documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var path = Path.Combine(documentsPath, fileName);
            var connection = new SQLiteConnection(path);

            return connection;
        }

        #endregion
    }
}