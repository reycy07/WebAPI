using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebAPI.Data;
using WebAPI.DTOs;
using WebAPI.Entities;
using WebAPI.Services;

namespace WebAPI.Controllers.V2
{
    [ApiController]
    [Route("api/v2/users")]
    public class UsersController : ControllerBase
    {
        private readonly IConfiguration config;
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;
        private readonly IUserService userService;
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public UsersController(
            IConfiguration config, 
            UserManager<User> userManager, 
            SignInManager<User> signInManager, 
            IUserService userService, 
            ApplicationDbContext context,
            IMapper mapper
            )
        {
            this.config = config;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.userService = userService;
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet]
        [Authorize(Policy = "isAdmin")]
        public async Task<IEnumerable<UserDTO>> Get()
        {
            List<User> users = await context.Users.ToListAsync();

            var usersDTO = mapper.Map<IEnumerable<UserDTO>>(users);

            return usersDTO;
        }

        [HttpPost(Name = "RegisterNewUserV2")]
        public async Task<ActionResult<AuthResponseDTO>> Register(UserCredentialsDTO credentialsDTO)
        {
            var user = new User
            {
                UserName = credentialsDTO.Email,
                Email = credentialsDTO.Email
            };

            var result = await userManager.CreateAsync(user, credentialsDTO.Password!);

            if(result.Succeeded)
            {
                var AuthResponse = await BuildToken(credentialsDTO);

                return AuthResponse;
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return ValidationProblem();
            }
        }

        [HttpGet("renew-token", Name = "RegenerateTokenV2")]
        [Authorize]
        public async Task<ActionResult<AuthResponseDTO>> RenewToken()
        {
            var user = await userService.GetUser();

            if (user is null) return NotFound();

            var userCredentialsDTO = new UserCredentialsDTO { Email = user.Email! };

            var authResponse = await BuildToken(userCredentialsDTO);

            return authResponse;
        }

        [HttpPost("upgrade-to-admin")]
        [Authorize(Policy = "isAdmin")]
        public async Task<ActionResult> Admin(EditClaimDTO editClaimDTO)
        {
            var user = await userManager.FindByEmailAsync(editClaimDTO.Email);
            if (user is null) return NotFound();
            await userManager.AddClaimAsync(user, new Claim("isAdmin", "true"));

            return NoContent();
        }

        [HttpPost("remove-to-admin")]
        [Authorize(Policy = "isAdmin")]
        public async Task<ActionResult> RemoveAdmin(EditClaimDTO editClaimDTO)
        {
            var user = await userManager.FindByEmailAsync(editClaimDTO.Email);
            if(user is null) return NotFound();

            await userManager.RemoveClaimAsync(user, new Claim("isAdmin", "false"));
            return NoContent();
        }
        private async Task<AuthResponseDTO> BuildToken(UserCredentialsDTO credentialsDTO)
        {
            var claims = new List<Claim>
            {
                new Claim("email", credentialsDTO.Email),
                new Claim("I want to", "everything I wanna")
            };

            var user = await userManager.FindByEmailAsync(credentialsDTO.Email);
            var claimsDB = await userManager.GetClaimsAsync(user!);

            claims.AddRange(claimsDB);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["jwtkey"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddYears(1);

            var securityToken = new JwtSecurityToken(issuer: null, audience: null, claims: claims,
                expires: expiration, signingCredentials: credentials);

            var token = new JwtSecurityTokenHandler().WriteToken(securityToken);

            return new AuthResponseDTO { Token = token, Expiration = expiration };
        }
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDTO>> Login(UserCredentialsDTO credentialsDTO)
        {
            var user = await userManager.FindByEmailAsync(credentialsDTO.Email);

            if(user is null) return IncorrectLoginError();

            var result = await signInManager.CheckPasswordSignInAsync(user, credentialsDTO.Password!, lockoutOnFailure: false);

            if (!result.Succeeded) return IncorrectLoginError();

            return await BuildToken(credentialsDTO);

        }

        [HttpPut]
        [Authorize]

        public async Task<ActionResult> Put(UpdateUserDTO updateUserDTO)
        {
            var user = await userService.GetUser();

            if (user is null) return NotFound();

            user.BirthDate = updateUserDTO.BirthDate;

            await userManager.UpdateAsync(user);

            return NoContent();
        }

        private ActionResult IncorrectLoginError()
        {
            ModelState.AddModelError(string.Empty, "Login Incorrecto");
            return ValidationProblem();
        }
    }
}
