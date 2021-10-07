using System.ComponentModel.DataAnnotations;

namespace WebApplication.Models
{
    public class SearchPostCodeModel
    {
        [Required, RegularExpression("([Gg][Ii][Rr] 0[Aa]{2})|((([A-Za-z][0-9]{1,2})|(([A-Za-z][A-Ha-hJ-Yj-y][0-9]{1,2})|(([A-Za-z][0-9][A-Za-z])|([A-Za-z][A-Ha-hJ-Yj-y][0-9][A-Za-z]?))))\\s?[0-9][A-Za-z]{2})")]
        public string PostCode { get; set; }
        
        [Required]
        public string Target { get; set; }
    }
}