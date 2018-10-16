using System.Threading.Tasks;

namespace SkinSelfie.Interfaces
{
	public interface IPermissions
	{
		Task<bool> GetCameraPermissions();
        Task<bool> GetContactsPermissions();
	}
}
