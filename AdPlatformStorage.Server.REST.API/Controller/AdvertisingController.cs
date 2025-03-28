using AdPlatformStorage.Server.Core.Exceptions;
using AdPlatformStorage.Server.REST.API.Models;
using AdPlatformStorage.Server.REST.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Exception = System.Exception;
using Microsoft.AspNetCore.Mvc;

namespace AdPlatformStorage.Server.REST.API.Controller
{
        
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [Route("/api/query")]
    [Produces("application/json")]
    public sealed class AdvertisingController:
        ControllerBase
    {
        
        #region Fields
        
        /// <summary>
        /// 
        /// </summary>
        private readonly ILogger<AdvertisingController> _logger;
        
        /// <summary>
        /// service for copying and saving data in a trie structure
        /// </summary>
        private readonly AddPlatformService _addPlatformService;
        
        #endregion
        
        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="addPlatformService"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public AdvertisingController(
            AddPlatformService addPlatformService,
            ILogger<AdvertisingController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _addPlatformService = addPlatformService ?? throw new ArgumentNullException(nameof(addPlatformService));
        }
        
        #endregion
        
        #region API methods

        /// <summary>
        /// Retrieves advertising platforms based on the provided source.
        /// </summary>
        /// <param name="source">
        /// The source model containing
        ///  public required string Source { get; set; }
        /// </param>
        /// <param name="cancellationToken">Token to cancel the operation if needed.</param>
        /// <returns>
        /// Returns a 200 OK response with the list of advertising platforms if successful.  
        /// Returns a 400 Bad Request if the source is invalid.  
        /// Returns a 500 Internal Server Error in case of unexpected errors.  
        /// </returns>
        /// <exception cref="FormatException">Thrown when the source format is incorrect.</exception>
        /// <exception cref="RestException.NotFoundStorageErrorException">
        /// Thrown when the requested data cannot be found in storage.
        /// </exception>
        [HttpGet("platforms")]
        public async Task<IActionResult> GetAdPlatformAsync(
            [FromBody] AdPlatformModelSource source, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Fetching advertising platforms for source: {Source}",
                    source.Source);
                
                var items = await _addPlatformService
                    .GetAdPlatformAsync(source.Source, cancellationToken);
                
                _logger.LogInformation(
                    "Successfully retrieved {Count} advertising platforms for source: {Source}",
                    items.Sources.Count(),
                    source.Source);
                
                return StatusCode(StatusCodes.Status200OK, items);

            }
            catch (RestException.NotFoundStorageErrorException ex)
            {
                
                _logger.LogError(ex, "Invalid source provided: {Source}", source.Source);
                return BadRequest(
                    new ApiError
                    {
                        ExceptionType= nameof(RestException.NotFoundStorageErrorException),
                        Message = "The source data is missing or invalid.",
                        Details = new Details
                        {
                            MessageException = ex.Message,
                            ErrorContextType = ex.TypeCallContext
                        }
                    });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to fetch advertising platforms");

                return StatusCode(
                    StatusCodes.Status500InternalServerError, 
                    new ApiError
                    {
                        ExceptionType = e.GetType().Name,
                        Message = "Failed to fetch advertising platforms. Retry after 5 seconds.",
                        Details = new Details
                        {
                            MessageException = e.Message,
                            InnerException = e.InnerException,
                            ErrorContextType = nameof(GetAdPlatformAsync)
                        }
                    });
            }
            
           
        }
            
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file">
        /// The input file should follow a structured format, where each line represents a platform  
        /// and its associated locations, separated by colons and commas. <br />
        /// Example file:
        /// <para>
        /// Yandex.Direct : /ru  <br />
        /// Revdinsky rabochy : /ru/svrd/revda, /ru/svrd/pervik <br /> 
        /// Newspaper of the Ural Muscovites : /ru/msk,/ru/permobl,/ru/chelobl  <br />
        /// Cool advertising : /ru/svrd 
        /// </para>
        /// </param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("upload")]
        public async Task<IActionResult> UploadAdPlatformAsync(
            [FromForm] IFormFile? file, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    throw new
                        RestException.NotFoundFileOrNullErrorException(
                            "File is empty or not provided.", 
                            nameof(UploadAdPlatformAsync));
                }
                
                // Optimal batch size for high-performance bulk processing.
                // This value was empirically determined
                // to maximize throughput when handling large datasets (~20+ in size).
                const int size = 1_000_000;
                
                var temp = new List<AdPlatformModel>(size);

                await using (var stream = file.OpenReadStream())
                {
                    using ( var reader = new StreamReader(stream))
                    {
                        
                        var lines = new List<string>();

                        while (await reader.ReadLineAsync(cancellationToken) is { } line)
                        {
                            lines.Add(line);

                            if (lines.Count < size) continue;
                            
                            await WriteInStorageAsync(lines, temp, cancellationToken);
                            lines.Clear();
                        }
                        
                        if (lines.Count > 0)
                        {
                            await WriteInStorageAsync(lines, temp, cancellationToken);
                        }
                        
                    }
                }
                
                _logger.LogInformation("Storage processed successfully.");
                
                return StatusCode(
                    StatusCodes.Status201Created, 
                    "File processed successfully.");
                
            }
            
            catch (RestException.InvalidFormatFileErrorException ex)
            {
                _logger.LogError(ex, "Invalid file format: {Message}", ex.Message);
                
                return BadRequest(new ApiError
                {
                    ExceptionType= nameof(RestException.InvalidFormatFileErrorException),
                    Message = "Invalid file format",
                    Details = new Details
                    {
                        ValidationExample = "Valid format examples:" +
                                            " AdPlatform:/ru/moscow ;" +
                                            " Campaign_123:/ny/summer-sale,/la/fall-offer ;" +
                                            " Test:/lon/price?id=123",
                        MessageException = ex.Message,
                        ErrorContextType = ex.TypeCallContext
                    }
                });
                
            }
            catch (RestException.NotFoundFileOrNullErrorException ex)
            {
                _logger.LogError(ex, "Not found file: {Message}", ex.Message);
                
                return BadRequest(new ApiError
                {
                    ExceptionType= nameof(RestException.NotFoundFileOrNullErrorException),
                    Message = "Not found file",
                    Details = new Details
                    {
                        ValidationExample = "Write something like this:" +
                                            " AdPlatform:/ru/moscow ;" +
                                            " Campaign_123:/ny/summer-sale,/la/fall-offer ;" +
                                            " Test:/lon/price?id=123",
                        MessageException = ex.Message,
                        ErrorContextType = ex.TypeCallContext
                    }
                });
                
            }

            catch (Exception e)
            {
                _logger.LogError(e, "Failed to fetch advertising platforms");

                return StatusCode(
                    StatusCodes.Status500InternalServerError, 
                    new ApiError
                    {
                        ExceptionType = e.GetType().Name,
                        Message = "Failed to fetch advertising platforms. Retry after 5 seconds.",
                        Details = new Details
                        {
                            MessageException = e.Message,
                            InnerException = e.InnerException,
                            ErrorContextType = nameof(UploadAdPlatformAsync)
                        }
                    });
            }
        }
        
        #endregion
        
        #region Private methods
        
      private async Task WriteInStorageAsync(
        List<string> lines, 
        List<AdPlatformModel> temp, 
        CancellationToken cancellationToken = default)
        {
            var tasks = lines.Select(line => Task.Run(() =>
            {
                try
                {
                    
                    if (!line.Contains(':'))
                    {
                        throw new RestException.InvalidFormatFileErrorException(
                            $"Line must contain ':' separator: '{line}'", 
                            nameof(WriteInStorageAsync));
                    }

                    var parts = line.Split(':', StringSplitOptions.TrimEntries);
                    
                    if (parts.Length != 2)
                    {
                        throw new RestException.InvalidFormatFileErrorException(
                            $"Line must contain exactly one ':' separator: '{line}'", 
                            nameof(WriteInStorageAsync));
                    }
                    
                    if (string.IsNullOrWhiteSpace(parts[0]))
                    {
                        throw new RestException.InvalidFormatFileErrorException(
                            "Platform name cannot be empty", 
                            nameof(WriteInStorageAsync));
                    }
                    
                    var pathStrings = parts[1].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    
                    var validPaths = new List<string>();

                    foreach (var path in pathStrings)
                    {
                        if (string.IsNullOrWhiteSpace(path))
                            continue;

                        if (!Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out var uri))
                        {
                            throw new RestException.InvalidFormatFileErrorException(
                                $"Invalid path format: '{path}'", 
                                nameof(WriteInStorageAsync));
                        }
                        
                        if (!uri.IsAbsoluteUri && !path.StartsWith('/'))
                        {
                            throw new RestException.InvalidFormatFileErrorException(
                                $"Relative paths must start with '/': '{path}'", 
                                nameof(WriteInStorageAsync));
                        }

                        validPaths.Add(path);
                    }

                    if (validPaths.Count == 0)
                    {
                        throw new RestException.InvalidFormatFileErrorException(
                            "At least one valid path must be provided ( for example, /ru )", 
                            nameof(WriteInStorageAsync));
                    }

                    lock (temp)
                    {
                        temp.Add(new AdPlatformModel 
                        { 
                            Name = parts[0], 
                            Sources = validPaths 
                        });
                    }
                }
                
                catch (RestException.InvalidFormatFileErrorException)
                {
                    throw; 
                }
                catch (Exception ex)
                {
                    throw new RestException.InvalidFormatFileErrorException(
                        $"Unexpected error processing line: '{line}'. {ex.Message}", 
                        nameof(WriteInStorageAsync));
                }
                
            }, cancellationToken)).ToList();

            await Task.WhenAll(tasks);
            
            if (temp.Count == 0)
            {
                throw new RestException.InvalidFormatFileErrorException(
                    "No valid data to save", 
                    nameof(WriteInStorageAsync));
            }

            await _addPlatformService.SaveAdPlatformAsync(temp, cancellationToken);
        }
      
        #endregion
        
    }
}
