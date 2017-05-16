namespace DiscordSoundboard
{
    public class DeviceItem
    {
        public int Id { get; set; }

        public string DeviceId { get; set; }

        public string DisplayName { get; set; }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
