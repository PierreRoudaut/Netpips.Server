
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Netpips.Identity.Model;
using Newtonsoft.Json;

namespace Netpips.Subscriptions.Model
{
    public class TvShowSubscription
    {
        [JsonIgnore]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string ShowTitle { get; set; }
        public int ShowRssId { get; set; }

        [JsonIgnore]
        public User User { get; set; }
        [JsonIgnore]
        public Guid UserId { get; set; }

    }
}
