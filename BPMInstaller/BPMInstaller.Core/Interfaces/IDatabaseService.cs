namespace BPMInstaller.Core.Interfaces
{
    public interface IDatabaseService
    {
        public bool ValidateConnection();

        public bool CreateDatabase();

        public void RestoreDatabase();
    }
}
