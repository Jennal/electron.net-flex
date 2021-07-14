using CommandLine;

namespace ElectronFlex
{
    public class CommandLineOptions
    {
        [Option(longName: "webport", Required = true, HelpText = "Web port")]
        public int WebPort { get; set; }

        [Option(longName: "wsport", Required = true, HelpText = "Web socket port")]
        public int WebSocketPort { get; set; }

        [Option(shortName: 'e', longName: "electron", Required = false, HelpText = "Start from electron")]
        public bool StartFromElectron { get; set; }
    }
}