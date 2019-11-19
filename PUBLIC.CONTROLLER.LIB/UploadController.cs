using EPAIE.REPO.LIB;
using EPAIE.REPO.LIB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PUBLIC.CONTROLLER.LIB.DTOs;
using PUBLIC.CONTROLLER.LIB.Helpers;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

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
        private readonly string _idmRestApiUrl;
        private readonly string _solutionUrl;

        public UploadController(IConfiguration config, ILogger<AuthenticationController> logger, IEPaieRepositoryWrapper epaieRepo)
        {
            _config = config;
            _logger = logger;
            
            _epaieRepo = epaieRepo;

            _platformRestApiUrl = config["AppSettings:PlatformRestApiUrl"];
            _idmRestApiUrl = config["AppSettings:IDMRestApiUrl"];
            _solutionUrl = config["AppSettings:SolutionUrl"];
        }

        public class ePaieParams
        {
            public string Entreprise { get; set; }
            public string SIRET { get; set; }
            public string SIREN { get; set; }
            public string Logiciel { get; set; }
            public string CodeClient { get; set; }
            public string CodeAgence { get; set; }
            public string CodeClientInterne { get; set; }

        }

        [Authorize]
        [HttpPost("BeginOfTransfer")]
        [ProducesResponseType(200, Type = typeof(DtoUpload.TransferRegistered))]
        [ProducesResponseType(401)]
        public IActionResult BeginOfTransfer(DtoUpload.RegisterBeginOfTransfer dtoRegisterBeginOfTransfer)
        {
            string transferId = null;

            try
            {
                _logger.LogInformation("Starting begin of transfer");

                //Check if all parameters are given and if there are files to upload
                if (string.IsNullOrWhiteSpace(dtoRegisterBeginOfTransfer.SolutionName))
                    throw new Exception("Solution name cannot be empty !");
                if (string.IsNullOrWhiteSpace(dtoRegisterBeginOfTransfer.SolutionReference))
                    throw new Exception("Solution reference cannot be empty !");
                if (dtoRegisterBeginOfTransfer.UploadFiles.Count == 0)
                    throw new Exception("No files provided for the upload !");

                //Get authorization token
                string currentToken = Request.Headers["Authorization"];
                currentToken = currentToken.Replace("Bearer ", "");
                var currentTokenObj = new JwtSecurityTokenHandler().ReadJwtToken(currentToken);
                var argusAuthToken = currentTokenObj.Claims.Where(c => c.Type == ClaimTypes.Authentication).FirstOrDefault();

                if (string.IsNullOrWhiteSpace(argusAuthToken.Value))
                    return Unauthorized();
                                
                //Get authorized subscriptions for user
                PlatformRestApi platformRestApi = new PlatformRestApi(_platformRestApiUrl, "Bearer " + argusAuthToken.Value);
                var subscriptions = platformRestApi.GetAuthorizedSubscriptions();

                if (subscriptions.Count == 0)
                    throw new Exception("No authorized subscriptions for user !");

                bool subscriptionNameValid = false;
                bool subscriptionReferenceValid = false;
                PlatformRestApi.MdlSubscription currentSubscription = null;
                PlatformRestApi.MdlSolutionParams currentSolutionParams = null;

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
                                var subscriptionParams = JsonConvert.DeserializeObject<ePaieParams>(solutionParams.Solution.Params);
                                subscriptionReferenceValid = subscriptionParams.SIREN.ToLower().Equals(dtoRegisterBeginOfTransfer.SolutionReference.ToLower());

                                if (subscriptionReferenceValid && subscriptionNameValid)
                                {
                                    currentSubscription = subscription;
                                    currentSolutionParams = solutionParams;
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
                    throw new Exception($"Subscription with name '{dtoRegisterBeginOfTransfer.SolutionName}' not found or not authorized");
                if (!subscriptionReferenceValid)
                    throw new Exception($"Subscription with reference '{dtoRegisterBeginOfTransfer.SolutionReference}' and name '{dtoRegisterBeginOfTransfer.SolutionName}' not found or not authorized");

                //Register upload
                if (dtoRegisterBeginOfTransfer.SolutionName.ToLower().Equals("epaie"))
                {
                    var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                    var providerId = User.FindFirst(ClaimTypes.PrimarySid).Value.Split('\\')[0];
                    var tenantId = User.FindFirst(ClaimTypes.PrimarySid).Value.Split('\\')[1];
                    var loginName = User.FindFirst(ClaimTypes.Name).Value;
                    var displayName = User.FindFirst(ClaimTypes.UserData).Value;

                    var upload = new Upload()
                    {
                        TransferId = Guid.NewGuid().ToString(),
                        TimeStamp = DateTime.Now,
                        SubscriptionId = currentSubscription.Id,
                        LoginName = loginName,
                        FullName = displayName,
                        LastActionTimeStamp = DateTime.Now,
                        UserId = userId,
                        ProviderId = providerId,
                        TenantId = tenantId
                    };

                    _epaieRepo.Uploads.Create(upload);
                    _epaieRepo.Uploads.Save();

                    //Register each file
                    foreach (var fileInfo in dtoRegisterBeginOfTransfer.UploadFiles)
                    {
                        var uploadedFile = new UploadedFile
                        {
                            Id = Guid.NewGuid().ToString(),
                            UploadId = upload.Id,
                            FileSize = 0,
                            FingerPrint = fileInfo.FingerPrint,
                            FingerPrintAlgorithm = fileInfo.FingerPrintAlgorithm,
                            OriginalFileName = fileInfo.FileName,
                            NewFileName = ""
                        };

                        _epaieRepo.UploadedFiles.Create(uploadedFile);
                        _epaieRepo.UploadedFiles.Save();

                        transferId = upload.TransferId;
                    }
                }
                else
                    throw new Exception($"Upload not yet implemented for solution '{dtoRegisterBeginOfTransfer.SolutionName}'");

                return Ok(new DtoUpload.TransferRegistered() { TransferId = transferId });
            }
            catch (Exception exception)
            {
                if (!string.IsNullOrWhiteSpace(transferId))
                    TransferError(transferId);

                _logger.LogError($"Authenticate error: {exception.Message}");
                throw;
            }
        }

        [Authorize]
        [HttpPost("UploadFiles")]
        [DisableRequestSizeLimit]
        public IActionResult UploadFiles()
        {
            _logger.LogInformation("UploadFiles initiated.");

            string transferId = null;

            try
            {
                transferId = Request.Headers["X-TransferId"].ToString();
                if (string.IsNullOrWhiteSpace(transferId))
                    throw new Exception("Invalid or no transfer id provided !");

                var uploadFiles = HttpContext.Request.Form.Files;
                if (uploadFiles.Count() == 0)
                    throw new Exception("No upload file provided !");

                var uploadPath = _config["Appsettings:UploadFolder"];

                foreach (var uploadFile in uploadFiles)
                {
                    if (uploadFile.Length > 0)
                    {
                        var fileName = ContentDispositionHeaderValue.Parse(uploadFile.ContentDisposition).FileName.Trim('"');

                        string folderPath = Path.Combine(uploadPath, transferId);
                        string fullPath = Path.Combine(folderPath, fileName);

                        if (!Directory.Exists(folderPath))
                            Directory.CreateDirectory(folderPath);
                        
                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            uploadFile.CopyTo(stream);
                        }

                        var uploadedFile = new DtoUpload.RegisterUploadedFile()
                        {
                            fileName = fileName,
                            transferId = transferId,
                            fileSize = uploadFile.Length
                        };

                        var transfer = _epaieRepo.Uploads.FindByCondition(q => q.TransferId.Equals(transferId)).FirstOrDefault();

                        if (transfer == null)
                            throw new Exception("Can not update the uploaded file info. Transfer job not found!");

                        var uploadFileInfo = _epaieRepo.UploadedFiles.FindByCondition(q => q.UploadId.Equals(transfer.Id) && q.OriginalFileName.ToLower().Equals(uploadedFile.fileName.ToLower())).FirstOrDefault();

                        if (uploadFileInfo == null)
                            throw new Exception("Can not update the uploaded file info. File not found!");
                        
                        if (!string.IsNullOrWhiteSpace(uploadFileInfo.FingerPrint) && !string.IsNullOrWhiteSpace(uploadFileInfo.FingerPrintAlgorithm))
                        {
                            if (!CheckFileFingerprint(uploadFileInfo.FingerPrintAlgorithm, uploadFileInfo.FingerPrint, fullPath))
                                throw new Exception("Can not update the uploaded file info. File fingerprint check failed!");
                        }

                        uploadFileInfo.State = 1;
                        uploadFileInfo.FileSize = uploadedFile.fileSize;

                        _epaieRepo.UploadedFiles.Update(uploadFileInfo);
                        _epaieRepo.UploadedFiles.Save();
                    }
                    else
                    {
                        throw new Exception("File size cannot be null !");
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrWhiteSpace(transferId))
                    TransferError(transferId);

                return BadRequest(ex.Message);
            }
        }

        private bool CheckFileFingerprint(string fileInfoFingerPrintAlgorithm, string fileInfoFingerPrint, string uploadedFile)
        {
            var uploadedFileFingerPrint = "";

            using (var stream = System.IO.File.OpenRead(uploadedFile))
            {
                switch (fileInfoFingerPrintAlgorithm)
                {
                    case "MD5":
                        uploadedFileFingerPrint = BitConverter.ToString(MD5.Create().ComputeHash(stream)).Replace("-", "").ToLower();
                        break;
                    case "SHA-1":
                        uploadedFileFingerPrint = BitConverter.ToString(SHA1.Create().ComputeHash(stream)).Replace("-", "").ToLower();
                        break;
                    case "SHA-256":
                        uploadedFileFingerPrint = BitConverter.ToString(SHA256.Create().ComputeHash(stream)).Replace("-", "").ToLower();
                        break;
                    case "SHA-512":
                        uploadedFileFingerPrint = BitConverter.ToString(SHA512.Create().ComputeHash(stream)).Replace("-", "").ToLower();
                        break;
                }
            }

            return uploadedFileFingerPrint.Equals(fileInfoFingerPrint);
        }

        [Authorize]
        [HttpPost("EndOfTransfer")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> EndOfTransferAsync()
        {
            _logger.LogInformation("EndOfTransfer initiated.");

            string transferId = null;

            try
            {
                transferId = Request.Headers["X-TransferId"].ToString();
                if (string.IsNullOrWhiteSpace(transferId))
                    throw new Exception("Invalid or no transfer id provided !");

                var authToken = Request.Headers["Authorization"];
                PlatformRestApi platformRestApi = new PlatformRestApi(_platformRestApiUrl, authToken);

                var upload = _epaieRepo.Uploads.FindByCondition(q => q.TransferId.Equals(transferId)).FirstOrDefault();

                if (upload == null)
                    throw new Exception("Upload job not found.");

                var uploadedFiles = _epaieRepo.UploadedFiles.FindByCondition(q => q.UploadId.Equals(upload.Id)).ToList();
                var successfulUploads = uploadedFiles.Count(q => q.State.Equals(1));

                var extensions = new List<String>();

                foreach (var uploadedFile in uploadedFiles)
                {
                    var extension = Path.GetExtension(uploadedFile.OriginalFileName);

                    if (extensions.FirstOrDefault(q => q.Equals(extension)) == null)
                        extensions.Add(extension);
                }

                bool accompanyingFile = false || extensions.Count > 1;

                if (successfulUploads == uploadedFiles.Count)
                {
                    var subscriptionWithDetails = platformRestApi.GetSubscriptionById(upload.SubscriptionId);
                    var solutionParams = JsonConvert.DeserializeObject<PlatformRestApi.MdlSolutionParams>(subscriptionWithDetails.SolutionParams);
                    var subscriptionParams = JsonConvert.DeserializeObject<ePaieParams>(solutionParams.Solution.Params);

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
                        totalFiles = totalFiles / 2;

                    var previousFile = "";

                    foreach (var uploadedFile in uploadedFiles.OrderBy(q => q.OriginalFileName))
                    {
                        if (String.IsNullOrEmpty(previousFile))
                            previousFile = Path.GetFileNameWithoutExtension(uploadedFile.OriginalFileName);
                        else
                            if (String.Compare(Path.GetFileNameWithoutExtension(uploadedFile.OriginalFileName), previousFile) != 0)
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

                    // Create new job 

                    var newJob = new Job()
                    {
                        TransferId = transferId,

                        SubscriptionId = upload.SubscriptionId,
                        JobReference = "EPAIE",
                        JobState = JobState.Waiting,
                        TaskReference = "Deposit",
                        TaskState = TaskState.Undefined
                    };

                    await _epaieRepo.Jobs.CreateJobAsync(newJob);

                    _logger.LogInformation($"EPaieUploadController/EndOfTransfer: Created new job, jobId = {newJob.Id}");
                    _logger.LogInformation($"EPaieUploadController/EndOfTransfer: Job = {JsonConvert.SerializeObject(newJob)}");

                    // Create job summary

                    var newJobSummary = new JobSummary
                    {
                        JobId = newJob.Id,
                        NbrOfFiles = uploadedFiles.Count,
                        DepositedBy = upload.FullName,
                        DepositedAt = upload.TimeStamp,
                        DistributionPlannedAt = upload.TimeStamp.AddDays(3)
                    };

                    await _epaieRepo.JobSummaries.CreateJobSummaryAsync(newJobSummary);

                    _logger.LogInformation($"EPaieUploadController/EndOfTransfer: Created new job summary");
                    _logger.LogInformation($"EPaieUploadController/EndOfTransfer: Job summary = {JsonConvert.SerializeObject(newJobSummary)}");

                    // Create new job and copy the files to A4SB

                    _logger.LogInformation($"EPaieUploadController/EndOfTransfer: Create information package");

                    var ip = new InformationPackage.ip();

                    _logger.LogInformation($"EPaieUploadController/EndOfTransfer: before customer info");
                    ip.customer = easConnector != null && easConnectorParams != null && !string.IsNullOrEmpty(easConnectorParams.CustomerKey) && !string.IsNullOrEmpty(easConnectorParams.CustomerName) ? new InformationPackage.customer() { name = easConnectorParams.CustomerName, key = easConnectorParams.CustomerKey, solution = "EPaie" } : new InformationPackage.customer() { name = "EPAIE UPLOAD", key = "100030", solution = "EPaie" };
                    _logger.LogInformation($"EPaieUploadController/EndOfTransfer: after customer info");

                    ip.tickets = new InformationPackage.tickets { rootTicket = newJob.Id, dataTicket = newJob.Id };
                    ip.type = "dsip";
                    ip.version = "1.0";
                    ip.dataset = new InformationPackage.dataset { type = "argus/file", data = new List<string>() };

                    var a4SbFolderForValidationOrArchiving = _config["Appsettings:A4SBFolderForValidationOrArchiving"];
                    _logger.LogInformation($"EPaieUploadController/EndOfTransfer: A4SBFolderForValidationOrArchiving = {a4SbFolderForValidationOrArchiving}");
                    var a4SbFolderForWinScp = _config["Appsettings:A4SBFolderForWinSCP"];
                    _logger.LogInformation($"EPaieUploadController/EndOfTransfer: A4SBFolderForWinSCP = {a4SbFolderForWinScp}");
                    var uploadFolder = _config["Appsettings:UploadFolder"];
                    _logger.LogInformation($"EPaieUploadController/EndOfTransfer: UploadFolder = {uploadFolder}");

                    PlatformRestApi.MdlOption archiveOrValidation = null;

                    if (solutionParams.Options != null)
                        archiveOrValidation = solutionParams.Options.FirstOrDefault(q =>
                           (q.Name.Equals("Validation") && q.Value == 1) || (q.Name.Equals("Archiving") && q.Value == 1));

                    _logger.LogInformation($"EPaieUploadController/EndOfTransfer: ArchiveOrValidation = {archiveOrValidation}");

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

                        _logger.LogInformation($"EPaieUploadController/EndOfTransfer: Moving file, source = {sourceFile}, destination = {destinationFile}");

                        System.IO.File.Move(sourceFile, destinationFile);

                        ip.dataset.data.Add(System.IO.Path.GetFileName(destinationFile));
                    }

                    System.IO.Directory.Delete(Path.Combine(Path.Combine(uploadFolder, transferId)));

                    var ipFile = Path.Combine(destinationFolder, Guid.NewGuid().ToString().Replace("-", "") + ".dsp");

                    _logger.LogInformation($"EPaieUploadController/EndOfTransfer: Saving information package to {ipFile}");

                    // Save the information package
                    InformationPackage.SaveIP(ip, ipFile);

                    // Save job
                    _logger.LogInformation($"EPaieUploadController/EndOfTransfer: Saving job");

                    await _epaieRepo.Jobs.SaveAsync();

                }
                else
                {
                    _logger.LogInformation($"EPaieUploadController/EndOfTransfer: Updating upload jobState, jobState = 3");

                    upload.State = 3;
                    upload.LastActionTimeStamp = DateTime.Now;
                    _epaieRepo.Uploads.Update(upload);
                    _epaieRepo.Uploads.Save();
                }

                _logger.LogInformation($"EPaieUploadController/EndOfTransfer: End");

                return Ok();
            }
            catch (Exception exception)
            {
                if (!string.IsNullOrWhiteSpace(transferId))
                    TransferError(transferId);

                _logger.LogError($"Authenticate error: {exception.Message}");
                throw;
            }
        }

        [HttpPost("TransferError")]
        [ProducesResponseType(200)]
        public IActionResult TransferError(string transferId)
        {
            _logger.LogDebug("TransferError initiated.");
            _logger.LogInformation("TransferError initiated");

            var upload = _epaieRepo.Uploads.FindByCondition(q => q.TransferId.Equals(transferId)).FirstOrDefault();

            if (upload == null)
                throw new Exception("Upload job not found.");

            upload.State = 5;
            upload.LastActionTimeStamp = DateTime.Now;
            _epaieRepo.Uploads.Update(upload);
            _epaieRepo.Uploads.Save();

            _logger.LogDebug("Sending error mail");
            _logger.LogInformation("Sending error mail");

            // Get mail template
            PlatformRestApi platformRestApi = new PlatformRestApi(_platformRestApiUrl, "");

            var subscription = platformRestApi.GetSubscriptionById(upload.SubscriptionId);
            var solutionParams = JsonConvert.DeserializeObject<PlatformRestApi.MdlSolutionParams>(subscription.SolutionParams);
            var subscriptionParams = JsonConvert.DeserializeObject<ePaieParams>(solutionParams.Solution.Params);

            var mailTemplate = platformRestApi.GetMailTemplateByMailCodeAndReference("EPAIE_TRANSFER_ERROR", "FR", subscription.ReferenceType.ToString(), subscription.ReferenceId);

            if (mailTemplate != null)
            {
                _logger.LogInformation($"Mail for upload {JsonConvert.SerializeObject(upload)}");
                //Getting user details
                IdmRestApi.DtoGetByIds getByIdsDto = new IdmRestApi.DtoGetByIds
                {
                    UserByIds = new List<IdmRestApi.UserByIds>()
                };
                getByIdsDto.UserByIds.Add(new IdmRestApi.UserByIds()
                {
                    UserId = upload.UserId,
                    ProviderId = upload.ProviderId,
                    TenantId = upload.TenantId
                });
                IdmRestApi idmRestApi = new IdmRestApi(_idmRestApiUrl);
                var userDetails = idmRestApi.IdentityManagerUser_GetByList(getByIdsDto);

                foreach (var userDetail in userDetails)
                {
                    // Create new mail message
                    var msg = new MailMessage();

                    msg.To.Add(new MailAddress(userDetail.emailAddr));
                    msg.From = new MailAddress(mailTemplate.FromAddr, mailTemplate.DisplayName);
                    msg.Subject = mailTemplate.Subject;

                    var body = mailTemplate.Body;
                    body = body.Replace("{{date}}", HttpUtility.HtmlEncode(upload.TimeStamp.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture)));
                    body = body.Replace("{{hour}}", HttpUtility.HtmlEncode(upload.TimeStamp.ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture)));
                    body = body.Replace("{{targetCompany}}", HttpUtility.HtmlEncode(subscriptionParams.Entreprise));
                    body = body.Replace("{{siren}}", HttpUtility.HtmlEncode(subscriptionParams.SIREN));
                    body = body.Replace("{{rootLink}}", HttpUtility.HtmlEncode(_solutionUrl));

                    msg.IsBodyHtml = true;
                    msg.Body = body;

                    SendMail(msg);
                }
            }
            else
                _logger.LogInformation("No mail template found");
            
            return Ok();
        }

        internal void SendMail(MailMessage message)
        {
            var smtpClient = new SmtpClient { UseDefaultCredentials = false };

            if (!String.IsNullOrEmpty(_config.GetSection("SMTP:Username").Value))
                smtpClient.Credentials = new System.Net.NetworkCredential(_config.GetSection("SMTP:Username").Value, _config.GetSection("SMTP:Password").Value);

            smtpClient.Port = Convert.ToInt32(_config.GetSection("SMTP:Port").Value);
            smtpClient.Host = _config.GetSection("SMTP:Host").Value;
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.EnableSsl = Convert.ToBoolean(_config.GetSection("SMTP:EnableSsl").Value);

            try
            {
                _logger.LogDebug("Trying to send to mail.");

                smtpClient.Send(message);
            }
            catch (Exception ex)
            {
                _logger.LogCritical("Sending mail failed! Reason: " + ex.Message);

                if (ex.InnerException != null)
                    _logger.LogCritical("Inner exception: " + ex.InnerException.Message);

                throw;
            }

            _logger.LogDebug("Mail was send.");

        }
    }
}
