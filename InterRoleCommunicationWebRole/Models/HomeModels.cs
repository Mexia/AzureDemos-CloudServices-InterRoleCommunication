namespace InterRoleCommunicationWebRole.Models
{
    using System.ComponentModel.DataAnnotations;
    
    public class SendMessageModel
    {
        [Required]
        [Display(Name = "Message content")]
        public string MessageContent { get; set; }
    }
}