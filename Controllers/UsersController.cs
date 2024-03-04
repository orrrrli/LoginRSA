using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Context;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        public int p = 113;
        public int q = 127;
        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<User>> GetUser(int userId, string password)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                // El usuario no existe en la base de datos
                return NotFound();
            }

            // Desencriptar la contraseña almacenada
            string decryptedPassword = decryptPassword(user.PasswordEncrypted, user.d, p, q);

            // Comparar la contraseña desencriptada con la contraseña proporcionada
            if (password == decryptedPassword)
            {
                // La contraseña es correcta
                return Ok(user);
            }
            else
            {
                // La contraseña es incorrecta
                return Unauthorized();
            }
        }


        static string BigIntegerToString(BigInteger bigInteger)
        {
            return bigInteger.ToString();
        }





        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            if (_context.Users == null)
            {
                return Problem("Entity set 'AppDbContext.Users' is null.");
            }

            // Cifrar la contraseña antes de almacenarla
            string encryptedPassword = encryptPassword(user.PasswordEncrypted, p, q);
            user.PasswordEncrypted = encryptedPassword;

            // Calcular d
            BigInteger d = CalculateD(17, PhiFunction(p, q));

            // Asignar d al usuario
            user.d = (int)d;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.IdUser }, user);
        }



        static string encryptPassword(string password, BigInteger p, BigInteger q)
        {
            // Inicializar un StringBuilder para almacenar el mensaje cifrado
            StringBuilder encryptedStringBuilder = new StringBuilder();

            // Calcular n multiplicando p y q
            BigInteger n = p * q;
            BigInteger e = 17;

            foreach (char character in password)
            {
                // Convertir cada carácter en su valor ASCII
                int asciiValue = (int)character;

                // Cifrar el mensaje utilizando la fórmula RSA
                BigInteger encryptedMessage = BigInteger.ModPow(new BigInteger(asciiValue), e, n);

                // Convertir cada mensaje cifrado a una cadena y agregarlo al StringBuilder
                encryptedStringBuilder.Append(encryptedMessage.ToString());
                encryptedStringBuilder.Append(" "); // Agregar un espacio entre cada cifrado para separarlos
            }

            // Retornar el mensaje cifrado como una sola cadena
            return encryptedStringBuilder.ToString().Trim(); // Eliminar espacios en blanco adicionales al final
        }



        static string decryptPassword(string encryptedPassword, BigInteger d, BigInteger p, BigInteger q)
        {
            StringBuilder decryptedStringBuilder = new StringBuilder();

            // Calcular n multiplicando p y q
            BigInteger n = p * q;

            // Dividir la cadena cifrada en números separados por espacios
            string[] encryptedNumbers = encryptedPassword.Split(' ');

            foreach (string encryptedNumber in encryptedNumbers)
            {
                // Convertir el número cifrado a BigInteger
                BigInteger encryptedLetter = BigInteger.Parse(encryptedNumber);

                // Descifrar el número utilizando la clave privada d
                BigInteger decryptedValue = BigInteger.ModPow(encryptedLetter, d, n);

                // Convertir el resultado a un carácter ASCII y agregarlo al StringBuilder
                char decryptedLetter = (char)decryptedValue;
                decryptedStringBuilder.Append(decryptedLetter);
            }

            // Retornar el mensaje descifrado como una cadena
            return decryptedStringBuilder.ToString();
        }





        static BigInteger PhiFunction(BigInteger p, BigInteger q)
        {
            return (p - 1) * (q - 1);
        }


        static BigInteger CalculateD(BigInteger a, BigInteger m)
        {
            BigInteger m0 = m;
            BigInteger y = 0;
            BigInteger x = 1;

            if (m == 1)
                return 0;

            while (a > 1)
            {
                BigInteger q = a / m;
                BigInteger t = m;

                m = a % m;
                a = t;
                t = y;

                y = x - q * y;
                x = t;
            }

            if (x < 0)
                x += m0;

            return x;
        }

        

        static BigInteger StringToBigInteger(string s)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(s);
            Array.Reverse(bytes); // Reverse the byte array to avoid big-endian issue
            return new BigInteger(bytes);
        }


    // DELETE: api/Users/5
    [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (_context.Users == null)
            {
                return NotFound();
            }
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return (_context.Users?.Any(e => e.IdUser == id)).GetValueOrDefault();
        }
    }
}
