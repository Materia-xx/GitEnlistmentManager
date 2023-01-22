using GitEnlistmentManager.Globals;
using Newtonsoft.Json;

namespace GitEnlistmentManager.ClientServer
{
    public class GemCSCommand
    {
        public GemCSCommandType CommandType { get; set; }

        public object[]? CommandArgs { get; set; }

        public string? WorkingDirectory { get; set; }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this, GemJsonSerializer.Settings);
        }

        public static GemCSCommand? DeSerialize(string jsonString)
        {
            return JsonConvert.DeserializeObject<GemCSCommand>(jsonString, GemJsonSerializer.Settings);
        }
    }
}
