namespace AppWebBiblioteca.Models
{
    public class ApiBiblioteca
    {

        public HttpClient IniciarApi() { 
        
            HttpClient httpClient = new HttpClient();

            httpClient.BaseAddress = new Uri("https://localhost:7270/");
            return httpClient;

        }
    }
}
