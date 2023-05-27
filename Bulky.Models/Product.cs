using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Bulky.Models;

public record Product
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(30)]
    [DisplayName("Book Title")]
    public string Title { get; set; }
    
    [Required]
    public string Description { get; set; }
    
    [Required]
    public string ISBN { get; set; }
    
    [Required]
    public string Author { get; set; }
    
    [Required]
    [Display( Name = "List Price")]
    [Range(1,1000)]
    public double ListPrice { get; set; }

    [Required]
    [Display(Name = "Price for 1-50 books")]
    [Range(1, 1000)]
    public double Price { get; set; }

    [Required]
    [Display(Name = "Price for 50-99 books")]
    [Range(1, 1000)]
    public double Price50 { get; set; }

    [Required]
    [Display(Name = "Price for 100+ books")]
    [Range(1, 1000)]
    public double Price100 { get; set; }

    [Required]
    public int CategoryId { get; set; }
    
    [ForeignKey("CategoryId")]
    [ValidateNever]
    public Category Category { get; set; }
    
    [ValidateNever]
    public string ImageUrl { get; set; }

}
