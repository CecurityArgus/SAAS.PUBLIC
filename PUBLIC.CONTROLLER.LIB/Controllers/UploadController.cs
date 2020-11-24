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
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        private readonly ApiKeys _apiKeys;

        private readonly IRabbitMqPersistentConnection _rabbitMqPersistentConnection;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="logger"></param>
        /// <param name="apiKeys"></param>
        /// <param name="rabbitMqPersistentConnection"></param>
        public UploadController(IConfiguration config, ILogger<AuthenticationController> logger, ApiKeys apiKeys, IRabbitMqPersistentConnection rabbitMqPersistentConnection)
        {
            _config = config;
            _logger = logger;
            _apiKeys = apiKeys;

            _rabbitMqPersistentConnection = rabbitMqPersistentConnection;
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
        public virtual IActionResult BeginOfTransfer(RegisterBeginOfTransfer body)
        {
            var transferId = Guid.NewGuid().ToString();

            try
            {
                var requestApiKey = Request.Headers["X-TransferId"].ToString();

                _logger.LogInformation($"UploadController/BeginOfTransfer: Started. TransferId = {transferId}");

                var uploadService = new UploadService(_config, _logger, _apiKeys, Request.Headers["Authorization"]);
                var response = uploadService.BeginOfTransfer(requestApiKey, transferId, body);

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
        public IActionResult UploadFiles()
        {
            string transferId = Request.Headers["X-TransferId"].ToString();

            _logger.LogInformation($"UploadController/UploadFiles: Started. TransferId = {transferId}");

            try
            {
                var uploadedFiles = HttpContext.Request.Form.Files.ToList();

                var uploadService = new UploadService(_config, _logger, _apiKeys, Request.Headers["Authorization"]);
                uploadService.UploadFiles(transferId, uploadedFiles);                

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
        public IActionResult EndOfTransfer()
        {
            string transferId = Request.Headers["X-TransferId"].ToString();

            _logger.LogInformation($"UploadController/EndOfTransfer: Started. TransferId = {transferId}");

            try
            {                
                var uploadService = new UploadService(_config, _logger, _apiKeys, Request.Headers["Authorization"], User, _rabbitMqPersistentConnection);
                uploadService.EndOfTransfer(transferId);

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
        public IActionResult AbortTransfer()
        {
            string transferId = Request.Headers["X-TransferId"].ToString();
            
            _logger.LogInformation($"UploadController/AbortTransfer: Started. TransferId = {transferId}");

            try
            {
                var uploadService = new UploadService(_config, _logger, _apiKeys, Request.Headers["Authorization"]);
                uploadService.AbortTransfer(transferId);

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
