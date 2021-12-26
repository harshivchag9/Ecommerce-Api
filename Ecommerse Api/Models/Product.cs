using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace Ecommerse_Api.Models
{
    public partial class Product
    {
        public Product()
        {
            ProductImages = new HashSet<ProductImage>();
        }

        public long Id { get; set; }
        public string Productname { get; set; }
        public string Productsize { get; set; }
        public string Productdiscription { get; set; }
        public string Productimage { get; set; }
        public string Createdby { get; set; }
        public double? Productprice { get; set; }
        public double? Productrating { get; set; }
        public string Productcategory { get; set; }

        [ForeignKey("Productid")]
        public virtual ICollection<ProductImage> ProductImages { get; set; }
    }
}
