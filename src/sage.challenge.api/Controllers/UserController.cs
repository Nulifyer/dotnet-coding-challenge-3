using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using sage.challenge.data.Cache;
using sage.challenge.data.Entities;

namespace sage.challenge.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UserController : Controller
    {
        private readonly ISimpleObjectCache<Guid, User> _cache;

        public UserController(ISimpleObjectCache<Guid, User> cache)
        {
            _cache = cache;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            #region User Validation

            if (user == null)
                return BadRequest(new SimpleError("missing request body"));

            // user.FirstName
            user.FirstName = user.FirstName?.Trim();
            if (string.IsNullOrEmpty(user.FirstName))
                return BadRequest(new ArgumentException($"{nameof(user.FirstName)} is required", nameof(user.FirstName)).ToSimpleError());
            if (user.FirstName.Length > 128)
                return BadRequest(new ArgumentException($"{nameof(user.FirstName)} must be less than 128 characters", nameof(user.FirstName)).ToSimpleError());

            // user.LastName
            user.LastName = user.LastName?.Trim();
            if (user.LastName != null && user.LastName.Length > 128)
                return BadRequest(new ArgumentException($"{nameof(user.LastName)} must be less than 128 characters", nameof(user.LastName)).ToSimpleError());

            // user.Email
            user.Email = user.Email?.Trim();
            if (string.IsNullOrEmpty(user.Email))
                return BadRequest(new ArgumentException($"{nameof(user.Email)} is required", nameof(user.Email)).ToSimpleError());
            if (!IsValidEmail(user.Email))
                return BadRequest(new ArgumentException($"{nameof(user.Email)} is invalid", nameof(user.Email)).ToSimpleError());
            
            // user.DateOfBirth
            if (user.DateOfBirth == default)
                return BadRequest(new ArgumentException($"{nameof(user.DateOfBirth)} is required", nameof(user.DateOfBirth)).ToSimpleError());
            
            var min_age_datetime = DateTime.UtcNow.Date.AddYears(-18);
            if (user.DateOfBirth > min_age_datetime)
                return BadRequest(new ArgumentException($"user must be 18 years or older", nameof(user.DateOfBirth)).ToSimpleError());

            // check that email doesn't exist
            var users = await _cache.GetAllAsync();
            if (users.Any(x => string.Equals(x.Email, user.Email, StringComparison.OrdinalIgnoreCase)))
                return BadRequest(new ArgumentException($"{nameof(user.Email)} already exists", nameof(user.Email)).ToSimpleError());

            #endregion

            user.Id = Guid.CreateVersion7();

            await _cache.AddAsync(user.Id, user);
            
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _cache.GetAllAsync();
            return Ok(users);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetUser([FromRoute] Guid id)
        {
            if (id == default)
                return BadRequest(new ArgumentException($"{nameof(id)} is required", nameof(id)).ToSimpleError());

            var user = await _cache.GetAsync(id);
            if (user == default)
                return NotFound();

            return Ok(user);
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteUser([FromRoute] Guid id)
        {
            if (id == default)
                return BadRequest(new ArgumentException($"{nameof(id)} is required", nameof(id)).ToSimpleError());

            await _cache.DeleteAsync(id);
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> PutUser([FromBody] User user)
        {
            #region User Validation

            if (user == null)
                return BadRequest("missing request body");

            // user.Id
            if (user.Id == default)
                return BadRequest(new ArgumentException($"{nameof(user.Id)} is required", nameof(user.Id)).ToSimpleError());

            // user.FirstName
            user.FirstName = user.FirstName?.Trim();
            if (string.IsNullOrEmpty(user.FirstName))
                return BadRequest(new ArgumentException($"{nameof(user.FirstName)} is required", nameof(user.FirstName)).ToSimpleError());
            if (user.FirstName.Length > 128)
                return BadRequest(new ArgumentException($"{nameof(user.FirstName)} must be less than 128 characters", nameof(user.FirstName)).ToSimpleError());

            // user.LastName
            user.LastName = user.LastName?.Trim();
            if (user.LastName != null && user.LastName.Length > 128)
                return BadRequest(new ArgumentException($"{nameof(user.LastName)} must be less than 128 characters", nameof(user.LastName)).ToSimpleError());

            // user.Email
            user.Email = user.Email?.Trim();
            if (string.IsNullOrEmpty(user.Email))
                return BadRequest(new ArgumentException($"{nameof(user.Email)} is required", nameof(user.Email)).ToSimpleError());
            if (!IsValidEmail(user.Email))
                return BadRequest(new ArgumentException($"{nameof(user.Email)} is invalid", nameof(user.Email)).ToSimpleError());
            
            // user.DateOfBirth
            if (user.DateOfBirth == default)
                return BadRequest(new ArgumentException($"{nameof(user.DateOfBirth)} is required", nameof(user.DateOfBirth)).ToSimpleError());
            
            var min_age_datetime = DateTime.UtcNow.Date.AddYears(-18);
            if (user.DateOfBirth > min_age_datetime)
                return BadRequest(new ArgumentException($"user must be 18 years or older", nameof(user.DateOfBirth)).ToSimpleError());

            // check that email doesn't exist
            var users = await _cache.GetAllAsync();
            if (users.Any(x => x.Id != user.Id && string.Equals(x.Email, user.Email, StringComparison.OrdinalIgnoreCase)))
                return BadRequest(new ArgumentException($"{nameof(user.Email)} already exists", nameof(user.Email)).ToSimpleError());

            #endregion

            var foundUser = await _cache.GetAsync(user.Id);
            if (foundUser == null)
                return NotFound();
            
            await _cache.UpdateAsync(user.Id, user);
            
            return Ok(user);
        }
        
        private bool IsValidEmail(string email)
        {
            var trimmedEmail = email?.Trim();
            if (string.IsNullOrEmpty(trimmedEmail))
                return false;

            if (trimmedEmail.EndsWith("."))
                return false; // suggested by @TK-421

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == trimmedEmail;
            }
            catch
            {
                return false;
            }
        }
    }
}
