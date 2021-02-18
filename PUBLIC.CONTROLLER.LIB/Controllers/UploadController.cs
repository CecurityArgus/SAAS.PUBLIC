using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MQDispatch.Client;
using Platform.Framework;
using PUBLIC.CONTROLLER.LIB.Security;
using PUBLIC.SERVICE.LIB.Helpers;
using PUBLIC.SERVICE.LIB.Models;
using PUBLIC.SERVICE.LIB.Services;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using static PUBLIC.SERVICE.LIB.Helpers.MQErrors;

namespace PUBLIC.CONTROLLER.LIB.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("Public/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly UploadService _uploadService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="uploadService"></param>
        public UploadController(ILogger<AuthenticationController> logger, UploadService uploadService)
        {
            _logger = logger;
            _uploadService = uploadService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = BearerAuthenticationHandler.SchemeName)]
        [HttpPost("BeginOfTransfer")]
        [ProducesResponseType(200, Type = typeof(TransferRegistered))]
        [ProducesResponseType(401, Type = typeof(string))]
        [ProducesResponseType(500, Type = typeof(CecurityException))]
        public async Task<IActionResult> BeginOfTransfer(RegisterBeginOfTransfer body)
        {
            var transferId = Guid.NewGuid().ToString();

            try
            {
                _logger.LogInformation($"UploadController/BeginOfTransfer: Started. TransferId = {transferId}");

                var response = await _uploadService.BeginOfTransferAsync(transferId, body);

                _logger.LogInformation($"UploadController/BeginOfTransfer: Finished. TransferId = {transferId}");

                return Ok(response);
            }
            catch (CecurityException exception)
            {
                _logger.LogError($"UploadController/BeginOfTransfer: Error: {exception.Message}. TransferId = {transferId}");

                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError($"UploadController/BeginOfTransfer: Error: {exception.Message}. TransferId = {transferId}");

                throw new CecurityException((int)MQMessages.APP_ERR_BEGIN_OF_TRANSFER, exception.Message, null);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = BearerAuthenticationHandler.SchemeName)]
        [HttpPost("UploadFiles")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401, Type = typeof(string))]
        [ProducesResponseType(413)]
        [ProducesResponseType(500, Type = typeof(CecurityException))]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> UploadFiles()
        {
            string transferId = Request.Headers["X-TransferId"].ToString();

            _logger.LogInformation($"UploadController/UploadFiles: Started. TransferId = {transferId}");

            try
            {
                var uploadedFiles = HttpContext.Request.Form.Files.ToList();
                await _uploadService.UploadFilesAsync(transferId, uploadedFiles);                

                _logger.LogInformation($"UploadController/UploadFiles: Finished. TransferId = {transferId}");

                return Ok();
            }
            catch (CecurityException exception)
            {
                _logger.LogError($"UploadController/Uploadfiles: Error: {exception.Message}. TransferId = {transferId}");

                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError($"UploadController/Uploadfiles: Error: {exception.Message}. TransferId = {transferId}");

                throw new CecurityException((int)MQMessages.APP_ERR_UPLOAD, exception.Message, null);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = BearerAuthenticationHandler.SchemeName)]
        [HttpPost("EndOfTransfer")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401, Type = typeof(string))]
        [ProducesResponseType(500, Type = typeof(CecurityException))]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> EndOfTransfer()
        {
            string transferId = Request.Headers["X-TransferId"].ToString();

            _logger.LogInformation($"UploadController/EndOfTransfer: Started. TransferId = {transferId}");

            try
            {                
                await _uploadService.EndOfTransferAsync(transferId);

                return Ok();
            }
            catch (CecurityException exception)
            {
                _logger.LogError($"UploadController/EndOfTransfer: Error: {exception.Message}. TransferId = {transferId}");

                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError($"UploadController/EndOfTransfer: Error: {exception.Message}. TransferId = {transferId}");

                throw new CecurityException((int)MQMessages.APP_ERR_UPLOAD, exception.Message, null);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = BearerAuthenticationHandler.SchemeName)]
        [HttpPost("AbortTransfer")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401, Type = typeof(string))]
        [ProducesResponseType(500, Type = typeof(CecurityException))]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> AbortTransfer()
        {
            string transferId = Request.Headers["X-TransferId"].ToString();
            
            _logger.LogInformation($"UploadController/AbortTransfer: Started. TransferId = {transferId}");

            try
            {
                await _uploadService.AbortTransferAsync(transferId);

                return Ok();
            }
            catch (CecurityException exception)
            {
                _logger.LogError($"UploadController/AbortTransfer: Error: {exception.Message}. TransferId = {transferId}");

                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError($"UploadController/AbortTransfer: Error: {exception.Message}. TransferId = {transferId}");

                throw new CecurityException((int)MQMessages.APP_ERR_ABORT_TRANSFER, exception.Message, null);
            }
        }
    }
}
