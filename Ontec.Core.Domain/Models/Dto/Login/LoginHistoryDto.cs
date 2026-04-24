namespace Ontec.Core.Domain.Models.Dto.Login
{
    public  class LoginHistoryDto
    {
       public int id { get; set; }
        public int userid {  get; set; }
        public string logindatetime { get; set; }
        public int loginstatus { get; set; }
        public string ipaddress {  get; set; }
    }
}
