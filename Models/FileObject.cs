namespace NetDocsFileQueueFlusher.Models
{
    public class FileObject
    {
        public string Guid { get; set; } = "";
        public string SourceFile { get; set; } = "";
        public string NdFileName { get; set; } = "";
        public string NdUrl { get; set; } = "";
        public string NdFolderId { get; set; } = "";
        public string NdDocId { get; set; } = "";
        public string NdDocUrl { get; set; } = "";
        public string Status { get; set; } = "";
        public string Error { get; set; } = "";


    }
}
