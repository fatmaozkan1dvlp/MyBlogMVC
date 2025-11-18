using Microsoft.AspNetCore.Mvc;

namespace Basics.Controllers
{
    public class EmployeController :Controller
    {
        public IActionResult Index1()
        {
            string message = $"Hello World. {DateTime.Now.ToString()}";
            return View("Index1",message);
        }

        public ViewResult Index2()
        {
            var names = new string[]
            {
                "Ahmet",
                "Mehmet",
                "Ali"
            };
            return View(names);
        }

        public IActionResult Index3()
        {
            return Content("Employe");
        }
        
    }
}