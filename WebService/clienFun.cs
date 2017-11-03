using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Net;
using System.Collections;
using System.IO;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
//����������ʽ�����ã�����Ͳ��ܷ��������м���
namespace WebService
{
    class clientFun
    {

//1��ȡ�ַ���ָ����ǩ��ֵ
        ///
        /// ��ȡ�ַ���ָ����ǩ��ֵ
        ///
        /// �ַ���
        /// ��ǩ
        /// ֵ
        public static string GetTitleContent(string str, string title)
        {
            string tmpStr = string.Format("<{0}[^>]*?>(?[^<]*)</{1}>", title, title); //��ȡ
            Match TitleMatch = Regex.Match(str, tmpStr, RegexOptions.IgnoreCase);
            string result = TitleMatch.Groups["Text"].Value;
            return result;
        }
        ///  
        /// ��ȡ�ַ���ָ����ǩ��ֵ 
        ///  
        /// �ַ��� 
        /// ��ǩ 
        /// ������ 
        /// ���� 
        public static string GetTitleContent(string str, string title, string attrib)
        {
            string tmpStr = string.Format(@"<{0}[^>]*?{1}=(['""]?)(?[^'""\\s>]+)\\1[^>]*>", title, attrib); //��ȡ
            Match TitleMatch = Regex.Match(str, tmpStr, RegexOptions.IgnoreCase);
            string result = TitleMatch.Groups["url"].Value;
            return result;
        }
//2 ���л� �����л�
        ///
        /// ���л�����
        ///
        /// ��������
        /// ����
        ///
 
        public static string Serialize(T t)
        {
            using (StringWriter sw = new StringWriter())
            {
                XmlSerializer xz = new XmlSerializer(t.GetType());
                xz.Serialize(sw, t);
                return sw.ToString();
            }
        }
       
        ///
        /// �����л�Ϊ����
        ///
        /// ��������
        /// �������л����Xml�ַ���
        ///
        ///
        public static object Deserialize(Type type, string s)
        {
            using (StringReader sr = new StringReader(s))
            {
                XmlSerializer xz = new XmlSerializer(type);
                return xz.Deserialize(sr);
            }
        }

        //Serialize�����������£�
        ///
        /// ʵ���༯�����л�Ϊ�ַ���
        ///
        ///
        ///
        ///
        public static string Serializer2(T objToXml)
        {
            using (StringWriter writer = new StringWriter())
            {
                XmlSerializer serializer = new XmlSerializer(objToXml.GetType());
                serializer.Serialize(writer, objToXml);
                return writer.GetStringBuilder().ToString();
            }
        }

        //DeserializerCollection�����������£�
        ///
        ///
        ///
        ///
        ///
        public static Collection DeSerializerCollection(string sXml, Type type)
        {
            using (StringReader reader = new StringReader(sXml))
            {
                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(type);
                object obj = serializer.Deserialize(reader);
                return (Collection)obj;
            }
        }
//3 web ����
        ///
        /// ��ҪWebService֧��Post����
        ///
        public static XmlDocument QueryPostWebService(String URL, String MethodName, Hashtable Pars)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(URL + "/" + MethodName);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            SetWebRequest(request);
            byte[] data = EncodePars(Pars);
            WriteRequestData(request, data);
            return ReadXmlResponse(request.GetResponse());
        }
        ///
        /// ��ҪWebService֧��Get����
        ///
        public static XmlDocument QueryGetWebService(String URL, String MethodName, Hashtable Pars)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(URL + "/" + MethodName + "?" + ParsToString(Pars));
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded";
            SetWebRequest(request);
            return ReadXmlResponse(request.GetResponse());
        }
        ///
        /// ͨ��WebService����(Soap),����ParsΪString���͵Ĳ�����������ֵ
        ///
        public static XmlDocument QuerySoapWebService(String URL, String MethodName, Hashtable Pars)
        {
            if (_xmlNamespaces.ContainsKey(URL))
            {
                return QuerySoapWebService(URL, MethodName, Pars, _xmlNamespaces[URL].ToString());
            }
            else
            {
                return QuerySoapWebService(URL, MethodName, Pars, GetNamespace(URL));
            }
        }
        private static XmlDocument QuerySoapWebService(String URL, String MethodName, Hashtable Pars, string XmlNs)
        {
            _xmlNamespaces[URL] = XmlNs;//���뻺�棬���Ч��
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(URL);
            request.Method = "POST";
            request.ContentType = "text/xml; charset=utf-8";
            request.Headers.Add("SOAPAction", """ + XmlNs + (XmlNs.EndsWith("/") ? "" : "/") + MethodName + """);
            SetWebRequest(request);
            byte[] data = EncodeParsToSoap(Pars, XmlNs, MethodName);
            WriteRequestData(request, data);
            XmlDocument doc = new XmlDocument(), doc2 = new XmlDocument();
            doc = ReadXmlResponse(request.GetResponse());
            XmlNamespaceManager mgr = new XmlNamespaceManager(doc.NameTable);
            mgr.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");
            String RetXml = doc.SelectSingleNode("//soap:Body/*/*", mgr).InnerXml;
            doc2.LoadXml("" + RetXml + "");
            AddDelaration(doc2);
            return doc2;
        }
        private static string GetNamespace(String URL)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL + "?WSDL");
            SetWebRequest(request);
            WebResponse response = request.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(sr.ReadToEnd());
            sr.Close();
            return doc.SelectSingleNode("//@targetNamespace").Value;
        }
        private static byte[] EncodeParsToSoap(Hashtable Pars, String XmlNs, String MethodName)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">");
            AddDelaration(doc);
            //XmlElement soapBody = doc.createElement_x_x_x_x("soap", "Body", "http://schemas.xmlsoap.org/soap/envelope/");
            XmlElement soapBody = doc.CreateElement("soap", "Body", "http://schemas.xmlsoap.org/soap/envelope/");
            //XmlElement soapMethod = doc.createElement_x_x_x_x(MethodName);
            XmlElement soapMethod = doc.CreateElement(MethodName);
            soapMethod.SetAttribute("xmlns", XmlNs);
            foreach (string k in Pars.Keys)
            {
                //XmlElement soapPar = doc.createElement_x_x_x_x(k);
                XmlElement soapPar = doc.CreateElement(k);
                soapPar.InnerXml = ObjectToSoapXml(Pars[k]);
                soapMethod.A(soapPar);
            }
            soapBody.A(soapMethod);
            doc.DocumentElement.A(soapBody);
            return Encoding.UTF8.GetBytes(doc.OuterXml);
        }
        private static string ObjectToSoapXml(object o)
        {
            XmlSerializer mySerializer = new XmlSerializer(o.GetType());
            MemoryStream ms = new MemoryStream();
            mySerializer.Serialize(ms, o);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(Encoding.UTF8.GetString(ms.ToArray()));
            if (doc.DocumentElement != null)
            {
                return doc.DocumentElement.InnerXml;
            }
            else
            {
                return o.ToString();
            }
        }
        ///
        /// ����ƾ֤�볬ʱʱ��
        ///
        ///
        private static void SetWebRequest(HttpWebRequest request)
        {
            request.Credentials = CredentialCache.DefaultCredentials;
            request.Timeout = 10000;
        }
        private static void WriteRequestData(HttpWebRequest request, byte[] data)
        {
            request.ContentLength = data.Length;
            Stream writer = request.GetRequestStream();
            writer.Write(data, 0, data.Length);
            writer.Close();
        }
        private static byte[] EncodePars(Hashtable Pars)
        {
            return Encoding.UTF8.GetBytes(ParsToString(Pars));
        }
        private static String ParsToString(Hashtable Pars)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string k in Pars.Keys)
            {
                if (sb.Length > 0)
                {
                    sb.Append("&");
                }
                //sb.Append(HttpUtility.UrlEncode(k) + "=" + HttpUtility.UrlEncode(Pars[k].ToString()));
            }
            return sb.ToString();
        }
        private static XmlDocument ReadXmlResponse(WebResponse response)
        {
            StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            String retXml = sr.ReadToEnd();
            sr.Close();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(retXml);
            return doc;
        }
        private static void AddDelaration(XmlDocument doc)
        {
            XmlDeclaration decl = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            doc.InsertBefore(decl, doc.DocumentElement);
        }
        private static Hashtable _xmlNamespaces = new Hashtable();//����xmlNamespace�������ظ�����GetNamespace
    }