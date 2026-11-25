namespace SmartStorage.Blazor.Authentication
{
    /// <summary>
    /// Singleton porque o handler é criado de novo para cada HttpClient tipado: com a trava
    /// dentro dele, cada serviço renovaria por conta própria, e o servidor troca o refresh
    /// token a cada renovação, então a segunda chegaria com um token já trocado.
    /// </summary>
    public class RefreshLock
    {
        #region Properties

        public SemaphoreSlim Semaphore { get; } = new(1, 1);

        public string RefusedAccessToken { get; set; }

        #endregion
    }
}
