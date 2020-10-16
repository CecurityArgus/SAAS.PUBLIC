using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PUBLIC.SERVICE.LIB.Helpers
{
    /// <summary>
    /// PUBLIC Message Queue messages
    /// -------------------------------
    /// Message types:
    /// Error: ERR, Information: INF, Warning: WAR
    /// 
    /// Application messages: 
    /// APP_ERR: 100, APP_INF: 101, APP_WAR: 102
    /// Metrics messages
    /// MET_ERR: 200, MET_INF: 201, MET_WAR: 202
    /// Security messages
    /// SEC_ERR: 300, SEC_INF: 301, SEC_WAR: 302
    /// System messages
    /// SYS_ERR: 400, SYS_INF: 401, SYS_WAR: 402
    /// 
    /// Application PUBLIC Code: 005
    /// 
    /// Examples:
    /// Unhandled error: APP_ERR_UNHANDLED: 100005999
    /// Logon event: SEC_INF_ACCOUNT_VALIDATED: 301005001
    /// </summary>
    public class MQErrors
    {
        public enum MQMessages
        {
            APP_ERR_UNHANDLED = 100005999,
            APP_ERR_NOTYETIMPLEMENTED = 100005000,
            APP_ERR_APIKEYS,
            APP_ERR_NO_APIKEY_PROVIDED,
            APP_ERR_AUTHENTICATE_FAILED,
            APP_ERR_EMPTY_SOLUTION_NAME,
            APP_ERR_EMPTY_SOLUTION_REFERENCE,
            APP_ERR_NO_UPLOAD_FILES,
            APP_ERR_NO_AUTHORIZED_SUBSCRIPTIONS,
            APP_ERR_SOLUTION_NAME_NOT_FOUND_AUTHORIZED,
            APP_ERR_SOLUTION_REF_NOT_FOUND_AUTHORIZED,
            APP_ERR_BEGIN_OF_TRANSFER,
            APP_ERR_NO_TRANSFERID,
            APP_ERR_NO_FILES_PROVIDED,
            APP_ERR_UPLOAD_FOLDER_NOT_FOUND,
            APP_ERR_TRANSFER_CONFIG_FILE_NOT_FOUND,
            APP_ERR_FILE_LENGTH_NULL,
            APP_ERR_UNKNOWN_FILE,
            APP_ERR_FILE_SIZE_NOT_MATCH,
            APP_ERR_FINGERPRINT_CHECK,
            APP_ERR_UPLOAD,
            APP_ERR_END_OF_TRANSFER,
            APP_ERR_NOT_ALL_FILES_UPLOADED,
            APP_ERR_REGISTER_EPAIE_JOB,
            APP_ERR_REGISTER_EFACTURE_JOB,
            APP_ERR_ABORT_TRANSFER,

            APP_INF_APPLICATIONSTARTED = 101005001,

            //EPAIE METRICS
            MET_INF_IMPORTATION_EPAIE = 201003001,
            MET_INF_PREPARATION_EPAIE,
            MET_INF_VALIDATION_EPAIE,
            MET_INF_ARCHIVING_EPAIE,
            MET_INF_DISTRIBUTION_EPAIE,
            MET_INF_BPO_EPAIE,

            //EFACTURE METRICS
            MET_INF_IMPORTATION_EFACTURE = 201004001,
            MET_INF_PREPARATION_EFACTURE,
            MET_INF_VALIDATION_EFACTURE,
            MET_INF_SIGNING_EFACTURE,
            MET_INF_ARCHIVING_EFACTURE,
            MET_INF_DISTRIBUTION_EFACTURE,
            MET_INF_BPO_EFACTURE,
        }

        public class MQError
        {
            [DataMember(Name = "description")]
            public string Description { get; set; }
            [DataMember(Name = "shortDescription")]
            public string ShortDescription { get; set; }
        }

        public static Dictionary<int, MQError> MQErrorMessages = new Dictionary<int, MQError>() {
            { (int)MQMessages.APP_ERR_UNHANDLED, new MQError { ShortDescription = "UnhandledException", Description = "An unhandled exception occurred!"}},
            { (int)MQMessages.APP_ERR_NOTYETIMPLEMENTED, new MQError { ShortDescription = "NotYetImplemented", Description = "Error not yet implemented!"}},
            { (int)MQMessages.APP_ERR_APIKEYS, new MQError { ShortDescription = "ApiKeys", Description = "Error occured in the apikeys management!"}},
            { (int)MQMessages.APP_ERR_NO_APIKEY_PROVIDED, new MQError { ShortDescription = "NoApiKeyProvided", Description = "No APIKey provided!"}},
            { (int)MQMessages.APP_ERR_AUTHENTICATE_FAILED, new MQError { ShortDescription = "Authenticate", Description = "Authentication failed!"}},
            { (int)MQMessages.APP_ERR_EMPTY_SOLUTION_NAME, new MQError { ShortDescription = "EmptySolutionName", Description = "Solution name cannot be empty!"}},
            { (int)MQMessages.APP_ERR_EMPTY_SOLUTION_REFERENCE, new MQError { ShortDescription = "EmptySolutionReference", Description = "Solution reference cannot be empty!"}},
            { (int)MQMessages.APP_ERR_NO_UPLOAD_FILES, new MQError { ShortDescription = "NoUploadFiles", Description = "No files provided for the upload!"}},
            { (int)MQMessages.APP_ERR_NO_AUTHORIZED_SUBSCRIPTIONS, new MQError { ShortDescription = "NoAuthorizedSubscriptions", Description = "No authorized subscriptions for user!"}},
            { (int)MQMessages.APP_ERR_SOLUTION_NAME_NOT_FOUND_AUTHORIZED, new MQError { ShortDescription = "SolutionNameNotFoundAuthorized", Description = "Solution name not found or not authorized!"}},
            { (int)MQMessages.APP_ERR_SOLUTION_REF_NOT_FOUND_AUTHORIZED, new MQError { ShortDescription = "SolutionRefNotFoundAuthorized", Description = "Subscription not found or not authorized!"}},
            { (int)MQMessages.APP_ERR_BEGIN_OF_TRANSFER, new MQError { ShortDescription = "BeginOfTransfer", Description = "Error occured in begin of transfer!"}},
            { (int)MQMessages.APP_ERR_NO_TRANSFERID, new MQError { ShortDescription = "NoTransferId", Description = "Invalid or no transfer id provided!"}},
            { (int)MQMessages.APP_ERR_NO_FILES_PROVIDED, new MQError { ShortDescription = "NoFilesProvided", Description = "No files provided for the upload!"}},
            { (int)MQMessages.APP_ERR_UPLOAD_FOLDER_NOT_FOUND, new MQError { ShortDescription = "UploadFolderNotFound", Description = "No transfer upload folder found for the given transferId!"}},
            { (int)MQMessages.APP_ERR_TRANSFER_CONFIG_FILE_NOT_FOUND, new MQError { ShortDescription = "TransferConfigFileNotFound", Description = "No transfer configuration file found for the given transferId!"}},
            { (int)MQMessages.APP_ERR_FILE_LENGTH_NULL, new MQError { ShortDescription = "FileLengthNull", Description = "Uploaded file cannot have a null length!"}},
            { (int)MQMessages.APP_ERR_UNKNOWN_FILE, new MQError { ShortDescription = "UnknownFile", Description = "Unknown file provided!"}},
            { (int)MQMessages.APP_ERR_FILE_SIZE_NOT_MATCH, new MQError { ShortDescription = "FileSizeNotMatch", Description = "Filesizes do not match!"}},
            { (int)MQMessages.APP_ERR_FINGERPRINT_CHECK, new MQError { ShortDescription = "FingerPrintCheck", Description = "File fingerprint check failed!"}},
            { (int)MQMessages.APP_ERR_UPLOAD, new MQError { ShortDescription = "Upload", Description = "Error occured while uploading files!"}},
            { (int)MQMessages.APP_ERR_END_OF_TRANSFER, new MQError { ShortDescription = "EndfTransfer", Description = "Error occured in end of transfer!"}},
            { (int)MQMessages.APP_ERR_NOT_ALL_FILES_UPLOADED, new MQError { ShortDescription = "NotAllFileUpload", Description = "Not all files have been uploaded!"}},
            { (int)MQMessages.APP_ERR_REGISTER_EPAIE_JOB, new MQError { ShortDescription = "RegisterEpaieJob", Description = "Error registering ePaie job!"}},
            { (int)MQMessages.APP_ERR_REGISTER_EFACTURE_JOB, new MQError { ShortDescription = "RegisterEfactureJob", Description = "Error registering eFacture job!"}},
            { (int)MQMessages.APP_ERR_ABORT_TRANSFER, new MQError { ShortDescription = "AbortTransfer", Description = "Error aborting transfer!"}},

            { (int)MQMessages.APP_INF_APPLICATIONSTARTED, new MQError { ShortDescription = "ApplicationStarted", Description = "PUBLIC Rest API application started" }},
        };
    }
}
