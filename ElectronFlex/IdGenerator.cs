namespace ElectronFlex
{
    public class IdGenerator
    {
        private byte next = 0;
        private byte max = byte.MaxValue;

        public byte Next()
        {
            return next++;
        }
    }
}