﻿using CambridgeSoft.COE.Framework.COETableEditorService;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using CambridgeSoft.COE.ChemBioViz.Services.COEChemBioVizService;
using Csla.Data;
using CambridgeSoft.COE.Framework.COEChemDrawConverterService;
using Newtonsoft.Json.Linq;
using System.Web;
using System.Net.Http.Headers;
using CambridgeSoft.COE.Framework.COESecurityService;

namespace PerkinElmer.COE.ChemBioViz.Server.Controllers
{
    public class RegSearchControllerBase : ApiController
    {
      
        protected JArray ExtractData(SafeDataReader reader)
        {
            var data = new JArray();
            var fieldCount = reader.FieldCount;
            while (reader.Read())
            {
                var row = new JObject();
                for (int i = 0; i < fieldCount; ++i)
                {
                    var fieldName = reader.GetName(i);
                    var fieldType = reader.GetFieldType(i);
                    object fieldData;
                    switch (fieldType.Name.ToLower())
                    {
                        case "int16":
                        case "int32":
                            fieldData = reader.GetInt32(i);
                            break;
                        case "datetime":
                            fieldData = reader.GetDateTime(i);
                            break;
                        case "decimal":
                            fieldData = (double)reader.GetDecimal(i);
                            break;
                        default:
                            fieldData = reader.GetString(i);
                            break;
                    }
                    row.Add(new JProperty(fieldName, fieldData));
                }
                data.Add(row);
            }
            return data;
        }

        //protected object ExtractValue(string sql, Dictionary<string, object> args = null)
        //{
            //using (var reader = GetReader(sql, args))
            //{
            //    object value = null;
            //    while (reader.Read())
            //    {
            //        var fieldType = reader.GetFieldType(0);
            //        switch (fieldType.Name.ToLower())
            //        {
            //            case "int16":
            //            case "int32":
            //                value = reader.GetInt32(0);
            //                break;
            //            case "datetime":
            //                value = reader.GetDateTime(0);
            //                break;
            //            default:
            //                value = reader.GetString(0);
            //                break;
            //        }
            //    }
            //    return value;
            //}
        //}

        //protected JArray ExtractData(string sql, Dictionary<string, object> args = null)
        //{
        //    using (var reader = GetReader(sql, args))
        //    {
        //        return ExtractData(reader);
        //    }
        //}

        protected void CheckAuthentication()
        {
            string sessionToken = "";
            CookieHeaderValue cookie = Request.Headers.GetCookies("COESSO").FirstOrDefault();
            if (cookie != null)
                sessionToken = cookie["COESSO"].Value;
            if (string.IsNullOrEmpty(sessionToken) || !COEPrincipal.Login(sessionToken, true))
                throw new InvalidOperationException("Authentication failed");
        }

        public static string GetAbsoluteUrl(string relativeUrl, bool globalScope = false)
        {
            if (string.IsNullOrEmpty(relativeUrl))
                return relativeUrl;

            if (HttpContext.Current == null)
                return relativeUrl;

            if (!globalScope)
            {
                if (relativeUrl.StartsWith("/"))
                    relativeUrl = relativeUrl.Insert(0, "~");
                if (!relativeUrl.StartsWith("~/"))
                    relativeUrl = relativeUrl.Insert(0, "~/");
                relativeUrl = VirtualPathUtility.ToAbsolute(relativeUrl);
            }

            var url = HttpContext.Current.Request.Url;
            var port = url.Port != 80 ? (":" + url.Port) : String.Empty;

            return String.Format("{0}://{1}{2}{3}", url.Scheme, url.Host, port, relativeUrl);
        }
    }
}