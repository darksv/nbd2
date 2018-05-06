using System.ComponentModel.DataAnnotations;

namespace NBD2.Model
{
    public enum Sex
    {
        [Display(Description = "Mężczyzna")]
        Male,
        [Display(Description = "Kobieta")]
        Female,
    }
}