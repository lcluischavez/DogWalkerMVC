using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;


namespace DogWalkerMVC.Models.ViewModels
{
    public class DogEditViewmodel
    {
        public int Id { get; set; }

        [Display(Name = "Name")]
        [Required]
        [MinLength(2)]
        public string Name { get; set; }

        [Display(Name = "Breed")]
        [Required(ErrorMessage = "Breed is Required")]
        [MinLength(3, ErrorMessage = "Breed should be at least 3 characters")]
        public string Breed { get; set; }

        [Display(Name = "Owner")]
        [Required]
        public int OwnerId { get; set; }

        [Display(Name = "Notes")]
        [Required]
        public string Notes { get; set; }

        [Display(Name = "ImageUrl")]
        [Required]
        public string ImageUrl { get; set; }

        public List<SelectListItem> OwnerOptions { get; set; }
    }
}
