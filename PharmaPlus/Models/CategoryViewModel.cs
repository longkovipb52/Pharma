using System.ComponentModel.DataAnnotations;

namespace PharmaPlus.Models
{
    public class CategoryDisplayModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ProductCount { get; set; }
        public int InStockProductCount { get; set; }
        public string IconClass { get; set; }
        public string ColorCode { get; set; }
    }
}