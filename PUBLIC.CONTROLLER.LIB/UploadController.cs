using EPAIE.REPO.LIB;
using EPAIE.REPO.LIB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PUBLIC.CONTROLLER.Helpers;
using PUBLIC.CONTROLLER.LIB.DTOs;
using PUBLIC.CONTROLLER.LIB.Helpers;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;

namespace PUBLIC.CONTROLLER.LIB.Controllers
{
    [Route("Public/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        public IEPaieRepositoryWrapper _epaieRepo;
        private readonly string _platformRestApiUrl;

        public UploadController(IConfiguration config, ILogger<AuthenticationController> logger, IEPaieRepositoryWrapper epaieRepo)
        {
            _config = config;
            _logger = logger;

            _epaieRepo = epaieRepo;

            _platformRestApiUrl = config["AppSettings:PlatformRestApiUrl"];
        }

        #region CONTROLLERS

        [Authorize]
        [HttpPost("BeginOfTransfer")]
        [ProducesResponseType(200, Type = typeof(DtoUpload.TransferRegistered))]
        [ProducesResponseType(401, Type = typeof(string))]
        [ProducesResponseType(500, Type = typeof(CecurityError))]
        public IActionResult BeginOfTransfer(DtoUpload.RegisterBeginOfTransfer dtoRegisterBeginOfTransfer)
        {            
            string transferId = Guid.NewGuid().ToString();

            _logger.LogInformation($"UploadController/BeginOfTransfer: Started. TransferId = {transferId}");

            try
            {
                //Check if all parameters are given and if there are files to upload
                if (string.IsNullOrWhiteSpace(dtoRegisterBeginOfTransfer.SolutionName))
                    throw new CecurityException("PUBLIC_API_00051", "Solution name cannot be empty !");

                if (string.IsNullOrWhiteSpace(dtoRegisterBeginOfTransfer.SolutionReference))
                    throw new CecurityException("PUBLIC_API_00052", "Solution reference cannot be empty !");

                if (dtoRegisterBeginOfTransfer.UploadFiles.Count == 0)
                    throw new CecurityException("PUBLIC_API_00053", "No files provided for the upload !");

                //Get authorization token
                string currentToken = Request.Headers["Authorization"];
                currentToken = currentToken.Replace("Bearer ", "");
                var currentTokenObj = new JwtSecurityTokenHandler().ReadJwtToken(currentToken);
                var idmAuthToken = currentTokenObj.Claims.Where(c => c.Type == "IDMAuthToken").FirstOrDefault();

                if (string.IsNullOrWhiteSpace(idmAuthToken.Value))
                    return Unauthorized();

                //Get authorized subscriptions for user
                PlatformRestApi platformRestApi = new PlatformRestApi(_platformRestApiUrl, "Bearer " + idmAuthToken.Value);
                var subscriptions = platformRestApi.GetAuthorizedSubscriptions();

                if (subscriptions.Count == 0)
                    throw new CecurityException("PUBLIC_API_00054", "No authorized subscriptions for user !");

                bool subscriptionNameValid = false;
                bool subscriptionReferenceValid = false;
                DtoUpload.TransferObject transferObject = null;
                //Check if subscription and reference are authorized for the logged on user
                foreach (var subscription in subscriptions)
                {
                    if (subscription.Solution.SolutionName.ToLower().Equals(dtoRegisterBeginOfTransfer.SolutionName.ToLower()))
                    {
                        subscriptionNameValid = true;

                        var solutionParams = JsonConvert.DeserializeObject<PlatformRestApi.MdlSolutionParams>(subscription.SolutionParams);
                        if (!string.IsNullOrEmpty(solutionParams.Solution.Params))
                        {
                            if (dtoRegisterBeginOfTransfer.SolutionName.ToLower().Equals("epaie"))
                            {
                                var subscriptionParams = JsonConvert.DeserializeObject<EPaieParams>(solutionParams.Solution.Params);
                                subscriptionReferenceValid = subscriptionParams.SIREN.ToLower().Equals(dtoRegisterBeginOfTransfer.SolutionReference.ToLower());

                                if (subscriptionReferenceValid && subscriptionNameValid)
                                {
                                    transferObject = new DtoUpload.TransferObject
                                    {
                                        SolutionName = dtoRegisterBeginOfTransfer.SolutionName,
                                        SolutionReference = dtoRegisterBeginOfTransfer.SolutionReference,
                                        UploadFiles = dtoRegisterBeginOfTransfer.UploadFiles,
                                        SubscriptionId = subscription.Id
                                    };
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        subscriptionNameValid = false;
                    }
                }

                if (!subscriptionNameValid)
                    throw new CecurityException("PUBLIC_API_00055", $"Solution with name '{dtoRegisterBeginOfTransfer.SolutionName}' not found or not authorized");
                if (!subscriptionReferenceValid)
                    throw new CecurityException("PUBLIC_API_00056", $"Subscription with reference '{dtoRegisterBeginOfTransfer.SolutionReference}' and Solution '{dtoRegisterBeginOfTransfer.SolutionName}' not found or not authorized");

                //Create upload folder for current transferId
                var uploadFolder = _config["Appsettings:UploadFolder"];
                string transferFolder = Path.Combine(uploadFolder, transferId);
                if (!Directory.Exists(transferFolder))
                    Directory.CreateDirectory(transferFolder);
                try
                {
                    string transferFile = Path.Combine(transferFolder, $"transfer.json");
                    System.IO.File.WriteAllText(transferFile, JsonConvert.SerializeObject(transferObject));
                }
                catch (Exception)
                {
                    if (Directory.Exists(transferFolder))
                        Directory.Delete(transferFolder, true);

                    throw;
                }

                _logger.LogInformation($"UploadController/BeginOfTransfer: Finished. TransferId = {transferId}");

                return Ok(new DtoUpload.TransferRegistered() { TransferId = transferId });
            }
            catch (CecurityException exception)
            {
                _logger.LogError($"UploadController/BeginOfTransfer: Error: {exception.Message}. TransferId = {transferId}");

                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError($"UploadController/BeginOfTransfer: Error: {exception.Message}. TransferId = {transferId}");

                throw new CecurityException("PUBLIC_API_00050", exception.Message);
            }
        }

        [Authorize]
        [HttpPost("UploadFiles")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(413)]
        [ProducesResponseType(500, Type = typeof(CecurityError))]
        [DisableRequestSizeLimit]
        public IActionResult UploadFiles()
        {
            string transferId = Request.Headers["X-TransferId"].ToString();
            if (string.IsNullOrWhiteSpace(transferId))
                throw new CecurityException("PUBLIC_API_00001", "Invalid or no transfer id provided !");

            _logger.LogInformation($"UploadController/UploadFiles: Started. TransferId = {transferId}");

            try
            {
                var uploadedFiles = HttpContext.Request.Form.Files;
                if (uploadedFiles.Count() == 0)
                    throw new CecurityException("PUBLIC_API_00101", "No files provided for the upload !");

                //Get uploadfolder for current transfer
                var uploadFolder = _config["Appsettings:UploadFolder"];
                string transferFolder = Path.Combine(uploadFolder, transferId);
                if (!System.IO.Directory.Exists(transferFolder))
                    throw new CecurityException("PUBLIC_API_00002", $"No transfer upload folder found for transfer '{transferId}' !", new { TransferId = transferId });
                //Read current transfer file
                string transferFile = Path.Combine(transferFolder, $"transfer.json");
                if (!System.IO.File.Exists(transferFile))
                    throw new CecurityException("PUBLIC_API_00003", $"No transfer configuration file found for transfer '{transferId}' !", new { TransferId = transferId });
                var transferObject = JsonConvert.DeserializeObject<DtoUpload.TransferObject>(System.IO.File.ReadAllText(transferFile));

                foreach (var uploadedFile in uploadedFiles)
                {
                    var uploadedFileName = ContentDispositionHeaderValue.Parse(uploadedFile.ContentDisposition).FileName.Trim('"');
                    string uploadedFileFullPath = Path.Combine(transferFolder, uploadedFileName);

                    if (!System.IO.File.Exists(uploadedFileFullPath))
                    {

                        if (uploadedFile.Length == 0)
                            throw new CecurityException("PUBLIC_API_00102", $"Uploaded file '{uploadedFileName}' cannot have a null length", new { UploadedFile = uploadedFileName });

                        if (!transferObject.UploadFiles.Any(f => f.FileName.ToLower().Equals(uploadedFileName.ToLower())))
                            throw new CecurityException("PUBLIC_API_00103", $"Unknown file provided: '{uploadedFileName}'", new { UploadedFile = uploadedFileName });
                        else
                        {
                            var transferObjectFile = transferObject.UploadFiles.Where(f => f.FileName.ToLower().Equals(uploadedFileName.ToLower())).First();
                            if (uploadedFile.Length != transferObjectFile.FileSize)
                                throw new CecurityException("PUBLIC_API_00104", $"Filesizes do not match. File: '{uploadedFileName}' - uploaded filesize: '{uploadedFile.Length}' - excpeected filesize: '{transferObjectFile.FileSize}'", new { UploadedFile = uploadedFileName, UploadedFileSize = uploadedFile.Length, ExcepectedFileSize = transferObjectFile.FileSize });

                            // If all check passes we can copy the uploaded file
                            using (var stream = new FileStream(uploadedFileFullPath, FileMode.Create))
                            {
                                uploadedFile.CopyTo(stream);

                                _logger.LogInformation($"UploadController/UploadFiles: File received '{uploadedFileName}'. TransferId = {transferId}");
                            }

                            //See if we have to check the file fingerprint
                            if (!string.IsNullOrWhiteSpace(transferObjectFile.FingerPrint) && !string.IsNullOrWhiteSpace(transferObjectFile.FingerPrintAlgorithm))
                            {
                                if (!CheckFileFingerprint(transferObjectFile.FingerPrintAlgorithm, transferObjectFile.FingerPrint, uploadedFileFullPath))
                                {
                                    //Fingerprints don't match so delete the uploaded file
                                    if (System.IO.File.Exists(uploadedFileFullPath))
                                        System.IO.File.Delete(uploadedFileFullPath);

                                    throw new CecurityException("PUBLIC_API_00105", $"File fingerprint check failed for file '{uploadedFileName}'", new { UploadedFile = uploadedFileName });
                                }
                            }
                        }
                    }
                    else
                        _logger.LogInformation($"UploadController/UploadFiles: File '{uploadedFileName}' already uploaded, ignoring file. TransferId = {transferId}");
                }

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

                throw new CecurityException("PUBLIC_API_00100", exception.Message);
            }
        }

        [Authorize]
        [HttpPost("EndOfTransfer")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500, Type = typeof(CecurityError))]
        [DisableRequestSizeLimit]
        public IActionResult EndOfTransfer()
        {
            string transferId = Request.Headers["X-TransferId"].ToString();
            if (string.IsNullOrWhiteSpace(transferId))
                throw new CecurityException("PUBLIC_API_00001", "Invalid or no transfer id provided !");

            _logger.LogInformation($"UploadController/EndOfTransfer: Started. TransferId = {transferId}");

            try
            {
                //Get uploadfolder for current transfer
                var uploadFolder = _config["Appsettings:UploadFolder"];
                string transferFolder = Path.Combine(uploadFolder, transferId);
                if (!System.IO.Directory.Exists(transferFolder))
                    throw new CecurityException("PUBLIC_API_00002", $"No transfer upload folder found for transfer '{transferId}' !", new { TransferId = transferId });
                //Read current transfer file
                string transferFile = Path.Combine(transferFolder, $"transfer.json");
                if (!System.IO.File.Exists(transferFile))
                    throw new CecurityException("PUBLIC_API_00003", $"No transfer configuration file found for transfer '{transferId}' !", new { TransferId = transferId });
                var transferObject = JsonConvert.DeserializeObject<DtoUpload.TransferObject>(System.IO.File.ReadAllText(transferFile));

                //Check if all files have been uploaded
                List<string> filesNotUploaded = new List<string>();
                foreach (var transferFileObject in transferObject.UploadFiles)
                {
                    var fileToCheck = Path.Combine(transferFolder, transferFileObject.FileName);
                    if (!System.IO.File.Exists(fileToCheck))
                        filesNotUploaded.Add(transferFileObject.FileName);
                }
                if (filesNotUploaded.Count > 0)
                    throw new CecurityException("PUBLIC_API_00151", $"Not all files have been uploaded ({string.Join(", ", filesNotUploaded)})", new { FilesNotUploaded = filesNotUploaded });

                //Get authorization token
                string currentToken = Request.Headers["Authorization"];
                currentToken = currentToken.Replace("Bearer ", "");
                var currentTokenObj = new JwtSecurityTokenHandler().ReadJwtToken(currentToken);
                var idmAuthToken = currentTokenObj.Claims.Where(c => c.Type == "IDMAuthToken").FirstOrDefault();

                if (string.IsNullOrWhiteSpace(idmAuthToken.Value))
                    return Unauthorized();

                if (transferObject.SolutionName.ToLower().Equals("epaie"))
                {
                    try
                    {
                        RegisterEPaieUpload(transferId, transferObject.SubscriptionId, transferObject.UploadFiles, idmAuthToken.Value);
                    }
                    catch (CecurityException exception)
                    {
                        throw new CecurityException("PUBLIC_API_00153", $"Register ePaie job error: {exception.Message}", exception.AdditionalInfo);
                    }
                    catch (Exception exception)
                    {
                        throw new CecurityException("PUBLIC_API_00153", $"Register ePaie job error: {exception.Message}");
                    }
                }
                else
                    throw new CecurityException("PUBLIC_API_00152", $"Upload for solution '{transferObject.SolutionName}' not yet implemented", new { transferObject.SolutionName });

                //Cleanup transfer folder
                if (Directory.Exists(transferFolder))
                    Directory.Delete(transferFolder, true);

                _logger.LogInformation($"UploadController/EndOfTransfer: Finished. TransferId = {transferId}");

                return Ok();
            }
            catch (CecurityException exception)
            {
                _logger.LogError($"UploadController/BeginOfTransfer: Error: {exception.Message}. TransferId = {transferId}");

                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError($"UploadController/BeginOfTransfer: Error: {exception.Message}. TransferId = {transferId}");

                throw new CecurityException("PUBLIC_API_00150", exception.Message);
            }
        }

        [Authorize]
        [HttpPost("AbortTransfer")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500, Type = typeof(CecurityError))]
        [DisableRequestSizeLimit]
        public IActionResult AbortTransfer()
        {
            string transferId = Request.Headers["X-TransferId"].ToString();
            if (string.IsNullOrWhiteSpace(transferId))
                throw new CecurityException("PUBLIC_API_00001", "Invalid or no transfer id provided !", null);

            _logger.LogInformation($"UploadController/AbortTransfer: Started. TransferId = {transferId}");

            try
            {
                //Get uploadfolder for current transfer
                var uploadFolder = _config["Appsettings:UploadFolder"];
                string transferFolder = Path.Combine(uploadFolder, transferId);
                //Cleanup the folder 
                if (Directory.Exists(transferFolder))
                    Directory.Delete(transferFolder, true);

                _logger.LogInformation($"UploadController/AbortTransfer: Finished. TransferId = {transferId}");

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

                throw new CecurityException("PUBLIC_API_00200", exception.Message);
            }
        }

        #endregion

        #region EPAIE
        public class EPaieParams
        {
            public string Entreprise { get; set; }
            public string SIRET { get; set; }
            public string SIREN { get; set; }
            public string Logiciel { get; set; }
            public string CodeClient { get; set; }
            public string CodeAgence { get; set; }
            public string CodeClientInterne { get; set; }

        }

        private void RegisterEPaieUpload(string transferId, string subscriptionId, List<DtoUpload.FileWithFingerPrintInfo> uploadedFiles, string idmAuthToken)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var providerId = User.FindFirst(ClaimTypes.PrimarySid).Value.Split('\\')[0];
            var tenantId = User.FindFirst(ClaimTypes.PrimarySid).Value.Split('\\')[1];
            var loginName = User.FindFirst(ClaimTypes.Name).Value;
            var displayName = User.FindFirst(ClaimTypes.UserData).Value;

            Upload upload = _epaieRepo.Uploads.FindByCondition(a => a.TransferId.Equals(transferId)).FirstOrDefault();
            if (upload == null)
            {
                upload = new Upload()
                {
                    TransferId = transferId,
                    TimeStamp = DateTime.Now,
                    SubscriptionId = subscriptionId,
                    LoginName = loginName,
                    FullName = displayName,
                    LastActionTimeStamp = DateTime.Now,
                    UserId = userId,
                    ProviderId = providerId,
                    TenantId = tenantId
                };

                _epaieRepo.Uploads.Create(upload);
                _epaieRepo.Uploads.Save();
            }

            var registeredUploadedFiles = new List<UploadedFile>();
            //Register each file
            foreach (var fileInfo in uploadedFiles)
            {
                UploadedFile uploadedFile = _epaieRepo.UploadedFiles.FindByCondition(a => a.UploadId.Equals(upload.Id) && a.OriginalFileName.Equals(fileInfo.FileName)).FirstOrDefault();
                if (uploadedFile == null)
                {
                    uploadedFile = new UploadedFile
                    {
                        Id = Guid.NewGuid().ToString(),
                        UploadId = upload.Id,
                        FileSize = 0,
                        FingerPrint = fileInfo.FingerPrint,
                        FingerPrintAlgorithm = fileInfo.FingerPrintAlgorithm,
                        OriginalFileName = fileInfo.FileName,
                        NewFileName = "",
                        State = 1
                    };

                    _epaieRepo.UploadedFiles.Create(uploadedFile);
                    _epaieRepo.UploadedFiles.Save();
                }

                registeredUploadedFiles.Add(uploadedFile);
            }

            try
            {
                RegisterEPaieJob(transferId, subscriptionId, registeredUploadedFiles, idmAuthToken, upload);
            }
            catch (Exception)
            {
                //Get uploadfolder for current transfer
                var uploadFolder = _config["Appsettings:UploadFolder"];
                string transferFolder = Path.Combine(uploadFolder, transferId);
                //Cleanup the folder 
                if (Directory.Exists(transferFolder))
                    Directory.Delete(transferFolder, true);

                throw;
            }
        }

        private void RegisterEPaieJob(string transferId, string subscriptionId, List<UploadedFile> uploadedFiles, string idmAuthToken, Upload upload)
        {
            //Check if there are accopanying files (PDF + CSV)
            var extensions = new List<String>();
            foreach (var uploadedFile in uploadedFiles)
            {
                var extension = Path.GetExtension(uploadedFile.OriginalFileName);

                if (extensions.FirstOrDefault(q => q.Equals(extension)) == null)
                    extensions.Add(extension);
            }
            bool accompanyingFile = false || extensions.Count > 1;

            //Get subscriptions for subscriptionId
            PlatformRestApi platformRestApi = new PlatformRestApi(_platformRestApiUrl, "Bearer " + idmAuthToken);
            var subscriptionWithDetails = platformRestApi.GetSubscriptionById(subscriptionId);
            var solutionParams = JsonConvert.DeserializeObject<PlatformRestApi.MdlSolutionParams>(subscriptionWithDetails.SolutionParams);
            var subscriptionParams = JsonConvert.DeserializeObject<EPaieParams>(solutionParams.Solution.Params);

            PlatformRestApi.MdlConnector easConnector = null;

            if (solutionParams.Connectors != null)
                easConnector = solutionParams.Connectors.FirstOrDefault(q => q.Name.Equals("EAS"));

            PlatformRestApi.MdlConnectorParams easConnectorParams = null;

            if (easConnector != null && easConnector.Params != null)
                easConnectorParams = JsonConvert.DeserializeObject<PlatformRestApi.MdlConnectorParams>(solutionParams.Connectors.First(q => q.Name.ToUpper().Equals("EAS")).Params);

            var SIREN = subscriptionParams.SIREN;

            var Logiciel = subscriptionParams.Logiciel;
            var CodeClient = subscriptionParams.CodeClient;
            var CodeAgence = subscriptionParams.CodeAgence;
            var CodeClientInterne = subscriptionParams.CodeClientInterne;

            var fileId = 1;
            var periode = DateTime.Now.ToString("yyyyMM");
            var uploadPath = Path.Combine(_config["AppSettings:UploadFolder"], "uploads");
            var folderPath = Path.Combine(uploadPath, transferId);

            var totalFiles = uploadedFiles.Count;

            if (accompanyingFile)
                totalFiles /= 2;

            var previousFile = "";

            foreach (var uploadedFile in uploadedFiles.OrderBy(q => q.OriginalFileName))
            {
                if (String.IsNullOrEmpty(previousFile))
                    previousFile = Path.GetFileNameWithoutExtension(uploadedFile.OriginalFileName);
                else if (String.Compare(Path.GetFileNameWithoutExtension(uploadedFile.OriginalFileName), previousFile) != 0)
                    fileId++;

                var fullPath = Path.Combine(folderPath, uploadedFile.OriginalFileName);

                // [SIREN]_[LOGICIEL]_[CODE CLIENT]_[CODE AGENCE]_[CODE CLIENT INTERNE]_[PERIODE]_[TRANSFER ID]_[FILE ID]_[TOTAL FILES]

                var destinationFileName = String.Format("{0}_{1}_{2}_{3}_{4}_{5}_{6}_{7:000}_{8:000}{9}",
                                                        SIREN, // 0
                                                        Logiciel, // 1
                                                        CodeClient, // 2
                                                        CodeAgence, // 3
                                                        CodeClientInterne, // 4
                                                        periode, // 5
                                                        transferId.Replace("-", ""), // 6,
                                                        fileId, // 7,
                                                        totalFiles, // 8,
                                                        Path.GetExtension(uploadedFile.OriginalFileName));

                uploadedFile.NewFileName = destinationFileName;

                previousFile = Path.GetFileNameWithoutExtension(uploadedFile.OriginalFileName);

                _epaieRepo.UploadedFiles.Update(uploadedFile);
                _epaieRepo.UploadedFiles.Save();
            }

            upload.State = 2;
            upload.LastActionTimeStamp = DateTime.Now;
            _epaieRepo.Uploads.Update(upload);
            _epaieRepo.Uploads.Save();

            // Create new job if not exist
            Job newJob = _epaieRepo.Jobs.FindByCondition(a => a.TransferId.Equals(transferId)).FirstOrDefault();
            if (newJob == null)
            {
                newJob = new Job()
                {
                    TransferId = transferId,
                    SubscriptionId = upload.SubscriptionId,
                    JobReference = "EPAIE",
                    JobState = JobState.Waiting,
                    TaskReference = "Deposit",
                    TaskState = TaskState.Undefined
                };

                _epaieRepo.Jobs.CreateJobAsync(newJob).Wait();

                _logger.LogInformation($"UploadController/EndOfTransfer: Created new job, jobId = {newJob.Id}. TransferId = {transferId}");
            }
            _logger.LogInformation($"UploadController/EndOfTransfer: Job = {JsonConvert.SerializeObject(newJob)}. TransferId = {transferId}");

            // Create job summary if not exist
            JobSummary newJobSummary = _epaieRepo.JobSummaries.FindByCondition(a => a.JobId.Equals(newJob.Id)).FirstOrDefault();

            if (newJobSummary == null)
            {
                newJobSummary = new JobSummary
                {
                    JobId = newJob.Id,
                    NbrOfFiles = uploadedFiles.Count,
                    DepositedBy = upload.FullName,
                    DepositedAt = upload.TimeStamp,
                    DistributionPlannedAt = upload.TimeStamp.AddDays(3)
                };

                _epaieRepo.JobSummaries.CreateJobSummaryAsync(newJobSummary).Wait();

                _logger.LogInformation($"UploadController/EndOfTransfer: Created new job summary. TransferId = {transferId}");
            }
            _logger.LogInformation($"UploadController/EndOfTransfer: Job summary = {JsonConvert.SerializeObject(newJobSummary)}. TransferId = {transferId}");

            // Create new job and copy the files to A4SB

            _logger.LogInformation($"UploadController/EndOfTransfer: Create information package. TransferId = {transferId}");

            var ip = new InformationPackage.ip();

            _logger.LogInformation($"UploadController/EndOfTransfer: before customer info. TransferId = {transferId}");
            ip.customer = easConnector != null && easConnectorParams != null && !string.IsNullOrEmpty(easConnectorParams.CustomerKey) && !string.IsNullOrEmpty(easConnectorParams.CustomerName) ? new InformationPackage.customer() { name = easConnectorParams.CustomerName, key = easConnectorParams.CustomerKey, solution = "EPaie" } : new InformationPackage.customer() { name = "EPAIE UPLOAD", key = "100030", solution = "EPaie" };
            _logger.LogInformation($"UploadController/EndOfTransfer: after customer info. TransferId = {transferId}");

            ip.tickets = new InformationPackage.tickets { rootTicket = newJob.Id, dataTicket = newJob.Id };
            ip.type = "dsip";
            ip.version = "1.0";
            ip.dataset = new InformationPackage.dataset { type = "argus/file", data = new List<string>() };

            var a4SbFolderForValidationOrArchiving = _config["Appsettings:A4SBFolderForValidationOrArchiving"];
            _logger.LogInformation($"UploadController/EndOfTransfer: A4SBFolderForValidationOrArchiving = {a4SbFolderForValidationOrArchiving}. TransferId = {transferId}");
            var a4SbFolderForWinScp = _config["Appsettings:A4SBFolderForWinSCP"];
            _logger.LogInformation($"UploadController/EndOfTransfer: A4SBFolderForWinSCP = {a4SbFolderForWinScp}. TransferId = {transferId}");
            var uploadFolder = _config["Appsettings:UploadFolder"];
            _logger.LogInformation($"UploadController/EndOfTransfer: UploadFolder = {uploadFolder}. TransferId = {transferId}");

            PlatformRestApi.MdlOption archiveOrValidation = null;

            if (solutionParams.Options != null)
                archiveOrValidation = solutionParams.Options.FirstOrDefault(q =>
                   (q.Name.Equals("Validation") && q.Value == 1) || (q.Name.Equals("Archiving") && q.Value == 1));

            _logger.LogInformation($"UploadController/EndOfTransfer: ArchiveOrValidation = {archiveOrValidation}. TransferId = {transferId}");

            var destinationFolder = a4SbFolderForValidationOrArchiving;


            if (archiveOrValidation == null)
            {
                ip.customer.solution = "EPaie Distribution";
                destinationFolder = a4SbFolderForWinScp;
            }

            uploadedFiles = _epaieRepo.UploadedFiles.FindByCondition(q => q.UploadId.Equals(upload.Id)).ToList();

            foreach (var file in uploadedFiles)
            {
                var sourceFile = Path.Combine(Path.Combine(uploadFolder, transferId), file.OriginalFileName);
                var destinationFile = Path.Combine(destinationFolder, file.NewFileName);

                _logger.LogInformation($"UploadController/EndOfTransfer: Moving file, source = {sourceFile}, destination = {destinationFile}. TransferId = {transferId}");

                System.IO.File.Move(sourceFile, destinationFile);

                ip.dataset.data.Add(System.IO.Path.GetFileName(destinationFile));
            }

            var ipFile = Path.Combine(destinationFolder, Guid.NewGuid().ToString().Replace("-", "") + ".dsp");

            _logger.LogInformation($"UploadController/EndOfTransfer: Saving information package to {ipFile}. TransferId = {transferId}");

            // Save the information package
            InformationPackage.SaveIP(ip, ipFile);

            // Save job
            _logger.LogInformation($"UploadController/EndOfTransfer: Saving job. TransferId = {transferId}");

            _epaieRepo.Jobs.SaveAsync().Wait();
        }

        #endregion

        #region FUNCTIONS
        private bool CheckFileFingerprint(string fileInfoFingerPrintAlgorithm, string fileInfoFingerPrint, string uploadedFile)
        {
            var uploadedFileFingerPrint = "";

            using (var stream = System.IO.File.OpenRead(uploadedFile))
            {
                switch (fileInfoFingerPrintAlgorithm)
                {
                    case "MD5":
                        using (MD5 md5Hash = MD5.Create())
                        {
                            uploadedFileFingerPrint = BitConverter.ToString(md5Hash.ComputeHash(stream)).Replace("-", "").ToLower();
                        }
                        break;
                    case "SHA-1":
                        using (SHA1 sha1Hash = SHA1.Create())
                        {
                            uploadedFileFingerPrint = BitConverter.ToString(sha1Hash.ComputeHash(stream)).Replace("-", "").ToLower();
                        }
                        break;
                    case "SHA-256":
                        using (SHA256 sha256Hash = SHA256.Create())
                        {
                            uploadedFileFingerPrint = BitConverter.ToString(sha256Hash.ComputeHash(stream)).Replace("-", "").ToLower();
                        }
                        break;
                    case "SHA-512":
                        using (SHA512 sha512Hash = SHA512.Create())
                        {
                            uploadedFileFingerPrint = BitConverter.ToString(sha512Hash.ComputeHash(stream)).Replace("-", "").ToLower();
                        }
                        break;
                }
            }

            return uploadedFileFingerPrint.Equals(fileInfoFingerPrint);
        }

        #endregion
    }
}
