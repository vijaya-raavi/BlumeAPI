using MediatR;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.Configuration;

namespace Ontec.Core.Domain.Requests.ConfigurationSettings.Command
{
    public class UpdateBusinessHoursConfigurations:IRequest<AddUpdateResultDto>
    {
        public List<Businessconfigurations> businessConfigurations { get; set; } = new List<Businessconfigurations>();

        public class Businessconfigurations
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool Enabled { get; set; }
            public string  From { get; set; }
            public string To { get; set; }
            public bool AcceptUserNonBusiness { get; set; }
            public bool AcceptUserBusiness { get; set; }

            public bool AcceptMeterNonBusiness { get; set; }

            public bool AcceptMeterBusiness { get; set; }
        }
    }
}
