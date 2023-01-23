namespace GitEnlistmentManager.DTOs.Commands
{
    public class RunProgramCommand : ICommand
    {
        public string? Program { get; set; }

        public string? Arguments { get; set; }
    }
}
