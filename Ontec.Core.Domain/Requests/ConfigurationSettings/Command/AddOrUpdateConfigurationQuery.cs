using MediatR;

namespace Ontec.Core.Domain.Requests.Configuration.Command
{
    public class AddOrUpdateConfigurationQuery : IRequest<string>
    {
        //public int CompanyId { get; set; }
        public List<Configuration> Configurations { get; set; } = new List<Configuration>();
        public class Configuration
        {
            public int Id { get; set; }
            public string Value { get; set; }
            
        }
       
    }
}
