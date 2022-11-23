namespace ElizerBot.Adapter
{
    public class FileDescriptorAdapter
    {
        private readonly Func<Task<Stream>> _readFile;

        public string FileName { get; set; }

        public FileDescriptorAdapter(string fileName, Func<Task<Stream>> readFile)
        {
            FileName = fileName;
            _readFile = readFile;
        }

        public Task<Stream> ReadFile()
        { 
            return _readFile();
        }
    }
}
