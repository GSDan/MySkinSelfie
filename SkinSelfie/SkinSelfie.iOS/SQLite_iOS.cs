using System.IO;
using SkinSelfie.Interfaces;
using SkinSelfie.iOS;
using SQLite;
using Xamarin.Forms;

[assembly: Dependency(typeof(SQLite_iOS))]
namespace SkinSelfie.iOS
{
	public class SQLite_iOS : ISQLite
	{
		public SQLite_iOS()
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
