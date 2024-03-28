namespace AxNotifierAPI
{
    public class Note
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public DateTime DateTime { get; set; }
        public string Type { get; set; }
        public bool IsRead { get; set; }
        public string Symbol { get; set; }
    }
}
