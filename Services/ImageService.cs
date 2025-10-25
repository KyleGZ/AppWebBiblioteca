using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace AppWebBiblioteca.Services
{
    public interface IImageService
    {
        Task<string> GuardarPortadaAsync(IFormFile imagenArchivo, string isbn = null);
        bool ValidarImagen(IFormFile imagenArchivo);
    }

    public class ImageService : IImageService
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ImageService> _logger;

        public ImageService(IConfiguration configuration, IWebHostEnvironment environment, ILogger<ImageService> logger)
        {
            _configuration = configuration;
            _environment = environment;
            _logger = logger;
        }

        public bool ValidarImagen(IFormFile imagenArchivo)
        {
            if (imagenArchivo == null || imagenArchivo.Length == 0)
                return false;

            // Validar tamaño (5MB máximo)
            var maxFileSize = _configuration.GetValue<long>("ImageSettings:MaxFileSize", 5 * 1024 * 1024);
            if (imagenArchivo.Length > maxFileSize)
            {
                _logger.LogWarning("Imagen demasiado grande: {Tamaño} bytes", imagenArchivo.Length);
                return false;
            }

            // Validar extensión
            var allowedExtensions = _configuration.GetSection("ImageSettings:AllowedExtensions").Get<string[]>()
                ?? new[] { ".jpg", ".jpeg", ".png", ".gif" };

            var fileExtension = Path.GetExtension(imagenArchivo.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
            {
                _logger.LogWarning("Extensión no permitida: {Extension}", fileExtension);
                return false;
            }

            return true;
        }

        public async Task<string> GuardarPortadaAsync(IFormFile imagenArchivo, string isbn = null)
        {
            if (!ValidarImagen(imagenArchivo))
                throw new ArgumentException("La imagen no es válida");

            try
            {
                // Generar nombre único usando ISBN + timestamp
                var fileName = GenerarNombreArchivo(isbn);
                var fullPath = Path.Combine(_environment.WebRootPath, "imagenes", "portadas", fileName);

                // Crear directorio si no existe
                var directory = Path.GetDirectoryName(fullPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Procesar y guardar la imagen
                await ProcesarYGuardarImagenAsync(imagenArchivo, fullPath);

                _logger.LogInformation("Imagen guardada exitosamente: {FileName}", fileName);
                return fileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar la imagen");
                throw new Exception("Error al procesar la imagen: " + ex.Message);
            }
        }

        private string GenerarNombreArchivo(string isbn)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var baseName = string.IsNullOrEmpty(isbn) ? "portada" : LimpiarISBN(isbn);
            return $"{baseName}_{timestamp}.webp";
        }

        private string LimpiarISBN(string isbn)
        {
            // Remover caracteres especiales del ISBN para nombre de archivo seguro
            return new string(isbn.Where(c => char.IsLetterOrDigit(c)).ToArray());
        }

        private async Task ProcesarYGuardarImagenAsync(IFormFile imagenArchivo, string outputPath)
        {
            using var imageStream = imagenArchivo.OpenReadStream();
            using var image = await Image.LoadAsync(imageStream);

            // Redimensionar manteniendo proporciones (máximo 400x600)
            var resizeOptions = new ResizeOptions
            {
                Size = new Size(400, 600),
                Mode = ResizeMode.Max
            };

            image.Mutate(x => x.Resize(resizeOptions));

            // Configurar compresión WebP (calidad 80%)
            var webpEncoder = new WebpEncoder
            {
                Quality = 80,
                Method = WebpEncodingMethod.Default
            };

            // Guardar imagen procesada
            await image.SaveAsync(outputPath, webpEncoder);
        }
    }
}