using Public.Api.Client.Api;
using Public.Api.Client.Model;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Public_Client_Test
{
    public partial class Form1 : Form
    {
        private MdlAuthenticated  _mdlAuthenticated;
        private AuthenticationApi _authenticationApi;
        private UploadApi         _uploadApi;

        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Log in with your login name and password.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ev"></param>
        private void Login_Click(object sender, EventArgs ev)
        {
            try
            {
                //creating config object for api
                var configuration = new Public.Api.Client.Client.Configuration();
                configuration.DefaultHeaders.Add("X-ApiKey", txtApiKey.Text);
                configuration.BasePath = publicUri.Text;

                //creating instance
                _authenticationApi = new Public.Api.Client.Api.AuthenticationApi(configuration);

                //executing login
                _mdlAuthenticated = _authenticationApi.PublicAuthenticationAuthenticatePost(new Public.Api.Client.Model.MdlLogonUser
                                                                    {
                                                                        Username = txtUserName.Text,
                                                                        Password = txtPassword.Text
                                                                    });

                Response.Text         += "Successful login:\n" + _mdlAuthenticated.ToJson();
                grpConnection.Enabled =  false;
                //grpLogin.Enabled      =  false;
                grpUpoad.Enabled      =  true;
            }
            catch (System.Exception e)
            {
                AddText($"\nERROR: {e.Source}\n{e.Message}\n{e.InnerException}\n{e.StackTrace}", true);
            }
        }

        /// <summary>
        /// Clear the textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClear_Click(object sender, EventArgs e)
        {
            Response.Clear();
        }

        /// <summary>
        /// Select one or more files to upload
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ev"></param>
        private void btnBrowse_Click(object sender, EventArgs ev)
        {
            try
            {
                if (openFileDialog1.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                txtFiles.Clear();

                foreach (var file in openFileDialog1.SafeFileNames)
                {
                    txtFiles.AppendText(file + "; ");
                }
            }
            catch (System.Exception e)
            {
                AddText($"\nERROR: {e.Source}\n{e.Message}\n{e.InnerException}\n{e.StackTrace}", true);
            }
        }

        /// <summary>
        /// start upload
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ev"></param>
        private void btnUpload_Click(object sender, EventArgs ev)
        {
            try
            {
                //creating config object for api
                var configuration = new Public.Api.Client.Client.Configuration();
                configuration.DefaultHeaders.Add("Authorization", "Bearer " + _mdlAuthenticated.AccessToken);
                configuration.BasePath = publicUri.Text;

                //creating instance
                _uploadApi = new Public.Api.Client.Api.UploadApi(configuration);

                //register transfer
                var uploadFiles = new List<FileWithFingerPrintInfo>();

                //collect file info
                foreach (var fileName in openFileDialog1.FileNames)
                {
                    var fileInfo = new FileInfo(fileName);

                    var file = new FileWithFingerPrintInfo
                               {
                                   FileName             = fileInfo.Name,
                                   FileSize             = fileInfo.Length,
                                   FingerPrint          = null,
                                   FingerPrintAlgorithm = null
                               };

                    uploadFiles.Add(file);
                }

                //register transfer to server
                var transfer = _uploadApi.PublicUploadBeginOfTransferPost(new RegisterBeginOfTransfer
                                                          {
                                                              SolutionName      = cmbSolutionName.Text,
                                                              SolutionReference = txtSiren.Text,
                                                              UploadFiles       = uploadFiles
                                                          });

                //add transfer reference to Client instance
                _uploadApi.Configuration.DefaultHeaders.Add("X-TransferId", transfer.TransferId);

                //commence upload
                UploadFiles(transfer);

                //end upload
                _uploadApi.PublicUploadEndOfTransferPost();

                //remove transfer Id from header
                _uploadApi.Configuration.DefaultHeaders.Remove("X-TransferId");
            }
            catch (System.Exception e)
            {
                AddText($"\nERROR: {e.Source}\n{e.Message}\n{e.InnerException}\n{e.StackTrace}", true);
            }
        }

        private void UploadFiles(TransferRegistered transfer)
        {
            try
            {
                var restClient = new RestSharp.RestClient(publicUri.Text);

                var request = new RestSharp.RestRequest("/Public/Upload/UploadFiles", RestSharp.Method.POST)
                              {
                                  AlwaysMultipartFormData = true
                              };

                request.AddHeader("Content-Type",  "multipart/form-data");
                request.AddHeader("X-TransferId",  transfer.TransferId);
                request.AddHeader("Authorization", "Bearer " + _mdlAuthenticated.AccessToken);

                foreach (var fileName in openFileDialog1.FileNames)
                {
                    var fileInfo = new FileInfo(fileName);

                    request.AddFile(fileInfo.Name, fileName);
                }

                var response = restClient.Execute(request);

                AddText("\nUpload completed: ");
            }
            catch (System.Exception e)
            {
                AddText($"\nERROR: {e.Source}\n{e.Message}\n{e.InnerException}\n{e.StackTrace}", true);
            }
        }

        private void AddText(string text, bool isError = false)
        {
            if (isError)
            {
                Response.SelectionColor = Color.Red;
            }

            Response.AppendText(text);

            Response.SelectedText   = text;
            Response.SelectionColor = Response.ForeColor;
        }
    }
}