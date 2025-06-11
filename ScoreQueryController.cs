using System.Web.Mvc;
using MesTAManagementSystem_New.ViewModels.Training.Testing;
using MesTAManagementSystem_New.Services;

namespace MesTAManagementSystem_New.Controllers.Training.Testing
{
    public class ScoreQueryController : Controller
    {
        private readonly TestingService _service = new TestingService();

        public ActionResult Index()
        {
            var vm = _service.InitializeScoreQueryVM(null);
            return View(vm);
        }

        [HttpPost]
        public ActionResult Index(ScoreQueryVM vm)
        {
            vm = _service.InitializeScoreQueryVM(vm);
            vm.ResultList = _service.GetScoreQueryResult(vm);
            return View(vm);
        }
    }
}