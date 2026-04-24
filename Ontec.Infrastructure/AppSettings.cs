using Ontec.Infrastructure.Persistence.Configurations;

namespace Ontec.Infrastructure
{
    public class AppSettings
    {
        public DatabaseSetting DatabaseSetting { get; set; } = new DatabaseSetting();
    }
}
