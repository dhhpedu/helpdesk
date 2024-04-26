﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TicketCore.ViewModels.MenuCategorys
{
    public class CreateMenuCategoryViewModel
    {
        [Display(Name = "Menu Category")]
        [Required(ErrorMessage = "Enter Category Name")]
        public string MenuCategoryName { get; set; }

        [Display(Name = "Role")]
        [Required(ErrorMessage = "Select Role")]
        public int RoleId { get; set; }

        [Display(Name = "IsActive")]
        [Required(ErrorMessage = "Required IsActive")]
        public bool Status { get; set; }

        public List<SelectListItem> ListofRoles { get; set; }
    }

    public class EditMenuCategoriesViewModel
    {
        public int MenuCategoryId { get; set; }

        [Display(Name = "Menu Category")]
        [Required(ErrorMessage = "Enter Category Name")]
        public string MenuCategoryName { get; set; }

        [Display(Name = "Role")]
        [Required(ErrorMessage = "Select Role")]
        public int RoleId { get; set; }

        [Display(Name = "IsActive")]
        [Required(ErrorMessage = "Required IsActive")]
        public bool Status { get; set; }

        public List<SelectListItem> ListofRoles { get; set; }
    }

    public class MenuCategoryGridViewModel
    {
        public string MenuCategoryName { get; set; }
        public string RoleName { get; set; }
        public string Status { get; set; }
        public int MenuCategoryId { get; set; }
    }

    public class RequestDeleteCategory
    {
        public int MenuCategoryId { get; set; }
    }

    public class RequestForMenuCategory
    {
        public int? RoleID { get; set; }
    }
}