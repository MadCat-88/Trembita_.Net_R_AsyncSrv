using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json;
using static PersonApi_async_REST.LogItem;

namespace PersonApi_async_REST
{

    /// <summary>
    /// визначає контекст бази даних, що є основним класом, що координує функціональні можливості Entity Framework для моделі даних.
    /// </summary>
    public class PersonDB(DbContextOptions<PersonDB> options) : DbContext(options)
    {
        public DbSet<PersonItem> Persons => Set<PersonItem>();

        #region Required
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PersonItem>().Property(b => b.Unzr).IsRequired(required: true);
            modelBuilder.Entity<PersonItem>().Property(b => b.LastUpdated).ValueGeneratedOnAddOrUpdate().Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Save);
        }
        #endregion
    }

    /// <summary>
    /// визначає контекст бази даних для логування запитів.
    /// </summary>
    public class LogDB(DbContextOptions<LogDB> options) : DbContext(options)
    {
        public DbSet<LogItem> Logs => Set<LogItem>();

        #region Required
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LogItem>().Property(b => b.Id).IsRequired(required: true);
            modelBuilder.Entity<LogItem>().Property(b => b.DateReq).ValueGeneratedOnAdd().Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Save);
        }
        #endregion
    }

    /// <summary>
    ///  Клас моделі представляє дані, якими керує наша програма.
    /// </summary>
    [Table("Persons")]
    [PrimaryKey(nameof(Unzr))]
    public class PersonItem
    {
        public enum GenderEnum
        {
            male,
            female
        }

        [JsonProperty(propertyName: nameof(Name), Required = Required.Always)] public string Name { set; get; } = string.Empty;
        [JsonProperty(propertyName: nameof(Surname), Required = Required.Always)] public string Surname { set; get; } = string.Empty;
        [JsonProperty(propertyName: nameof(Patronym))] public string? Patronym { set; get; }
        [JsonProperty(propertyName: nameof(DateOfBirth), Required = Required.Always)] public DateTime DateOfBirth { set; get; }
        [JsonProperty(propertyName: nameof(Gender), Required = Required.Always)] public GenderEnum Gender { set; get; }
        [JsonProperty(propertyName: nameof(Rnokpp))] public string? Rnokpp { set; get; }
        [JsonProperty(propertyName: nameof(PassportNumber))] public string? PassportNumber { set; get; }

        [Required]
        [JsonProperty(propertyName: nameof(Unzr), Required = Required.Always)] public required string Unzr { init; get; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime Inserted { get; init; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    ///  Клас моделі представляє дані логів.
    /// </summary>
    [Table("Log")]
    [PrimaryKey(nameof(Id))]
    public class LogItem
    {
        public enum StatusEnum
        {
            beginning,
            processing,
            result,
            error
        }
        [JsonProperty(propertyName: nameof(Id), Required = Required.Always)] public string Id { init; get; } = "0";
        [JsonProperty(propertyName: nameof(Status), Required = Required.Always)] public StatusEnum Status { set; get; } = StatusEnum.beginning;
        [JsonProperty(propertyName: nameof(DateReq), Required = Required.Always)] public DateTime DateReq { init; get; } = DateTime.UtcNow;
        [JsonProperty(propertyName: nameof(RequestBody), Required = Required.Always)] public string RequestBody { init; get; } = string.Empty;
        [JsonProperty(propertyName: nameof(DateResp))] public DateTime? DateResp { set; get; }
        [JsonProperty(propertyName: nameof(ResponceBody))] public string? ResponceBody { set; get; }
    }
    /// <summary>
    ///  Клас для надання структурованої відповіді
    /// </summary>
    public class Resp
    {
        [JsonProperty(propertyName: nameof(Id), Required = Required.Always)] public string Id { set; get; } = new TimeSpan().TotalNanoseconds.ToString();
        [JsonProperty(propertyName: nameof(Status), Required = Required.Always)] public StatusEnum Status { set; get; } = StatusEnum.beginning;
    }
}
