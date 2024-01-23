using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApiAutores.DTOs;
using WebApiAutores.Respuestas;
using WebApiAutores.Servicios;

namespace WebApiAutores.Controllers.V1
{
    [Route("api/V1/Cuentas")]
    [ApiController]
    public class CuentasController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly HashService _hashService;
        private readonly IDataProtector _dataProtector;

        public CuentasController(UserManager<IdentityUser> userManager, IConfiguration configuration,
            SignInManager<IdentityUser> signInManager, IDataProtectionProvider dataProtectionProvider,
            HashService hashService)
        {
            _userManager = userManager;
            _configuration = configuration;
            _signInManager = signInManager;
            _hashService = hashService;
            _dataProtector = dataProtectionProvider.CreateProtector("Valor_unico_y_quizas_secreto");  //Para Encriptado
        }

        [HttpGet("hash/{textoPlano}")]
        public IActionResult RealizarHash(string textoPlano) 
        { 
            var resultado1 = _hashService.Hash(textoPlano);
            var resultado2 = _hashService.Hash(textoPlano);
            return Ok(new 
            { 
                textoPlano = textoPlano,
                Hash1 = resultado1,
                Hash2 = resultado2
            });
        }


        [HttpGet("Encriptar")]
        public ActionResult Encriptar()
        {
            var textPlano = "Hebert Merchan";
            var textCifrado = _dataProtector.Protect(textPlano);
            var textoDesCifrado = _dataProtector.Unprotect(textCifrado);

            return Ok(new
            {
                textoPlano = textPlano,
                textocifrado = textCifrado,
                textoDescifrado = textoDesCifrado
            });
        }

        [HttpGet("EncriptarPorTiempo")]
        public ActionResult EncriptarPorTiempo()
        {
            var protectorLimitgadoPorTiempo = _dataProtector.ToTimeLimitedDataProtector();

            var textPlano = "Hebert Merchan";
            var textCifrado = protectorLimitgadoPorTiempo.Protect(textPlano, lifetime: TimeSpan.FromSeconds(5));
            Thread.Sleep(6000); //Esperamos 6 seg para que ya no pueda desencriptar el dato
            var textoDesCifrado = protectorLimitgadoPorTiempo.Unprotect(textCifrado);

            return Ok(new
            {
                textoPlano = textPlano,
                textocifrado = textCifrado,
                textoDescifrado = textoDesCifrado
            });
        }


        [HttpPost("registrar", Name = "registrarUsuario")] //para registrar clientes
        public async Task<ActionResult<RespuestaAutenticacion>> Registrar(CredencialesUsuarios credencialesUsuarios)
        {
            var usuario = new IdentityUser
            {
                UserName = credencialesUsuarios.Email,
                Email = credencialesUsuarios.Email
            };

            var resultado = await _userManager.CreateAsync(usuario, credencialesUsuarios.Password);
            if (resultado.Succeeded)
            {
                return await ConstruirToken(credencialesUsuarios);
            }
            else
            {
                return BadRequest(resultado.Errors);
            }
        }

        [HttpPost("Login", Name = "loginUsuario")]
        public async Task<ActionResult<RespuestaAutenticacion>> Login(CredencialesUsuarios credencialesUsuarios)
        {
            var resultado = await _signInManager.PasswordSignInAsync(credencialesUsuarios.Email, credencialesUsuarios.Password,
                isPersistent: false, lockoutOnFailure: false);
            if (resultado.Succeeded)
            {
                return await ConstruirToken(credencialesUsuarios);
            }
            else
            {
                return BadRequest("Login Incorrecto");
            }
        }

        [HttpGet("RenovarToken", Name ="renovarToken")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<RespuestaAutenticacion>> Renovar()
        {
            //Esto para obtener el valor que viene en el Claims cuando llega el Token
            var emailClaim = HttpContext.User.Claims.Where(x => x.Type == "email").FirstOrDefault();
            var email = emailClaim.Value;

            var credencialesUsuario = new CredencialesUsuarios()
            {
                Email = email,
            };

            return await ConstruirToken(credencialesUsuario);
        }

        private async Task<ActionResult<RespuestaAutenticacion>> ConstruirToken(CredencialesUsuarios credencialesUsuarios)
        {
            var Claims = new List<Claim>()
            {
                new Claim("email", credencialesUsuarios.Email),
                new Claim("Lo que yo quiera", "Cualquier cosa")
            };

            //Esto es para agregar al Token los Policy que pueda tener el Usuario.
            var usuario = await _userManager.FindByEmailAsync(credencialesUsuarios.Email);
            var ClaimDB = await _userManager.GetClaimsAsync(usuario);

            Claims.AddRange(ClaimDB);
            //...

            var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["llavejwt"]));
            var creds = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddYears(1);

            var securityToken = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: Claims,
                expires: expiration,
                signingCredentials: creds);

            var Token = new RespuestaAutenticacion()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(securityToken),
                Expiracion = expiration
            };

            return Token;
        }

        [HttpPost("HecerAdmin", Name = "hacerAdmin")]
        public async Task<ActionResult> HacerAdmin(EditarAdminDTO editarAdminDTO)
        {
            var usuario = await _userManager.FindByEmailAsync(editarAdminDTO.Email);
            await _userManager.AddClaimAsync(usuario, new Claim("EsAdmin", "1"));

            return NoContent();
        }

        [HttpPost("RemoverAdmin", Name = "removerAdmin")]
        public async Task<ActionResult> RemoverAdmin(EditarAdminDTO editarAdminDTO)
        {
            var usuario = await _userManager.FindByEmailAsync(editarAdminDTO.Email);
            await _userManager.RemoveClaimAsync(usuario, new Claim("EsAdmin", "1"));

            return NoContent();
        }
    }
}
