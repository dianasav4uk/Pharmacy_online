﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Repos;
using Pharmacy.UI.Models;
using System.Diagnostics;
using XStats.Core;

namespace Pharmacy.UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly PharmacyDbContext _dbcontext;
        private readonly CategoryRepository _categoryRepository;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, CategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View(_categoryRepository.GetAllCategory());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}