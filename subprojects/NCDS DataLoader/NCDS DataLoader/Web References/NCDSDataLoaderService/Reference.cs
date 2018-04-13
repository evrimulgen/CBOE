﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.3053
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by Microsoft.VSDesigner, Version 2.0.50727.3053.
// 
#pragma warning disable 1591

namespace CambridgeSoft.NCDS_DataLoader.NCDSDataLoaderService {
    using System.Diagnostics;
    using System.Web.Services;
    using System.ComponentModel;
    using System.Web.Services.Protocols;
    using System;
    using System.Xml.Serialization;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "2.0.50727.3053")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="NCDSDataLoaderServiceSoap", Namespace="http://localhost/NCDSDataLoaderService")]
    public partial class NCDSDataLoaderService : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        private System.Threading.SendOrPostCallback GetNonStructuralDuplicateCheckSettingsOperationCompleted;
        
        private bool useDefaultCredentialsSetExplicitly;
        
        /// <remarks/>
        public NCDSDataLoaderService() {
            this.Url = global::CambridgeSoft.NCDS_DataLoader.Properties.Settings.Default.CambridgeSoft_NCDS_DataLoader_NCDSDataLoaderService_NCDSDataLoaderService;
            if ((this.IsLocalFileSystemWebService(this.Url) == true)) {
                this.UseDefaultCredentials = true;
                this.useDefaultCredentialsSetExplicitly = false;
            }
            else {
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        public new string Url {
            get {
                return base.Url;
            }
            set {
                if ((((this.IsLocalFileSystemWebService(base.Url) == true) 
                            && (this.useDefaultCredentialsSetExplicitly == false)) 
                            && (this.IsLocalFileSystemWebService(value) == false))) {
                    base.UseDefaultCredentials = false;
                }
                base.Url = value;
            }
        }
        
        public new bool UseDefaultCredentials {
            get {
                return base.UseDefaultCredentials;
            }
            set {
                base.UseDefaultCredentials = value;
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        /// <remarks/>
        public event GetNonStructuralDuplicateCheckSettingsCompletedEventHandler GetNonStructuralDuplicateCheckSettingsCompleted;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://localhost/NCDSDataLoaderService/GetNonStructuralDuplicateCheckSettings", RequestNamespace="http://localhost/NCDSDataLoaderService", ResponseNamespace="http://localhost/NCDSDataLoaderService", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string[] GetNonStructuralDuplicateCheckSettings() {
            object[] results = this.Invoke("GetNonStructuralDuplicateCheckSettings", new object[0]);
            return ((string[])(results[0]));
        }
        
        /// <remarks/>
        public void GetNonStructuralDuplicateCheckSettingsAsync() {
            this.GetNonStructuralDuplicateCheckSettingsAsync(null);
        }
        
        /// <remarks/>
        public void GetNonStructuralDuplicateCheckSettingsAsync(object userState) {
            if ((this.GetNonStructuralDuplicateCheckSettingsOperationCompleted == null)) {
                this.GetNonStructuralDuplicateCheckSettingsOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetNonStructuralDuplicateCheckSettingsOperationCompleted);
            }
            this.InvokeAsync("GetNonStructuralDuplicateCheckSettings", new object[0], this.GetNonStructuralDuplicateCheckSettingsOperationCompleted, userState);
        }
        
        private void OnGetNonStructuralDuplicateCheckSettingsOperationCompleted(object arg) {
            if ((this.GetNonStructuralDuplicateCheckSettingsCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetNonStructuralDuplicateCheckSettingsCompleted(this, new GetNonStructuralDuplicateCheckSettingsCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        public new void CancelAsync(object userState) {
            base.CancelAsync(userState);
        }
        
        private bool IsLocalFileSystemWebService(string url) {
            if (((url == null) 
                        || (url == string.Empty))) {
                return false;
            }
            System.Uri wsUri = new System.Uri(url);
            if (((wsUri.Port >= 1024) 
                        && (string.Compare(wsUri.Host, "localHost", System.StringComparison.OrdinalIgnoreCase) == 0))) {
                return true;
            }
            return false;
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "2.0.50727.3053")]
    public delegate void GetNonStructuralDuplicateCheckSettingsCompletedEventHandler(object sender, GetNonStructuralDuplicateCheckSettingsCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "2.0.50727.3053")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetNonStructuralDuplicateCheckSettingsCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal GetNonStructuralDuplicateCheckSettingsCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string[] Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string[])(this.results[0]));
            }
        }
    }
}

#pragma warning restore 1591