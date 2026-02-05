using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinCoreBank.Models;
using MinCoreBank.Repositories;
using System.Threading.Tasks;

namespace MinCoreBank.Controllers
{
    namespace MinCoreBank.Controllers
    {
        [Authorize(Roles = "admin")]
        public class GeneralLedgerViewController : Controller
        {
            private readonly IGeneralLedgerRepository _repository;

            public GeneralLedgerViewController(IGeneralLedgerRepository repository)
            {
                _repository = repository;
            }

            // GET: GeneralLedger
            public async Task<IActionResult> Index()
            {
                var accounts = await _repository.GetAllAsync();
                return View(accounts);
            }

            // GET: GeneralLedger/Create
            public IActionResult Create()
            {
                return View();
            }

            // GET: GeneralLedger/Edit/{id}
            public async Task<IActionResult> Edit(int id)
            {
                var account = await _repository.GetByIdAsync(id);
                if (account == null)
                {
                    return NotFound();
                }
                return View(account);
            }

            // GET: GeneralLedger/Details/{id}
            public async Task<IActionResult> Details(int id)
            {
                var account = await _repository.GetByIdAsync(id);
                if (account == null)
                {
                    return NotFound();
                }
                return View(account);
            }

            // GET: GeneralLedger/Delete/{id}
            public async Task<IActionResult> Delete(int id)
            {
                var account = await _repository.GetByIdAsync(id);
                if (account == null)
                {
                    return NotFound();
                }
                return View(account);
            }
        }
    }
}