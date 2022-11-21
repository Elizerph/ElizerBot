namespace ElizerBot.Adapter
{
    public class FileDescriptorAdapter
    {
        private readonly Func<Stream> _readFile;

        public string FileName { get; set; }

        public FileDescriptorAdapter(string fileName, Func<Stream> readFile)
        {
            FileName = fileName;
            _readFile = readFile;
        }

        public Stream ReadFile()
        { 
            return _readFile();
        }

        public static FileDescriptorAdapter FromFilePath(string fileName, string filePath) 
        {
            return new FileDescriptorAdapter(fileName, () => File.OpenRead(filePath));
        }
    }
}
