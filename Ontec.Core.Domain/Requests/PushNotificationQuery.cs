using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ontec.Core.Domain.Requests
{
    public class PushNotificationQuery
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string DeviceToken { get; set; }
    }
}
