namespace Data.JSONObjects
{
#pragma warning disable CS8618
    public class Data
    {
        public string AdminToken { get; set; }
        public string CallbackURL { get; set; }
        public List<DataSub> Subs { get; set; }
    }
#pragma warning restore CS8618
}
