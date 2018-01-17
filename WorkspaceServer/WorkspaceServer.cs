namespace WorkspaceServer
{
    public static class WorkspaceServer
    {
#if DEBUG
        public const int DefaultTimeoutInSeconds = 10;
#else
        public const int DefaultTimeoutInSeconds = 5;
#endif
    }
}