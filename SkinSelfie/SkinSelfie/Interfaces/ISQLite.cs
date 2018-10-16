using SQLite;

namespace SkinSelfie.Interfaces
{
    public interface ISQLite
    {
        SQLiteConnection GetConnection();
    }
}
