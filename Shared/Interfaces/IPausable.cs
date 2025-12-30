namespace ClaudiaIDE.Interfaces
{
    internal interface IPausable
    {
        bool IsPaused { get; }
        void Pause();
        void Resume();
    }
}