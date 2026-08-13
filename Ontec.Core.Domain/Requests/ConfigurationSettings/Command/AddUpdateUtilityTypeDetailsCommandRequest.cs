using MediatR;
using Ontec.Core.Domain.Models.Dto.Common;

namespace Ontec.Core.Domain.Requests.ConfigurationSettings.Command
{
    public class AddUpdateUtilityTypeDetailsCommandRequest : IRequest<AddUpdateResultDto>
    {
        public List<UtilityWiseDailyTarget> UtilityDailyTarget { get; set; } = new List<UtilityWiseDailyTarget>();

        public class UtilityWiseDailyTarget
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public double MinDailyTarget { get; set; }
            public double MaxDailyTarget { get; set; }
        }
    }
}
