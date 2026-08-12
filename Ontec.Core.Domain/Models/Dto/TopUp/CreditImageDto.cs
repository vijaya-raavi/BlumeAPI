using System.Numerics;
using Ontec.Core.Domain.Models.Dto.Meter;

namespace Ontec.Core.Domain.Models.Dto.TopUp
{
    public class CreditImageDto
    {
        public int Id { get; set; }
        public int ImageCount { get; set; }
        public List<ImagePathValueDto> ImagePathDetails { get; set; }
    }

    public class ImagePathValueDto
    { 
       public string Images { get; set; }
    }
    public class CreditImagesRawDto
    {
        public string Image_Path_1 { get; set; }
        public string Image_Path_2 { get; set; }
        public string Image_Path_3 { get; set; }
        public string Image_Path_4 { get; set; }
        public string Image_Path_5 { get; set; }
        public string Image_Path_6 { get; set; }
    }
    public class ImageWithDate
    {
        public string Path { get; set; }
        public DateTime? Date { get; set; }
    }
    public class UserCreditImageDto
    {
        public long Id { get; set; }                // table PK
        public int UserId { get; set; }                // table PK
        public int CreditImageId { get; set; }     // enum position (1–6)
        public string Name { get; set; }           // enum name (image_path_1…)
        public string DisplayName { get; set; }    // enum display
        public string ImageUrl { get; set; }
        public string CreatedAt { get; set; }
        public string ModifiedAt { get; set; }
        public double Credit { get; set; }

        public DocumentResultDto Image { get; set; }
    }
}
