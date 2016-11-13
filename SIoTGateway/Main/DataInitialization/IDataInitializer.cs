using System.Threading.Tasks;

namespace Main.DataInitialization
{
    public interface IDataInitializer
    {
        void BootstrapDevice(string id);
    }
}