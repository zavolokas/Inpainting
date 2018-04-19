namespace InpaintService
{
    public class InpaintRequest
    {
        public string Container { get; set; }
        public string Image { get; set; }
        public string RemoveMask { get; set; }
        public InpaintServiceSettings Settings { get; set; } = new InpaintServiceSettings();
    }
}
