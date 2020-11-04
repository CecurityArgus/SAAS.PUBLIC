using Efacture.Api.Client.Api;
using Efacture.Api.Client.Model;
using Epaie.Api.Client.Api;
using Epaie.Api.Client.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MQDispatch.Client;
using Newtonsoft.Json;
using Platform.Api.Client.Api;
using Platform.Framework;
using PUBLIC.SERVICE.LIB.Helpers;
using PUBLIC.SERVICE.LIB.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using static PUBLIC.SERVICE.LIB.Helpers.MQErrors;

namespace PUBLIC.SERVICE.LIB.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class UploadService
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private readonly ApiKeys _apiKeys;
        private readonly Platform.Api.Client.Client.Configuration _platformConfig;
        private readonly Epaie.Api.Client.Client.Configuration _epaieConfig;
        private readonly Efacture.Api.Client.Client.Configuration _efactureConfig;
        private readonly string _authToken;
        private readonly ClaimsPrincipal _user;
        private readonly IRabbitMqPersistentConnection _rabbitMqPersistentConnection;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="logger"></param>
        /// <param name="apiKeys"></param>
        public UploadService(IConfiguration config, ILogger logger, ApiKeys apiKeys, string publicAuthToken, ClaimsPrincipal user = null, IRabbitMqPersistentConnection rabbitMqPersistentConnection = null)
        {
            _logger = logger;
            _config = config;
            _apiKeys = apiKeys;
            _user = user;

            _rabbitMqPersistentConnection = rabbitMqPersistentConnection;

            //Get authorization token
            var publicAuthHeader = System.Net.Http.Headers.AuthenticationHeaderValue.Parse(publicAuthToken);
            var currentTokenObj = new JwtSecurityTokenHandler().ReadJwtToken(publicAuthHeader.Parameter);
            var idmAuthToken = currentTokenObj.Claims.Where(c => c.Type == "IDMAuthToken").FirstOrDefault();
            
            if (string.IsNullOrWhiteSpace(idmAuthToken.Value))
                throw new UnauthorizedAccessException();

            _authToken = idmAuthToken.Value;

            _platformConfig = new Platform.Api.Client.Client.Configuration()
            {
                BasePath = _config.GetSection("AppSettings:PlatformRestApiUrl").Value
            };
            _platformConfig.ApiKey.Add("Authorization", _authToken);
            _platformConfig.ApiKeyPrefix.Add("Authorization", "Bearer");

            _epaieConfig = new Epaie.Api.Client.Client.Configuration()
            {
                BasePath = _config.GetSection("AppSettings:EpaieRestApiUrl").Value
            };
            _epaieConfig.ApiKey.Add("Authorization", _authToken);
            _epaieConfig.ApiKeyPrefix.Add("Authorization", "Bearer");

            _efactureConfig = new Efacture.Api.Client.Client.Configuration()
            {
                BasePath = _config.GetSection("AppSettings:EfactureRestApiUrl").Value
            };
            _efactureConfig.ApiKey.Add("Authorization", _authToken);
            _efactureConfig.ApiKeyPrefix.Add("Authorization", "Bearer");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestAPIKey"></param>
        /// <param name="transferId"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public TransferRegistered BeginOfTransfer(string requestAPIKey, string transferId, RegisterBeginOfTransfer body)
        {
            //Check if all parameters are given and if there are files to upload
            if (string.IsNullOrWhiteSpace(body.SolutionName))
                throw new CecurityException((int)MQMessages.APP_ERR_EMPTY_SOLUTION_NAME, MQErrorMessages[(int)MQMessages.APP_ERR_EMPTY_SOLUTION_NAME].Description, null);

            if (string.IsNullOrWhiteSpace(body.SolutionReference))
                throw new CecurityException((int)MQMessages.APP_ERR_EMPTY_SOLUTION_REFERENCE, MQErrorMessages[(int)MQMessages.APP_ERR_EMPTY_SOLUTION_REFERENCE].Description, null);

            if (body.UploadFiles.Count == 0)
                throw new CecurityException((int)MQMessages.APP_ERR_NO_UPLOAD_FILES, MQErrorMessages[(int)MQMessages.APP_ERR_NO_UPLOAD_FILES].Description, null);

            //Get authorized subscriptions for user
            var subscriptionsApi = new PlatformSubscriptionsApi(_platformConfig);
            var subscriptions = subscriptionsApi.PlatformPlatformSubscriptionsGetAuthorizedSubscriptionsGet();// subscriptionsApi.PlatformPlatformSubscriptionsGetAuthorizedSubscriptionsGet();

            if (subscriptions.Count == 0)
                throw new CecurityException((int)MQMessages.APP_ERR_NO_AUTHORIZED_SUBSCRIPTIONS, MQErrorMessages[(int)MQMessages.APP_ERR_NO_AUTHORIZED_SUBSCRIPTIONS].Description, null);

            bool subscriptionNameValid = false;
            bool subscriptionReferenceValid = false;
            TransferObject transferObject = null;
            //Check if subscription and reference are authorized for the logged on user
            foreach (var subscription in subscriptions)
            {
                if (subscription.Solution.SolutionName.ToLower().Equals(body.SolutionName.ToLower()))
                {
                    subscriptionNameValid = true;

                    var solutionParams = JsonConvert.DeserializeObject<SolutionParams>(subscription.SolutionParams);
                    if (!string.IsNullOrEmpty(solutionParams.Solution.Params))
                    {
                        if (body.SolutionName.ToLower().Equals("epaie"))
                        {
                            var epaieSubscriptionParams = JsonConvert.DeserializeObject<EPaieParams>(solutionParams.Solution.Params);
                            subscriptionReferenceValid = epaieSubscriptionParams.SIREN.ToLower().Equals(body.SolutionReference.ToLower());

                            if (subscriptionReferenceValid && subscriptionNameValid)
                            {
                                transferObject = new TransferObject
                                {
                                    SolutionName = body.SolutionName,
                                    SolutionReference = body.SolutionReference,
                                    UploadFiles = body.UploadFiles,
                                    SubscriptionId = subscription.Id
                                };
                                break;
                            }
                        }
                        else if (body.SolutionName.ToLower().Equals("efacture"))
                        {
                            var efactureSubscriptionParams = JsonConvert.DeserializeObject<EFactureParams>(solutionParams.Solution.Params);
                            subscriptionReferenceValid = efactureSubscriptionParams.SIREN.ToLower().Equals(body.SolutionReference.ToLower());

                            if (subscriptionReferenceValid && subscriptionNameValid)
                            {
                                transferObject = new TransferObject
                                {
                                    SolutionName = body.SolutionName,
                                    SolutionReference = body.SolutionReference,
                                    UploadFiles = body.UploadFiles,
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
                throw new CecurityException((int)MQMessages.APP_ERR_SOLUTION_NAME_NOT_FOUND_AUTHORIZED, $"Solution with name '{body.SolutionName}' not found or not authorized!", null);
            if (!subscriptionReferenceValid)
                throw new CecurityException((int)MQMessages.APP_ERR_SOLUTION_REF_NOT_FOUND_AUTHORIZED, $"Subscription with reference '{body.SolutionReference}' and Solution '{body.SolutionName}' not found or not authorized!", null);

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

            return new TransferRegistered() { TransferId = transferId };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transferId"></param>
        /// <param name="uploadedFiles"></param>
        public void UploadFiles(string transferId, List<IFormFile> uploadedFiles)
        {
            if (string.IsNullOrWhiteSpace(transferId))
                throw new CecurityException((int)MQMessages.APP_ERR_NO_TRANSFERID, MQErrorMessages[(int)MQMessages.APP_ERR_NO_TRANSFERID].Description, null);

            if (uploadedFiles.Count == 0)
                throw new CecurityException((int)MQMessages.APP_ERR_NO_UPLOAD_FILES, MQErrorMessages[(int)MQMessages.APP_ERR_NO_UPLOAD_FILES].Description, null);

            //Get uploadfolder for current transfer
            var uploadFolder = _config["Appsettings:UploadFolder"];
            string transferFolder = Path.Combine(uploadFolder, transferId);
            if (!System.IO.Directory.Exists(transferFolder))
                throw new CecurityException((int)MQMessages.APP_ERR_UPLOAD_FOLDER_NOT_FOUND, $"No transfer upload folder found for transfer '{transferId}' !", null);
            
            //Read current transfer file
            string transferFile = Path.Combine(transferFolder, $"transfer.json");
            if (!System.IO.File.Exists(transferFile))
                throw new CecurityException((int)MQMessages.APP_ERR_TRANSFER_CONFIG_FILE_NOT_FOUND, $"No transfer configuration file found for transfer '{transferId}' !", null);
            var transferObject = JsonConvert.DeserializeObject<TransferObject>(System.IO.File.ReadAllText(transferFile));

            foreach (var uploadedFile in uploadedFiles)
            {
                var uploadedFileName = System.Net.Http.Headers.ContentDispositionHeaderValue.Parse(uploadedFile.ContentDisposition).FileName.Trim('"');
                string uploadedFileFullPath = Path.Combine(transferFolder, uploadedFileName);

                if (!System.IO.File.Exists(uploadedFileFullPath))
                {

                    if (uploadedFile.Length == 0)
                        throw new CecurityException((int)MQMessages.APP_ERR_FILE_LENGTH_NULL, $"Uploaded file '{uploadedFileName}' cannot have a null length", null);

                    if (!transferObject.UploadFiles.Any(f => f.FileName.ToLower().Equals(uploadedFileName.ToLower())))
                        throw new CecurityException((int)MQMessages.APP_ERR_UNKNOWN_FILE, $"Unknown file provided: '{uploadedFileName}'", null);
                    else
                    {
                        var transferObjectFile = transferObject.UploadFiles.Where(f => f.FileName.ToLower().Equals(uploadedFileName.ToLower())).First();
                        if (uploadedFile.Length != transferObjectFile.FileSize)
                            throw new CecurityException((int)MQMessages.APP_ERR_UNKNOWN_FILE, $"Filesizes do not match. File: '{uploadedFileName}' - uploaded filesize: '{uploadedFile.Length}' - excpeected filesize: '{transferObjectFile.FileSize}'", null);

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

                                throw new CecurityException((int)MQMessages.APP_ERR_UNKNOWN_FILE, $"File fingerprint check failed for file '{uploadedFileName}'", null);
                            }
                        }
                    }
                }
                else
                    _logger.LogInformation($"UploadController/UploadFiles: File '{uploadedFileName}' already uploaded, ignoring file. TransferId = {transferId}");
            }
        }

        public void EndOfTransfer(string transferId)
        {
            if (string.IsNullOrWhiteSpace(transferId))
                throw new CecurityException((int)MQMessages.APP_ERR_NO_TRANSFERID, MQErrorMessages[(int)MQMessages.APP_ERR_NO_TRANSFERID].Description, null);

            //Get uploadfolder for current transfer
            var uploadFolder = _config["Appsettings:UploadFolder"];
            string transferFolder = Path.Combine(uploadFolder, transferId);
            if (!System.IO.Directory.Exists(transferFolder))
                throw new CecurityException((int)MQMessages.APP_ERR_UPLOAD_FOLDER_NOT_FOUND, $"No transfer upload folder found for transfer '{transferId}' !", null);
            //Read current transfer file
            string transferFile = Path.Combine(transferFolder, $"transfer.json");
            if (!System.IO.File.Exists(transferFile))
                throw new CecurityException((int)MQMessages.APP_ERR_TRANSFER_CONFIG_FILE_NOT_FOUND, $"No transfer configuration file found for transfer '{transferId}' !", null);
            var transferObject = JsonConvert.DeserializeObject<TransferObject>(System.IO.File.ReadAllText(transferFile));

            //Check if all files have been uploaded
            List<string> filesNotUploaded = new List<string>();
            foreach (var transferFileObject in transferObject.UploadFiles)
            {
                var fileToCheck = Path.Combine(transferFolder, transferFileObject.FileName);
                if (!System.IO.File.Exists(fileToCheck))
                    filesNotUploaded.Add(transferFileObject.FileName);
            }
            if (filesNotUploaded.Count > 0)
                throw new CecurityException((int)MQMessages.APP_ERR_NOT_ALL_FILES_UPLOADED, $"Not all files have been uploaded ({string.Join(", ", filesNotUploaded)})", null);

            var currentTokenObj = new JwtSecurityTokenHandler().ReadJwtToken(_authToken);   

            if (transferObject.SolutionName.ToLower().Equals("epaie"))
            {
                try
                {
                    RegisterEPaieUpload(transferId, transferObject.SubscriptionId, transferObject.UploadFiles);
                }
                catch (CecurityException exception)
                {
                    throw new CecurityException((int)MQMessages.APP_ERR_REGISTER_EPAIE_JOB, $"Error registering ePaie job: {exception.Message}", exception.AdditionalInfo(), null);
                }
                catch (Epaie.Api.Client.Client.ApiException apiException)
                {
                    throw new CecurityException((int)MQMessages.APP_ERR_REGISTER_EPAIE_JOB, $"Error registering ePaie job: {apiException.Message}", null);
                }
                catch (Platform.Api.Client.Client.ApiException apiException)
                {
                    throw new CecurityException((int)MQMessages.APP_ERR_REGISTER_EPAIE_JOB, $"Error registering ePaie job: {apiException.Message}", null);
                }
                catch (Exception exception)
                {
                    throw new CecurityException((int)MQMessages.APP_ERR_REGISTER_EPAIE_JOB, $"Error registering ePaie job: {exception.Message}", null);
                }
            }
            else if (transferObject.SolutionName.ToLower().Equals("efacture"))
            {
                try
                {
                    RegisterEFactureUpload(transferId, transferObject.SubscriptionId, transferObject.UploadFiles);
                }
                catch (CecurityException exception)
                {
                    throw new CecurityException((int)MQMessages.APP_ERR_REGISTER_EFACTURE_JOB, $"Error registering eFacture job: {exception.Message}", exception.AdditionalInfo(), null);
                }
                catch (Efacture.Api.Client.Client.ApiException apiException)
                {
                    throw new CecurityException((int)MQMessages.APP_ERR_REGISTER_EFACTURE_JOB, $"Error registering eFacture job: {apiException.Message}", null);
                }
                catch (Platform.Api.Client.Client.ApiException apiException)
                {
                    throw new CecurityException((int)MQMessages.APP_ERR_REGISTER_EFACTURE_JOB, $"Error registering eFacture job: {apiException.Message}", null);
                }
                catch (Exception exception)
                {
                    throw new CecurityException((int)MQMessages.APP_ERR_REGISTER_EFACTURE_JOB, $"Error registering eFacture job: {exception.Message}", null);
                }
            }
            else
                throw new CecurityException((int)MQMessages.APP_ERR_NOTYETIMPLEMENTED, $"Upload for solution '{transferObject.SolutionName}' not yet implemented", null);

            //Cleanup transfer folder
            if (Directory.Exists(transferFolder))
                Directory.Delete(transferFolder, true);

            _logger.LogInformation($"UploadController/EndOfTransfer: Finished. TransferId = {transferId}");
        }

        public void AbortTransfer(string transferId)
        {
            if (string.IsNullOrWhiteSpace(transferId))
                throw new CecurityException((int)MQMessages.APP_ERR_NO_TRANSFERID, MQErrorMessages[(int)MQMessages.APP_ERR_NO_TRANSFERID].Description, null);
            
            //Get uploadfolder for current transfer
            var uploadFolder = _config["Appsettings:UploadFolder"];
            string transferFolder = Path.Combine(uploadFolder, transferId);

            //Cleanup the folder 
            if (Directory.Exists(transferFolder))
                Directory.Delete(transferFolder, true);

            _logger.LogInformation($"UploadController/AbortTransfer: Finished. TransferId = {transferId}");
        }

        #region EPAIE

        private void RegisterEPaieUpload(string transferId, string subscriptionId, List<FileWithFingerPrintInfo> uploadedFiles)
        {
            var epaieUploadsApi = new EPaieUploadApi(_epaieConfig);

            //Get subscriptions for subscriptionId
            var subscriptionsApi = new PlatformSubscriptionsApi(_platformConfig);
            var subscription = subscriptionsApi.SubscriptionById(subscriptionId);
            var solutionParams = JsonConvert.DeserializeObject<SolutionParams>(subscription.SolutionParams);
            var subscriptionParams = JsonConvert.DeserializeObject<EPaieParams>(solutionParams.Solution.Params);

            //If no input formats defined return an error
            if (subscriptionParams.AllowedInputFormats.Count == 0)
                throw new Exception("No allowed input formats have been defined for the subscription");

            var userId = _user.FindFirst(ClaimTypes.NameIdentifier).Value;
            var providerId = _user.FindFirst(ClaimTypes.PrimarySid).Value.Split('\\')[0];
            var tenantId = _user.FindFirst(ClaimTypes.PrimarySid).Value.Split('\\')[1];
            var loginName = _user.FindFirst(ClaimTypes.Name).Value;
            var displayName = _user.FindFirst(ClaimTypes.UserData).Value;

            var upload = epaieUploadsApi.EPAIEEPaieUploadGetUploadByTransferIdGet(transferId);
            if (upload == null)
            {               
                upload = epaieUploadsApi.EPAIEEPaieUploadPost(new Epaie.Api.Client.Model.UploadAddUpdateRequest() 
                {
                    SubscriptionId = subscriptionId,
                    TransferId = transferId,
                    FullName = displayName, 
                    LastActionTimeStamp = DateTime.Now,
                    LoginName = loginName,
                    ProviderId = providerId,
                    TenantId = tenantId,
                    UserId = userId
                });
            }

            var registeredUploadedFiles = new List<Epaie.Api.Client.Model.UploadedFile>();
            //Register each file
            foreach (var fileInfo in uploadedFiles)
            {
                Epaie.Api.Client.Model.UploadedFile uploadedFile = epaieUploadsApi.EPAIEEPaieUploadFilesGet(upload.Id, fileInfo.FileName).FirstOrDefault();
                if (uploadedFile == null)
                {
                    uploadedFile = epaieUploadsApi.EPAIEEPaieUploadFilesPost(new Epaie.Api.Client.Model.UploadedFileAddUpdateRequest()
                    {
                        FileSize = fileInfo.FileSize,
                        FingerPrint = fileInfo.FingerPrint,
                        FingerPrintAlgorithm = fileInfo.FingerPrintAlgorithm,
                        OriginalFileName = fileInfo.FileName,
                        NewFileName = "",
                        State = 1,
                        UploadId = upload.Id
                    });
                }

                registeredUploadedFiles.Add(uploadedFile);
            }

            try
            {
                RegisterEPaieJob(transferId, solutionParams, subscriptionParams, registeredUploadedFiles, upload, epaieUploadsApi);
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

        private void RegisterEPaieJob(string transferId, SolutionParams solutionParams, EPaieParams subscriptionParams, List<Epaie.Api.Client.Model.UploadedFile> uploadedFiles, Epaie.Api.Client.Model.Upload upload, EPaieUploadApi epaieUploadsApi)
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

            if (subscriptionParams != null)
            {
                if (accompanyingFile && !(subscriptionParams.AllowedInputFormats.Contains(EPaieInputFormat.PDFCSV) || subscriptionParams.AllowedInputFormats.Contains(EPaieInputFormat.PDFA3CSV)))
                {
                    throw new Exception("PDF with accompanying file not allowed as input format for this subscription !");
                }
                else if (!accompanyingFile && !(subscriptionParams.AllowedInputFormats.Contains(EPaieInputFormat.PDF) || subscriptionParams.AllowedInputFormats.Contains(EPaieInputFormat.PDFA3CSV)))
                {
                    throw new Exception("PDF file not allowed as input format for this subscription !");
                }
            }

            Connector easConnector = null;

            if (solutionParams.Connectors != null)
                easConnector = solutionParams.Connectors.FirstOrDefault(q => q.Name.Equals("EAS"));

            EasConnectorParams easConnectorParams = null;

            if (easConnector != null && easConnector.Params != null)
                easConnectorParams = JsonConvert.DeserializeObject<EasConnectorParams>(solutionParams.Connectors.First(q => q.Name.ToUpper().Equals("EAS")).Params);

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

                epaieUploadsApi.EPAIEEPaieUploadFilesUploadedFileIdPut(uploadedFile.Id, new Epaie.Api.Client.Model.UploadedFileAddUpdateRequest()
                {
                    FileSize = uploadedFile.FileSize,
                    FingerPrint = uploadedFile.FingerPrint,
                    FingerPrintAlgorithm = uploadedFile.FingerPrintAlgorithm,
                    NewFileName = uploadedFile.NewFileName,
                    OriginalFileName = uploadedFile.OriginalFileName,
                    State = uploadedFile.State,
                    UploadId = uploadedFile.UploadId
                });
            }

            upload.State = 2;
            upload.LastActionTimeStamp = DateTime.Now;
            epaieUploadsApi.EPAIEEPaieUploadUploadIdPut(upload.Id, new Epaie.Api.Client.Model.UploadAddUpdateRequest()
            {
                FullName = upload.FullName,
                LastActionTimeStamp = upload.LastActionTimeStamp,
                LoginName = upload.LoginName,
                ProviderId = upload.ProviderId,
                State = upload.State,
                SubscriptionId = upload.SubscriptionId,
                TenantId = upload.TenantId,
                TransferId = upload.TransferId,
                UserId = upload.UserId
            });

            // Create new job if not exist
            var epaieJobsApi = new EPaieJobsApi(_epaieConfig);

            Epaie.Api.Client.Model.Job newJob = epaieJobsApi.EPAIEEPaieJobsGet(transferId).FirstOrDefault();
            if (newJob == null)
            {
                newJob = epaieJobsApi.EPAIEEPaieJobsPost(new Epaie.Api.Client.Model.JobAddUpdateRequest()
                {
                    TransferId = transferId,
                    SubscriptionId = upload.SubscriptionId,
                    JobReference = "EPAIE",
                    JobState = Epaie.Api.Client.Model.JobState.NUMBER_0,
                    TaskReference = "Deposit"
                });

                _logger.LogInformation($"UploadController/EndOfTransfer: Created new job, jobId = {newJob.Id}. TransferId = {transferId}");
            }
            _logger.LogInformation($"UploadController/EndOfTransfer: Job = {JsonConvert.SerializeObject(newJob)}. TransferId = {transferId}");

            // Create job summary if not exist
            Epaie.Api.Client.Model.JobSummary newJobSummary = epaieJobsApi.EPAIEEPaieJobsSummaryGet(newJob.Id);

            if (newJobSummary == null)
            {
                newJobSummary = epaieJobsApi.EPAIEEPaieJobsSummaryPost(new Epaie.Api.Client.Model.JobSummaryAddUpdateRequest()
                {
                    JobId = newJob.Id,
                    NbrOfFiles = uploadedFiles.Count,
                    DepositedBy = upload.FullName,
                    DepositedAt = upload.TimeStamp,
                    DistributionPlannedAt = upload.TimeStamp.AddDays(3)
                });

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

            var epaiePreprocessingFolder = _config["Appsettings:EpaiePreprocessingFolder"];
            _logger.LogInformation($"UploadController/EndOfTransfer: EpaiePreprocessingFolder = {epaiePreprocessingFolder}. TransferId = {transferId}");
            var epaieLegacyFolder = _config["Appsettings:EpaieLegacyFolder"];
            _logger.LogInformation($"UploadController/EndOfTransfer: EpaieLegacyFolder = {epaieLegacyFolder}. TransferId = {transferId}");
            var uploadFolder = _config["Appsettings:UploadFolder"];
            _logger.LogInformation($"UploadController/EndOfTransfer: UploadFolder = {uploadFolder}. TransferId = {transferId}");

            bool isLegacy = false;

            if (solutionParams.Options != null)
            {
                isLegacy = solutionParams.Options.Any(q => q.Name.ToLower().Equals("preprocessing") && q.Value == 0);
            }

            _logger.LogInformation($"UploadControllerEndOfTransfer: ArchiveOrValidation = {isLegacy}");

            var destinationFolder = epaiePreprocessingFolder;

            if (isLegacy)
            {
                ip.customer.solution = "EPaie Distribution";
                destinationFolder = epaieLegacyFolder;
            }

            uploadedFiles = epaieUploadsApi.EPAIEEPaieUploadFilesGet(upload.Id);

            long totalBytesDep = 0;

            foreach (var file in uploadedFiles)
            {
                var sourceFile = Path.Combine(Path.Combine(uploadFolder, transferId), file.OriginalFileName);
                var destinationFile = Path.Combine(destinationFolder, file.NewFileName);

                _logger.LogInformation($"UploadController/EndOfTransfer: Moving file, source = {sourceFile}, destination = {destinationFile}. TransferId = {transferId}");

                System.IO.File.Move(sourceFile, destinationFile);
                
                totalBytesDep += new System.IO.FileInfo(destinationFile).Length;

                ip.dataset.data.Add(System.IO.Path.GetFileName(destinationFile));
            }

            var ipFile = Path.Combine(destinationFolder, Guid.NewGuid().ToString().Replace("-", "") + ".dsp");

            _logger.LogInformation($"UploadController/EndOfTransfer: Saving information package to {ipFile}. TransferId = {transferId}");

            // Save the information package
            InformationPackage.SaveIP(ip, ipFile);

            var platformResellerApi = new PlatformResellersApi(_platformConfig);

            var data = new List<Metrics.Api.Client.Model.NameValuePair>();
            data.Add(new Metrics.Api.Client.Model.NameValuePair() { Name = "SubscriptionId", Value = newJob.SubscriptionId });
            data.Add(new Metrics.Api.Client.Model.NameValuePair() { Name = "JobId", Value = newJob.Id });
            data.Add(new Metrics.Api.Client.Model.NameValuePair() { Name = "TransferId", Value = newJob.TransferId });

            var resellers = platformResellerApi.PlatformPlatformResellersGetParentResellersBySubscriptionIdGet(newJob.SubscriptionId);
            if (resellers != null)
            {
                var resellersDeserialized = JsonConvert.DeserializeObject(resellers).ToString();
                var resellersObjects = JsonConvert.DeserializeObject<Dictionary<string, object>>(resellersDeserialized);
                foreach (var resellersObject in resellersObjects)
                    data.Add(new Metrics.Api.Client.Model.NameValuePair() { Name = resellersObject.Key, Value = resellersObject.Value.ToString() });
            }

            data.Add(new Metrics.Api.Client.Model.NameValuePair() { Name = "Siren", Value = subscriptionParams.SIREN });
            data.Add(new Metrics.Api.Client.Model.NameValuePair() { Name = "ClientCode", Value = subscriptionParams.CodeClient });
            data.Add(new Metrics.Api.Client.Model.NameValuePair() { Name = "MgmtCode", Value = subscriptionParams.CodeAgence });
            data.Add(new Metrics.Api.Client.Model.NameValuePair() { Name = "DepositedFiles", Value = newJobSummary.NbrOfFiles.ToString() });
            data.Add(new Metrics.Api.Client.Model.NameValuePair() { Name = "TotalBytesDeposited", Value = totalBytesDep.ToString() });
            data.Add(new Metrics.Api.Client.Model.NameValuePair() { Name = "CompanyName", Value = subscriptionParams.Entreprise });
            data.Add(new Metrics.Api.Client.Model.NameValuePair() { Name = "BillingReseller", Value = subscriptionParams.EntiteFacturable });
            data.Add(new Metrics.Api.Client.Model.NameValuePair() { Name = "NbrEmployees", Value = subscriptionParams.NombreEmployes });

            CommonService.UpdateMtricsForJobId((int)MQMessages.MET_INF_IMPORTATION_EPAIE, data, _config);

            // Save job
            _logger.LogInformation($"UploadController/EndOfTransfer: Saving job. TransferId = {transferId}");
        }

        #endregion

        #region EFACTURE

        private void RegisterEFactureUpload(string transferId, string subscriptionId, List<FileWithFingerPrintInfo> uploadedFiles)
        {
            var efactureUploadsApi = new EFactureUploadApi(_efactureConfig);

            //Get subscriptions for subscriptionId
            var subscriptionsApi = new PlatformSubscriptionsApi(_platformConfig);
            var subscription = subscriptionsApi.SubscriptionById(subscriptionId);
            var solutionParams = JsonConvert.DeserializeObject<SolutionParams>(subscription.SolutionParams);
            var subscriptionParams = JsonConvert.DeserializeObject<EFactureParams>(solutionParams.Solution.Params);

            //If no input formats defined return an error
            if (subscriptionParams.AllowedInputFormats.Count == 0)
                throw new Exception("No allowed input formats have been defined for the subscription");

            var userId = _user.FindFirst(ClaimTypes.NameIdentifier).Value;
            var providerId = _user.FindFirst(ClaimTypes.PrimarySid).Value.Split('\\')[0];
            var tenantId = _user.FindFirst(ClaimTypes.PrimarySid).Value.Split('\\')[1];
            var loginName = _user.FindFirst(ClaimTypes.Name).Value;
            var displayName = _user.FindFirst(ClaimTypes.UserData).Value;

            var upload = efactureUploadsApi.EFACTUREEFactureUploadGetUploadByTransferIdGet(transferId);
            if (upload == null)
            {
                upload = efactureUploadsApi.EFACTUREEFactureUploadPost(new Efacture.Api.Client.Model.UploadAddUpdateRequest()
                {
                    SubscriptionId = subscriptionId,
                    TransferId = transferId,
                    FullName = displayName,
                    LastActionTimeStamp = DateTime.Now,
                    LoginName = loginName,
                    ProviderId = providerId,
                    TenantId = tenantId,
                    UserId = userId
                });
            }

            var registeredUploadedFiles = new List<Efacture.Api.Client.Model.UploadedFile>();
            //Register each file
            foreach (var fileInfo in uploadedFiles)
            {
                Efacture.Api.Client.Model.UploadedFile uploadedFile = efactureUploadsApi.EFACTUREEFactureUploadFilesGet(upload.Id, fileInfo.FileName).FirstOrDefault();
                if (uploadedFile == null)
                {
                    uploadedFile = efactureUploadsApi.EFACTUREEFactureUploadFilesPost(new Efacture.Api.Client.Model.UploadedFileAddUpdateRequest()
                    {
                        FileSize = fileInfo.FileSize,
                        FingerPrint = fileInfo.FingerPrint,
                        FingerPrintAlgorithm = fileInfo.FingerPrintAlgorithm,
                        OriginalFileName = fileInfo.FileName,
                        NewFileName = "",
                        State = 1,
                        UploadId = upload.Id
                    });
                }

                registeredUploadedFiles.Add(uploadedFile);
            }

            try
            {
                RegisterEFactureJob(transferId, solutionParams, subscriptionParams, registeredUploadedFiles, upload, efactureUploadsApi);
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

        private void RegisterEFactureJob(string transferId, SolutionParams solutionParams, EFactureParams subscriptionParams, List<Efacture.Api.Client.Model.UploadedFile> uploadedFiles, Efacture.Api.Client.Model.Upload upload, EFactureUploadApi efactureUploadsApi)
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

            if (subscriptionParams != null)
            {
                if (accompanyingFile && !subscriptionParams.AllowedInputFormats.Contains(EFactureInputFormat.PDFCSV))
                {
                    throw new Exception("PDF with accompanying file not allowed as input format for this subscription !");
                }
                else if (!accompanyingFile && !subscriptionParams.AllowedInputFormats.Contains(EFactureInputFormat.PDF))
                {
                    throw new Exception("PDF file not allowed as input format for this subscription !");
                }
            }

            Connector easConnector = null;

            if (solutionParams.Connectors != null)
                easConnector = solutionParams.Connectors.FirstOrDefault(q => q.Name.Equals("EAS"));

            EasConnectorParams easConnectorParams = null;

            if (easConnector != null && easConnector.Params != null)
                easConnectorParams = JsonConvert.DeserializeObject<EasConnectorParams>(solutionParams.Connectors.First(q => q.Name.ToUpper().Equals("EAS")).Params);

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

                efactureUploadsApi.EFACTUREEFactureUploadFilesUploadedFileIdPut(uploadedFile.Id, new Efacture.Api.Client.Model.UploadedFileAddUpdateRequest()
                {
                    FileSize = uploadedFile.FileSize,
                    FingerPrint = uploadedFile.FingerPrint,
                    FingerPrintAlgorithm = uploadedFile.FingerPrintAlgorithm,
                    NewFileName = uploadedFile.NewFileName,
                    OriginalFileName = uploadedFile.OriginalFileName,
                    State = uploadedFile.State,
                    UploadId = uploadedFile.UploadId
                });
            }

            upload.State = 2;
            upload.LastActionTimeStamp = DateTime.Now;
            efactureUploadsApi.EFACTUREEFactureUploadUploadIdPut(upload.Id, new Efacture.Api.Client.Model.UploadAddUpdateRequest()
            {
                FullName = upload.FullName,
                LastActionTimeStamp = upload.LastActionTimeStamp,
                LoginName = upload.LoginName,
                ProviderId = upload.ProviderId,
                State = upload.State,
                SubscriptionId = upload.SubscriptionId,
                TenantId = upload.TenantId,
                TransferId = upload.TransferId,
                UserId = upload.UserId
            });

            // Create new job if not exist
            var efactureJobsApi = new EFactureJobsApi(_efactureConfig);

            Efacture.Api.Client.Model.Job newJob = efactureJobsApi.EFACTUREEFactureJobsGet(transferId).FirstOrDefault();
            if (newJob == null)
            {
                newJob = efactureJobsApi.EFACTUREEFactureJobsPost(new Efacture.Api.Client.Model.JobAddUpdateRequest()
                {
                    TransferId = transferId,
                    SubscriptionId = upload.SubscriptionId,
                    JobReference = "EPAIE",
                    JobState = Efacture.Api.Client.Model.JobState.NUMBER_0,
                    TaskReference = "Deposit"
                });

                _logger.LogInformation($"UploadController/EndOfTransfer: Created new job, jobId = {newJob.Id}. TransferId = {transferId}");
            }
            _logger.LogInformation($"UploadController/EndOfTransfer: Job = {JsonConvert.SerializeObject(newJob)}. TransferId = {transferId}");

            // Create job summary if not exist
            Efacture.Api.Client.Model.JobSummary newJobSummary = efactureJobsApi.EFACTUREEFactureJobsSummaryGet(newJob.Id);

            if (newJobSummary == null)
            {
                newJobSummary = efactureJobsApi.EFACTUREEFactureJobsSummaryPost(new Efacture.Api.Client.Model.JobSummaryAddUpdateRequest()
                {
                    JobId = newJob.Id,
                    NbrOfFiles = uploadedFiles.Count,
                    DepositedBy = upload.FullName,
                    DepositedAt = upload.TimeStamp,
                    DistributionPlannedAt = upload.TimeStamp.AddDays(3)
                });

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

            var efacturePreprocessingFolder = _config["Appsettings:EfacturePreprocessingFolder"];
            _logger.LogInformation($"UploadController/EndOfTransfer: EpaiePreprocessingFolder = {efacturePreprocessingFolder}. TransferId = {transferId}");
            var efacturePrevalidationFolder = _config["Appsettings:EfacturePrevalidationFolder"];
            _logger.LogInformation($"UploadController/EndOfTransfer: EpaiePreprocessingFolder = {efacturePrevalidationFolder}. TransferId = {transferId}");
            var uploadFolder = _config["Appsettings:UploadFolder"];
            _logger.LogInformation($"UploadController/EndOfTransfer: UploadFolder = {uploadFolder}. TransferId = {transferId}");
            
            var destinationFolder = efacturePreprocessingFolder;

            if (accompanyingFile)
            {
                ip.customer.solution = "EFacture Validation";
                destinationFolder = efacturePrevalidationFolder;
            }

            uploadedFiles = efactureUploadsApi.EFACTUREEFactureUploadFilesGet(upload.Id);

            long totalBytesDep = 0;

            foreach (var file in uploadedFiles)
            {
                var sourceFile = Path.Combine(Path.Combine(uploadFolder, transferId), file.OriginalFileName);
                var destinationFile = Path.Combine(destinationFolder, file.NewFileName);

                _logger.LogInformation($"UploadController/EndOfTransfer: Moving file, source = {sourceFile}, destination = {destinationFile}. TransferId = {transferId}");

                System.IO.File.Move(sourceFile, destinationFile);

                totalBytesDep += new System.IO.FileInfo(destinationFile).Length;

                ip.dataset.data.Add(System.IO.Path.GetFileName(destinationFile));
            }

            var ipFile = Path.Combine(destinationFolder, Guid.NewGuid().ToString().Replace("-", "") + ".dsp");

            _logger.LogInformation($"UploadController/EndOfTransfer: Saving information package to {ipFile}. TransferId = {transferId}");

            // Save the information package
            InformationPackage.SaveIP(ip, ipFile);

            var data = new List<Metrics.Api.Client.Model.NameValuePair>();
            data.Add(new Metrics.Api.Client.Model.NameValuePair() { Name = "SubscriptionId", Value = newJob.SubscriptionId });
            data.Add(new Metrics.Api.Client.Model.NameValuePair() { Name = "JobId", Value = newJob.Id });
            data.Add(new Metrics.Api.Client.Model.NameValuePair() { Name = "TransferId", Value = newJob.TransferId });

            var platformResellerApi = new PlatformResellersApi(_platformConfig);

            var resellers = platformResellerApi.PlatformPlatformResellersGetParentResellersBySubscriptionIdGet(newJob.SubscriptionId);
            if (resellers != null)
            {
                var resellersDeserialized = JsonConvert.DeserializeObject(resellers).ToString();
                var resellersObjects = JsonConvert.DeserializeObject<Dictionary<string, object>>(resellersDeserialized);
                foreach (var resellersObject in resellersObjects)
                    data.Add(new Metrics.Api.Client.Model.NameValuePair() { Name = resellersObject.Key, Value = resellersObject.Value.ToString() });
            }

            data.Add(new Metrics.Api.Client.Model.NameValuePair() { Name = "Siren", Value = subscriptionParams.SIREN });
            data.Add(new Metrics.Api.Client.Model.NameValuePair() { Name = "ClientCode", Value = subscriptionParams.CodeClient });
            data.Add(new Metrics.Api.Client.Model.NameValuePair() { Name = "MgmtCode", Value = subscriptionParams.CodeAgence });
            data.Add(new Metrics.Api.Client.Model.NameValuePair() { Name = "DepositedFiles", Value = newJobSummary.NbrOfFiles.ToString() });
            data.Add(new Metrics.Api.Client.Model.NameValuePair() { Name = "TotalBytesDeposited", Value = totalBytesDep.ToString() });
            data.Add(new Metrics.Api.Client.Model.NameValuePair() { Name = "CompanyName", Value = subscriptionParams.Entreprise });
            data.Add(new Metrics.Api.Client.Model.NameValuePair() { Name = "BillingReseller", Value = subscriptionParams.EntiteFacturable });
            data.Add(new Metrics.Api.Client.Model.NameValuePair() { Name = "NbrEmployees", Value = subscriptionParams.NombreEmployes });

            CommonService.UpdateMtricsForJobId((int)MQMessages.MET_INF_IMPORTATION_EFACTURE, data, _config);

            // Save job
            _logger.LogInformation($"UploadController/EndOfTransfer: Saving job. TransferId = {transferId}");
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
                        using (System.Security.Cryptography.MD5 md5Hash = System.Security.Cryptography.MD5.Create())
                        {
                            uploadedFileFingerPrint = BitConverter.ToString(md5Hash.ComputeHash(stream)).Replace("-", "").ToLower();
                        }
                        break;
                    case "SHA-1":
                        using (System.Security.Cryptography.SHA1 sha1Hash = System.Security.Cryptography.SHA1.Create())
                        {
                            uploadedFileFingerPrint = BitConverter.ToString(sha1Hash.ComputeHash(stream)).Replace("-", "").ToLower();
                        }
                        break;
                    case "SHA-256":
                        using (System.Security.Cryptography.SHA256 sha256Hash = System.Security.Cryptography.SHA256.Create())
                        {
                            uploadedFileFingerPrint = BitConverter.ToString(sha256Hash.ComputeHash(stream)).Replace("-", "").ToLower();
                        }
                        break;
                    case "SHA-512":
                        using (System.Security.Cryptography.SHA512 sha512Hash = System.Security.Cryptography.SHA512.Create())
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