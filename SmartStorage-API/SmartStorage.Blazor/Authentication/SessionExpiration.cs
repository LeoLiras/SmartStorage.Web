namespace SmartStorage.Blazor.Authentication
{
    public class SessionExpiration
    {
        #region Properties

        public bool IsExpired { get; private set; }

        public event Func<Task> Expired;

        #endregion

        #region Methods

        public async Task NotifyExpired()
        {
            if (IsExpired)
                return;

            IsExpired = true;

            if (Expired is not null)
                await Expired.Invoke();
        }

        public void Reset()
        {
            IsExpired = false;
        }

        #endregion
    }
}
