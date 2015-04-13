namespace Newtonsoft.Json.Tests.TestObjects
{
    public class DocumentChanged<TKey, TDoc>
    {
        public TKey Id { get; set; }
        public TDoc Document { get; set; }
    }
}
