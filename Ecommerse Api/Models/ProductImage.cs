using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace Ecommerse_Api.Models
{
    public partial class ProductImage
    {
        public long Id { get; set; }

        public long? Productid { get; set; }
        public string Url { get; set; }

        public virtual Product Product { get; set; }
    }
}
