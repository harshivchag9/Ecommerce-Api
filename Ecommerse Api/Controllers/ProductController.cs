using Ecommerse_Api.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using ProjectApi.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerse_Api.Controllers
{
    [Route("api/Products")]
    public class ProductController : Controller
    {
        private readonly EcomContext _context;
        private ResponseMessage response;

        public ProductController(EcomContext dbContext)
        {
            _context = dbContext;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return Ok("harshiv");
        }

        [Route("getproducts")]
        [HttpGet]
        public IActionResult getProducts()
        {
            var data = _context.Products.Select(product => new
            {
                Id = product.Id,
                Productname = product.Productname,
                productcategory = product.Productcategory,
                productsize = product.Productsize,
                productdiscription = product.Productdiscription,
                createdby = product.Createdby,
                productprice = product.Productprice,
                productrating = product.Productrating,
                productImages = product.ProductImages.Where(productimage => product.Id == productimage.Productid).Select(image => new
                {
                    Id = image.Id,
                    Url = image.Url,
                }).ToList()
            });

            return Ok(data);

            //return Ok(_context.Products.ToList());
        }


        [Route("searchproducts")]
        [HttpGet]
        public IActionResult searchProducts([FromQuery] PaginationParams @params)
        {
            int CurrentPage = @params.Page;
            int PageSize = @params.ItemsPerPage;

            int count = _context.Products.Where(x => x.Productname.ToLower().Trim().Contains(!string.IsNullOrEmpty(@params.Search) ? @params.Search.Trim().ToLower() : "")).Count();

            var items = _context.Products.Where(x => x.Productname.ToLower().Trim().Contains(!string.IsNullOrEmpty(@params.Search) ? @params.Search.Trim().ToLower() : "")).Skip((CurrentPage - 1) * PageSize).Take(PageSize).Select(product => new
            {
                Id = product.Id,
                Productname = product.Productname,
                productcategory = product.Productcategory,
                productsize = product.Productsize,
                productdiscription = product.Productdiscription,
                createdby = product.Createdby,
                productprice = product.Productprice,
                productrating = product.Productrating,
                productImages = product.ProductImages.Where(productimage => product.Id == productimage.Productid).Select(image => new
                {
                    Id = image.Id,
                    Url = image.Url,
                }).ToList()
            });
            int TotalPages = (int)Math.Ceiling(count / (double)PageSize);
            int TotalCount = count;
            var previousPage = CurrentPage > 1 ? "Yes" : "No";
            var nextPage = CurrentPage < TotalPages ? "Yes" : "No";
            var paginationMetadata = new
            {
                totalCount = count,
                pageSize = PageSize,
                currentPage = @params.Page,
                totalPages = TotalPages,
                previousPage,
                nextPage,
                QuerySearch = string.IsNullOrEmpty(@params.Search) ? "No Parameter Passed" : @params.Search
            };

           
            Response.Headers.Add("x-paging", JsonConvert.SerializeObject(paginationMetadata));
            return Ok(items);
        }


        [Route("getproductimages/{id}")]
        [HttpGet]
        public IActionResult getProductImage(int id)
        {
            return Ok(_context.ProductImages.Where(productimage => productimage.Productid == id).ToList());
        }

        [Route("deleteimage/{id}")]
        [Authorize(Roles = Role.Admin)]
        [HttpGet]
        public IActionResult deleteImage(int id)
        {
            try
            {
                var itemToRemove = _context.ProductImages.SingleOrDefault(x => x.Id == id);
                if (itemToRemove != null)
                {
                    _context.ProductImages.Remove(itemToRemove);
                    var path = Path.Combine(Directory.GetCurrentDirectory(), itemToRemove.Url);

                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }
                else
                {
                    this.response = new ResponseMessage(0, "false", 200, "Product Not Found.");
                    return Ok(this.response);
                }
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                this.response = new ResponseMessage(0, "false", 200, "Something Went Wrong");
                return Ok(this.response);
            }
            this.response = new ResponseMessage(0, "true", 200, "Image Deleted Successfully");
            return Ok(this.response);
        }

        [Route("insertimage")]
        [Authorize(Roles = Role.Admin)]
        [HttpPost]
        public IActionResult AddImage(int id)
        {
            //IFormFile[] file
            try
            {
                var itemToCheck = _context.Products.SingleOrDefault(x => x.Id == id);
                if (itemToCheck != null)
                {
                    var files = Request.Form.Files;
                    var foldername = Path.Combine("Assets", "ProductImages");
                    List<ProductImage> productimages = new List<ProductImage>();
                    var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), foldername);

                    if (files == null || files.Count == 0)
                    {
                        this.response = new ResponseMessage(0, "false", 200, "File is not selected");
                        return NotFound(this.response);
                    }

                    foreach (var formFile in files)
                    {
                        var supportedTypes = new[] { "jpg", "jpeg", "png" };
                        var fileExt = System.IO.Path.GetExtension(formFile.FileName).Substring(1);
                        if (!supportedTypes.Contains(fileExt))
                        {
                            //ErrorMessage = "File Extension Is InValid - Only Upload WORD/PDF/EXCEL/TXT File";
                            this.response = new ResponseMessage(0, "false", 200, "File Extension Is InValid - Only Upload jpg/jpeg/png File");
                            return Ok(this.response);
                        }
                    }

                    foreach (var formFile in files)
                    {
                        if (formFile.Length > 0)
                        {
                          
                            var fileName = DateTime.Now.Ticks.ToString() + ContentDispositionHeaderValue.Parse(formFile.ContentDisposition).FileName.Trim('"');
                            var fullPath = Path.Combine(pathToSave, fileName);
                            var dbPath = Path.Combine(foldername, fileName);

                            using (var stream = new FileStream(fullPath, FileMode.Create))
                            {
                                formFile.CopyTo(stream);
                            }
                            productimages.Add(new ProductImage { Productid = id, Url = dbPath.ToString() });
                        }
                    }
                    _context.ProductImages.AddRange(productimages);
                    _context.SaveChanges();
                }
                else
                {
                    this.response = new ResponseMessage(0, "false", 200, "Product not Found");
                    return NotFound(this.response);
                }


            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                this.response = new ResponseMessage(0, "false", 200, "Something Went Wrong");
                return NotFound(this.response);
            }
            this.response = new ResponseMessage(0, "true", 200, "Product Added Successfully");
            return Ok(this.response);
        }
     



        [Authorize(Roles = Role.Admin)]
        [Route("editproduct")]
        [HttpPost]
        public IActionResult editProduct([FromBody] Product product)
        {
            try
            {
                var update = _context.Products.FirstOrDefault(e => e.Id == product.Id);
                if (update != null)
                {
                    update.Productname = product.Productname;
                    update.Productsize = product.Productsize;
                    update.Productdiscription = product.Productdiscription;
                    update.Createdby = product.Createdby;
                    update.Productprice = product.Productprice;
                    update.Productcategory = product.Productcategory;
                }
                else
                {
                    this.response = new ResponseMessage(0, "false", 403, "Something Went Wrong");
                    return NotFound(this.response);
                }
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                this.response = new ResponseMessage(0, "false", 403, "Something Went Wrong");
                Console.WriteLine(e);
                return NotFound(this.response);
            }
            this.response = new ResponseMessage(0, "true", 200, "Product Updated Successfully");
            return Ok(this.response);
        }

        [Authorize(Roles = Role.Admin)]
        [Route("addproduct")]
        [HttpPost]
        public IActionResult addProduct(String data, IFormFile[] file)
        {


            var product = JsonConvert.DeserializeObject<Product>(data);
            try
            {
                var files = Request.Form.Files;
                var foldername = Path.Combine("Assets", "ProductImages");
                List<ProductImage> productimages = new List<ProductImage>();
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), foldername);

                if (files == null || files.Count == 0)
                {
                    this.response = new ResponseMessage(0, "false", 200, "File is not selected");
                    return NotFound(this.response);
                }

                foreach (var formFile in files)
                {
                    var supportedTypes = new[] { "jpg", "jpeg", "png" };
                    var fileExt = System.IO.Path.GetExtension(formFile.FileName).Substring(1);
                    if (!supportedTypes.Contains(fileExt))
                    {
                        //ErrorMessage = "File Extension Is InValid - Only Upload WORD/PDF/EXCEL/TXT File";
                        this.response = new ResponseMessage(0, "false", 200, "File Extension Is InValid - Only Upload jpg/jpeg/png File");
                        return Ok(this.response);
                    }
                }

                foreach (var formFile in files)
                {
                    if (formFile.Length > 0)
                    {
                        var fileName = DateTime.Now.Ticks.ToString() + ContentDispositionHeaderValue.Parse(formFile.ContentDisposition).FileName.Trim('"');
                        var fullPath = Path.Combine(pathToSave, fileName);
                        var dbPath = Path.Combine(foldername, fileName);

                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            formFile.CopyTo(stream);
                        }
                        productimages.Add(new ProductImage { Productid = 0, Url = dbPath.ToString(), Product = product });
                    }
                }
                _context.Products.Add(product);
                _context.SaveChanges();
                foreach (var image in productimages)
                {
                    image.Productid = product.Id;
                }
                _context.ProductImages.AddRange(productimages);
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                this.response = new ResponseMessage(0, "false", 200, "Something Went Wrong");
                return NotFound(this.response);
            }
            this.response = new ResponseMessage(0, "true", 200, "Product Added Successfully");
            return Ok(this.response);
        }

        [Authorize(Roles = Role.Admin)]
        [Route("deleteproduct/{id}")]
        [HttpPost]
        public IActionResult deleteProduct(int id)
        {
            try
            {
                var itemToRemove = _context.Products.SingleOrDefault(x=> x.Id == id);
                if (itemToRemove != null)
                {
                    _context.Products.Remove(itemToRemove);
                    _context.SaveChanges();
                }
                else
                {
                    this.response = new ResponseMessage(0, "false", 200, "Product Not Found.");
                    return NotFound(this.response);
                }
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                this.response = new ResponseMessage(0, "false", 200, "Something Went Wrong");
                return NotFound(this.response);
            }
            this.response = new ResponseMessage(0, "true", 200, "Product Deleted Successfully");
            return Ok(this.response);
        }

    }
}
