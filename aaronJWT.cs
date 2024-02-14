using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ApiJWTAaron
{
    /// <summary>
    /// Esta clase hace conexion con la API segun los estandares establecidos
    /// Es necesario incluir los datos que seran enviados, a traves de atributo data. Este es un objeto de tipo ApiRequest.
    /// Se deben setear objetos de clases hijas de ApiRequest para que rellenar todos los datos a enviar.
    /// ApiRequest solo contiene los atributos fijos necesarios para enviar a la Api. Las clases hijas implementan los demas datos a enviar
    /// Siempre se deben establecer el usuario y la contrasena a la instancia de esta clase
    /// Siempre se debe establecer el TypeOfResponse para la correcta deserealizacion del json de respuesta
    /// GetResponseAsObject siempre regresa un objeto generico de de ApiResponse
    /// Para obtener todos los atributos del objeto respuesta, es necesario especificar el tipo de objeto desde donde se llama el metodo GetResponse
    /// ClaseHijaDeApiResponse Ob = InstanceOfThisClass.GetResponseByObject as ClaseHijaDeApiResponse;
    /// 
    /// </summary>
    public class aaronJWT
    {
        private string user;
        private string pass;
        private string url_server;
        private string type_process;
        private dynamic data;
        private long iat;
        private long exp;
        private string encoded_header;
        private string encoded_payload;
        private string header_payload;
        private string secret_key;
        private string signature;
        private string jwt_token;
        private string msgErr;
        private int indErr;
        private dynamic? response_jwt;
        private string recieved_signature;
        private string recievedHeaderAndPayload;
        private string resultedsignature;
        private int expNumb;
        private string str_response_server;
        private Type type_of_response;
        private bool error_reporting;

        public aaronJWT()
        {
            //this.url_server = "skmexico.mx/pos/api/v1/w.php";
            //this.data = new ApiRequest();
            //this.secret_key = "OkMex2022";
            this.msgErr = "";
            this.indErr = 0;
            this.expNumb = 1;
            this.url_server = "localhost/apiAC/v1/w.php";//"sistemasyservicios.mx/api/v1/w.php";
            this.secret_key = "Admin2022";//"*@@roN-2O22";

            //encode header
            byte[] toEncodeHeaderAsBytes = ASCIIEncoding.ASCII.GetBytes(JsonConvert.SerializeObject(new Dictionary<string, string>() { ["alg"] = "HS256", ["typ"] = "JWT" }));
            encoded_header = Convert.ToBase64String(toEncodeHeaderAsBytes);
        }

        //Setters
        public string User
        {
            set => user = value;
        }
        public string Passw
        {
            set { this.pass = value; }
        }

        public bool ViewError
        {
            set { this.error_reporting = value; }
        }

        public dynamic Data
        {
            set { this.data = value; }
            get => this.data;
        }
        public string TypeProccess
        {
            set { this.type_process = value; }
        }

        public void setTypeOfResponse(Type type)
        {
            this.type_of_response = type;
        }

        public int ExpireNumb
        {
            set { this.expNumb = value; }
        }

        /*//////////           Demas funciones                  /////*/
        public void addErr(string mensaje)
        {
            //this.indErr++;
            if (this.msgErr != "") { this.msgErr += "<br>"; }
            this.msgErr += mensaje;
        }

        public string getErrDesc()
        {
            return this.msgErr;
        }

        public dynamic GetResponseAsObject()
        {
            return this.response_jwt;
        }

        public string GetResponseAsString()
        {
            return this.str_response_server;
        }

        static string GetZone()
        {
            TimeZoneInfo info = TimeZoneInfo.Local;
            string zone = info.DisplayName;
            return zone;
        }

        /////// Funcion principal
        public async Task<bool> Execute()
        {
            //Obtiene el tiempo a unixTime
            DateTime now = DateTime.Now;
            this.iat = ((DateTimeOffset)now).ToUnixTimeSeconds() -20;
            this.exp = ((DateTimeOffset)now).ToUnixTimeSeconds() + (60 * this.expNumb) +20;
            this.data.iat = this.iat;
            this.data.exp = this.exp;
            this.data.cmd = this.type_process;
            this.data.usr = this.user;
            this.data.pasw = this.pass;
            //this.data.zone = GetZone();

            //Cifra array de datos a enviar
            string serializedData = JsonConvert.SerializeObject(this.data);
            //System.Diagnostics.Trace.WriteLine(serializedData);
            byte[] toEncodeAsBytes = ASCIIEncoding.ASCII.GetBytes(serializedData);
            string bytearray = Convert.ToBase64String(toEncodeAsBytes);
            encoded_payload = bytearray.ToString();

            //Prepara header
            header_payload = encoded_header + "." + encoded_payload;

            //Crea la firma
            var HMA = new HMACSHA256(Encoding.UTF8.GetBytes(secret_key));
            var m = HMA.ComputeHash(Encoding.UTF8.GetBytes(header_payload));
            byte[] utfsignature = Encoding.UTF8.GetBytes(ToHexadecimal(m));
            signature = Convert.ToBase64String(utfsignature);


            jwt_token = header_payload + "." + signature;

            string full_url = "https://" + this.url_server + "?cmd=" + HttpUtility.UrlEncode(jwt_token) + "&onErr=1";
            try
            {
                HttpWebRequest request = HttpWebRequest.CreateHttp(full_url);
                request.Timeout = 20000;
                request.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                WebResponse webResponse = await request.GetResponseAsync();
                StreamReader sr = new StreamReader(webResponse.GetResponseStream());
                this.str_response_server = await sr.ReadToEndAsync();
            }
            catch (Exception e)
            {
                addErr("Ha ocurrido un error al conectarse con el servidor. Compruebe su conexion a internet.");
                return false;
            }

            ////////////////////////// Validaciones /////////////////////////// 

            if (String.IsNullOrEmpty(str_response_server) || str_response_server == "")
            {
                this.addErr("El servicio no esta disponible por el momento.");
                return false;
            }

            //Convierte la cadena de respuesta a objeto, de acuerdo al tipo de objeto de respuesta que se espera
            //Arroja error si las propiedades de la clase response y las propiedades del string json no son las mismas
            try
            {
                response_jwt = JsonConvert.DeserializeObject<ExpandoObject>(str_response_server);
            }
            catch(Exception er) //Si ocurre error agrega un error
            {
                addErr("No se recibio una respuesta valida.");
                return false;
            }

            try
            {
                if (this.response_jwt is null || this.response_jwt.errNo == null || this.response_jwt.msg == null)
                {
                    this.addErr("El formato de la respuesta es invalida.");
                    //ErrorHandler.ErrorLog.RecordError(new Exception("Error en aaronJWT linea 198: response status o msg eran null"));
                    return false;
                }
            }
            catch (Exception e)
            {
                this.addErr("El formato de la respuesta es invalida.");
                System.Diagnostics.Trace.WriteLine(e.ToString());
            }
            return true;
        }


        private string ToHexadecimal(byte[] tohexa)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i<tohexa.Length; i++)
            {
                sb.Append(tohexa[i].ToString("X2"));
            }

            return sb.ToString().ToLower();
        }


        //public bool Valid(string recievedJwt)
        //{
        //    //Separa la cadena por . 
        //    string[] jwt_values = recievedJwt.Split('.');

        //    if (jwt_values.Length != 3)
        //    {
        //        this.addErr("El formato de la solicitud es invalidad");
        //        return false;
        //    }

        //    //Extrayendo signature
        //    this.recieved_signature = jwt_values[2];

        //    //Concatena los primeras dos partes de la cadena
        //    this.recievedHeaderAndPayload = jwt_values[0] + "." + jwt_values[1];

        //    //Creando la firma en base64
        //    var HMA = new HMACSHA256(Encoding.UTF8.GetBytes(this.secret_key));
        //    var m = HMA.ComputeHash(Encoding.UTF8.GetBytes(this.recievedHeaderAndPayload));
        //    this.resultedsignature = Convert.ToBase64String(m);

        //     if(this.resultedsignature != this.recieved_signature)
        //    {
        //        this.addErr("Firma invalida");
        //        return false;
        //    }


        //    byte[] fromBase = Convert.FromBase64String(jwt_values[1]);
        //    this.data= JsonSerializer.Deserialize(ASCIIEncoding.ASCII.GetString(fromBase), this.data.GetType()) as ApiRequest;
        //    if (this.data == null)
        //    {
        //        this.addErr("El array de datos es invalido.");
        //        return false;
        //    }

        //    if(this.data.iat.Equals(null) && ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds() <= this.data.exp)
        //    {
        //        this.addErr("La peticion ha caducado.");
        //        return false;
        //    }

        //    //Restaura valores
        //    this.iat = this.data.iat;
        //    this.exp = this.data.exp;
        //    this.type_process = this.data.cmd;
        //    this.user = this.data.usr;
        //    this.pass = this.data.pasw;

        //    //Quita indices del vector que no se usarán
        //     return true;
        //}

    }
}
