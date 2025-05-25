using Microsoft.AspNetCore.Mvc;

namespace SoruCevapPortali.Ui.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Questions()
        {
            return View();
        }

        public IActionResult QuestionDetail(int id)
        {
            ViewBag.QuestionId = id;
            return View();
        }

        public IActionResult Answers()
        {
            return View();
        }
    }
}
