namespace GitEnlistmentManager.ClientServer
{
    public enum GemCSCommandType
    {
        /// <summary>
        /// We've capture something passed into a commandline on another instance of GEM
        /// that info is being sent to the main GEM instance to be processed.
        /// </summary>
        InterpretCommandLine
    }
}
