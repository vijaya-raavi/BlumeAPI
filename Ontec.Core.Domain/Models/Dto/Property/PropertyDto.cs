namespace Ontec.Core.Domain.Models.Dto.Property
{
    public class PropertyDto : BaseModel
    {
        public string Name { get; set; }
        public string UnitNumber { get; set; }
        public string Address { get; set; }
        public int MeterCount { get; set; }
        public string RoleName { get; set; }
        public int TenentCount { get; set; }
        public int PropertyRelationId { get; set; }
        public List<string> MeterTypeList { get; set; }
        public string AddedOn { get; set; }
        public int EstateId {  get; set; }
        public string Estate {  get; set; }

        public int StatusId {  get; set; }
        public string Status {  get; set; }
        public string Owner {  get; set; }
        public string MeterNumbers {  get; set; }

        public string Sources {  get; set; }

    }
}
