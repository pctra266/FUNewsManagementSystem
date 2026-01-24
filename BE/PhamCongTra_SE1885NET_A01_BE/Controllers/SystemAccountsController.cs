using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FuNewsManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SystemAccountsController : ControllerBase
    {
        private readonly ISystemAccountRepository _systemAccountRepository;

        public SystemAccountsController(ISystemAccountRepository systemAccountRepository)
        {
            _systemAccountRepository = systemAccountRepository;
        }

        // GET: api/SystemAccounts
        [Authorize(Roles = "ADMIN,STAFF")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SystemAccount>>> GetSystemAccounts()
        {
            var accounts = await Task.Run(() => _systemAccountRepository.GetSystemAccounts());
            return Ok(accounts);
        }

        // GET: api/SystemAccounts/5
        [HttpGet("{id}")]
        [Authorize(Roles = "ADMIN,STAFF")]
        public async Task<ActionResult<SystemAccount>> GetSystemAccount(short id)
        {
            var account = await Task.Run(() => _systemAccountRepository.GetSystemAccountById(id));
            if (account == null)
            {
                return NotFound();
            }
            return Ok(account);
        }

        // PUT: api/SystemAccounts/5
        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN,STAFF")]
        public async Task<IActionResult> PutSystemAccount(short id, SystemAccount systemAccount)
        {
            if (id != systemAccount.AccountId)
            {
                return BadRequest();
            }

            try
            {
                await Task.Run(() => _systemAccountRepository.UpdateSystemAccount(systemAccount));
            }
            catch (Exception)
            {
                throw;
            }

            return NoContent();
        }

        // POST: api/SystemAccounts
        [HttpPost]
        [Authorize(Roles = "ADMIN,STAFF")]
        public async Task<ActionResult<SystemAccount>> PostSystemAccount(SystemAccount systemAccount)
        {
            try
            {
                await Task.Run(() => _systemAccountRepository.AddSystemAccount(systemAccount));
            }
            catch (Exception)
            {
                throw;
            }

            return CreatedAtAction("GetSystemAccount", new { id = systemAccount.AccountId }, systemAccount);
        }

        // DELETE: api/SystemAccounts/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN,STAFF")]
        public async Task<IActionResult> DeleteSystemAccount(short id)
        {
            var account = await Task.Run(() => _systemAccountRepository.GetSystemAccountById(id));
            if (account == null)
            {
                return NotFound();
            }

            await Task.Run(() => _systemAccountRepository.DeleteSystemAccount(id));
            return NoContent();
        }
    }
}
