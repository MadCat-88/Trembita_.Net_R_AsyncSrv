using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace PersonApi_async_REST
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ������ �������� ���� ����� � ��������� ������������ ����������� (DI). ��������� DI ���� ������ �� ��������� ���� ����� �� ����� �����.
            builder.Services.AddDbContext<PersonDB>(opt => opt.UseInMemoryDatabase("PersonList"));
            builder.Services.AddDbContext<LogDB>(opt => opt.UseInMemoryDatabase("LogList"));

            // �������� ������� API, ���� ����� ����� ������, ��� ���� ������� ��� HTTP-API. ������� API ��������������� Swagger ��� ��������� ��������� Swagger.
            builder.Services.AddEndpointsApiExplorer();

            // ������ ��������� ��������� Swagger OpenAPI �� ����� ������� � ����������� ���� ��� ������� ���������� ��������� ��� API, ����� �� ����� �� �����.
            // ��� �������� �������� ������� ��� API, ���. � ����� "������� ������ � NSwag" ��
            // ASP.NET Core (https://learn.microsoft.com/uk-ua/aspnet/core/tutorials/getting-started-with-nswag?view=aspnetcore-8.0#customize-api-documentation)
            builder.Services.AddOpenApiDocument(config =>
            {
                config.DocumentName = "PersonAPI";
                config.Title = "PersonAPI v1";
                config.Version = "v1";
            });

            var app = builder.Build();

            // ��� ��������� ���� ��������� ����� ������� ����� �� ��������� ��������� URL-������ �� ��������� ������ MapGroup.
            var PersonItems = app.MapGroup("/api");

            PersonItems.MapGet("/", GetAllPerson); // ����� �� �������� �񳺿 ���� ����� � ������������� GET
            PersonItems.MapPost("/", CreatePerson); // ����� �� ��������� ������ � ��� ����� � ������������� POST
            PersonItems.MapGet("/{unzr}", GetPerson); // ����� �� �������� ������ ���� ����� �� ��������� ���� � ������������� GET
            PersonItems.MapPost("/{unzr}", UpdatePerson); // ����������� ������ ���� ����� �� ��������� ���� � ������������� POST
            PersonItems.MapDelete("/{unzr}", DeletePerson); // ��������� ������ � ���� ����� �� ��������� ���� � ������������� DELETE
            PersonItems.MapGet("/status/{id}", GetStatus); // �������� ������� ������ � ������������� GET
            PersonItems.MapPut("/status/{id}", ViewResult); // �������� ���������� ������ � ������������� PUT
            PersonItems.MapGet("/status", GetLog); // �������� ���� ������ ������������� GET


            if (app.Environment.IsDevelopment())
            {
                // ������ ������� ��������� ������������ ��� �������������� ��������� OpenAPI 3.0
                // �������� �� �������: http://localhost:<port>/swagger/v1/swagger.json
                app.UseOpenApi();

                // ������ ���-��������� ��� �����䳿 � ����������
                // �������� �� �������: http://localhost:<port>/swagger
                app.UseSwaggerUi();
            }

            // 
            app.Run();

            // ֳ ������ ���������� ��'����, �� ��������� IResult. ������ ��������������� TypedResults ������ Results.
            // �� �� ����� �������, ��������� ���������� � ����������� ���������� ������� ���� ������ ��� OpenAPI, ��� ������� ������ �����.

            static string GenID(LogDB log)
            {
                return log.Logs.Count().ToString() + DateTime.Now.Ticks.ToString();
            }

            static async Task<IResult> GetAllPerson(PersonDB db, LogDB log)
            {
                var logLine = new LogItem()
                {
                    Id = GenID(log),
                    Status = LogItem.StatusEnum.beginning,
                    DateReq = DateTime.UtcNow,
                    RequestBody = "GetAllPerson"
                };
                log.Logs.Add(logLine);
                await log.SaveChangesAsync();

                return TypedResults.Ok(new Resp() { Id = logLine.Id, Status = logLine.Status });
            }

            static async Task<IResult> GetLog(LogDB log)
            {
                var logLine = new LogItem()
                {
                    Id = GenID(log),
                    Status = LogItem.StatusEnum.result,
                    DateReq = DateTime.UtcNow,
                    RequestBody = "GetLog",
                    DateResp = DateTime.UtcNow,
                    ResponceBody = JsonConvert.SerializeObject("the result is archived")
                };
                log.Logs.Add(logLine);
                await log.SaveChangesAsync();

                return TypedResults.Ok(await log.Logs.ToArrayAsync());
            }

            static async Task<IResult> CreatePerson(PersonItem inputPers, PersonDB db, LogDB log)
            {
                try
                {
                    var Pers = new PersonItem()
                    {
                        Name = inputPers.Name,
                        Surname = inputPers.Surname,
                        Patronym = inputPers.Patronym,
                        DateOfBirth = inputPers.DateOfBirth,
                        Gender = inputPers.Gender,
                        Rnokpp = inputPers.Rnokpp,
                        PassportNumber = inputPers.PassportNumber,
                        Unzr = inputPers.Unzr,
                        Inserted = DateTime.Now,
                        LastUpdated = DateTime.Now
                    };

                    db.Persons.Add(Pers);
                    await db.SaveChangesAsync();

                    var logLine = new LogItem()
                    {
                        Id = GenID(log),
                        Status = LogItem.StatusEnum.result,
                        DateReq = DateTime.UtcNow,
                        RequestBody = "CreatePerson",
                        DateResp = DateTime.UtcNow,
                        ResponceBody = JsonConvert.SerializeObject(Pers)
                    };
                    log.Logs.Add(logLine);
                    await log.SaveChangesAsync();

                    return TypedResults.Ok(new Resp() { Id = logLine.Id, Status = logLine.Status });
                }
                catch (Exception e)
                {
                    return TypedResults.Problem(e.Message);
                }
            };

            static async Task<IResult> GetPerson(string unzr, PersonDB db, LogDB log)
            {
                try
                {
                    var r = new Resp();
                    var pers = await db.Persons.FindAsync(unzr);

                    if (pers is null)
                    {
                        var logLine = new LogItem()
                        {
                            Id = GenID(log),
                            Status = LogItem.StatusEnum.error,
                            DateReq = DateTime.UtcNow,
                            RequestBody = "GetPerson",
                            DateResp = DateTime.UtcNow,
                            ResponceBody = JsonConvert.SerializeObject("Not Found")
                        };
                        log.Logs.Add(logLine);
                        await log.SaveChangesAsync();
                        r.Id = logLine.Id;
                        r.Status = logLine.Status;
                    }
                    else
                    {
                        var logLine = new LogItem()
                        {
                            Id = GenID(log),
                            Status = LogItem.StatusEnum.processing,
                            DateReq = DateTime.UtcNow,
                            RequestBody = "GetPerson",
                            DateResp = DateTime.UtcNow,
                            ResponceBody = JsonConvert.SerializeObject(pers)
                        };
                        log.Logs.Add(logLine);
                        await log.SaveChangesAsync();
                        r.Id = logLine.Id;
                        r.Status = logLine.Status;
                    }
                    return TypedResults.Ok(r);
                }
                catch (Exception e)
                {
                    return TypedResults.Problem(e.Message);
                }
            };

            static async Task<IResult> GetStatus(string id, LogDB log, PersonDB db)
            {
                try
                {
                    var r = new Resp();
                    var logline = await log.Logs.FindAsync(id);

                    if (logline is null)
                    {
                        var logLine = new LogItem()
                        {
                            Id = GenID(log),
                            Status = LogItem.StatusEnum.error,
                            DateReq = DateTime.UtcNow,
                            RequestBody = "GetStatus",
                            DateResp = DateTime.UtcNow,
                            ResponceBody = JsonConvert.SerializeObject("Not Found")
                        };
                        log.Logs.Add(logLine);
                        await log.SaveChangesAsync();

                        return TypedResults.NotFound(id);
                    }
                    else
                    {
                        switch (logline.Status)
                        {
                            case LogItem.StatusEnum.error:
                                return TypedResults.Ok(new Resp() { Id = logline.Id, Status = logline.Status });

                            case LogItem.StatusEnum.beginning:
                                logline.Status = LogItem.StatusEnum.processing;
                                await log.SaveChangesAsync();
                                return TypedResults.Ok(new Resp() { Id = logline.Id, Status = logline.Status });

                            case LogItem.StatusEnum.processing:
                                logline.Status = LogItem.StatusEnum.result;
                                if (logline.RequestBody == "GetAllPerson")
                                {
                                    logline.ResponceBody = JsonConvert.SerializeObject(await db.Persons.ToArrayAsync());
                                }
                                await log.SaveChangesAsync();

                                return TypedResults.Ok(new Resp() { Id = logline.Id, Status = logline.Status });

                            default:
                                return TypedResults.Ok(logline.ResponceBody);
                        }
                    }
                }
                catch (Exception e)
                {
                    return TypedResults.Problem(e.Message);
                }
            };

            static async Task<IResult> UpdatePerson(string unzr, PersonItem inputPers, PersonDB db, LogDB log)
            {
                try
                {
                    var r = new Resp();
                    var pers = await db.Persons.FindAsync(unzr);

                    if (pers is null)
                    {
                        var logLine = new LogItem()
                        {
                            Id = GenID(log),
                            Status = LogItem.StatusEnum.error,
                            DateReq = DateTime.UtcNow,
                            RequestBody = "GetPerson",
                            DateResp = DateTime.UtcNow,
                            ResponceBody = JsonConvert.SerializeObject("Not Found")
                        };
                        log.Logs.Add(logLine);
                        await log.SaveChangesAsync();
                        r.Id = logLine.Id;
                        r.Status = logLine.Status;
                    }
                    else
                    {
                        pers.Name = inputPers.Name;
                        pers.Surname = inputPers.Surname;
                        pers.Patronym = inputPers.Patronym;
                        pers.DateOfBirth = inputPers.DateOfBirth;
                        pers.Gender = inputPers.Gender;
                        pers.Rnokpp = inputPers.Rnokpp;
                        pers.PassportNumber = inputPers.PassportNumber;
                        pers.LastUpdated = DateTime.Now;

                        await db.SaveChangesAsync();

                        var logLine = new LogItem()
                        {
                            Id = GenID(log),
                            Status = LogItem.StatusEnum.result,
                            DateReq = DateTime.UtcNow,
                            RequestBody = "UpdatePerson",
                            DateResp = DateTime.UtcNow,
                            ResponceBody = JsonConvert.SerializeObject(pers)
                        };
                        log.Logs.Add(logLine);
                        await log.SaveChangesAsync();
                        r.Id = logLine.Id;
                        r.Status = logLine.Status;
                    }
                    return TypedResults.Ok(r);
                }
                catch (Exception e)
                {
                    return TypedResults.Problem(e.Message);
                }
            };

            static async Task<IResult> DeletePerson(string unzr, PersonDB db, LogDB log)
            {
                try
                {
                    var r = new Resp();
                    if (await db.Persons.FindAsync(unzr) is PersonItem pers)
                    {
                        db.Persons.Remove(pers);
                        await db.SaveChangesAsync();

                        var logLine = new LogItem()
                        {
                            Id = GenID(log),
                            Status = LogItem.StatusEnum.result,
                            DateReq = DateTime.UtcNow,
                            RequestBody = "DeletePerson",
                            DateResp = DateTime.UtcNow,
                            ResponceBody = JsonConvert.SerializeObject(pers)
                        };
                        log.Logs.Add(logLine);
                        await log.SaveChangesAsync();
                        r.Id = logLine.Id;
                        r.Status = logLine.Status;
                    }
                    else
                    {
                        var logLine = new LogItem()
                        {
                            Id = GenID(log),
                            Status = LogItem.StatusEnum.error,
                            DateReq = DateTime.UtcNow,
                            RequestBody = "DeletePerson",
                            DateResp = DateTime.UtcNow,
                            ResponceBody = JsonConvert.SerializeObject("Not Found")
                        };
                        log.Logs.Add(logLine);
                        await log.SaveChangesAsync();
                        r.Id = logLine.Id;
                        r.Status = logLine.Status;
                    }

                    return TypedResults.Ok(r);
                }
                catch (Exception e)
                {
                    return TypedResults.Problem(e.Message);
                }
            }

            static async Task<IResult> ViewResult(string id, LogDB log)
            {
                try
                {
                    var r = new Resp();
                    var logline = await log.Logs.FindAsync(id);

                    if (logline is null) return TypedResults.NotFound(id);
                    if (logline.Status == LogItem.StatusEnum.result)
                    {
                        return logline.RequestBody switch
                        {
                            "GetAllPerson" => TypedResults.Ok(JsonConvert.DeserializeObject<PersonItem[]>(logline.ResponceBody)),
                            "GetPerson" => TypedResults.Ok(JsonConvert.DeserializeObject<PersonItem>(logline.ResponceBody)),
                            "UpdatePerson" => TypedResults.Ok(JsonConvert.DeserializeObject<PersonItem>(logline.ResponceBody)),
                            "DeletePerson" => TypedResults.Ok(JsonConvert.DeserializeObject<PersonItem>(logline.ResponceBody)),
                            _ => TypedResults.Ok(JsonConvert.DeserializeObject(logline.ResponceBody)),
                        };
                    }
                    else
                    {
                        return TypedResults.Problem($"Status is {logline.Status}");
                    }
                }
                catch (Exception e)
                {
                    return TypedResults.Problem(e.Message);
                }
            };
        }
    }
}